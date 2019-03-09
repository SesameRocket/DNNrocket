﻿using System;
using System.Collections.Generic;
using System.Linq;
using DNNrocket.Login;
using DNNrocketAPI;
using Simplisity;

namespace RocketMod
{
    public class startconnect : DNNrocketAPI.APInterface
    {
        private static ModuleData _moduleData;
        private static string _appthemeRelPath;
        private static string _appthemeMapPath;
        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, string userHostAddress, string editlang = "")
        {
            var strOut = "*TriggerLogin*";
            _appthemeRelPath = "/DesktopModules/DNNrocket/AppThemes";
            _appthemeMapPath = DNNrocketUtils.MapPath(_appthemeRelPath);

            var rocketInterface = new DNNrocketInterface(interfaceInfo);

            // we should ALWAYS pass back the moduleid in the template post.
            // But for the admin start we need it to be passed by the admin.aspx url parameters.  Which then puts it in the s-fields for the simplsity start call.
            var moduleid = postInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            if (moduleid == 0) moduleid = postInfo.ModuleId;
            if (moduleid == 0)
            {
                strOut = "ERROR: No moduleId has been passed to the API";
            }
            else
            {
                _moduleData = new ModuleData(moduleid);

                // use command form cookie if we have set it.
                var cookieCmd = DNNrocketUtils.GetCookieValue("rocketmod_cmd");
                if (cookieCmd != "")
                {
                    paramCmd = cookieCmd;
                    DNNrocketUtils.DeleteCookieValue("rocketmod_cmd");
                }

                if (DNNrocketUtils.SecurityCheckCurrentUser(rocketInterface))
                {
                    switch (paramCmd)
                    {
                        case "rocketmod_edit":
                            strOut = EditData(moduleid, rocketInterface, postInfo);
                            break;
                        case "rocketmod_savedata":
                            strOut = SaveData(moduleid, rocketInterface, postInfo);
                            break;
                        case "rocketmod_delete":
                            DeleteData(moduleid, postInfo);
                            strOut = EditData(moduleid, rocketInterface, postInfo);
                            break;
                        case "rocketmod_saveconfig":
                            _moduleData.SaveConfig(postInfo);
                            _moduleData.PopulateConfig();
                            strOut = GetDashBoard(moduleid, rocketInterface);
                            break;
                        case "rocketmod_getsetupmenu":
                            strOut = GetSetup(rocketInterface);
                            break;
                        case "rocketmod_dashboard":
                            strOut = GetDashBoard(moduleid, rocketInterface);
                            break;
                        case "rocketmod_reset":
                            strOut = ResetRocketMod(moduleid, rocketInterface);
                            break;
                    }
                }
                switch (paramCmd)
                {
                    case "rocketmod_getdata":
                        strOut = GetDisplay(rocketInterface);
                        break;
                }
            }
            switch (paramCmd)
            {
                case "rocketmod_login":
                    strOut = LoginUtils.DoLogin(postInfo, userHostAddress);
                    break;
                case "rocketmod_adminurl":
                    strOut = "/desktopmodules/dnnrocket/RocketMod/admin.aspx";
                    break;
            }

            if (strOut == "*TriggerLogin*")
            {
                strOut = LoginUtils.LoginForm(postInfo, rocketInterface.InterfaceKey);
            }

            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }

        public static void DeleteData(int moduleid, SimplisityInfo postInfo)
        {
            var selecteditemid = postInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            var objCtrl = new DNNrocketController();
            objCtrl.Delete(selecteditemid);
        }

        public static String EditData(int moduleid, DNNrocketInterface rocketInterface, SimplisityInfo postInfo)
        {
            try
            {
                var strOut = "";
                var themeFolder = "";
                var razortemplate = "";

                if (!_moduleData.ConfigExists)
                {
                    // no display type set, return dashboard.
                    return GetDashBoard(moduleid, rocketInterface);
                }

                if (_moduleData.IsList)
                {
                    razortemplate = "editlist.cshtml";
                }
                else
                {
                    razortemplate = "edit.cshtml";
                }

                themeFolder = _moduleData.ConfigInfo.GetXmlProperty("genxml/select/apptheme");
                postInfo.ModuleId = moduleid; // make sure we have correct moduleid.

                var passSettings = postInfo.ToDictionary();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, _appthemeRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());
                strOut = DNNrocketUtils.RazorList(razorTempl, _moduleData.List, passSettings,_moduleData.HeaderInfo);

                if (strOut == "") strOut = "ERROR: No data returned for " + _appthemeMapPath + "\\" + themeFolder + "\\default\\" + razortemplate;
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static String SaveData(int moduleid, DNNrocketInterface rocketInterface, SimplisityInfo postInfo)
        {
            try
            {
                var objCtrl = new DNNrocketController();
                var info = postInfo;
                if (_moduleData.List.Count() > 0)
                {
                    info = _moduleData.List.First();
                    info.XMLData = postInfo.XMLData;
                }
                info.ModuleId = moduleid;
                objCtrl.SaveData(moduleid.ToString(), rocketInterface.EntityTypeCode, info, -1, moduleid);
                _moduleData.PopulateList();
                return EditData(moduleid,rocketInterface, postInfo);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String ResetRocketMod(int moduleid, DNNrocketInterface rocketInterface)
        {
            try
            {
                ConfigUtils.DeleteConfig(moduleid);
                return GetDashBoard(moduleid, rocketInterface);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String GetDashBoard(int moduleid, DNNrocketInterface  rocketInterface)
        {
            try
            {
                var controlRelPath = rocketInterface.TemplateRelPath;
                if (controlRelPath == "") controlRelPath = ControlRelPath;

                var themeFolder = rocketInterface.DefaultTheme;
                var razortemplate = "dashboard.cshtml";
                var passSettings = rocketInterface.ToDictionary();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, controlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());
                passSettings.Add("mappathAppThemeFolder", _appthemeMapPath);
                
                return DNNrocketUtils.RazorDetail(razorTempl, _moduleData.ConfigInfo, passSettings);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String GetDisplay(DNNrocketInterface rocketInterface)
        {

            try
            {
                var strOut = "";
                if (_moduleData.ConfigExists)
                {
                    var objCtrl = new DNNrocketController();

                    var razortemplate = "view.cshtml";
                    var themeFolder = _moduleData.ConfigInfo.GetXmlProperty("genxml/select/apptheme");

                    var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, _appthemeRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());
                    strOut = DNNrocketUtils.RazorList(razorTempl, _moduleData.List, _moduleData.ConfigInfo.ToDictionary(), _moduleData.HeaderInfo);

                }
                else
                {
                    strOut = GetSetup(rocketInterface);
                }

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        private static String GetSetup(DNNrocketInterface interfaceInfo)
        {
            try
            {
                interfaceInfo.ModuleId = _moduleData.ModuleId;
                var strOut = "";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData("setup.cshtml", interfaceInfo.TemplateRelPath, interfaceInfo.DefaultTheme, DNNrocketUtils.GetCurrentCulture());
                return DNNrocketUtils.RazorDetail(razorTempl, interfaceInfo.Info);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}

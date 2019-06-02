﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketSettings
{
    public class startconnect : DNNrocketAPI.APInterface
    {
        private static string _appthemeRelPath;
        private static string _appthemeMapPath;
        private static SimplisityInfo _postInfo;
        private static CommandSecurity _commandSecurity;
        private static DNNrocketInterface _rocketInterface;
        private static SettingsData _settingsData;

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, string userHostAddress, string langRequired = "")
        {
            var strOut = "ERROR"; // return ERROR if not matching commands.

            paramCmd = paramCmd.ToLower();

            _rocketInterface = new DNNrocketInterface(interfaceInfo);

            var appPath = _rocketInterface.TemplateRelPath;
            if (appPath == "") appPath = "/DesktopModules/DNNrocket/Settings";
            _appthemeRelPath = appPath;
            _appthemeMapPath = DNNrocketUtils.MapPath(_appthemeRelPath);
            _postInfo = postInfo;

            // we should ALWAYS pass back the moduleid & tabid in the template post.
            // But for the admin start we need it to be passed by the admin.aspx url parameters.  Which then puts it in the s-fields for the simplsity start call.
            var moduleid = _postInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            if (moduleid == 0) moduleid = _postInfo.ModuleId;
            var parentitemid = _postInfo.GetXmlPropertyInt("genxml/hidden/parentitemid");
            if (parentitemid == 0) parentitemid = _postInfo.ParentItemId;

            var tabid = _postInfo.GetXmlPropertyInt("genxml/hidden/tabid"); // needed for security.
            if ((tabid == 0 || moduleid == 0) && parentitemid <= 0)
            {
                strOut = "Interface must be attached to a module or parent.";
            }
            else
            {
                if (parentitemid > 0)
                {
                    var guidkey = "parentitemid" + parentitemid;
                    _settingsData = new SettingsData(guidkey, langRequired, _rocketInterface.EntityTypeCode);
                }
                else
                {
                    _settingsData = new SettingsData(tabid, moduleid, langRequired, _rocketInterface.EntityTypeCode);
                }

                _commandSecurity = new CommandSecurity(tabid, moduleid, _rocketInterface);
                _commandSecurity.AddCommand("rocketsettings_edit", true);
                _commandSecurity.AddCommand("rocketsettings_add", true);
                _commandSecurity.AddCommand("rocketsettings_save", true);
                _commandSecurity.AddCommand("rocketsettings_delete", true);

                _commandSecurity.AddCommand("rocketsettings_getdata", false);
                _commandSecurity.AddCommand("rocketsettings_login", false);

                if (!_commandSecurity.HasSecurityAccess(paramCmd))
                {
                    strOut = LoginUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
                    return ReturnString(strOut);
                }

                    switch (paramCmd)
                    {
                        case "rocketsettings_edit":
                            strOut = EditData();
                            break;
                        case "rocketsettings_add":
                            _settingsData.AddRow();
                            strOut = EditData();
                            break;
                        case "rocketsettings_save":
                            _settingsData.Save(postInfo);
                            strOut = EditData();
                            break;
                        case "rocketsettings_delete":
                            _settingsData.Delete();
                            strOut = EditData();
                            break;
                        case "rocketsettings_login":
                            strOut = LoginUtils.DoLogin(systemInfo, postInfo, userHostAddress);
                            break;
                    }

            }

            return ReturnString(strOut);
        }

        public static Dictionary<string, string> ReturnString(string strOut, string jsonOut = "")
        {
            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("outputhtml", strOut);
            rtnDic.Add("outputjson", jsonOut);
            return rtnDic;
        }

        public static String EditData()
        {
            try
            {
                var strOut = "";
                var theme = _postInfo.GetXmlProperty("genxml/hidden/theme");
                if (theme == "") theme = _rocketInterface.DefaultTheme;
                if (theme == "") theme = "config-w3";
                var razortemplate = _postInfo.GetXmlProperty("genxml/hidden/template");
                if (razortemplate == "") razortemplate = _rocketInterface.DefaultTemplate;                
                if (razortemplate == "") razortemplate = "settings.cshtml";

                var passSettings = _postInfo.ToDictionary();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, _appthemeRelPath, theme, DNNrocketUtils.GetEditCulture());
                strOut = DNNrocketUtils.RazorDetail(razorTempl, _settingsData, passSettings);

                if (strOut == "") strOut = "ERROR: No data returned for " + _appthemeMapPath + "\\Themes\\" + theme + "\\default\\" + razortemplate;
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}

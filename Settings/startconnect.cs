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
        private static SimplisityInfo _paramInfo;
        private static CommandSecurity _commandSecurity;
        private static DNNrocketInterface _rocketInterface;
        private static SettingsData _settingsData;

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string userHostAddress, string langRequired = "")
        {
            var strOut = "ERROR"; // return ERROR if not matching commands.

            paramCmd = paramCmd.ToLower();

            _rocketInterface = new DNNrocketInterface(interfaceInfo);

            var appPath = _rocketInterface.TemplateRelPath;
            if (appPath == "") appPath = "/DesktopModules/DNNrocket/Settings";
            _appthemeRelPath = appPath;
            _appthemeMapPath = DNNrocketUtils.MapPath(_appthemeRelPath);
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            // -------------------------------------------------------------------------------------
            // For Modules Level data we should ALWAYS pass back the moduleid & tabid in the template post.
            // Pass ParentItemId for data linked to other Rockeyt record.
            // Use system and interface key for all others (Install level)
            // But for the admin start we need it to be passed by the admin.aspx url parameters.  Which then puts it in the s-fields for the simplsity start call.
            var guidkey = "";
            var parentitemid = _paramInfo.GetXmlPropertyInt("genxml/hidden/parentitemid");
            if (parentitemid == 0) parentitemid = _paramInfo.ParentItemId;
            var moduleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            if (moduleid == 0) moduleid = _paramInfo.ModuleId;
            var tabid = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid"); // needed for security.

            if (tabid <= 0) tabid = -1;
            if (moduleid <= 0) moduleid = -1;
            if (parentitemid <= 0) parentitemid = -1;

            if (tabid == -1 && moduleid == -1 && parentitemid == -1) guidkey = systemInfo.GUIDKey + "." +  _rocketInterface.InterfaceKey;
            if (parentitemid > 0 ) guidkey = "parentitemid" + parentitemid;
            // -------------------------------------------------------------------------------------

            var listname = _paramInfo.GetXmlProperty("genxml/hidden/listname");
            if (listname == "") listname = "settingsdata";
            if (guidkey == "")
            {
                _settingsData = new SettingsData(tabid, moduleid, langRequired, _rocketInterface.EntityTypeCode, listname);
            }
            else
            {
                _settingsData = new SettingsData(guidkey, langRequired, _rocketInterface.EntityTypeCode, listname);
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
                var theme = _paramInfo.GetXmlProperty("genxml/hidden/theme");
                if (theme == "") theme = _rocketInterface.DefaultTheme;
                if (theme == "") theme = "config-w3";
                var razortemplate = _paramInfo.GetXmlProperty("genxml/hidden/template");
                if (razortemplate == "") razortemplate = _rocketInterface.DefaultTemplate;                
                if (razortemplate == "") razortemplate = "settings.cshtml";

                var passSettings = _paramInfo.ToDictionary();
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

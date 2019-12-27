﻿using System;
using System.Collections.Generic;
using System.Xml;
using DNNrocketAPI;
using Simplisity;

namespace Rocket.Tools
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private static DNNrocketInterface _rocketInterface;
        private static SystemInfoData _systemInfoData;
        private static Dictionary<string, string> _passSettings;
        private static SimplisityInfo _postInfo;
        private static SimplisityInfo _paramInfo;
        private static string _pageref;
        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var rtnDic = new Dictionary<string, string>();

            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _pageref = _paramInfo.GetXmlProperty("genxml/hidden/pageref");

            _systemInfoData = new SystemInfoData(systemInfo);
            _rocketInterface = new DNNrocketInterface(interfaceInfo);
            _passSettings = new Dictionary<string, string>();

            if (DNNrocketUtils.IsSuperUser())
            {
                switch (paramCmd)
                {
                    case "rockettools_login":
                        strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
                        break;
                    case "rocketroles_roles":
                        strOut = RolesAdmin();
                        break;
                    case "rocketroles_getmodules":
                        SaveTreeView();
                        strOut = GetModules();
                        break;
                    case "rocketroles_getroles":
                        SaveModules();
                        strOut = GetRoles();
                        break;
                    case "rocketroles_applyroles":
                        SaveModules();
                        strOut = ApplyRoles();
                        break;
                }
            }
            else
            {
                strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
            }


            rtnDic.Add("outputhtml", strOut);
            return rtnDic;

        }
        public static string ApplyRoles()
        {
            var info = GetCachedInfo();
            foreach (var m in info.GetRecordList("tabmodules"))
            {
                var moduleid = m.GetXmlPropertyInt("genxml/moduleid");
                if (moduleid > 0)
                {
                    var nodList1 = _postInfo.XMLDoc.SelectNodes("genxml/rolecheckbox/*");
                    foreach (XmlNode nod1 in nodList1)
                    {
                        var roleid = _postInfo.GetXmlPropertyInt("genxml/rolecheckbox/" + nod1.Name);
                        if (roleid > 0)
                        {
                            if (nod1.InnerText.ToLower() == "true")
                            {
                                DNNrocketUtils.AddRoleToModule(DNNrocketUtils.GetPortalId(), moduleid, roleid);
                            }
                            else
                            {
                                DNNrocketUtils.RemoveRoleToModule(DNNrocketUtils.GetPortalId(), moduleid, roleid);
                            }
                        }
                    }
                }
            }

            return RolesOK();
        }
        public static void SaveModules()
        {
            var info = GetCachedInfo();
            info.GUIDKey = "";  // clear flag on new selection.

            var nodList = _postInfo.XMLDoc.SelectNodes("genxml/checkbox/*");
            info.RemoveRecordList("tabmodules");
            foreach (XmlNode nod in nodList)
            {
                if (nod.InnerText.ToLower() == "true")
                {
                    var sRec = new SimplisityRecord();
                    sRec.SetXmlProperty("genxml/elementid", nod.Name);
                    sRec.SetXmlProperty("genxml/moduleid", nod.Name.Replace("moduleid", ""));
                    info.AddRecordListItem("tabmodules", sRec);
                }
            }
            CacheUtils.SetCache(_pageref, info, "roles");
        }
        public static void SaveTreeView()
        {
            var info = GetCachedInfo();
            info.GUIDKey = "";  // clear flag on new selection.

            var nodList = _postInfo.XMLDoc.SelectNodes("genxml/treeview/*/*");
            info.RemoveRecordList("tabtreeview");
            foreach (XmlNode nod in nodList)
            {
                if (nod.InnerText.ToLower() == "true")
                {
                    var sRec = new SimplisityRecord();
                    sRec.SetXmlProperty("genxml/treeid", nod.Name);
                    sRec.SetXmlProperty("genxml/tabid", nod.Name.Replace("tabid",""));
                    info.AddRecordListItem("tabtreeview", sRec);
                }
            }
            CacheUtils.SetCache(_pageref, info, "roles");
        }
        public static SimplisityInfo GetCachedInfo()
        {
            var info = (SimplisityInfo)CacheUtils.GetCache(_pageref, "roles");
            if (info == null)
            {
                info = new SimplisityInfo();
                info.GUIDKey = "new";  // flag to check if we have lost the previous selection
            }
            return info;
        }
        public static String GetRoles()
        {
            try
            {
                var info = GetCachedInfo();
                if (info.GUIDKey == "new") return "reload"; // we have lost the cache and page data, reload and start agian.

                info.RemoveRecordList("rolelist");
                foreach (var t in info.GetRecordList("tabtreeview"))
                {
                    var tabid = t.GetXmlPropertyInt("genxml/tabid");
                    var l = DNNrocketUtils.GetRoles(DNNrocketUtils.GetPortalId());
                    foreach (var m in l)
                    {
                        var sRec = new SimplisityRecord();
                        sRec.SetXmlProperty("genxml/roleid", m.Key.ToString());
                        sRec.SetXmlProperty("genxml/rolename", m.Value);
                        info.AddRecordListItem("rolelist", sRec);
                    }
                }

                _passSettings.Add("portalid", DNNrocketUtils.GetPortalId().ToString());
                var controlRelPath = _rocketInterface.TemplateRelPath;
                var themeFolder = _rocketInterface.DefaultTheme;
                var razortemplate = "roleselectsection.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, controlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                return DNNrocketUtils.RazorDetail(razorTempl, info, _passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public static String GetModules()
        {
            try
            {
                var info = GetCachedInfo();
                if (info.GUIDKey == "new") return "reload"; // we have lost the cache and page data, reload and start agian.

                info.RemoveRecordList("modulelist");
                foreach (var t in info.GetRecordList("tabtreeview"))
                {
                    var tabid = t.GetXmlPropertyInt("genxml/tabid");
                    var l = DNNrocketUtils.GetTabModuleTitles(tabid);
                    foreach (var m in l)
                    {
                        var sRec = new SimplisityRecord();
                        sRec.SetXmlProperty("genxml/moduleid", m.Key.ToString());
                        sRec.SetXmlProperty("genxml/moduletitle", m.Value);
                        info.AddRecordListItem("modulelist", sRec);
                    }
                }

                _passSettings.Add("portalid", DNNrocketUtils.GetPortalId().ToString());
                var controlRelPath = _rocketInterface.TemplateRelPath;
                var themeFolder = _rocketInterface.DefaultTheme;
                var razortemplate = "rolesmodulesection.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, controlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                return DNNrocketUtils.RazorDetail(razorTempl, info, _passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String RolesAdmin()
        {
            try
            {
                _passSettings.Add("portalid", DNNrocketUtils.GetPortalId().ToString());
                var controlRelPath = _rocketInterface.TemplateRelPath;
                var themeFolder = _rocketInterface.DefaultTheme;
                var razortemplate = "roles.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, controlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                return DNNrocketUtils.RazorDetail(razorTempl, new SimplisityInfo(), _passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String RolesOK()
        {
            try
            {
                var controlRelPath = _rocketInterface.TemplateRelPath;
                var themeFolder = _rocketInterface.DefaultTheme;
                var razortemplate = "rolesok.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, controlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                return DNNrocketUtils.RazorDetail(razorTempl, new SimplisityInfo(), _passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


    }
}

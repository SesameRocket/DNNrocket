﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Rocket.AppThemes.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace DNNrocket.System
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private SystemData _systemData;
        private string _controlRelPath;
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private DNNrocketInterface _rocketInterface;
        private UserStorage _userStorage;
        private Dictionary<string, string> _passSettings;

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string editlang = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _systemData = new SystemData(systemInfo);
            _rocketInterface = new DNNrocketInterface(interfaceInfo);
            _passSettings = _paramInfo.ToDictionary();

            var commandSecurity = new CommandSecurity(-1, -1, _rocketInterface);
            
            DNNrocketUtils.CreateRocketDirectories();

            _userStorage = new UserStorage();
            if (_paramInfo.GetXmlPropertyBool("genxml/hidden/reload"))
            {
                var menucmd = _userStorage.GetCommand("systemapi");
                if (menucmd != "")
                {
                    paramCmd = menucmd;
                    _paramInfo = _userStorage.GetParamInfo("systemapi");
                }
            }
            else
            {
                if (_paramInfo.GetXmlPropertyBool("genxml/hidden/track"))
                {
                    _userStorage.Track("systemapi", paramCmd, _paramInfo, "");
                }
            }

            _controlRelPath = "/DesktopModules/DNNrocket/SystemData";

            var strOut = "ERROR!! - No Security rights or function command.  Ensure your systemkey is defined. [SystemDataList]";

            var rtnInfo = new SimplisityInfo();
            // Security Check MUST be in the extension.
            if (commandSecurity.SecurityCheckIsSuperUser())
            {
                switch (paramCmd)
                {
                    case "systemapi_admin_getsystemlist":
                        strOut = SystemAdminList(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_admin_getdetail":
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_adminaddnew":
                        strOut = SystemAddNew(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_addinterface":
                        SystemAddListItem(paramInfo, "interfacedata");
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_admin_save":
                        SystemSave(postInfo, paramInfo);
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_admin_delete":
                        SystemDelete(paramInfo);
                        strOut = SystemAdminList(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_export":
                        SystemExport(paramInfo);
                        strOut = "<i class='fas fa-file-export fa-fw fa-lg simplisity_fadeout '></i>";
                        break;                        
                    case "systemapi_addparam":
                        SystemAddListItem(paramInfo, "idxfielddata");
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_addsetting":
                        SystemAddListItem(paramInfo, "settingsdata");
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_addgroup":
                        SystemAddListItem(paramInfo, "groupsdata");
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_addprovtype":
                        SystemAddListItem(paramInfo, "provtypesdata");
                        strOut = SystemAdminDetail(_controlRelPath);
                        break;
                    case "systemapi_rebuildindex":
                        RebuildIndex(paramInfo, false);
                        strOut = "<h1>Rebuilding Index</h1>";
                        break;
                    case "systemapi_deleterebuildindex":
                        RebuildIndex(paramInfo, true);
                        strOut = "<h1>Deleting and Rebuilding Index</h1>";
                        break;
                    case "systemapi_copyinterface":
                        strOut = CopyInterface(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_clearallcache":
                        CacheFileUtils.ClearAllCache();
                        DNNrocketUtils.ClearAllCache();
                        strOut = SystemAdminList(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_clearmemcache":
                        CacheUtils.ClearAllCache();
                        DNNrocketUtils.ClearAllCache();
                        strOut = SystemAdminList(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_clearfilecache":
                        CacheFileUtils.ClearFileCache();
                        strOut = SystemAdminList(paramInfo, _controlRelPath);
                        break;
                    case "systemapi_cleartempdb":
                        CacheFileUtils.ClearAllCache();
                        DNNrocketUtils.ClearAllCache();
                        _systemData.ClearTempDB();
                        strOut = "<h1>DNNrocketTemp Database table clear</h1>";
                        break;                        
                    case "systemapi_globaldetail":
                        strOut = SystemGlobalDetail();
                        break;
                    case "systemapi_globalsave":
                        SystemGlobalSave();
                        strOut = SystemGlobalDetail();
                        break;

                    case "systemapi_licenselist":
                        strOut = GetLicenseList();
                        break;
                    case "systemapi_licensesave":
                        strOut = GetLicenseList();
                        break;
                    case "systemapi_deletelicense":
                        DeleteLicense();
                        strOut = GetLicenseList();
                        break;
                    case "systemapi_recycleapppool":
                        CacheUtils.ClearAllCache();
                        DNNrocketUtils.ClearAllCache();
                        DNNrocketUtils.RecycleApplicationPool();
                        strOut = "<h1>Recycle App Pool</h1>";
                        break;

                    case "systemapi_plugins":
                        strOut = GetPluginList();
                        break;

                }
            }

            switch (paramCmd)
            {
                case "login_sendreset":
                    //strOut = ResetPass(sInfo);
                    break;
                case "systemapi_licenserecieveremote":
                    strOut = "FAIL";
                    var licenseXml = _paramInfo.GetXmlProperty("genxml/postform/licensecode");
                    if (SaveRemoteLicense(licenseXml)) strOut = "OK";
                    break;
                case "systemapi_licenseverify":
                    strOut = "<i class='fas fa-times w3-text-red w3-right fa-3x'></i>";
                    if (verifyLicense()) strOut = "<i class='fas fa-check w3-text-green w3-right fa-3x'></i>";
                    break;
                case "systemapi_entercertificatekey":
                    EnterCertificateKey();
                    strOut = "";
                    break;
                case "systemapi_updatecertificatekey":
                    EnterCertificateKey();
                    strOut = GetLicenseList();
                    break;
                case "systemapi_licensepopup":
                    strOut = LicensePopup();
                    break;
                case "rocketadmin_adminpanel":
                    strOut = "";
                    if (commandSecurity.SecurityCheckIsSuperUser()) strOut = AdminPanel();
                    break;
                default:
                    if (!commandSecurity.SecurityCheckIsSuperUser())
                    {
                        strOut = UserUtils.LoginForm(systemInfo, rtnInfo, "systemapi", UserUtils.GetCurrentUserId());
                    }
                    break;
            }

            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("outputhtml", strOut);
            return rtnDic;            
        }


        public String AdminPanel()
        {
            try
            {
                var systemGlobalData = new SystemGlobalData();
                var strOut = "";
                var passSettings = _paramInfo.ToDictionary();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData("adminpanel.cshtml", _controlRelPath, "config-w3", DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                strOut = DNNrocketUtils.RazorDetail(razorTempl, new SimplisityInfo(), passSettings);
                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }



        public String LicensePopup()
        {
            try
            {
                var licenseid = _paramInfo.GetXmlPropertyInt("genxml/hidden/licenseid");
                var strOut = "";
                var passSettings = _paramInfo.ToDictionary();

                if (licenseid > 0)
                {
                    var licenseData = new LicenseData(licenseid);
                    var razorTempl = DNNrocketUtils.GetRazorTemplateData("LicensePopup.cshtml", _controlRelPath, "config-w3", DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                    strOut = DNNrocketUtils.RazorDetail(razorTempl, licenseData, passSettings);
                }


                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        public bool GetRemoteLicense()
        {
            try
            {
                var systemGlobalData = new SystemGlobalData();
                var reqparm = new NameValueCollection();
                reqparm.Add("sitekey", DNNrocketUtils.SiteGuid());
                reqparm.Add("systemkey", _systemData.SystemKey);
                reqparm.Add("domainurl", DNNrocketUtils.GetDefaultWebsiteDomainUrl());
                var rtnLicenseStatus = SimplisityUtils.PostData(systemGlobalData.LicenseUrl.TrimEnd('/') + "/Desktopmodules/dnnrocket/api/rocket/action", "rocketlicense", "clientlicense_getlicense", "", "", reqparm);
                if (rtnLicenseStatus == "OK") return true;
                return false;
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
                return false;
            }
        }

        public bool verifyLicense()
        {
            try
            {
                var getremote = GetRemoteLicense();
                if (!getremote) return false;
                var licensecode = _paramInfo.GetXmlProperty("genxml/postform/licensecode");
                if (licensecode != "")
                {
                    var licenseInfo = new SimplisityInfo();
                    licenseInfo.FromXmlItem(GeneralUtils.DeCode(licensecode));
                    var sitekey = licenseInfo.GetXmlProperty("genxml/textbox/sitekey");
                    var systemkey = licenseInfo.GetXmlProperty("genxml/select/systemkey");
                    var licenseData = new LicenseData(systemkey, sitekey);
                    return licenseData.ValidateCertificateKey(sitekey);
                }
                return false;
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
                return false;
            }
        }

        public bool SaveRemoteLicense(string licenseXml)
        {
            try
            {
                if (licenseXml != "")
                {
                    licenseXml = GeneralUtils.DeCode(licenseXml);
                    var sRec = new SimplisityRecord();
                    sRec.FromXmlItem(licenseXml);

                    var systemkey = sRec.GetXmlProperty("genxml/select/systemkey");
                    var sitekey = sRec.GetXmlProperty("genxml/textbox/sitekey");
                    var licenseData = new LicenseData(systemkey, sitekey);
                    if (licenseData.Exists)
                    {
                        licenseData.Record.XMLData = sRec.XMLData;
                        licenseData.Update();
                    }
                    else
                    {
                        var objCtrl = new DNNrocketController();
                        sRec.ItemID = -1;
                        objCtrl.Update(sRec);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
                return false;
            }
        }

        public void DeleteLicense()
        {
            try
            {
                var licenseid = _paramInfo.GetXmlPropertyInt("genxml/hidden/licenseid");
                if (licenseid > 0)
                {
                    var licenseData = new LicenseData(licenseid);
                    licenseData.Delete();
                }
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
            }

        }

        public void EnterCertificateKey()
        {
            try
            {
                var licenseid = _paramInfo.GetXmlPropertyInt("genxml/hidden/licenseid");
                var certificateKey = _postInfo.GetXmlProperty("genxml/hidden/certificatekey");
                var domainurl = DNNrocketUtils.GetDefaultWebsiteDomainUrl(); ;
                var systemkey = _systemData.SystemKey;
                var sitekey = DNNrocketUtils.SiteGuid();
                if (licenseid > 0)
                {
                    var licenseData = new LicenseData(licenseid);
                    licenseData.CertificateKey = certificateKey;
                    licenseData.Update();
                }
                else
                {
                    var licenseData = new LicenseData(systemkey, sitekey);
                    if (certificateKey != "")
                    {
                        licenseData.DomainUrl = domainurl;
                        licenseData.CreateNew(-1);
                        licenseData.CertificateKey = certificateKey;
                        licenseData.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
            }

        }

        public String GetLicenseList()
        {
            try
            {
                var LicenseListData = new LicenseListData();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData("Admin_SystemLicense.cshtml", _controlRelPath, "config-w3", DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                var passSettings = _postInfo.ToDictionary();
                return DNNrocketUtils.RazorDetail(razorTempl, LicenseListData, passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        public String GetPluginList()
        {
            try
            {
                var pluginDataList = new PluginDataList();
                var razorTempl = DNNrocketUtils.GetRazorTemplateData("Admin_Plugins.cshtml", _controlRelPath, "config-w3", DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                var passSettings = _postInfo.ToDictionary();
                return DNNrocketUtils.RazorDetail(razorTempl, pluginDataList, passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


        public string SystemAdminList(SimplisityInfo sInfo, string templateControlRelPath)
        {
            try
            {
                var systemDataList = new SystemDataList();
                var list = systemDataList.GetSystemList();
                return RenderSystemAdminList(list, sInfo, 0, templateControlRelPath);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String SystemAdminDetail(string templateControlRelPath)
        {
            try
            {
                var strOut = "Invalid ItemId";
                var selecteditemid = _paramInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var themeFolder = _paramInfo.GetXmlProperty("genxml/hidden/theme");
                var razortemplate = _paramInfo.GetXmlProperty("genxml/hidden/template");

                if (selecteditemid > 0)
                {
                    var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);

                    var objCtrl = new DNNrocketController();
                    var info = objCtrl.GetInfo(selecteditemid);

                    strOut = DNNrocketUtils.RazorDetail(razorTempl, info, _passSettings);
                }


                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String SystemGlobalDetail()
        {
            try
            {
                var passSettings = _paramInfo.ToDictionary();

                var razorTempl = DNNrocketUtils.GetRazorTemplateData("Admin_SystemGlobalDetail.cshtml", _controlRelPath, "config-w3", DNNrocketUtils.GetCurrentCulture(), "1.0", true);

                var globalData = new SystemGlobalData();

                var strOut = DNNrocketUtils.RazorDetail(razorTempl, globalData, passSettings);
                return strOut;
            }
            catch (Exception ex)
            {
                DNNrocketUtils.LogException(ex);
                return ex.ToString();
            }
        }

        public void SystemGlobalSave()
        {
            var globalData = new SystemGlobalData();
            globalData.Save(_postInfo);
            CacheFileUtils.ClearAllCache();
            DNNrocketUtils.ClearAllCache();
        }

        public String RenderSystemAdminList(List<SimplisityInfo> list, SimplisityInfo sInfo, int recordCount, string templateControlRelPath)
        {

            try
            {
                if (list == null) return "";
                var strOut = "";

                // select a specific entity data type for the product (used by plugins)
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                if (themeFolder == "") themeFolder = "config-w3";
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");

                var passSettings = sInfo.ToDictionary();

                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);

                strOut = DNNrocketUtils.RazorList(razorTempl, list, passSettings);

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        public String SystemAddNew(SimplisityInfo sInfo, string templateControlRelPath)
        {
            try
            {
                var strOut = "";
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                if (themeFolder == "") themeFolder = "config-w3";
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");

                var passSettings = sInfo.ToDictionary();

                var info = new SimplisityInfo();
                info.ItemID = -1;
                info.PortalId = 99999;
                info.Lang = DNNrocketUtils.GetCurrentCulture();
                info.SetXmlProperty("genxml/hidden/index", "99");
                info.TypeCode = "SYSTEM";
                info.GUIDKey = GeneralUtils.GetUniqueKey();
                var objCtrl = new DNNrocketController();
                info.ItemID = objCtrl.SaveRecord(info).ItemID;

                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);

                strOut = DNNrocketUtils.RazorDetail(razorTempl, info, passSettings);

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void SystemAddListItem(SimplisityInfo sInfo, string listname)
        {
            try
            {
                var selecteditemid = sInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                if (GeneralUtils.IsNumeric(selecteditemid))
                {
                    var objCtrl = new DNNrocketController();
                    var info = objCtrl.GetInfo(Convert.ToInt32(selecteditemid));
                    info.AddListItem(listname);
                    objCtrl.SaveRecord(info);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void SystemExport(SimplisityInfo paramInfo)
        {
            var selecteditemid = paramInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            if (selecteditemid > 0)
            {
                DNNrocketUtils.LogDebug("Export System:" + selecteditemid);
                var systemData = new SystemData(selecteditemid);
                var exportFileMapPath = DNNrocketUtils.MapPath("/DesktopModules/DNNrocket/SystemData/Systems").TrimEnd('\\') + "\\" + systemData.SystemKey + "_system.xml";
                FileUtils.SaveFile(exportFileMapPath, systemData.Export());
            }
        }

        public void SystemSave(SimplisityInfo postInfo, SimplisityInfo paramInfo)
        {
            // remove any debug logs created in debug mode.
            DNNrocketUtils.LogDebugClear();

            var selecteditemid = paramInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
            if (selecteditemid > 0)
            {
                var systemData = new SystemData(selecteditemid);
                systemData.Save(postInfo);
                CacheUtils.ClearAllCache();
                _passSettings.Add("saved", "true");

            }
        }

        public void SystemDelete(SimplisityInfo sInfo)
        {
            var itemid = sInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                objCtrl.Delete(Convert.ToInt32(itemid));

                CacheUtils.ClearAllCache();
            }
        }


        public void RebuildIndex(SimplisityInfo postInfo, bool deleteindex )
        {
            var itemid = postInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                var sysInfo = objCtrl.GetInfo(Convert.ToInt32(itemid));

                var entityList = new List<string>();
                // get interface data, the language needs indexing.
                foreach (var i in sysInfo.GetList("interfacedata"))
                {
                    var entityTypeCode = i.GetXmlProperty("genxml/textbox/entitytypecode");
                    if (!entityList.Contains(entityTypeCode) && entityTypeCode != "")
                    {
                        entityList.Add(entityTypeCode);
                    }
                }
                foreach (var i in sysInfo.GetList("idxfielddata"))
                {
                    var entityTypeCode = i.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                    if (!entityList.Contains(entityTypeCode) && entityTypeCode != "")
                    {
                        entityList.Add(entityTypeCode);
                    }
                }
                foreach (var entityCode in entityList)
                {
                    if (deleteindex)
                    {
                        var l2 = objCtrl.GetList(-1, -1, entityCode);
                        foreach (var sInfo in l2)
                        {
                            objCtrl.DeleteIndex(sInfo);
                        }
                    }
                    var l = objCtrl.GetList(-1, -1, entityCode + "LANG","","","",0,0,0,0);
                    if (l.Count == 0)
                    {
                        // the record may NOT have a LANG record.
                        l = objCtrl.GetList(-1, -1, entityCode, "", "", "", 0, 0, 0, 0);
                        foreach (var sInfo in l)
                        {
                            objCtrl.RebuildIndex(sInfo);
                        }
                    }
                    else
                    {
                        foreach (var sInfo in l)
                        {
                            objCtrl.RebuildLangIndex(sInfo.PortalId, sInfo.ItemID);
                        }
                    }
                }

                CacheUtils.ClearAllCache();
            }

        }


        public string CopyInterface(SimplisityInfo postInfo, string templateControlRelPath)
        {
            var strOut = "";
            var info = new SimplisityInfo();
            info.SetXmlProperty("genxml/delay", "2000");
            var itemid = postInfo.GetXmlProperty("genxml/hidden/fromsystemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                var sysInfo = objCtrl.GetInfo(Convert.ToInt32(itemid));
                if (sysInfo != null)
                {
                    var interfacekey = postInfo.GetXmlProperty("genxml/hidden/interfacekey");
                    var tosystemid = postInfo.GetXmlProperty("genxml/hidden/tosystemid");

                    var sysInfoTo = objCtrl.GetInfo(Convert.ToInt32(tosystemid));
                    if (sysInfoTo != null)
                    {
                        var interfaceToCopy = sysInfo.GetListItem("interfacedata", "genxml/textbox/interfacekey", interfacekey);
                        if (interfaceToCopy != null)
                        {
                            var interfaceExists = sysInfoTo.GetListItem("interfacedata", "genxml/textbox/interfacekey", interfacekey);
                            if (interfaceExists == null)
                            {
                                sysInfoTo.AddListItem("interfacedata", interfaceToCopy);
                                objCtrl.SaveRecord(sysInfoTo);
                                info.SetXmlProperty("genxml/message", "Interface Copied");
                                info.SetXmlProperty("genxml/color", "w3-pale-green");
                                info.SetXmlProperty("genxml/delay", "2000");
                            }
                            else
                            {
                                interfaceToCopy.SetXmlProperty("genxml/textbox/interfacekey", interfaceToCopy.GetXmlProperty("genxml/textbox/interfacekey") + "-copy");
                                sysInfoTo.AddListItem("interfacedata", interfaceToCopy);
                                objCtrl.SaveRecord(sysInfoTo);
                                info.SetXmlProperty("genxml/message", "Interface Copied - Refresh page to View");
                                info.SetXmlProperty("genxml/color", "w3-pale-green");
                                info.SetXmlProperty("genxml/delay", "2000");
                            }

                        }
                        else
                        {
                            info.SetXmlProperty("genxml/message", "Interface does not exists.");
                            info.SetXmlProperty("genxml/color", "w3-pale-red");
                        }
                    }
                    else
                    {
                        info.SetXmlProperty("genxml/message", "System does not exists.");
                        info.SetXmlProperty("genxml/color", "w3-pale-red");
                    }
                }
                CacheUtils.ClearAllCache();
                var themeFolder = postInfo.GetXmlProperty("genxml/hidden/theme");
                var razortemplate = postInfo.GetXmlProperty("genxml/hidden/template");
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                strOut = DNNrocketUtils.RazorDetail(razorTempl, info);

            }
            return strOut;
        }

    }
}

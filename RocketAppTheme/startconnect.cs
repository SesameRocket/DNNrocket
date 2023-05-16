﻿using DNNrocketAPI;
using DNNrocketAPI.Components;
using DNNrocketAPI.Interfaces;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace DNNrocket.AppThemes
{
    public partial class StartConnect : IProcessCommand
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private string _editLang;
        private SystemLimpet _systemData;
        private Dictionary<string, string> _passSettings;        
        private string _appThemeFolder;
        private string _appVersionFolder;
        private AppThemeLimpet _appTheme;
        private AppThemeDNNrocketLimpet _appThemeSystem;
        private const string _systemKey = "rocketapptheme";
        private SessionParams _sessionParams;
        private PortalLimpet _portalData;
        private string _moduleref;
        private string _projectName;
        private AppThemeProjectLimpet _appThemeProjectData;
        private string _returnUrl;

        public Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            object jsonOut = null;
            var strOut = "ERROR - Must be SuperUser"; // return ERROR if not matching commands.

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var sk = _paramInfo.GetXmlProperty("genxml/remote/securitykeyedit");
            if (UserUtils.IsSuperUser() || _portalData.SecurityKeyEdit == sk)
            {

                AssignEditLang();

                switch (paramCmd)
                {
                    case "rocketapptheme_adminpanel":
                        strOut = AdminPanel();
                        break;
                    case "rocketapptheme_getlist":
                        strOut = GetEditList();
                        break;
                    case "rocketapptheme_getappstore":
                        strOut = GetAppStoreList();
                        break;                        
                    case "rocketapptheme_clearcache":
                        ClearServerCacheLists();
                        strOut = GetAppStoreList();
                        break;
                    case "rocketapptheme_downloadallgithub":
                        strOut = DownloadAppThemes();
                        break;
                    default:

                        switch (paramCmd)
                        {                            
                            case "rocketapptheme_getdetail":
                                strOut = GetDetail("AppThemeDetails.cshtml");
                                break;
                            case "rocketapptheme_addimage":
                                strOut = AddListImage();
                                break;
                            case "rocketapptheme_addcss":
                                AddCssFile();
                                strOut = GetDetail();
                                break;
                            case "rocketapptheme_addjs":
                                AddJsFile();
                                strOut = GetDetail();
                                break;
                            case "rocketapptheme_addtemplate":
                                AddTemplateFile();
                                strOut = GetDetail();
                                break;
                            case "rocketapptheme_createversion":
                                strOut = CreateNewVersion();
                                break;
                            case "rocketapptheme_deleteversion":
                                strOut = DeleteVersion();
                                break;
                            case "rocketapptheme_deletetheme":
                                strOut = DeleteTheme();
                                break;
                            case "rocketapptheme_addapptheme":
                                strOut = CreateNewAppTheme();
                                break;
                            case "rocketapptheme_export":
                                return ExportAppTheme();
                            case "rocketapptheme_docopy":
                                strOut = DoCopyAppTheme();
                                break;

                            case "rocketapptheme_getresxdata":
                                strOut = GetResxDetail();
                                break;
                            case "rocketapptheme_addresxdata":
                                strOut = AddResxDetail();
                                break;
                            case "rocketapptheme_removeresxdata":
                                strOut = RemoveResxDetail();
                                break;
                            case "rocketapptheme_saveresxdata":
                                strOut = SaveResxDetail();
                                break;

                            case "rocketapptheme_getdepdata":
                                strOut = GetDepDetail();
                                break;
                            case "rocketapptheme_adddepdata":
                                strOut = AddDep();
                                break;
                            case "rocketapptheme_savedepdata":
                                strOut = SaveDep();
                                break;


                            case "rocketapptheme_geteditor":
                                strOut = GetEditorDetail();
                                break;
                            case "rocketapptheme_saveeditor":
                                strOut = SaveEditor();
                                break;

                            case "rocketapptheme_deletefile":
                                strOut = DeleteFile();
                                break;
                            case "rocketapptheme_deleteimagefile":
                                strOut = DeleteImageFile();
                                break;

                            case "rocketapptheme_downloadgithub":
                                var folderExists = false;
                                AppThemeUtils.DownloadGitHubAppTheme(_paramInfo.GetXmlProperty("genxml/hidden/htmlurl"), PortalUtils.DNNrocketThemesDirectoryMapPath());
                                if (!folderExists) DNNrocketUtils.RecycleApplicationPool();// recycle so we pickup new AppTheme Folders.
                                strOut = GetAppStoreList();
                                break;

                            case "rocketapptheme_versionjson":
                                strOut = "";
                                var appThemeFolder = _postInfo.GetXmlProperty("genxml/hidden/activevalue");
                                var projectName = _postInfo.GetXmlProperty("genxml/select/selectedprojectname");
                                var appTheme = new AppThemeLimpet(_portalData.PortalId, appThemeFolder, "", projectName);
                                jsonOut = appTheme.VersionListJson();
                                break;
                            case "rocketapptheme_appthemejson":
                                strOut = "";
                                var projectName2 = _postInfo.GetXmlProperty("genxml/hidden/activevalue");
                                var appThemeList2 = new AppThemeDataList(projectName2, _paramInfo.GetXmlProperty("genxml/hidden/selectedsystemkey"));
                                var addEmpty = _postInfo.GetXmlPropertyBool("genxml/hidden/addempty");
                                jsonOut = appThemeList2.NameListJson(addEmpty);
                                break;                                

                        }

                        break;
                }
            }
            else
            {
                strOut = ReloadPage();
            }

            return DNNrocketUtils.ReturnString(strOut, jsonOut);
        }


        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _passSettings = new Dictionary<string, string>();
            _systemData = SystemSingleton.Instance(_systemKey);
            _appThemeSystem = new AppThemeDNNrocketLimpet(_systemData.SystemKey);
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _portalData = new PortalLimpet(PortalUtils.GetPortalId());
            _appThemeProjectData = new AppThemeProjectLimpet();

            // Assign Langauge
            DNNrocketUtils.SetCurrentCulture();
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            // The moduleref are passed via the URL, for module level template editing.
            var requestUrl = _paramInfo.GetXmlProperty("genxml/hidden/requesturl");
            if (requestUrl != "" && requestUrl.Contains("?"))
            {
                Uri requestUri = new Uri(requestUrl);
                if (HttpUtility.ParseQueryString(requestUri.Query).Get("moduleref") != "")
                {
                    _moduleref = HttpUtility.ParseQueryString(requestUri.Query).Get("moduleref");
                    _appThemeFolder = HttpUtility.ParseQueryString(requestUri.Query).Get("appthemefolder");
                    _appVersionFolder = HttpUtility.ParseQueryString(requestUri.Query).Get("appversionfolder");
                    _projectName = HttpUtility.ParseQueryString(requestUri.Query).Get("project");
                    _returnUrl = HttpUtility.UrlDecode(GeneralUtils.DeCode(HttpUtility.ParseQueryString(requestUri.Query).Get("rtn")));
                }
            }
            else
            {
                _moduleref = "";
                _appThemeFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
                _appVersionFolder = _paramInfo.GetXmlProperty("genxml/hidden/appversionfolder");
                _projectName = _paramInfo.GetXmlProperty("genxml/remote/selectedproject");
                if (_projectName == "") _projectName = _paramInfo.GetXmlProperty("genxml/hidden/selectedproject");
                if (_projectName == "") _projectName = _appThemeProjectData.DefaultProjectName();
                _returnUrl = "";
            }
            _sessionParams.ModuleRef = _moduleref;
            _sessionParams.Set("returnurl", _returnUrl);

            _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appVersionFolder, _projectName);

            return paramCmd;
        }
        private string AdminPanel()
        {
            var razorTempl = _appThemeSystem.GetTemplate("AdminPanel.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalData, null, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string DeleteFile()
        {
            var filename = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            if (filename != "")
            {
                var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                _appTheme.DeleteFile(filename, moduleref);
                _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appVersionFolder, _projectName);
            }
            return GetDetail();
        }
        public string DeleteImageFile()
        {
            var filename = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            if (filename != "")
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var fileNameMapPath = _appTheme.AppThemeVersionFolderMapPath.Trim('\\') + "\\img\\" + filename;

                File.Delete(fileNameMapPath);

                GC.Collect();
                GC.WaitForPendingFinalizers();

                _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appVersionFolder, _projectName);
            }
            return GetDetail();
        }

        public string CreateNewVersion()
        {
            try
            {
                if (PortalUtils.GetPortalId() != 0) return "";

                var rtn = _appTheme.CopyVersion(_appTheme.AppVersion, _appTheme.LatestVersion + 1);
                _appVersionFolder = _appTheme.AppVersionFolder.ToString();
                ClearServerCacheLists();
                if (rtn != "") return rtn;
                return GetDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string DeleteVersion()
        {
            try
            {
                if (PortalUtils.GetPortalId() != 0) return "";

                _appTheme.DeleteVersion();
                _appVersionFolder = _appTheme.AppVersionFolder;
                ClearServerCacheLists();
                return GetDetail();
            }
            catch (Exception)
            {
                var strErr = "<div class='w3-panel w3-red'>";
                strErr += "<p>" + DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/Api/App_LocalResources", "DNNrocket.deletethemeerror") + "</p>";
                strErr += "</div>";
                return strErr;
            }
        }
        public string DeleteTheme()
        {
            try
            {
                if (PortalUtils.GetPortalId() != 0) return "";

                _appTheme.DeleteTheme();
                ClearServerCacheLists();
                return GetAppStoreList();
            }
            catch (Exception)
            {
                var strErr = "<div class='w3-panel w3-red'>";
                strErr += "<p>" + DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/Api/App_LocalResources", "DNNrocket.deletethemeerror") + "</p>";
                strErr += "</div>";
                return strErr;
            }
        }

        public void ClearServerCacheLists()
        {
            // clear all cache for aptheme
            CacheUtils.ClearAllCache();
            CacheFileUtils.ClearAllCache();
            DNNrocketUtils.ClearPortalCache();
            CacheUtils.ClearAllCache();
        }

        public string CreateNewAppTheme()
        {
            try
            {
                if (PortalUtils.GetPortalId() != 0) return "";

                var appthemeprefix = GeneralUtils.AlphaNumeric(FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemeprefix")));
                var appthemename = GeneralUtils.AlphaNumeric(FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemename")));

                var newAppThemeName = appthemename;
                if (appthemeprefix != "") newAppThemeName = appthemeprefix + "_" + newAppThemeName;

                var appSystemThemeFolderRel = "/DesktopModules/RocketThemes/";
                var appThemeFolderRel = appSystemThemeFolderRel + "/" + newAppThemeName;
                var appThemeFolderMapPath = DNNrocketUtils.MapPath(appThemeFolderRel);

                if (Directory.Exists(appThemeFolderMapPath))
                {
                    return DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/AppThemes/App_LocalResources/", "AppThemes.appthemeexists");
                }

                // crearte new _appTheme.
                var appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), newAppThemeName, "1.0", _projectName);

                ClearServerCacheLists();

                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String GetResxDetail()
        {
            try
            {
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                _passSettings.Add("filename", fname);
                var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                var resxData = _appTheme.GetResx(fname, moduleref);
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("resxData", resxData);
                var razorTempl = _appThemeSystem.GetTemplate("ResxPopUp.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _appTheme, dataObjects, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String AddResxDetail()
        {
            try
            {
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                var resxData = _appTheme.GetResx(fname, moduleref);
                var key = "" + (resxData.DataDictionary.Count + 1).ToString() + ".Text";
                var lp = (resxData.DataDictionary.Count + 1);
                while (resxData.DataDictionary.ContainsKey(key))
                {
                    lp += 1;
                    key = "" + (lp).ToString() + ".Text";
                }
                resxData.AddField(key, "");
                _appTheme.SaveResx(fname, resxData, moduleref);
                return GetResxDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String RemoveResxDetail()
        {
            try
            {
                var key = _paramInfo.GetXmlProperty("genxml/hidden/key");
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                var resxData = _appTheme.GetResx(fname, moduleref);
                resxData.RemoveField(key);
                _appTheme.SaveResx(fname, resxData, moduleref);
                return GetResxDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String SaveResxDetail()
        {
            var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            var resxData = _appTheme.GetResx(fname, moduleref);
            resxData.RemoveAllFields();
            var resxlist = _postInfo.GetRecordList("resxdictionarydata");
            foreach (var r in resxlist)
            {
                resxData.AddField(r.GetXmlProperty("genxml/key"), r.GetXmlProperty("genxml/value"));
            }
            _appTheme.SaveResx(fname, resxData, moduleref);
            CacheFileUtils.ClearAllCache();
            return GetResxDetail();
        }

        public String GetDepDetail()
        {
            var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            _passSettings.Add("filename", fname);
            var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            var depData = _appTheme.GetDep(fname, moduleref);
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("depdata", depData);
            _passSettings.Add("interfacekey", _rocketInterface.InterfaceKey);
            var razorTempl = _appThemeSystem.GetTemplate("DepDetail.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _appTheme, dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AddDep()
        {
            var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _appTheme.AddDep(fname, moduleref);
            return GetDepDetail();
        }
        public String SaveDep()
        {
            var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _appTheme.SaveDep(fname, _postInfo, moduleref);
            _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appVersionFolder, _projectName);
            CacheFileUtils.ClearAllCache();
            return GetDepDetail();
        }

        public String GetDetail(string templateName = "AppThemeDetails.cshtml")
        {
            try
            {
                return GetEditTemplate(_appTheme, templateName);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String GetAppStoreList()
        {
            var appThemeDataList = new AppThemeDataList(_projectName);
            var razorTempl = _appThemeSystem.GetTemplate("AppStoreList.cshtml");
            var passSettings = _postInfo.ToDictionary();

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, null, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String GetEditList()
        {
            var appThemeDataList = new AppThemeDataList(_projectName);
            var razorTempl = _appThemeSystem.GetTemplate("EditList.cshtml");
            var passSettings = _postInfo.ToDictionary();

            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, null, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        public string DownloadAppThemes()
        {
            _appThemeProjectData.DownloadGitHubProject(_projectName);
            return GetAppStoreList();
        }
        private Dictionary<string, object> ExportAppTheme()
        {
            var appThemeFolder = GeneralUtils.DeCode(_paramInfo.GetXmlProperty("genxml/urlparams/appthemefolder"));
            if (appThemeFolder == "") appThemeFolder = _appThemeFolder;
            var appVersionFolder = GeneralUtils.DeCode(_paramInfo.GetXmlProperty("genxml/urlparams/appversionfolder"));
            if (appVersionFolder == "") appVersionFolder = _appVersionFolder;
            var appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), appThemeFolder, appVersionFolder, _projectName);

            var exportZipMapPath = appTheme.ExportZipFile();

            var rtnDic = new Dictionary<string, object>();
            rtnDic.Add("filenamepath", exportZipMapPath);
            rtnDic.Add("downloadname", appTheme.AppThemeFolder + ".zip");

            return rtnDic;
        }
        public string DoCopyAppTheme()
        {
            try
            {
                if (PortalUtils.GetPortalId() != 0) return "";

                var appthemeprefix = FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemeprefix"));
                var appthemename = FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemename"));
                var newAppThemeName = appthemename;
                if (appthemeprefix == "") appthemeprefix = _systemData.SystemKey;
                newAppThemeName = appthemeprefix.Replace(".", "") + "." + newAppThemeName.Replace(".","");
                var newAppThemeDirName = _appTheme.AppThemeFolderMapPath.TrimEnd('\\') + "\\..\\" + newAppThemeName;

                _appTheme.Copy(newAppThemeDirName);
                _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), newAppThemeName, _appTheme.LatestVersionFolder, _projectName);
                ClearServerCacheLists();
                return GetDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public String GetEditorDetail()
        {
            try
            {
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                var jsonString = GeneralUtils.EnCode(_appTheme.GetTemplate(fname, _moduleref));
                _passSettings.Add("filename", fname);
                _passSettings.Add("jsonFileData", jsonString);


                var editormode = "htmlmixed";
                if (Path.GetExtension(fname) == ".js") editormode = "javascript";
                if (Path.GetExtension(fname) == ".css") editormode = "css";
                _passSettings.Add("editormode", editormode);

                _passSettings.Add("interfacekey", _rocketInterface.InterfaceKey);

                var razorTempl = _appThemeSystem.GetTemplate("EditorPopUp.cshtml");
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, _appTheme, null, _passSettings, _sessionParams, true);
                if (pr.StatusCode != "00") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string SaveEditor()
        {
            var editorcode = _postInfo.GetXmlProperty("genxml/hidden/editorcodesave");
            var filename = _paramInfo.GetXmlProperty("genxml/hidden/filename");
            var moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _appTheme.SaveEditor(filename, editorcode, moduleref);
            CacheFileUtils.ClearAllCache();
            _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appTheme.AppThemeFolder, _appTheme.AppVersionFolder, _projectName);
            return GetEditorDetail();
        }

        public string AddListImage()
        {
            ImgUtils.MoveImageToFolder(_postInfo, _appTheme.ImageFolderMapPath);
            _appTheme.Populate();
            return GetDetail();
        }
        private void AddCssFile()
        {
            var cssfilename = _postInfo.GetXmlProperty("genxml/textbox/cssfilename");
            if (cssfilename != "")
            {
                if (Path.GetExtension(cssfilename) != ".css") cssfilename =  Path.GetFileNameWithoutExtension(cssfilename) + ".css";
                var fileMapPath = _appTheme.AppThemeVersionFolderMapPath + "\\css\\" + cssfilename;
                var cssFileData = "";
                if (File.Exists(fileMapPath)) cssFileData = FileUtils.ReadFile(fileMapPath);
                FileUtils.SaveFile(fileMapPath, cssFileData);
                _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appTheme.AppVersionFolder, _projectName);
            }
        }
        private void AddJsFile()
        {
            var jsfilename = _postInfo.GetXmlProperty("genxml/textbox/jsfilename");
            if (jsfilename != "")
            {
                if (Path.GetExtension(jsfilename) != ".js") jsfilename = Path.GetFileNameWithoutExtension(jsfilename) + ".js";
                var fileMapPath = _appTheme.AppThemeVersionFolderMapPath + "\\js\\" + jsfilename;
                var jsFileData = "";
                if (File.Exists(fileMapPath)) jsFileData = FileUtils.ReadFile(fileMapPath);
                FileUtils.SaveFile(fileMapPath, jsFileData);
                _appTheme = new AppThemeLimpet(PortalUtils.GetCurrentPortalId(), _appThemeFolder, _appTheme.AppVersionFolder, _projectName);
            }
        }
        private void AddTemplateFile()
        {
            var templatefilename = _postInfo.GetXmlProperty("genxml/textbox/templatefilename");
            if (templatefilename != "")
            {
                if (Path.GetExtension(templatefilename) != ".cshtml") templatefilename = Path.GetFileNameWithoutExtension(templatefilename) + ".cshtml";
                var fileMapPath = _appTheme.AppThemeVersionFolderMapPath + "\\default\\" + templatefilename;
                var templateFileData = "";
                if (File.Exists(fileMapPath)) templateFileData = FileUtils.ReadFile(fileMapPath);
                FileUtils.SaveFile(fileMapPath, templateFileData);
                _appTheme = new AppThemeLimpet(_portalData.PortalId, _appThemeFolder, _appTheme.AppVersionFolder, _projectName);
            }
        }
        private void AssignEditLang()
        {
            var nextLang = _paramInfo.GetXmlProperty("genxml/hidden/nextlang");
            if (nextLang != "") _editLang = DNNrocketUtils.SetEditCulture(nextLang);
        }
        private string GetEditTemplate(AppThemeLimpet appTheme, string templateName)
        {
            var razorTempl = _appThemeSystem.GetTemplate(templateName);
            var rtn = RenderRazorUtils.RazorProcessData(razorTempl, appTheme, null, _passSettings, _sessionParams, true);
            return rtn.RenderedText;
        }
        private string ReloadPage()
        {
            try
            {
                // user does not have access, logoff
                UserUtils.SignOut();

                var razorTempl = _appThemeSystem.GetTemplate("Reload.cshtml");
                var rtn = RenderRazorUtils.RazorProcessData(razorTempl, _portalData, null, _passSettings, _sessionParams, true);
                return rtn.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}

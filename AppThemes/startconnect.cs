﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Rocket.AppThemes.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DNNrocket.AppThemes
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private DNNrocketInterface _rocketInterface;
        private string _editLang;
        private SystemData _systemData;
        private Dictionary<string, string> _passSettings;
        private string _appThemeFolder;
        private string _appVersionFolder;
        private const string _tableName = "DNNRocket";
        private UserStorage _userStorage;
        private string _selectedSystemKey;
        private string _paramCmd;

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = "ERROR - Must be SuperUser"; // return ERROR if not matching commands.

            _passSettings = new Dictionary<string, string>(); 
            _systemData = new SystemData(systemInfo);
            _rocketInterface = new DNNrocketInterface(interfaceInfo);
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            _editLang = langRequired;
            if (_editLang == "") _editLang = DNNrocketUtils.GetEditCulture();

            if (DNNrocketUtils.IsSuperUser())
            {
                // Do NOT globally clear cache, performace drop on external call for FTP and HTML AppTheme Lists
                //CacheUtils.ClearAllCache("apptheme");

                _paramCmd = paramCmd;
                _userStorage = new UserStorage();

                if (_paramInfo.GetXmlPropertyBool("genxml/hidden/reload"))
                {
                    var menucmd = _userStorage.GetCommand(_systemData.SystemKey);
                    if (menucmd != "")
                    {
                        paramCmd = menucmd;
                        _paramInfo = _userStorage.GetParamInfo(_systemData.SystemKey);
                        var interfacekey = _userStorage.GetInterfaceKey(_systemData.SystemKey);
                        _rocketInterface = new DNNrocketInterface(systemInfo, interfacekey);
                    }
                }
                else
                {
                    if (_paramInfo.GetXmlPropertyBool("genxml/hidden/track"))
                    {
                        _userStorage.Track(_systemData.SystemKey, paramCmd, _paramInfo, _rocketInterface.InterfaceKey);
                    }
                }


                _appThemeFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
                _appVersionFolder = _paramInfo.GetXmlProperty("genxml/hidden/appversionfolder");
                if (_appVersionFolder == "") _appVersionFolder = _userStorage.Get("selectedappversion");
                if (_appVersionFolder == "") _appVersionFolder = "1.0";
                _userStorage.Set("selectedappversion", _appVersionFolder);

                if (_paramInfo.GetXmlPropertyBool("genxml/hidden/clearselectedsystemkey"))
                {
                    _userStorage.Set("selectedsystemkey", "");
                }

                _selectedSystemKey = _paramInfo.GetXmlProperty("genxml/hidden/urlparamsystemkey");
                if (_selectedSystemKey == "")  _selectedSystemKey = _paramInfo.GetXmlProperty("genxml/hidden/selectedsystemkey");
                if (_selectedSystemKey == "")
                {
                    _selectedSystemKey = _userStorage.Record.GetXmlProperty("genxml/hidden/selectedsystemkey");
                }
                else
                {
                    _userStorage.Set("selectedsystemkey", _selectedSystemKey);
                }

                AssignEditLang();

                switch (paramCmd)
                {
                    case "rocketapptheme_getlist":
                        strOut = GetList();
                        break;
                    case "rocketapptheme_getdetail":
                        strOut = GetDetail();
                        break;
                    case "rocketapptheme_save":
                        SaveData();
                        strOut = GetDetail();
                        break;
                    case "rocketapptheme_addimage":
                        AddListImage();
                        strOut = GetDetail();
                        break;
                    case "rocketapptheme_addresx":
                        SaveData();
                        AddResxFile();
                        strOut = GetDetail();
                        break;
                    case "rocketapptheme_rebuildresx":
                        strOut = RebuildResx();
                        break;  
                    case "rocketapptheme_addfield":
                        SaveData();
                        strOut = AddListField();
                        break;
                    case "rocketapptheme_addsettingfield":
                        SaveData();
                        strOut = AddSettingListField();
                        break;
                    case "rocketapptheme_createversion":
                        strOut = CreateNewVersion();
                        break;
                    case "rocketapptheme_changeversion":
                        _appVersionFolder = _paramInfo.GetXmlProperty("genxml/hidden/appversionfolder");
                        var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                        appTheme.Record.SetXmlProperty("genxml/select/versionfolder", _appVersionFolder);
                        appTheme.Update();
                        _userStorage.Set("selectedappversion", _appVersionFolder);
                        strOut = GetDetail();
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
                    case "rocketapptheme_import":
                        strOut = ImportAppTheme();
                        break;
                    case "rocketapptheme_saveeditor":
                        strOut = SaveEditor();
                        break;
                    case "rocketapptheme_docopy":
                        strOut = DoCopyAppTheme();
                        break;
                    case "rocketapptheme_uploadapptheme":
                        strOut = UploadAppTheme();
                        break;
                    case "rocketapptheme_refreshprivatelist":
                        var ftpConnect = new FtpConnect(_selectedSystemKey);
                        ftpConnect.ReindexPrivateXmlList();
                        strOut = GetPrivateListAppTheme(false);
                        break;
                    case "rocketapptheme_getprivatelist":
                        strOut = GetPrivateListAppTheme(true);
                        break;
                    case "rocketapptheme_saveftpdetails":
                        strOut = SaveServerDetails();
                        break;
                    case "rocketapptheme_downloadprivate":
                        strOut = GetPrivateAppTheme();
                        break;
                    case "rocketapptheme_refreshpubliclist":
                        strOut = GetPublicListAppTheme(false);
                        break;
                    case "rocketapptheme_getpubliclist":
                        strOut = GetPublicListAppTheme(true);
                        break;
                    case "rocketapptheme_downloadpublic":
                        strOut = GetPublicAppTheme();
                        break;
                    case "rocketapptheme_downloadallpublic":
                        strOut = GetAllPublicAppThemes();
                        break;
                    case "rocketapptheme_downloadallprivate":
                        strOut = GetAllPrivateAppThemes();
                        break;
                    case "rocketapptheme_getfiledata":
                        strOut = GetFileData();
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
                }
            }
            else
            {
                strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
            }

            return DNNrocketUtils.ReturnString(strOut);
        }


        public string GetFileData()
        {
            try
            {
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);

                // output json, so we can get filename and filedata.
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                var ext = Path.GetExtension(fname);
                var strOut = "{\"editorfilename\":\"" + fname + "\",\"editorfiledata\":\"" + GeneralUtils.EnCode(appTheme.GetTemplate(fname)) + "\"}";
                return strOut;
            }
            catch (Exception ex)
            {
                return GeneralUtils.EnCode(ex.ToString());
            }
        }

        public string CreateNewVersion()
        {
            try
            {
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var rtn = appTheme.CopyVersion(appTheme.AppVersionFolder, (Convert.ToDouble(appTheme.LatestVersionFolder) + 1).ToString("0.0"));
                _appVersionFolder = appTheme.AppVersionFolder;
                _userStorage.Set("selectedappversion", _appVersionFolder);
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
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                appTheme.DeleteVersion();
                _appVersionFolder = appTheme.AppVersionFolder;
                ClearServerCacheLists();
                return GetDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string DeleteTheme()
        {
            try
            {
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                appTheme.DeleteTheme();
                ClearServerCacheLists();
                return GetList();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void ClearServerCacheLists()
        {
            // clear all cache for aptheme
            CacheUtils.ClearAllCache();
            CacheFileUtils.ClearAllCache();
            DNNrocketUtils.ClearPortalCache();
        }

        public string CreateNewAppTheme()
        {
            try
            {
                var appthemeprefix = GeneralUtils.AlphaNumeric(FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemeprefix")));
                var appthemename = GeneralUtils.AlphaNumeric(FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemename")));

                var newAppThemeName = appthemename;
                if (appthemeprefix != "") newAppThemeName = appthemeprefix + "_" + newAppThemeName;

                var appSystemThemeFolderRel = "/DesktopModules/DNNrocket/SystemThemes/" + _selectedSystemKey;
                var appThemeFolderRel = appSystemThemeFolderRel + "/" + newAppThemeName;
                var appThemeFolderMapPath = DNNrocketUtils.MapPath(appThemeFolderRel);

                if (Directory.Exists(appThemeFolderMapPath))
                {
                    return DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/AppThemes/App_LocalResources/", "AppThemes.appthemeexists");
                }

                // crearte new apptheme.
                var appTheme = new AppTheme(_selectedSystemKey, newAppThemeName, "1.0");
                appTheme.AppThemePrefix = appthemeprefix;
                appTheme.AppThemeName = newAppThemeName;
                appTheme.Update();

                _userStorage.Set("selectedappversion", "1.0");

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
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var resxData = new ResxData(appTheme.GetFileMapPath(fname));
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("resxData", resxData);
                var razorTempl = DNNrocketUtils.GetRazorTemplateData("ResxPopUp.cshtml", _rocketInterface.TemplateRelPath, _rocketInterface.DefaultTheme, DNNrocketUtils.GetCurrentCulture(), _rocketInterface.ThemeVersion, true);
                return DNNrocketUtils.RazorObjectRender(razorTempl, appTheme, dataObjects, _passSettings, null, true);
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
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var resxData = new ResxData(appTheme.GetFileMapPath(fname));

                var key = "new" + (resxData.DataDictionary.Count + 1).ToString() + ".Text";
                var lp = (resxData.DataDictionary.Count + 1);
                while (resxData.DataDictionary.ContainsKey(key))
                {
                    lp += 1;
                    key = "new" + (lp).ToString() + ".Text";
                }
                resxData.AddField(key, "");
                resxData.Save();
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
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var resxData = new ResxData(appTheme.GetFileMapPath(fname));
                resxData.RemoveField(key);
                resxData.Save();
                return GetResxDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String SaveResxDetail()
        {
            try
            {
                var key = _paramInfo.GetXmlProperty("genxml/hidden/key");
                var fname = _paramInfo.GetXmlProperty("genxml/hidden/filename");
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var resxData = new ResxData(appTheme.GetFileMapPath(fname));
                resxData.RemoveAllFields();
                var resxlist = _postInfo.GetRecordList("resxdictionarydata");
                foreach (var r in resxlist)
                {
                    resxData.AddField(r.GetXmlProperty("genxml/key"), r.GetXmlProperty("genxml/value"));
                }
                resxData.Save();
                return GetResxDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public String GetDetail()
        {
            try
            {
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                return GetEditTemplate(appTheme);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public String GetList()
        {
            try
            {
                var appThemeDataList = new AppThemeDataList(_selectedSystemKey);
                var template = _rocketInterface.DefaultTemplate;
                if (template == "") template = "appthemelist.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(template, appThemeDataList.AppProjectFolderRel, _rocketInterface.DefaultTheme, DNNrocketUtils.GetCurrentCulture(),"1.0",true);
                var passSettings = _postInfo.ToDictionary();
                passSettings.Add("AppProjectThemesFolderMapPath", appThemeDataList.AppProjectThemesFolderMapPath);

                return DNNrocketUtils.RazorDetail(razorTempl, appThemeDataList, passSettings,null,true);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string GetAllPublicAppThemes()
        {
            try
            {
                var appThemeDataPublicList = new AppThemeDataPublicList(_selectedSystemKey, true);
                foreach (var a in appThemeDataPublicList.List)
                {
                    if (!a.GetXmlPropertyBool("genxml/hidden/islatestversion") && !a.GetXmlPropertyBool("genxml/hidden/localupdated"))
                    {
                        DownloadPublicAppTheme(a.GetXmlProperty("genxml/hidden/appthemefolder"));
                    }
                }
                return GetPublicListAppTheme(false);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        public string GetAllPrivateAppThemes()
        {
            try
            {
                var appThemeDataPrivateList = new AppThemeDataPublicList(_selectedSystemKey, true);
                foreach (var a in appThemeDataPrivateList.List)
                {
                    if (!a.GetXmlPropertyBool("genxml/hidden/islatestversion") && !a.GetXmlPropertyBool("genxml/hidden/localupdated"))
                    {
                        DownloadPrivateAppTheme(a.GetXmlProperty("genxml/hidden/appthemefolder"));
                    }
                }
                return GetPublicListAppTheme(false);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        private void DownloadPublicAppTheme(string appThemeFolder = "")
        {
            if (appThemeFolder == "") appThemeFolder = _appThemeFolder;
            var httpConnect = new HttpConnect(_selectedSystemKey);
            var userid = DNNrocketUtils.GetCurrentUserId();
            var destinationMapPath = DNNrocketUtils.TempDirectoryMapPath() + "\\" + userid + "_" + appThemeFolder + ".zip";
            httpConnect.DownloadAppThemeToFile(appThemeFolder, destinationMapPath);
            var appTheme = new AppTheme(destinationMapPath);
            appTheme.Update();
            File.Delete(destinationMapPath);
            ClearServerCacheLists();
        }

        public string GetPublicAppTheme(string appThemeFolder = "")
        {
            try
            {
                DownloadPublicAppTheme(appThemeFolder);
                return GetPublicListAppTheme(false);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string GetPublicListAppTheme(bool useCache)
        {
            try
            {
                var appThemeDataList = new AppThemeDataList(_selectedSystemKey);
                var appThemeDataPublicList = new AppThemeDataPublicList(_selectedSystemKey, useCache);
                var template = "AppThemeOnlinePublicList.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(template, appThemeDataList.AppProjectFolderRel, _rocketInterface.DefaultTheme, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                var passSettings = _postInfo.ToDictionary();

                return DNNrocketUtils.RazorDetail(razorTempl, appThemeDataPublicList, passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        private void DownloadPrivateAppTheme(string appThemeFolder = "")
        {
            if (appThemeFolder == "") appThemeFolder = _appThemeFolder;

            var ftpConnect = new FtpConnect(_selectedSystemKey);
            var userid = DNNrocketUtils.GetCurrentUserId();
            var destinationMapPath = DNNrocketUtils.TempDirectoryMapPath() + "\\" + userid + "_" + appThemeFolder + ".zip";
            ftpConnect.DownloadAppThemeToFile(appThemeFolder, destinationMapPath);
            var appTheme = new AppTheme(destinationMapPath);
            appTheme.Update();
            File.Delete(destinationMapPath);
            ClearServerCacheLists();
        }
        public string GetPrivateAppTheme(string appThemeFolder = "")
        {
            try
            {
                DownloadPrivateAppTheme(appThemeFolder);
                return GetPrivateListAppTheme(false);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string GetPrivateListAppTheme(bool useCache)
        {
            try
            {
                var appThemeDataList = new AppThemeDataList(_selectedSystemKey);
                var appThemeDataPrivateList = new AppThemeDataPrivateList(appThemeDataList.SelectedSystemKey, useCache);
                var template = "AppThemeOnlinePrivateList.cshtml";
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(template, appThemeDataList.AppProjectFolderRel, _rocketInterface.DefaultTheme, DNNrocketUtils.GetCurrentCulture(), "1.0", true);
                var passSettings = _postInfo.ToDictionary();

                return DNNrocketUtils.RazorDetail(razorTempl, appThemeDataPrivateList, passSettings, null, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        private Dictionary<string, string> ExportAppTheme()
        {
            var appThemeFolder = _paramInfo.GetXmlProperty("genxml/urlparams/appthemefolder");
            var appVersionFolder = _paramInfo.GetXmlProperty("genxml/urlparams/appversionfolder");
            var appTheme = new AppTheme(_selectedSystemKey, appThemeFolder, appVersionFolder);

            var exportZipMapPath = appTheme.ExportZipFile();

            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("filenamepath", exportZipMapPath);
            rtnDic.Add("downloadname", appTheme.AppThemeFolder + ".zip");

            return rtnDic;
        }
        private string UploadAppTheme()
        {
            var rtnDic = ExportAppTheme();
            if (rtnDic["filenamepath"] != null && rtnDic["filenamepath"] != "")
            {
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                var ftpConnect = new FtpConnect(_selectedSystemKey);
                var rtn = ftpConnect.UploadAppTheme(appTheme);
                var appThemeDataPrivateList = new AppThemeDataPrivateList(_selectedSystemKey, true);
                appThemeDataPrivateList.ClearCache();
                ClearServerCacheLists();
                return rtn;
            }
            return "FTP Failed, No AppTheme export file found.";
        }

        private string ImportAppTheme()
        {
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var userid = DNNrocketUtils.GetCurrentUserId();
                        var userFolder = DNNrocketUtils.TempDirectoryMapPath();
                        var friendlyname = GeneralUtils.DeCode(f);
                        var fname = userFolder + "\\" + userid + "_" + friendlyname;
                        var _appTheme = new AppTheme(fname);
                        // delete import file
                        File.Delete(fname);
                    }
                }
                ClearServerCacheLists();
            }
            return GetList();
        }

        public void SaveData()
        {
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            appTheme.Save(_postInfo);
            ClearServerCacheLists();
        }
        public string DoCopyAppTheme()
        {
            try
            {
                var appthemeprefix = FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemeprefix"));
                var appthemename = FileUtils.RemoveInvalidFileChars(_postInfo.GetXmlProperty("genxml/textbox/appthemename"));
                var newAppThemeName = appthemename;
                if (appthemeprefix != "") newAppThemeName = appthemeprefix + "_" + newAppThemeName;
                var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
                appTheme.Copy(newAppThemeName);
                appTheme = new AppTheme(_selectedSystemKey, newAppThemeName, appTheme.LatestVersionFolder);
                ClearServerCacheLists();
                return GetDetail();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string SaveServerDetails()
        {
            var systemGlobalData = new SystemGlobalData();
            systemGlobalData.FtpServer = _postInfo.GetXmlProperty("genxml/textbox/ftpserver");
            systemGlobalData.FtpUserName = _postInfo.GetXmlProperty("genxml/textbox/ftpuser");
            systemGlobalData.FtpPassword = _postInfo.GetXmlProperty("genxml/textbox/ftppassword");
            systemGlobalData.Update();

            var systemData = new SystemData(_selectedSystemKey);
            systemData.FtpRoot = _postInfo.GetXmlProperty("genxml/textbox/ftproot");
            systemData.Update();

            ClearServerCacheLists();

            return "OK";
        }

        public string SaveEditor()
        {
            var editorcode = _postInfo.GetXmlProperty("genxml/hidden/editorcodesave");
            var filename = _postInfo.GetXmlProperty("genxml/hidden/editorfilenamesave");
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            appTheme.SaveEditor(filename, editorcode);
            return "OK";
        }

        public void AddListImage()
        {
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            appTheme.AddListImage();
        }

        public string RebuildResx()
        {
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            var culturecoderesx = _paramInfo.GetXmlProperty("genxml/hidden/culturecoderesx");

            // [TODO: rebuild resx]
            //appTheme.ReBuildResx(culturecoderesx);

            return GetEditTemplate(appTheme);
        }

        private void AddResxFile()
        {
            var culturecoderesx = _paramInfo.GetXmlProperty("genxml/hidden/culturecoderesx");
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            if (culturecoderesx != "") culturecoderesx = "." + culturecoderesx;
            var fileMapPath = appTheme.AppThemeVersionFolderMapPath + "\\resx\\" + appTheme.AppThemeFolder + culturecoderesx + ".resx";
            var resxData = new ResxData(fileMapPath);
            if (!resxData.Exists)
            {
                // save a base (or default) resx file, to we get the format easier.
                var defaultrexFileMapPath = appTheme.AppThemeVersionFolderMapPath + "\\resx\\" + appTheme.AppThemeFolder + ".resx";
                var resxFileData = "";
                if (File.Exists(defaultrexFileMapPath))
                {
                    resxFileData = FileUtils.ReadFile(defaultrexFileMapPath);
                }
                else
                {
                    var baserexFileMapPath = appTheme.AppProjectFolderMapPath + "\\AppThemeBase\\resxtemplate.xml";
                    resxFileData = FileUtils.ReadFile(baserexFileMapPath);
                }
                FileUtils.SaveFile(fileMapPath, resxFileData);
            }
        }

        private string AddListField()
        {
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            appTheme.AddListField();
            return GetEditTemplate(appTheme);
        }
        private string AddSettingListField()
        {
            var appTheme = new AppTheme(_selectedSystemKey, _appThemeFolder, _appVersionFolder);
            appTheme.AddSettingListField();
            return GetEditTemplate(appTheme);
        }
        private void AssignEditLang()
        {
            var nextLang = _paramInfo.GetXmlProperty("genxml/hidden/nextlang");
            if (nextLang != "") _editLang = DNNrocketUtils.SetEditCulture(nextLang);
        }
        private string GetEditTemplate(AppTheme appTheme)
        {
            var razorTempl = DNNrocketUtils.GetRazorTemplateData("AppThemeDetails.cshtml", _rocketInterface.TemplateRelPath, _rocketInterface.DefaultTheme, DNNrocketUtils.GetCurrentCulture(), _rocketInterface.ThemeVersion, true);
            return DNNrocketUtils.RazorDetail(razorTempl, appTheme, _passSettings, null, true);
        }


    }
}

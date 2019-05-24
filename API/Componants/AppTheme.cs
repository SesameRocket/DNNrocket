﻿using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DNNrocketAPI.Componants
{

    public class AppTheme
    {
        private string _themeFolderMapPath;
        private string _themeFolder;
        private string _controlMapPath;
        private Dictionary<string, string> _templates;
        private Dictionary<string, string> _cssPath;
        private Dictionary<string, string> _imgPath;
        private Dictionary<string, string> _resxPath;

        public AppTheme(string appName, string versionFolder)
        {
            InitClass(appName, "/DesktopModules/DNNrocket/AppThemes", "", versionFolder);
        }

        public AppTheme(string appName, string langRequired, string versionFolder)
        {
            InitClass(appName, "/DesktopModules/DNNrocket/AppThemes", langRequired, versionFolder);
        }

        public AppTheme(string appName, string appFolder = "/DesktopModules/DNNrocket/AppThemes", string langRequired = "", string versionFolder = "v1")
        {
            InitClass(appName, appFolder, langRequired, versionFolder);
        }

        private void InitClass(string appName, string appFolder, string langRequired, string versionFolder)
        {
            AppVersionFolder = versionFolder;
            if (langRequired == "") langRequired = DNNrocketUtils.GetCurrentCulture();
            AppName = appName;
            AppFolder = appFolder;
            AppCultureCode = langRequired;
            _controlMapPath = DNNrocketUtils.MapPath(AppFolder);
            _themeFolderMapPath = _controlMapPath.TrimEnd('\\') + "\\Themes\\" + AppName;
            _themeFolder = AppFolder.TrimEnd('/') + "/Themes/" + AppName;
            Populate();
        }

        public void Populate()
        {
            var themeFolderPath = "Themes\\" + AppName + "\\" + AppVersionFolder;
            var templCtrl = new Simplisity.TemplateEngine.TemplateGetter(DNNrocketUtils.HomeDirectory(), themeFolderPath, _controlMapPath);
            ActiveViewTemplate = templCtrl.GetTemplateData("view.cshtml", AppCultureCode);
            ActiveEditTemplate = templCtrl.GetTemplateData("edit.cshtml", AppCultureCode);
            ActivePageHeaderTemplate = templCtrl.GetTemplateData("pageheader.cshtml", AppCultureCode);
            var logoMapPath = _themeFolderMapPath + "\\Logo.png";
            Logo = _themeFolder + "/Logo.png";
            if (!File.Exists(logoMapPath)) Logo = "";

            var xmlMeta = FileUtils.ReadFile(_themeFolderMapPath + "\\Meta.xml");
            MetaInfo = new SimplisityInfo(AppCultureCode);
            MetaInfo.XMLData = xmlMeta;

            Summary = MetaInfo.GetXmlProperty("genxml/lang-" + AppCultureCode + "/genxml/textbox/summary");
            if (Summary == "") Summary = MetaInfo.GetXmlProperty("genxml/lang-en-US/genxml/textbox/summary");
            DisplayName = MetaInfo.GetXmlProperty("genxml/lang-" + AppCultureCode + "/genxml/textbox/displayname");
            if (DisplayName == "") DisplayName = MetaInfo.GetXmlProperty("genxml/lang-en-US/genxml/textbox/displayname");
        }

        public List<SimplisityInfo> ListFields()
        {
            return MetaInfo.GetList("settingsdata");
        }


        public void SaveTheme()
        {
            var strXml = "<lang-" + AppCultureCode  + ">";
            strXml += MetaInfo.GetLangRecord().XMLData;
            strXml += "</lang-" + AppCultureCode + ">";

            var xmlMeta = FileUtils.ReadFile(_themeFolderMapPath + "\\Meta.xml");
            var mInfo = new SimplisityInfo();
            if (xmlMeta != "") mInfo.XMLData = xmlMeta;

            mInfo.SetXmlProperty("genxml/textbox/appname", AppName);
            mInfo.SetXmlProperty("genxml/hidden/logo", Logo);
            mInfo.SetXmlProperty("genxml/select/versionfolder", AppVersionFolder);
            mInfo.Lang = AppCultureCode;

            if (AppCultureCode != "")
            {
                mInfo.SetXmlProperty("genxml/lang-" + AppCultureCode + "/genxml/textbox/summary", Summary);
                mInfo.SetXmlProperty("genxml/lang-" + AppCultureCode + "/genxml/textbox/displayname", DisplayName);
            }

            if (!Directory.Exists(_themeFolderMapPath))
            {
                Directory.CreateDirectory(_themeFolderMapPath);
            }
            FileUtils.SaveFile(_themeFolderMapPath + "\\Meta.xml", mInfo.XMLData);
        }

        public void DeleteTheme()
        {
            if (Directory.Exists(_themeFolderMapPath))
            {
                Directory.Delete(_themeFolderMapPath,true);
            }
        }
        public void DeleteVersion(string versionFolder)
        {
            if (Directory.Exists(_themeFolderMapPath + "\\" + versionFolder))
            {
                Directory.Delete(_themeFolderMapPath + "\\" + versionFolder, true);
            }
        }

        public void CreateVersion(string versionFolder)
        {
            if (!Directory.Exists(_themeFolderMapPath + "\\" + versionFolder))
            {
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder);
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder + "\\css");
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder + "\\default");
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder + "\\resx");
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder + "\\js");
                Directory.CreateDirectory(_themeFolderMapPath + "\\" + versionFolder + "\\img");
            }
        }

        public void CopyVersion(string sourceVersionFolder, string destVersionFolder)
        {
            sourceVersionFolder = _themeFolderMapPath + "\\" + sourceVersionFolder;
            destVersionFolder = _themeFolderMapPath + "\\" + destVersionFolder;

            if (Directory.Exists(sourceVersionFolder))
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourceVersionFolder, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourceVersionFolder, destVersionFolder));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourceVersionFolder, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourceVersionFolder, destVersionFolder), true);
            }
        }


        #region "properties"

        public string AppName { get; private set; }
        public SimplisityInfo MetaInfo { get; private set; }
        public string Logo { get; private set; }
        public string DisplayName { get; set; }
        public string Summary { get; set; }
        public string AppCultureCode { get; set; }
        public string AppFolder { get; set; }
        public string AppVersionFolder { get; set; }
        public string ActiveViewTemplate { get; set; }
        public string ActiveEditTemplate { get; set; }
        public string ActivePageHeaderTemplate { get; set; }

        #endregion


    }

}

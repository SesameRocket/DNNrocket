﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Componants
{
    public class SystemGlobalData
    {
        private static string _cacheKey; 
        public SystemGlobalData(bool cache = true)
        {
            _cacheKey = "rocketGLOBALSETTINGS";
            if (cache) Info = (SimplisityInfo)CacheUtils.GetCache(_cacheKey);
            if (Info == null) LoadData();

            if (cache) ConfigInfo = (SimplisityRecord)CacheUtils.GetCache(_cacheKey + "ConfigInfo");
            if (ConfigInfo == null) LoadConfig();
        }
        public void Save(SimplisityInfo postInfo)
        {
             var objCtrl = new DNNrocketController();
            Info.XMLData = postInfo.XMLData;
            Update();
        }
        public void Update()
        {
            var objCtrl = new DNNrocketController();
            objCtrl.Update(Info);
            CacheUtils.ClearAllCache();
            CacheUtils.SetCache(_cacheKey, Info);
        }
        private void LoadConfig()
        {
            ConfigInfo = new SimplisityRecord();
            //import the config data from XML file.
            var fullFileName = DNNrocketUtils.MapPath("/DesktopModules/DNNrocket").TrimEnd('\\') + "\\globalconfig.xml";
            var xmlData = FileUtils.ReadFile(fullFileName);
            if (xmlData != "") ConfigInfo.XMLData = xmlData;
            CacheUtils.SetCache(_cacheKey + "ConfigInfo", ConfigInfo);
        }

        private void LoadData()
        {
            var objCtrl = new DNNrocketController();
            Info = objCtrl.GetByType(DNNrocketUtils.GetPortalId(), -1, "GLOBALSETTINGS");
            if (Info == null)
            {
                Info = new SimplisityInfo();
                Info.ItemID = -1;
                Info.PortalId = DNNrocketUtils.GetPortalId();
                Info.TypeCode = "GLOBALSETTINGS";
                Info.ItemID = objCtrl.Update(Info);
            }
            CacheUtils.SetCache(_cacheKey, Info);
        }

        public SimplisityInfo Info { get; set; }
        public SimplisityRecord ConfigInfo { get; set; }

        public string FtpUserName { get { return Info.GetXmlProperty("genxml/textbox/ftpuser"); } set { Info.SetXmlProperty("genxml/textbox/ftpuser",value); } }
        public string FtpPassword { get { return Info.GetXmlProperty("genxml/textbox/ftppassword"); } set { Info.SetXmlProperty("genxml/textbox/ftppassword", value); } }
        public string FtpServer { get { return Info.GetXmlProperty("genxml/textbox/ftpserver"); } set { Info.SetXmlProperty("genxml/textbox/ftpserver", value); } }
        public string ImageType { get { return Info.GetXmlProperty("genxml/select/imagetype"); } set { Info.SetXmlProperty("genxml/select/imagetype", value); } }
        public bool PngImage { get { if (Info.GetXmlProperty("genxml/select/imagetype") != "jpg") return true; else return false; } }
        public string CKEditorCssList { get { return Info.GetXmlProperty("genxml/textbox/ckeditorcsslist"); } set { Info.SetXmlProperty("genxml/textbox/ckeditorcsslist", value); } }


        // globalconfig.xml - Config XML file data
        public string LicenseUrl { get { return ConfigInfo.GetXmlProperty("genxml/hidden/licenseurl"); } }
        public string PublicAppThemeURI { get { return ConfigInfo.GetXmlProperty("genxml/hidden/publicappthemeuri"); } }

        public string GlobalPageHeading
        {
            get { return Info.GetXmlProperty("genxml/textbox/globalheading"); }
            set { Info.SetXmlProperty("genxml/textbox/globalheading", value); }
        }


    }
}

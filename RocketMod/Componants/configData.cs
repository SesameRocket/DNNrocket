﻿using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RocketMod
{

    public class ConfigData
    {

        private bool _configExists;
        private int _tabid;
        private int _moduleid;
        private int _systemid;
        private int _portalid;


        public SimplisityInfo ConfigInfo;

        public ConfigData(int portalId, int systemId, int tabId, int moduleId)
        {
            _portalid = portalId;
            _tabid = tabId;
            _moduleid = moduleId;
            _systemid = systemId;

            PopulateConfig();
        }

        #region "CONFIG"

        public void PopulateConfig()
        {
            var objCtrl = new DNNrocketController();
            ConfigInfo = objCtrl.GetData("rocketmod_" + _moduleid, "CONFIG",DNNrocketUtils.GetCurrentCulture(), -1, _moduleid, true);
            if (ConfigInfo == null)
            {
                _configExists = false;
                ConfigInfo = new SimplisityInfo();
                ConfigInfo.ModuleId = _moduleid;
            }
            else
            {
                if (AppTheme == "")
                {
                    _configExists = false;
                }
                else
                {
                    _configExists = true;
                }
            }
        }

        public void DeleteConfig()
        {
            var objCtrl = new DNNrocketController();
            var info = objCtrl.GetData("rocketmod_" + _moduleid, "CONFIG", DNNrocketUtils.GetCurrentCulture(), -1, _moduleid, true);
            if (info != null)
            {
                objCtrl.Delete(info.ItemID);
                CacheUtils.ClearCache("rocketmod" + ModuleId);
                PopulateConfig();
            }
        }

        public void SaveAppTheme(string appTheme)
        {
            if (appTheme != "")
            {
                ConfigInfo.SetXmlProperty("genxml/hidden/apptheme", appTheme);
                var objCtrl = new DNNrocketController();
                var info = objCtrl.SaveData("rocketmod_" + _moduleid, "CONFIG", ConfigInfo, _systemid, _moduleid);
                CacheUtils.ClearCache("rocketmod" + ModuleId);
                PopulateConfig();
            }
        }

        public void SaveConfig(SimplisityInfo postInfo)
        {
            //remove any params
            postInfo.RemoveXmlNode("genxml/postform");
            postInfo.RemoveXmlNode("genxml/urlparams");

            if (postInfo.GetXmlProperty("genxml/hidden/apptheme") != "")
            {
                ConfigInfo.SetXmlProperty("genxml/dropdownlist/paymentprovider", postInfo.GetXmlProperty("genxml/dropdownlist/paymentprovider"));
            }
            else
            {
                postInfo.SetXmlProperty("genxml/dropdownlist/paymentprovider", ConfigInfo.GetXmlProperty("genxml/dropdownlist/paymentprovider"));
            }
            postInfo.SetXmlProperty("genxml/checkbox/noiframeedit", "False"); // we do not want iframe edit

            ConfigInfo.XMLData = postInfo.XMLData;

            var objCtrl = new DNNrocketController();
            var info = objCtrl.SaveData("rocketmod_" + _moduleid, "CONFIG", ConfigInfo, _systemid, _moduleid);
            CacheUtils.ClearCache("rocketmod" + ModuleId);
            PopulateConfig();
        }

        #endregion

        public string ProviderAssembly { get { return ConfigInfo.GetXmlProperty("genxml/textbox/assembly"); } }
        public string ProviderClass { get { return ConfigInfo.GetXmlProperty("genxml/textbox/namespaceclass"); } }
        public string ManagerEmail { get { return ConfigInfo.GetXmlProperty("genxml/textbox/manageremail"); } }
        public string AppTheme { get { return ConfigInfo.GetXmlProperty("genxml/hidden/apptheme"); } }
        public string AppThemeVersion { get { return ConfigInfo.GetXmlProperty("genxml/select/versionfolder"); } }
        public string ImageFolderRel { get{ return DNNrocketUtils.HomeRelDirectory() + "/" + ImageFolder; } }
        public string DocumentFolderRel { get{ return DNNrocketUtils.HomeRelDirectory() + "/" + DocumentFolder;} }

        public string DocumentFolder
        {
            get
            {
                if (ConfigInfo.GetXmlProperty("genxml/textbox/documentfolder") == "")
                {
                    return "docs";
                }
                else
                {
                    return ConfigInfo.GetXmlProperty("genxml/textbox/documentfolder");
                }
            }
        }
        public string ImageFolder
        {
            get
            {
                if (ConfigInfo.GetXmlProperty("genxml/textbox/imagefolder") == "")
                {
                    return "images";
                }
                else
                {
                    return ConfigInfo.GetXmlProperty("genxml/textbox/imagefolder");
                }
            }
        }
        public string DocumentFolderMapPath { get { return DNNrocketUtils.MapPath(DocumentFolderRel); } }
        public string ImageFolderMapPath { get { return DNNrocketUtils.MapPath(ImageFolderRel); } }

        public bool Exists { get { return _configExists; } }
        public int ModuleId { get {return _moduleid;} }
        public int TabId { get { return _tabid; } }
        public int SystemId { get { return _systemid; } }


    }

}

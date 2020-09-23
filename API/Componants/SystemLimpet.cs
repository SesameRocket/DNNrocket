﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;
using DNNrocketAPI.Componants;
using System.IO;
using System.Xml;

namespace DNNrocketAPI.Componants
{
    public class SystemLimpet
    {
        private DNNrocketController _objCtrl;
        public SystemLimpet(string systemKey)
        {
            var systemInfo = (SimplisityInfo)CacheUtils.GetCache(systemKey); // use cache for SystemKey read.
            if (systemInfo == null)
            {
                _objCtrl = new DNNrocketController();
                systemInfo = _objCtrl.GetByGuidKey(-1, -1, "SYSTEM", systemKey);
            }
            InitSystem(systemInfo);
        }
        public SystemLimpet(int systemId)
        {
            // always read from DB when systemId is used to construct Limpet
            _objCtrl = new DNNrocketController();
            var systemInfo = _objCtrl.GetInfo(systemId);
            InitSystem(systemInfo);
        }
        public SystemLimpet(SimplisityInfo systemInfo)
        {
            InitSystem(systemInfo);        
        }
        private void InitSystem(SimplisityInfo systemInfo)
        {
            if (systemInfo == null)
            {
                systemInfo = new SimplisityInfo();
                systemInfo.ItemID = -1;
                Exists = false;
            }
            else
            {
                Exists = true;
            }
            Info = systemInfo;
            EventList = new List<RocketInterface>();
            SchedulerList = new List<RocketInterface>();            
            InterfaceList = new Dictionary<string, RocketInterface>();
            Settings = new Dictionary<string, string>();
            var l = Info.GetList("interfacedata");
            foreach (var r in l)
            {
                var rocketInterface = new RocketInterface(r);
                if (rocketInterface.IsProvider("eventprovider") && rocketInterface.Assembly != "" && rocketInterface.NameSpaceClass != "")
                {
                    EventList.Add(rocketInterface);
                }
                if (rocketInterface.IsProvider("scheduler") && rocketInterface.Assembly != "" && rocketInterface.NameSpaceClass != "")
                {
                    SchedulerList.Add(rocketInterface);
                }
                InterfaceList.Add(rocketInterface.InterfaceKey, rocketInterface);
            }
            var l2 = Info.GetList("settingsdata");
            foreach (var s in l2)
            {
                var key = s.GetXmlProperty("genxml/textbox/name");
                if (key != "" && !Settings.ContainsKey(key)) Settings.Add(key, s.GetXmlProperty("genxml/textbox/value"));
            }
        }

        public void Import(string importXml)
        {
            var objCtrl = new DNNrocketController();

            var infoFromXml = new SimplisityInfo();
            infoFromXml.FromXmlItem(importXml);

            var systemKey = infoFromXml.GUIDKey;

            // find if existing, so we can overwrite
            var systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", systemKey);
            if (systemInfo != null) Info = systemInfo; // use existing

            Info.XMLData = infoFromXml.XMLData;
            Info.GUIDKey = systemKey;
            Info.PortalId = 99999;
            Info.TypeCode = "SYSTEM";
            Info.SortOrder = infoFromXml.SortOrder;
            Info.ParentItemId = infoFromXml.ParentItemId;
            Info.XrefItemId = infoFromXml.XrefItemId;

            var fileMapPath = DNNrocketUtils.MapPath(Info.GetXmlProperty("genxml/hidden/imagepathlogo"));
            //var base64 = Info.GetXmlProperty("genxml/hidden/logobase64");
            //try
            //{
            //    FileUtils.SaveBase64ToFile(fileMapPath, base64);
            //}
            //catch (Exception ex)
            //{
            //    LogUtils.LogException(ex);
            //}

            Info.SetXmlProperty("genxml/hidden/logobase64", "");

            Update();

            // reload
            systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", Info.GUIDKey);
            InitSystem(systemInfo);

        }

        public string Export()
        {
            var exportInfo = (SimplisityInfo)Info.Clone();
            var logoMapPath = DNNrocketUtils.MapPath(Info.GetXmlProperty("genxml/hidden/imagepathlogo"));
            if (File.Exists(logoMapPath))
            {
                //var newImage = ImgUtils.CreateThumbnail(logoMapPath, Convert.ToInt32(140), Convert.ToInt32(140));

                // Convert the image to byte[]
                //System.IO.MemoryStream stream = new System.IO.MemoryStream();
                //newImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                //byte[] imageBytes = stream.ToArray();
                //string base64String = Convert.ToBase64String(imageBytes);
                //exportInfo.SetXmlProperty("genxml/hidden/logobase64", base64String, TypeCode.String, true);
            }

            return exportInfo.ToXmlItem();
        }

        public void Save(SimplisityInfo postInfo)
        {
            //remove any params
            postInfo.RemoveXmlNode("genxml/urlparams");

            Info.XMLData = postInfo.XMLData;
            Info.GUIDKey = postInfo.GetXmlProperty("genxml/textbox/ctrlkey");

            if (Info.GetXmlProperty("genxml/textbox/defaultinterface") == "")
            {
                Info.SetXmlProperty("genxml/textbox/defaultinterface", Info.GetXmlProperty("genxml/interfacedata/genxml[1]/textbox/interfacekey"));
            }

            Info.SetXmlProperty("genxml/hidden/logobase64", "");

            Info.PortalId = 99999;
            Info.TypeCode = "SYSTEM";

            Info.SortOrder = Info.GetXmlPropertyInt("genxml/textbox/sortorder");

            Update();

            // Capture existing SYSTEMLINK records, so we can selectivly delete. To protect the system during operation, so records are always there.
            var systemlinklist = _objCtrl.GetList(Info.PortalId, -1, "SYSTEMLINK");
            var systemlinkidxlist = _objCtrl.GetList(Info.PortalId, -1, "SYSTEMLINKIDX");
            var newsystemlinklist = new List<int>();
            var newsystemlinkidxlist = new List<int>();

            // make systemlink record
            var entityList = new List<string>();
            foreach (var i in Info.GetList("idxfielddata"))
            {
                var entitytypecode = i.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                var xreftypecode = i.GetXmlProperty("genxml/dropdownlist/xreftypecode");
                var entityguidkey = entitytypecode;

                if (!entityList.Contains(entityguidkey))
                {
                    //build xref list
                    var xmldata = "<genxml>";
                    foreach (XmlNode xrefnod in Info.XMLDoc.SelectNodes("genxml/idxfielddata/genxml[dropdownlist/entitytypecode/text()='" + entitytypecode + "']"))
                    {
                        xmldata += xrefnod.OuterXml;
                    }
                    xmldata += "</genxml>";

                    var idxinfo = _objCtrl.GetByGuidKey(Info.PortalId, Info.ItemID, "SYSTEMLINK", entityguidkey); // use system id as moduleid
                    if (idxinfo == null)
                    {
                        idxinfo = new SimplisityInfo();
                        idxinfo.PortalId = Info.PortalId;
                        idxinfo.TypeCode = "SYSTEMLINK";
                        idxinfo.GUIDKey = entityguidkey;
                        idxinfo.XMLData = xmldata;
                        idxinfo.ParentItemId = Info.ItemID;
                        idxinfo.ModuleId = Info.ItemID;
                        var itemid = _objCtrl.SaveRecord(idxinfo).ItemID;
                        idxinfo.ItemID = itemid;
                    }
                    else
                    {
                        idxinfo.ParentItemId = Info.ItemID;
                        idxinfo.XMLData = xmldata;
                        _objCtrl.SaveRecord(idxinfo);
                    }
                    newsystemlinklist.Add(idxinfo.ItemID);
                    entityList.Add(entityguidkey);

                    // SYSTEMLINKIDX
                    foreach (XmlNode xrefnod in idxinfo.XMLDoc.SelectNodes("genxml/genxml"))
                    {
                        var i2 = new SimplisityRecord();
                        i2.XMLData = xrefnod.OuterXml;

                        var idxref = i2.GetXmlProperty("genxml/textbox/indexref");
                        var typecodeIdx = i2.GetXmlProperty("genxml/dropdownlist/xreftypecode");
                        if (typecodeIdx == "")
                        {
                            typecodeIdx = i2.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                        }
                        var idxinfo2 = _objCtrl.GetByGuidKey(Info.PortalId, Info.ItemID, "SYSTEMLINK" + typecodeIdx, idxref);
                        if (idxinfo2 == null)
                        {
                            idxinfo2 = new SimplisityInfo();
                            idxinfo2.PortalId = Info.PortalId;
                            idxinfo2.TypeCode = "SYSTEMLINK" + typecodeIdx;
                            idxinfo2.GUIDKey = idxref;
                            idxinfo2.ParentItemId = idxinfo.ItemID;
                            idxinfo2.ModuleId = Info.ItemID;
                            idxinfo2.XMLData = i2.XMLData;
                            idxinfo2.TextData = typecodeIdx;
                            var itemid = _objCtrl.SaveRecord(idxinfo2).ItemID;
                            idxinfo2.ItemID = itemid;
                        }
                        else
                        {
                            idxinfo2.ParentItemId = idxinfo.ItemID;
                            idxinfo2.XMLData = i2.XMLData;
                            idxinfo2.TextData = typecodeIdx;
                            _objCtrl.SaveRecord(idxinfo2);
                        }
                        newsystemlinkidxlist.Add(idxinfo2.ItemID);
                    }
                }
            }

            // delete any that have been remove.
            foreach (var sl in systemlinklist)
            {
                if (!newsystemlinklist.Contains(sl.ItemID))
                {
                    _objCtrl.Delete(sl.ItemID);
                }
            }
            foreach (var sl in systemlinkidxlist)
            {
                if (!newsystemlinkidxlist.Contains(sl.ItemID))
                {
                    _objCtrl.Delete(sl.ItemID);
                }
            }

            Update();
        }

        public SimplisityInfo SystemInfo { get { return Info; } }
        public SimplisityInfo Info { get; set; }
        public List<RocketInterface> EventList { get; set;}
        public List<RocketInterface> SchedulerList { get; set; }
        public bool Exists { get; set; }
        public Dictionary<string, RocketInterface> InterfaceList { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public string GetSetting(string key)
        {
            if (Settings.ContainsKey(key)) return Settings[key];
            return "";
        }
        public bool HasInterface(string interfaceKey)
        {
            return InterfaceList.ContainsKey(interfaceKey);
        }
        public List<RocketInterface> GetInterfaceList()
        {
            var rtnList = new List<RocketInterface>();
            var s = Info.GetList("interfacedata");
            if (s == null) return null;
            foreach (var i in s)
            {
                var iface = new RocketInterface(i);
                rtnList.Add(iface);
            }
            return rtnList;
        }

        public RocketInterface GetInterface(string interfaceKey)
        {
            var s = Info.GetListItem("interfacedata", "genxml/textbox/interfacekey", interfaceKey);
            if (s == null) return null;
            return new RocketInterface(s);
        }
        public void ClearTempDB()
        {
            var objCtrl = new DNNrocketController();
            objCtrl.DeleteAllData("DNNrocketTemp");
        }
        public void Update()
        {
            var objCtrl = new DNNrocketController();
            objCtrl.Update(Info);
            if (SystemKey != "") CacheUtils.SetCache(SystemKey, Info);
        }

        public string SystemKey
        {
            get { return Info.GetXmlProperty("genxml/textbox/ctrlkey"); }
            set { Info.SetXmlProperty("genxml/textbox/ctrlkey", value); }
        }
        public string SystemName
        {
            get { return Info.GetXmlProperty("genxml/textbox/systemname"); }
            set { Info.SetXmlProperty("genxml/textbox/systemname", value); }
        }
        public string ApiUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/apiurl"); }
            set { Info.SetXmlProperty("genxml/textbox/apiurl", value); }
        }
        public string AdminUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/adminurl"); }
            set { Info.SetXmlProperty("genxml/textbox/adminurl", value); }
        }
        public bool CacheOff
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/cacheoff"); }
            set { Info.SetXmlProperty("genxml/checkbox/cacheoff", value.ToString()); }
        }
        public bool CacheOn
        {
            get { return !Info.GetXmlPropertyBool("genxml/checkbox/cacheoff"); }
        }
        public bool LogOn
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/logon"); }
        }
        public bool DebugMode
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/debugmode"); }
            set { Info.SetXmlProperty("genxml/checkbox/debugmode", value.ToString()); }
        }
        public string LicenseKey
        {
            get { return Info.GetXmlProperty("genxml/textbox/licensekey"); }
            set { Info.SetXmlProperty("genxml/textbox/licensekey", value); }
        }
        public string EncryptKey
        {
            get { return Info.GetXmlProperty("genxml/textbox/encryptkey"); }
            set { Info.SetXmlProperty("genxml/textbox/encryptkey", value); }
        }
        public string Logo
        {
            get { return Info.GetXmlProperty("genxml/hidden/imagepathlogo"); }
            set { Info.SetXmlProperty("genxml/hidden/imagepathlogo", value); }
        }
        public int SystemId
        {
            get { return Info.ItemID; }
        }
        public string SystemRelPath
        {
            get { return Info.GetXmlProperty("genxml/textbox/systemrelpath"); }
        }
        public string SystemMapPath
        {
            get { return DNNrocketUtils.MapPath(Info.GetXmlProperty("genxml/textbox/systemrelpath")); }
        }
        public string ImageFolderRelPath
        {
            get { return Info.GetXmlProperty("genxml/textbox/systemrelpath").TrimEnd('/') + "/images"; }
        }
        public string ImageFolderMapPath
        {
            get { return DNNrocketUtils.MapPath(Info.GetXmlProperty("genxml/textbox/systemrelpath").TrimEnd('/') + "/images"); }
        }
        public string DefaultInterface
        {
            get { return Info.GetXmlProperty("genxml/textbox/defaultinterface"); }
        }
        public string FtpRoot
        {
            get { return Info.GetXmlProperty("genxml/textbox/ftproot"); }
            set { Info.SetXmlProperty("genxml/textbox/ftproot", value); }
        }

    }
}

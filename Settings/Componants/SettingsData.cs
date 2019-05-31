﻿using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketSettings
{

    public class SettingsData : SimplisityData
    {
        private List<SimplisityInfo> _dataList;
        private int _tabid;
        private int _moduleid;
        private int _parentitemid;
        private string _langRequired;
        private string _entityTypeCode;
        private string _listName;
        private List<string> _cultureList;

        public SimplisityInfo Info;

        public SettingsData(int parentitemid, string langRequired, string entityTypeCode = "ROCKETSETTINGS", string listname = "settingsdata")
        {
            _entityTypeCode = entityTypeCode;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetEditCulture();
            _tabid = -1;
            _moduleid = -1;
            _parentitemid = parentitemid;
            _dataList = new List<SimplisityInfo>();
            _listName = listname;
            if (_parentitemid > 0)
            {
                Info = GetSettingData(_langRequired);
                Info.ParentItemId = _parentitemid;
                _cultureList = GetCultureList();
                Populate();
                PopulateList();
            }
            else
            {
                Info = new SimplisityInfo();
            }
        }



        public SettingsData(int tabId, int moduleId, string langRequired, string entityTypeCode = "ROCKETSETTINGS", string listname = "settingsdata")
        {
            _entityTypeCode = entityTypeCode;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetEditCulture();
            _tabid = tabId;
            _moduleid = moduleId;
            _parentitemid = -1;
            _dataList = new List<SimplisityInfo>();            
            _listName = listname;

            if (_moduleid > 0)
            {
                Info = GetSettingData(_langRequired);
                _cultureList = GetCultureList();
                Populate();
                PopulateList();
            }
            else
            {
                Info = new SimplisityInfo();
            }
        }

        public List<string> GetCultureList()
        {
            var objCtrl = new DNNrocketController();

            if (Info == null)
            {
                Info = new SimplisityInfo();
                Info.ModuleId = _moduleid;
                Info.ParentItemId = _parentitemid;
            }

            // Load ALL language records, so we can update lists correctly
            var cultureList = new List<string>();
            if (Info.ParentItemId > 0)
            {
                var l = objCtrl.GetList(-1, -1, _entityTypeCode + "LANG", " and ParentItemId = " + Info.ItemID);
                foreach (var s in l)
                {
                    if (!cultureList.Contains(s.Lang)) cultureList.Add(s.Lang);
                }
            }

            var ls = DNNrocketUtils.GetCultureCodeList();
            foreach (var s in ls)
            {
                if (!cultureList.Contains(s)) cultureList.Add(s);
            }

            return cultureList;
        }

        public void Populate()
        {
            foreach (var cultureCode in _cultureList)
            {
                var s = GetSettingData(cultureCode);
                if (s != null)
                {
                    AddSimplisityInfo(s, cultureCode);
                }
            }
        }

        public void PopulateList()
        {
            _dataList = new List<SimplisityInfo>();
            var objCtrl = new DNNrocketController();
            if (!SimplisityInfoList.ContainsKey(_langRequired))
            {
                AddSimplisityInfo(new SimplisityInfo(_langRequired), _langRequired);  // new edit lang
            }
            var info = SimplisityInfoList[_langRequired];
            if (info != null)
            {
                _dataList = info.GetList(_listName);
            }
        }

        public void Delete()
        {
            var info = GetSettingData(DNNrocketUtils.GetEditCulture());
            if (info != null)
            {
                var objCtrl = new DNNrocketController();
                objCtrl.Delete(info.ItemID);
                ClearCache();
                Populate();
                PopulateList();
            }
        }

        public void Save(SimplisityInfo postInfo)
        {
            //remove any params
            postInfo.RemoveXmlNode("genxml/postform");
            postInfo.RemoveXmlNode("genxml/urlparams");

            var editlang = DNNrocketUtils.GetEditCulture();

            var dbInfo = GetSettingData(editlang);

            if (!SimplisityInfoList.ContainsKey("editlang"))
            {
                // new lang record, so add it to list
                AddSimplisityInfo(dbInfo, editlang);
            }

            RemovedDeletedListRecords(_listName, dbInfo, postInfo);

            // Update ALL langauge records.
            foreach (var listItem in SimplisityInfoList)
            {
                var saveInfo = (SimplisityInfo)postInfo.Clone();
                if (editlang != listItem.Value.Lang)
                {
                    // If it's not the same langauge, update the data with the listItem.
                    saveInfo.RemoveLangRecord();
                    saveInfo.SetLangRecord(listItem.Value.GetLangRecord());

                    // resequance the other language list, by rebuilding from sorted GetList.
                    var l = saveInfo.GetList(_listName);
                    saveInfo.RemoveList(_listName);
                    foreach (var s in l)
                    {
                        saveInfo.AddListRow(_listName, s);
                    }
                }

                SaveSettingData(saveInfo);
            }

            ClearCache();
            Populate();
            PopulateList();
        }


        public void AddRow()
        {
            AddListRow(_listName);

            // Update ALL langauge records.
            foreach (var listItem in SimplisityInfoList)
            {
                SaveSettingData(listItem.Value);
            }

            ClearCache();
            Populate();
            PopulateList();
        }

        public void ClearCache()
        {
            CacheUtils.ClearCache("rocketmod" + _moduleid);
        }


        public string ExportData(bool withTextData = false)
        {
            var xmlOut = "<root>";
            foreach (var listItem in SimplisityInfoList)
            {
                xmlOut += listItem.Value.ToXmlItem(withTextData);
            }
            xmlOut += "</root>";

            return xmlOut;
        }

        public void ImportData(string XmlIn)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XmlIn);

            var nodList = xmlDoc.SelectNodes("root/item");
            foreach (XmlNode nod in nodList)
            {
                var s = new SimplisityInfo();
                s.FromXmlItem(nod.OuterXml);
                AddSimplisityInfo(s, s.Lang);
                if (_langRequired == s.Lang)
                {
                    Info = s;
                    Info.ParentItemId = _parentitemid;
                }
            }
        }


        #region "properties"

        public int ModuleId { get {return _moduleid;} }
        public int TabId { get { return _tabid; } }
        public string EntityTypeCode { get { return _entityTypeCode; } set { _entityTypeCode = value; } }        

        public List<SimplisityInfo> List
        {
            get { return _dataList; }
        }

        #endregion

        #region "private methods"

        private SimplisityInfo GetSettingData(string cultureCode)
        {
            var guidkey = "moduleid" + _moduleid;
            if (_moduleid <= 0) guidkey = "parentitemid" + _parentitemid;
            var objCtrl = new DNNrocketController();
            return objCtrl.GetData(guidkey, _entityTypeCode, cultureCode, -1, _moduleid, false);
        }

        private void SaveSettingData(SimplisityInfo sInfo)
        {
            var guidkey = "moduleid" + _moduleid;
            if (_moduleid <= 0) guidkey = "parentitemid" + _parentitemid;
            var objCtrl = new DNNrocketController();
            sInfo.ParentItemId = _parentitemid;
            sInfo.ModuleId = _moduleid;
            objCtrl.SaveData(guidkey, _entityTypeCode, sInfo, -1, _moduleid);
        }

        #endregion


    }

}

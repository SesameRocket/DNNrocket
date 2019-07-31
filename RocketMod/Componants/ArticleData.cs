﻿using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketMod
{

    public class ArticleData
    {
        private string _langRequired;
        private const string _tableName = "DNNRocket";
        private const string _entityTypeCode = "ROCKETMOD";
        private DNNrocketController _objCtrl;

        public ArticleData(int itemId, string langRequired)
        {
            _objCtrl = new DNNrocketController();
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetEditCulture();
            if (itemId <= 0)
            {
                AddArticle();
            }
            else
            {
                Populate(itemId);
            }
        }

        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        public void Save(SimplisityInfo postInfo)
        {
            var dbInfo = _objCtrl.GetData( _entityTypeCode, Info.ItemID, _langRequired, -1,-1, true, _tableName);
            if (dbInfo != null)
            {
                dbInfo.XMLData = postInfo.XMLData;

                _objCtrl.SaveData(dbInfo, Info.ItemID, _tableName);

                // update all langauge record which are empty.
                var cc = DNNrocketUtils.GetCultureCodeList();
                foreach (var l in cc)
                {
                    var dbRecord = _objCtrl.GetRecordLang(Info.ItemID, l, false, _tableName);
                    var nodList = dbRecord.XMLDoc.SelectNodes("genxml/*");
                    if (nodList.Count == 0)
                    {
                        dbInfo = _objCtrl.GetData(_entityTypeCode, Info.ItemID, l, -1, -1, true, _tableName);
                        if (dbInfo != null)
                        {
                            dbInfo.XMLData = postInfo.XMLData;
                            _objCtrl.SaveData(dbInfo, Info.ItemID, _tableName);
                        }
                    }
                }
            }
        }

        private void AddArticle()
        {
            Info = _objCtrl.GetData(_entityTypeCode, -1, _langRequired, -1, -1, false, _tableName);    
        }

        public void Populate(int ItemId)
        {
            Info = _objCtrl.GetData(_entityTypeCode, ItemId, _langRequired, -1, -1, true, _tableName);
        }

        public string EntityTypeCode { get { return _entityTypeCode; } }

        public SimplisityInfo Info { get; private set; }

        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int ParentItemId { get { return Info.ParentItemId; } set { Info.ParentItemId = value; } }
        public int SystemId { get { return Info.SystemId; } set { Info.SystemId = value; } }
    }

}

﻿using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocketMod.Components
{

    public class ModuleParamList
    {
        private string _langRequired;
        private const string _tableName = "DNNRocket";
        private const string _entityTypeCode = "MODULEPARAMS";
        private DNNrocketController _objCtrl;
        private bool _useCache;
        private bool _sharedDataOnly;
        public ModuleParamList(string systemKey, string langRequired = "", bool sharedDataOnly = false, bool useCache = true)
        {
            _sharedDataOnly = sharedDataOnly;
            _useCache = useCache;
            _langRequired = langRequired;
            if (_langRequired == "") _langRequired = DNNrocketUtils.GetCurrentCulture();
            _objCtrl = new DNNrocketController();
            Populate(systemKey);
        }
        private void Populate(string systemKey)
        {
            DataList = new List<ModuleParams>();
            var searchFilter = " ";
            var searchOrderBy = "";
            //var searchOrderBy = " order by R1.[XMLData].value('(genxml/hidden/name)[1]', 'nvarchar(100)') ";
            var l = _objCtrl.GetList(-1, -1, _entityTypeCode, searchFilter, _langRequired, searchOrderBy, 0, 0, 0, 0, _tableName);
            foreach (var i in l)
            {
                var m = new ModuleParams(i.ModuleId, systemKey, _useCache, _tableName);
                var tabInfo = DNNrocketUtils.GetTabInfo(m.TabId);
                if (tabInfo != null)
                {
                    m.SetValue("tabname", tabInfo.TabName);
                }
                if (_sharedDataOnly)
                {
                    if ((m.ShareData == "1" || m.ShareData == "2" || m.ShareData == "")) DataList.Add(m);
                }
                else
                {
                    DataList.Add(m);
                }
            }
        }

        public List<ModuleParams> DataList { get; private set; }
     
    }

}

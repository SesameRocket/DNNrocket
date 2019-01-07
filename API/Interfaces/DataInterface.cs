﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Interfaces
{

    public abstract class DNNrocketCtrlInterface
    {
        public abstract int GetListCount(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "");
        public abstract List<SimplisityInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0);
        public abstract SimplisityInfo GetInfo(int itemId, string lang);
        public abstract SimplisityRecord GetRecord(int itemId);
        public abstract int Update(SimplisityRecord objInfo);
        public abstract void Delete(int itemId);
        public abstract void CleanData();
    }
}

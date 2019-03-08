﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Componants
{
    public class SideMenu
    {
        private SimplisityInfo _sysInfo;

        public string SystemKey { get; set; }
        public int SystemId { get; set; }
        public int ModuleId { get; set; }

        public SideMenu(SimplisityInfo sysInfo)
        {
            _sysInfo = sysInfo;
            SystemKey = sysInfo.GetXmlProperty("genxml/textbox/ctrlkey");
        }

        public List<SimplisityRecord> GetGroups()
        {
            var rtnList = new List<SimplisityRecord>();

            foreach (var i in _sysInfo.GetList("groupsdata"))
            {
                // [TODO: add security]
                rtnList.Add(i);
            }

            return rtnList;
        }
        public List<SimplisityRecord> GetInterfaces(string groupref)
        {
            var rtnList = new List<SimplisityRecord>();

            foreach (var i in _sysInfo.GetList("interfacedata"))
            {
                // [TODO: add security]
                if (groupref == i.GetXmlProperty("genxml/dropdownlist/group"))
                {
                    rtnList.Add(i);
                }
            }

            return rtnList;
        }

        public List<SimplisityRecord> GetMenuOnUserSecurity()
        {
            var roles = UserUtils.GetCurrentUserRoles();
            var rtnList = new List<SimplisityRecord>();
            foreach (var i in _sysInfo.GetList("interfacedata"))
            {


            }

            return rtnList;
        }


    }
}

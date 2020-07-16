﻿using DNNrocket.Country.Componants;
using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Newtonsoft.Json;
using Simplisity;
using System;
using System.Collections.Generic;

namespace DNNrocket.Country
{
    public class StartConnect : APInterface
    {
        private SimplisityInfo _systemInfo;
        private RocketInterface _rocketInterface;
        private Dictionary<string,string> _passSettings; 
        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _rocketInterface = new RocketInterface(interfaceInfo);
            var commandSecurity = new CommandSecurity(-1,-1, _rocketInterface);
            commandSecurity.AddCommand("settingcountry_save", true);
            commandSecurity.AddCommand("settingcountry_get", true);
            commandSecurity.AddCommand("settingcountry_getregion", false);

            //CacheUtilsDNN.ClearAllCache();
            _systemInfo = systemInfo;

            var rtnDic = new Dictionary<string, object>();

            if (commandSecurity.HasSecurityAccess(paramCmd))
            {
                _passSettings = paramInfo.ToDictionary();
                switch (paramCmd)
                {
                    case "settingcountry_save":
                        CountryUtils.CountrySave(postInfo, _rocketInterface);
                        rtnDic.Add("outputhtml", CountryUtils.CountryDetail(_rocketInterface,true));
                        break;
                    case "settingcountry_get":
                        rtnDic.Add("outputhtml", CountryUtils.CountryDetail(_rocketInterface,false));
                        break;
                    case "settingcountry_getregion":
                        rtnDic.Add("outputhtml", "");
                        var regionlist = CountryUtils.RegionListJson(paramInfo.GetXmlProperty("genxml/hidden/activevalue"), paramInfo.GetXmlPropertyBool("genxml/hidden/allowempty"));
                        rtnDic.Add("outputjson", regionlist);
                        break;
                    case "settingcountry_selectculturecode":
                        rtnDic.Add("outputhtml", CountryUtils.CultureSelect(postInfo, langRequired, _rocketInterface));
                        break;
                }
            }
            switch (paramCmd)
            {
                case "settingcountry_changeculture":
                    // this can also be activated from the root "action" API, "changeculture" cmd.
                    DNNrocketUtils.SetCookieValue("language", paramInfo.GetXmlProperty("genxml/hidden/culturecode"));
                    rtnDic.Add("outputhtml", paramInfo.GetXmlProperty("genxml/hidden/culturecode"));
                    break;
            }
            return rtnDic;
        }



    }
}

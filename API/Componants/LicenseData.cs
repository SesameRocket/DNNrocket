﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Componants
{
    public class LicenseData
    {
        private string _encrypteddomain;
        private DNNrocketController _objCtrl;
        public LicenseData(string licenseKey, string domain)
        {
            LicenseKey = licenseKey;
            DomainUrl = domain;
            LoadLicense();
            if (!IsLicensed)
            {
                if (GeneralUtils.IsDate(Record.GetXmlPropertyRaw("genxml/checkdate"),"en-US") &&  Convert.ToDateTime(Record.GetXmlPropertyRaw("genxml/checkdate")) < DateTime.Now.Date)
                {
                    CheckOnlIneLicense();
                }
                _encrypteddomain = DNNrocketUtils.Encrypt(DomainUrl, LicenseKey);
                if (_encrypteddomain == Record.GetXmlProperty("genxml/encryptedkey"))
                {
                    IsLicensed = true;
                }
            }
        }

        public bool IsLicensed { get; private set; }
        public string LicenseMessage { get; private set; }
        public SimplisityRecord Record { get; set; }
        public string LicenseKey { get; private set; }
        public string DomainUrl { get; private set; }
        public DateTime ExpireDate { get; private set; }

        private void CheckOnlIneLicense()
        {
            //var uri = "https://license.rocket-templates.ovh/";
            //WebClient client = new WebClient();
            //var rtnString = client.DownloadString(uri);
            //return Convert.ToBoolean(rtnString);


            IsLicensed = true;

        }
        private void LoadLicense()
        {
            if (LicenseKey != "")
            {
                Record = (SimplisityRecord)CacheUtils.GetCache(LicenseKey);
                if (Record == null)
                {
                    Record = _objCtrl.GetRecordByType(DNNrocketUtils.GetPortalId(), -1, "LICENSE");
                    if (Record == null)
                    {
                        Record = new SimplisityRecord();
                        Record.ItemID = -1;
                        Record.PortalId = DNNrocketUtils.GetPortalId();
                        Record.TypeCode = "LICENSE";

                        Record.SetXmlProperty("genxml/license", LicenseKey);
                        Record.SetXmlProperty("genxml/domain", DomainUrl);
                        Record.SetXmlProperty("genxml/encryptedkey", "FREE");
                        Record.SetXmlProperty("genxml/fail", "0");
                        ExpireDate = DateTime.Now.Date.AddDays(-1);
                        Record.SetXmlProperty("genxml/expiredate", ExpireDate.ToString("O"), TypeCode.DateTime);
                        var checkdate = DateTime.Now.Date.AddDays(-1);
                        Record.SetXmlProperty("genxml/checkdate", checkdate.ToString("O"), TypeCode.DateTime);
                        Record.SetXmlProperty("genxml/validate", "30");

                        Record.ItemID = _objCtrl.Update(Record);

                        LicenseMessage = "FREE - Community Version";
                    }
                    CacheUtils.SetCache(LicenseKey, Record);
                }
            }
            else
            {
                IsLicensed = false;
            }
        }

    }
}

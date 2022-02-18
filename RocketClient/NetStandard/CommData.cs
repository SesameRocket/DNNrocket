﻿using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketComm
{
    /// <summary>
    /// Data class to keep returned data from RocketCDS.
    /// </summary>
    public class CommData
    {
        public CommData()
        {
            StatusCode = "";
            ErrorMsg = "";
            FirstHeader = "";
            LastHeader = "";
            SeoHeaderXml = "";
            ViewHtml = "";
            JsonReturn = "{\"\":\"\"}";
            CacheFlag = false;
            SettingsXml = "";
        }

        public MetaSEO SeoHeader()
        {
            var metaSEO = new MetaSEO();
            try
            {
                var sRec = new SimplisityRecord();
                sRec.FromXmlItem(SeoHeaderXml);
                metaSEO.Title = GeneralUtils.CleanInput(sRec.GetXmlProperty("genxml/title"));
                metaSEO.Description = sRec.GetXmlProperty("genxml/description");
                metaSEO.KeyWords = sRec.GetXmlProperty("genxml/keywords");
            }
            catch (Exception)
            {
                // ignore
            }
            return metaSEO;
        }
        public SimplisityInfo SettingsInfo()
        {
            var sRec = new SimplisityInfo();
            try
            {
                sRec.FromXmlItem(SettingsXml);
            }
            catch (Exception)
            {
                // ignore
            }
            return sRec;
        }

        public string StatusCode { set; get; }
        public string ErrorMsg { set; get; }
        public string FirstHeader { set; get; }
        public string LastHeader { set; get; }
        public string SeoHeaderXml { set; get; }
        public string ViewHtml { set; get; }
        public string JsonReturn { set; get; }
        public string SettingsXml { set; get; }
        public bool CacheFlag { set; get; }

    }
}

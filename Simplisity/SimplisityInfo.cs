﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Simplisity
{

    public class SimplisityInfo : SimplisityRecord, ICloneable 
    {
        public object CloneInfo()
        {
            var obj = (SimplisityInfo)this.MemberwiseClone();
            obj.XMLData = this.XMLData;
            return obj;
        }

        public SimplisityInfo()
        {
            if (XMLDoc == null) XMLData = "<genxml></genxml>"; // if we don;t have anything, create an empty default to stop errors.
        }

        public SimplisityInfo(string lang)
        {
            this.Lang = lang;
            if (XMLDoc == null) XMLData = "<genxml></genxml>"; // if we don;t have anything, create an empty default to stop errors.
        }

        public SimplisityInfo(SimplisityRecord simplisityRecord)
        {
            this.ItemID = simplisityRecord.ItemID;
            this.PortalId = simplisityRecord.PortalId;
            this.ModuleId = simplisityRecord.ModuleId;
            this.TypeCode = simplisityRecord.TypeCode;
            this.GUIDKey = simplisityRecord.GUIDKey;
            this.ModifiedDate = simplisityRecord.ModifiedDate;
            this.TextData = simplisityRecord.TextData;
            this.XrefItemId = simplisityRecord.XrefItemId;
            this.ParentItemId = simplisityRecord.ParentItemId;
            this.XMLData = simplisityRecord.XMLData;
            this.Lang = simplisityRecord.Lang;
            this.UserId = simplisityRecord.UserId;
            this.RowCount = simplisityRecord.RowCount;
            this.EncodingKey = simplisityRecord.EncodingKey;
            this.SortOrder = simplisityRecord.SortOrder;
        }


        public SimplisityRecord GetLangRecord()
        {
            var rtn = (SimplisityRecord)base.Clone();
            rtn.XMLData = GetXmlNode("genxml/lang");
            if (rtn.XMLData == "") rtn.XMLData = "<genxml/>";
            return rtn;
        }

        public string GetLangXml()
        {
            var rtn = GetXmlNode("genxml/lang");
            if (rtn == "") rtn = "<genxml/>";
            return rtn;
        }

        public void SetLangRecord(SimplisityRecord sRecord)
        {
            SetLangXml(sRecord.XMLData);
            if (sRecord.Lang != "")
            {
                base.Lang = sRecord.Lang;
            }
        }

        public void SetLangXml(string strXml)
        {
            if (XMLDoc.SelectSingleNode("genxml/lang") == null)
            {
                SetXmlProperty("genxml/lang", "", System.TypeCode.String, false);
            }
            AddXmlNode(strXml, "genxml", "genxml/lang");
        }

        public void RemoveLangRecord()
        {
            RemoveXmlNode("genxml/lang");
        }

        #region "Lists"

        public List<string> GetLists()
        {
            var rtnList = new List<string>();

            if (XMLDoc != null)
            {
                var lp = 1;
                var listNames = XMLDoc.SelectNodes("genxml/*[@list]");
                foreach (XmlNode i in listNames)
                {
                    rtnList.Add(i.Name);
                    lp += 1;
                }
            }
            return rtnList;
        }


        public List<SimplisityInfo> GetList(string listName)
        {
            var rtnList = new List<SimplisityInfo>();

            if (XMLDoc != null)
            {
                var lp = 1;
                var listRecords = XMLDoc.SelectNodes("genxml/" + listName + "/*");
                if (listRecords != null)
                {
                    foreach (XmlNode i in listRecords)
                    {
                        var nbi = new SimplisityInfo();
                        nbi.XMLData = i.OuterXml;
                        nbi.TypeCode = "LIST";
                        nbi.GUIDKey = listName;

                        var listXmlNode = "";
                        var listitemref = nbi.GetXmlProperty("genxml/hidden/simplisity-listitemref");
                        if (listitemref == "")
                        {
                            listXmlNode = GetXmlNode("genxml/lang/genxml/" + listName + "/genxml[" + lp + "]");
                        }
                        else
                        {
                            listXmlNode = GetXmlNode("genxml/lang/genxml/" + listName + "/genxml[hidden/simplisity-listitemreflang='" + listitemref + "']");
                        }

                        nbi.SetLangXml("<genxml>" + listXmlNode + "</genxml>");
                        rtnList.Add(nbi);
                        lp += 1;
                    }
                }
            }
            return rtnList;
        }

        public void RemoveList(string listName)
        {
            if (XMLDoc != null)
            {
                RemoveXmlNode("genxml/" + listName);
                RemoveXmlNode("genxml/lang/genxml/" + listName);
            }
        }

        public void RemoveListItem(string listName, int index)
        {
            if (XMLDoc != null && index > 0)
            {
                RemoveXmlNode("genxml/" + listName + "/genxml[" + index + "]");
                RemoveXmlNode("genxml/lang/genxml/" + listName + "/genxml[" + index + "]");
            }
        }

        public SimplisityInfo GetListItem(string listName, int index)
        {
            if (XMLDoc != null)
            {
                var list = GetList(listName);
                if (index > (list.Count - 1)) return new SimplisityInfo();
                return list[index];
            }
            return null;
        }

        public SimplisityInfo GetListItem(string listName, string itemkeyxpath, string itemkey)
        {
            if (XMLDoc != null)
            {

                var list = GetList(listName);
                foreach (var i in list)
                {
                    if (itemkey == i.GetXmlProperty(itemkeyxpath))
                    {
                        return i;
                    }
                }
            }
            return null;
        }

        public void AddListItem(string listName, SimplisityInfo sInfo)
        {
            if (XMLDoc != null)
            {
                var xmllangdata = sInfo.GetLangXml();
                sInfo.RemoveLangRecord();
                var xmldata = sInfo.XMLData;

                AddListItem(listName, xmldata, xmllangdata);

            }
        }

        public void AddListItem(string listName, string xmldata = "<genxml></genxml>", string xmllangdata = "<genxml></genxml>")
        {
            if (XMLDoc != null)
            {
                // get listcount, so we can add a sort value
                var l = GetList(listName);
                var sortcount = l.Count + 1;

                if (XMLDoc.SelectSingleNode("genxml/" + listName) == null)
                {
                    SetXmlProperty("genxml/" + listName, "", System.TypeCode.String, false);
                }

                AddXmlNode(xmldata, "genxml", "genxml/" + listName);

                SetXmlProperty("genxml/" + listName + "/genxml[last()]/index", sortcount.ToString(), System.TypeCode.String, false);

                if (XMLDoc.SelectSingleNode("genxml/lang") == null)
                {
                    SetXmlProperty("genxml/lang", "", System.TypeCode.String, false);
                    SetXmlProperty("genxml/lang/genxml", "", System.TypeCode.String, false);
                }
                if (XMLDoc.SelectSingleNode("genxml/lang/genxml/" + listName) == null)
                {
                    SetXmlProperty("genxml/lang/genxml/" + listName, "", System.TypeCode.String, false);
                }

                AddXmlNode(xmllangdata, "genxml", "genxml/lang/genxml/" + listName);

            }
        }

        /// <summary>
        /// Get Dictionary of all values on XML. 
        /// For both Neutral and Language data.  
        /// Excludes Lists.  
        /// The nodes on the 3rd level will be returned "genxml/mynode/*"
        /// </summary>
        /// <returns></returns>
        public new Dictionary<string, string> ToDictionary()
        {
            var temp = new SimplisityInfo();
            temp.XMLData = XMLData;
            var tempRec = new SimplisityRecord(temp);
            var rtnDictionary = tempRec.ToDictionary();
            var langRecord = GetLangRecord();
            foreach (var d in langRecord.ToDictionary())
            {
                if (!rtnDictionary.ContainsKey(d.Key)) rtnDictionary.Add(d.Key, d.Value);
            }
            return rtnDictionary;
        }


        #endregion 

    }

}

﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Simplisity
{

    public class SimplisityInfo : SimplisityRecord
    {

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
            this.SystemId = simplisityRecord.SystemId;
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
                foreach (XmlNode i in listRecords)
                {
                    var nbi = new SimplisityInfo();
                    nbi.XMLData = i.OuterXml;
                    nbi.TypeCode = "LIST";
                    nbi.GUIDKey = listName;
                    nbi.SetLangXml("<genxml>" + GetXmlNode("genxml/lang/genxml/" + listName + "/genxml[" + lp + "]") + "</genxml>");
                    rtnList.Add(nbi);
                    lp += 1;
                }
            }
            return rtnList;
        }

        public SimplisityInfo GetListItem(string listName, int index)
        {
            if (XMLDoc != null)
            {

                var list = GetList(listName);
                var lp = 0;
                foreach (var i in list)
                {
                    if (lp == index)
                    {
                        return i;
                    }
                    lp += 1;
                }
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

        public int GetListItemIndex(string listName, string itemkeyxpath, string itemkey)
        {
            var lp = 1;
            if (XMLDoc != null)
            {
                var list = GetList(listName);
                foreach (var i in list)
                {
                    if (itemkey == i.GetXmlProperty(itemkeyxpath))
                    {
                        return lp;
                    }
                    lp += 1;
                }
            }
            return lp;
        }


        public void AddListRow(string listName, SimplisityInfo sInfo)
        {
            if (XMLDoc != null)
            {
                var xmllangdata = sInfo.GetLangXml();
                sInfo.RemoveLangRecord();
                var xmldata = sInfo.XMLData;

                AddListRow(listName, xmldata);

            }
        }

        public void AddListRow(string listName, string xmldata = "<genxml></genxml>")
        {
            if (XMLDoc != null)
            {
                if (XMLDoc.SelectSingleNode("genxml/" + listName) == null)
                {
                    SetXmlProperty("genxml/" + listName, "", System.TypeCode.String, false);
                }

                AddXmlNode(xmldata, "genxml", "genxml/" + listName);

                if (XMLDoc.SelectSingleNode("genxml/lang") == null)
                {
                    SetXmlProperty("genxml/lang", "", System.TypeCode.String, false);
                }
                if (XMLDoc.SelectSingleNode("genxml/lang/genxml/" + listName) == null)
                {
                    SetXmlProperty("genxml/lang/genxml/" + listName, "", System.TypeCode.String, false);
                }
                AddXmlNode("<genxml></genxml>", "genxml", "genxml/lang/genxml/" + listName);

            }
        }

        public void RemoveListRow(string listName, int index)
        {
            if (XMLDoc != null)
            {
                RemoveXmlNode("genxml/" + listName + "/genxml[" + index + "]");
                RemoveXmlNode("genxml/lang/genxml/" + listName + "/genxml[" + index + "]");
            }
        }

        public void RemoveListRowByKey(string listName, string recordKey)
        {
            if (XMLDoc != null)
            {
                var index = GetListItemIndex(listName, "genxml/recordkey", recordKey);
                RemoveListRow(listName, index);
            }
        }


    }

}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace Simplisity
{
    public static class CtrlTypes
    {
        public const String checkbox = "checkbox";
        public const String textbox = "text";
        public const String radio = "radio";
        public const String select = "select";
        public const String hidden = "hidden";
    }


    public class SimplisityJson
    {

        public static SimplisityInfo GetSimplisityInfoFromJson(string requestJson, string editlang)
        {
            requestJson = "{'?xml': {'@version': '1.0','@standalone': 'no'},'root' :" + requestJson + "}";
            XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(requestJson);

            // get fields into SimplsityInfo
            var sInfo = new SimplisityInfo();
            sInfo.ItemID = -1;
            sInfo.TypeCode = "JSONCONVERT";
            sInfo.XMLData = doc.OuterXml;


            // -------------------------------------------------------------
            // -------------- OUTPUT TEST DATA -----------------------------
            // -------------------------------------------------------------

            //var debugOutMapPath = (string)CacheUtils.GetCache("debugOutMapPath");
            //if (!String.IsNullOrEmpty(debugOutMapPath))
            //{
            //    FileUtils.SaveFile(debugOutMapPath + @"\jasonxmlpost.xml", sInfo.XMLData);
            //}

            // -------------------------------------------------------------
            // -------------- OUTPUT TEST DATA -----------------------------
            // -------------------------------------------------------------




            var rtnInfo = ConvertJsonToSimplisityInfo(sInfo, editlang,"postdata",false);
            var langInfo = ConvertJsonToSimplisityInfo(sInfo, editlang, "postdata", true);


            // ------------------- LISTDATA ------------------------------------
            var listNames = new List<string>();
            var listall = GetListXml(sInfo);
            foreach (var l in listall)
            {
                var listname = l.GetXmlProperty("listdata/listname");
                if (!listNames.Contains(listname))
                {
                    if (listname != "")
                    {
                        listNames.Add(listname);
                    }
                }
            }
            foreach (var lnameselector in listNames)
            {
                var listname = lnameselector.Replace(".", "");
                // --------- Stadard Data
                rtnInfo.SetXmlProperty("genxml/" + listname, "", TypeCode.String, false);
                rtnInfo.SetXmlProperty("genxml/" + listname + "/@list", "true", TypeCode.String, false);
                var listInfoList = ConvertJsonToSimplisityInfoList(sInfo, editlang, lnameselector,false);
                foreach (var listInfo in listInfoList)
                {
                    rtnInfo.AddXmlNode(listInfo.XMLData, "genxml", "genxml/" + listname);
                }
                // --------- Localized data
                langInfo.SetXmlProperty("genxml/" + listname, "", TypeCode.String, false);
                langInfo.SetXmlProperty("genxml/" + listname + "/@list", "true", TypeCode.String, false);
                listInfoList = ConvertJsonToSimplisityInfoList(sInfo, editlang, lnameselector, true);
                foreach (var listInfo in listInfoList)
                {
                    langInfo.AddXmlNode(listInfo.XMLData, "genxml", "genxml/" + listname);
                }

            }

            // merge localized data into SimplisityInfo
            rtnInfo.SetXmlProperty("genxml/lang", "", TypeCode.String, false);
            rtnInfo.AddXmlNode(langInfo.XMLData, "genxml", "genxml/lang");


            // -------------------------------------------------------------
            // -------------- OUTPUT TEST DATA -----------------------------
            // -------------------------------------------------------------

            //if (!String.IsNullOrEmpty(debugOutMapPath))
            //{
            //    FileUtils.SaveFile(debugOutMapPath + @"\xmlout.xml", rtnInfo.XMLData);
            //}

            // -------------------------------------------------------------
            // -------------- OUTPUT TEST DATA -----------------------------
            // -------------------------------------------------------------



            return rtnInfo;
        }

        private static SimplisityInfo ConvertJsonToSimplisityInfo(SimplisityInfo requestJsonXml, string editlang, string dataroot, bool lang)
        {
            var xmlOut = new SimplisityInfo();
            xmlOut.Lang = editlang;
            if (!lang) // only save s-fields in standard data.
            {
                // ------------------- S-FIELDS ------------------------------------
                var sfieldList = requestJsonXml.XMLDoc.SelectNodes("root/sfield/*");
                foreach (XmlNode nod in sfieldList)
                {
                    var xpath = "genxml/hidden/" + nod.Name;
                    xmlOut.SetXmlProperty(xpath, GeneralUtils.DeCode(nod.InnerText));
                }
            }

            // ------------------- POSTDATA ------------------------------------
            // Merge postdata fields into xmlOut
            var postList = GetPostXml(requestJsonXml, CtrlTypes.hidden, dataroot);
            AddToSimplisty(ref xmlOut, requestJsonXml, postList, dataroot, lang);

            postList = GetPostXml(requestJsonXml, CtrlTypes.textbox, dataroot);
            AddToSimplisty(ref xmlOut, requestJsonXml, postList, dataroot, lang);

            postList = GetPostXml(requestJsonXml, CtrlTypes.select, dataroot);
            AddToSimplisty(ref xmlOut, requestJsonXml, postList, dataroot, lang);

            postList = GetPostXml(requestJsonXml, CtrlTypes.checkbox, dataroot);
            AddCheckBoxToSimplisty(ref xmlOut, requestJsonXml, postList, dataroot, lang);

            postList = GetPostXml(requestJsonXml, CtrlTypes.radio, dataroot);
            AddToSimplisty(ref xmlOut, requestJsonXml, postList, dataroot, lang);

            return xmlOut;
        }

        private static List<SimplisityInfo> ConvertJsonToSimplisityInfoList(SimplisityInfo requestJsonXml, string editlang, string lnameselector, bool lang)
        {
            var rtnList = new List<SimplisityInfo>();

            var row = 1;
            var listrow = GetListXml(requestJsonXml, lnameselector, row);
            while (listrow.Count() > 0)
            {
                var rowXml = "<root>";
                foreach (var rowInfo in listrow)
                {
                    rowXml += rowInfo.XMLData;
                }
                rowXml += "</root>";

                var rowInfojoined = new SimplisityInfo();
                rowInfojoined.XMLData = rowXml;
                rtnList.Add(ConvertJsonToSimplisityInfo(rowInfojoined, editlang, "listdata", lang));
                row += 1;
                listrow = GetListXml(requestJsonXml, lnameselector,row);
            }

            return rtnList;
        }


        private static List<SimplisityInfo> GetPostXml(SimplisityInfo JsonConvertToXml, string ctrltype, string dataroot = "postdata")
        {
            // get postdata list
            var postdataList = new List<SimplisityInfo>();
            var nodList = JsonConvertToXml.XMLDoc.SelectNodes("root/" + dataroot + "[type='" + ctrltype + "']");
            foreach (XmlNode nod in nodList)
            {
                var sI = new SimplisityInfo();
                sI.ItemID = -1;
                sI.TypeCode = "POSTFIELDS";
                sI.XMLData = nod.OuterXml;
                postdataList.Add(sI);
            }
            return postdataList;
        }

        private static List<SimplisityInfo> GetListXml(SimplisityInfo JsonConvertToXml, string listname = "",int row = 0)
        {
            // get listdata list
            var postdataList = new List<SimplisityInfo>();
            var xpath = "root/listdata";
            if (row > 0)
            {
                xpath = "root/listdata[row='" + row + "' and listname='" + listname + "']";
            }
            var nodList = JsonConvertToXml.XMLDoc.SelectNodes(xpath);

            foreach (XmlNode nod in nodList)
            {
                var sI = new SimplisityInfo();
                sI.ItemID = -1;
                sI.TypeCode = "LISTFIELDS";
                sI.XMLData = nod.OuterXml;
                postdataList.Add(sI);
            }
            return postdataList;
        }

        private static List<SimplisityInfo> GetSFieldXml(SimplisityInfo JsonConvertToXml)
        {
            // get listdata list
            var postdataList = new List<SimplisityInfo>();
            var nodList = JsonConvertToXml.XMLDoc.SelectNodes("root/sfield/*");
            foreach (XmlNode nod in nodList)
            {
                var sI = new SimplisityInfo();
                sI.ItemID = -1;
                sI.TypeCode = "SFIELDS";
                sI.XMLData = nod.OuterXml;
                postdataList.Add(sI);
            }
            return postdataList;
        }


        private static void AddToSimplisty(ref SimplisityInfo xmlOut, SimplisityInfo sInfo, List<SimplisityInfo> ctrllist, string dataroot, bool lang)
        {
            foreach (var smi in ctrllist)
            {
                var supdate = smi.GetXmlProperty(dataroot + "/s-update");
                var xpath = smi.GetXmlProperty(dataroot + "/s-xpath");
                var type = smi.GetXmlProperty(dataroot + "/type");
                var checkfield = smi.GetXmlPropertyBool(dataroot + "/checked");

                var processControl = true;
                if (type.ToLower() == "radio")
                {
                    if (!checkfield) processControl = false;
                }

                if (processControl)
                {
                    if (xpath == "")
                    {
                        // try and build the xpath.
                        var id = smi.GetXmlProperty(dataroot + "/id");
                        xpath = "genxml/" + type + "/" + id;
                    }

                    if (xpath != "")
                    {
                        if (xpath.StartsWith("genxml/lang"))
                        {
                            supdate = "lang";
                            xpath = xpath.Substring(12);
                        }

                        var addNode = true;
                        if (lang)
                        {
                            if (supdate != "lang") addNode = false;
                        }
                        else
                        {
                            if (supdate == "lang") addNode = false;
                        }

                        if (addNode)
                        {

                            var val = smi.GetXmlProperty(dataroot + "/value");
                            switch (smi.GetXmlProperty(dataroot + "/s-datatype").ToLower())
                            {
                                case "date":
                                    xmlOut.SetXmlProperty(xpath, val, TypeCode.DateTime);
                                    break;
                                case "double":
                                    xmlOut.SetXmlProperty(xpath, val, TypeCode.Double);
                                    break;
                                case "coded":
                                    xmlOut.SetXmlProperty(xpath, GeneralUtils.DeCode(val));
                                    break;
                                default:
                                    xmlOut.SetXmlProperty(xpath, val);
                                    break;
                            }
                        }
                    }
                }
            }

        }


        private static void AddCheckBoxToSimplisty(ref SimplisityInfo xmlOut, SimplisityInfo sInfo, List<SimplisityInfo> ctrllist, string dataroot, bool lang)
        {
            var doneCheckboxes = new List<string>();
            foreach (var smi in ctrllist)
            {

                var supdate = smi.GetXmlProperty(dataroot + "/s-update");
                var xpath = smi.GetXmlProperty(dataroot + "/s-xpath");
                var type = smi.GetXmlProperty(dataroot + "/type");
                var checkfield = smi.GetXmlPropertyBool(dataroot + "/checked");

                if (xpath.StartsWith("genxml/lang"))
                {
                    supdate = "lang";
                    xpath = xpath.Substring(12);
                }

                var addNode = true;
                if (lang)
                {
                    if (supdate != "lang") addNode = false;
                }
                else
                {
                    if (supdate == "lang") addNode = false;
                }

                if (addNode)
                {
                    var ctrlname = smi.GetXmlProperty(dataroot + "/name");
                    if (ctrlname == "")
                    {
                        var val = "false";
                        if (checkfield) val = "true";
                        xmlOut.SetXmlProperty(xpath, val);
                    }
                    else
                    {
                        // checkbox list, select siblings.
                        if (!doneCheckboxes.Contains(xpath))
                        {
                            xmlOut.SetXmlProperty(xpath, "");
                            var nodList = sInfo.XMLDoc.SelectNodes("root/" + dataroot + "[s-xpath='" + xpath + "']");
                            foreach (XmlNode nod in nodList)
                            {
                                var checkedvalue = nod.SelectSingleNode("checked").InnerText;
                                if (checkedvalue == "") checkedvalue = "false";
                                xmlOut.AddXmlNode("<chk value='" + checkedvalue + "' data='" + nod.SelectSingleNode("value").InnerText + "' />", "chk", xpath);
                            }
                            doneCheckboxes.Add(xpath);
                        }
                    }
                }
            }
        }


    }
}

﻿using DNNrocket.Login;
using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Xml;

namespace DNNrocket.SystemData
{
    public class startconnect : DNNrocketAPI.APInterface
    {

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo,string userHostAddress, string editlang = "")
        {

            //CacheUtils.ClearAllCache();

            var controlRelPath = "/DesktopModules/DNNrocket/SystemData";

            var strOut = "ERROR!! - No Security rights or function command.  Ensure your systemprovider is defined. [SystemData]";

            var rtnInfo = new SimplisityInfo();
            // Security Check MUST be in the extension.
            if (DNNrocketUtils.SecurityCheckCurrentUser(systemInfo, "")) // only check superuser for this.
            {
                if (paramCmd == "login_signout")
                {
                    DNNrocketUtils.SignUserOut();
                    strOut = LoginUtils.LoginForm(rtnInfo);
                }
                else
                {
                    switch (paramCmd)
                    {
                        case "systemapi_admin_getsystemlist":
                            strOut = SystemAdminList(postInfo, controlRelPath);
                            break;
                        case "systemapi_admin_getdetail":
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_adminaddnew":
                            strOut = SystemAddNew(postInfo, controlRelPath);
                            break;
                        case "systemapi_addinterface":
                            SystemAddListRow(postInfo, "interfacedata");
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_admin_save":
                            SystemSave(postInfo);
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_admin_delete":
                            SystemDelete(postInfo);
                            strOut = SystemAdminList(postInfo, controlRelPath);
                            break;
                        case "systemapi_addparam":
                            SystemAddListRow(postInfo, "idxfielddata");
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_addsetting":
                            SystemAddListRow(postInfo, "settingsdata");
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_addgroup":
                            SystemAddListRow(postInfo, "groupsdata");
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_addprovtype":
                            SystemAddListRow(postInfo, "provtypesdata");
                            strOut = SystemAdminDetail(postInfo, controlRelPath);
                            break;
                        case "systemapi_rebuildindex":
                            RebuildIndex(postInfo, false);
                            strOut = "<h1>Rebuilding Index</h1>";
                            break;
                        case "systemapi_deleterebuildindex":
                            RebuildIndex(postInfo, true);
                            strOut = "<h1>Deleting and Rebuilding Index</h1>";
                            break;
                        case "systemapi_copyinterface":
                            strOut = CopyInterface(postInfo, controlRelPath);
                            break;
                        case "systemapi_clearallcache":
                            CacheUtils.ClearAllCache();
                            strOut = SystemAdminList(postInfo, controlRelPath);
                            break;
                    }
                }
            }
            else
            {

                switch (paramCmd)
                {
                    case "login_login":
                        strOut = LoginUtils.DoLogin(postInfo, userHostAddress);
                        break;
                    case "login_sendreset":
                        //strOut = ResetPass(sInfo);
                        break;
                    default:
                        strOut = LoginUtils.LoginForm(rtnInfo);
                        break;
                }
            }
            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("outputhtml", strOut);
            return rtnDic;            
        }




        public static string SystemAdminList(SimplisityInfo sInfo, string templateControlRelPath)
        {
            try
            {
                var systemData = new DNNrocketAPI.SystemData();
                var list = systemData.GetSystemList();
                return RenderSystemAdminList(list, sInfo, 0, templateControlRelPath);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static String SystemAdminDetail(SimplisityInfo sInfo, string templateControlRelPath)
        {
            try
            {
                var strOut = "Invalid ItemId";
                var selecteditemid = sInfo.GetXmlPropertyInt("genxml/hidden/selecteditemid");
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");

                if (selecteditemid > 0)
                {
                    var passSettings = sInfo.ToDictionary();


                    var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());

                    var objCtrl = new DNNrocketController();
                    var info = objCtrl.GetInfo(selecteditemid);

                    strOut = DNNrocketUtils.RazorDetail(razorTempl, info, passSettings);
                }


                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static String RenderSystemAdminList(List<SimplisityInfo> list, SimplisityInfo sInfo, int recordCount, string templateControlRelPath)
        {

            try
            {
                if (list == null) return "";
                var strOut = "";

                // select a specific entity data type for the product (used by plugins)
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                if (themeFolder == "") themeFolder = "config-w3";
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");

                var passSettings = sInfo.ToDictionary();

                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());

                strOut = DNNrocketUtils.RazorList(razorTempl, list, passSettings);

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        public static String SystemAddNew(SimplisityInfo sInfo, string templateControlRelPath)
        {
            try
            {
                var strOut = "";
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                if (themeFolder == "") themeFolder = "config-w3";
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");

                var passSettings = sInfo.ToDictionary();

                var info = new SimplisityInfo();
                info.ItemID = -1;
                info.PortalId = 99999;
                info.Lang = DNNrocketUtils.GetCurrentCulture();
                info.SetXmlProperty("genxml/hidden/index", "99");
                info.TypeCode = "SYSTEM";
                info.GUIDKey = GeneralUtils.GetUniqueKey(12);
                var objCtrl = new DNNrocketController();
                info.ItemID = objCtrl.Update(info);

                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());

                strOut = DNNrocketUtils.RazorDetail(razorTempl, info, passSettings);

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static void SystemAddListRow(SimplisityInfo sInfo, string listname)
        {
            try
            {
                var selecteditemid = sInfo.GetXmlProperty("genxml/hidden/selecteditemid");
                if (GeneralUtils.IsNumeric(selecteditemid))
                {
                    var objCtrl = new DNNrocketController();
                    var info = objCtrl.GetInfo(Convert.ToInt32(selecteditemid));
                    info.AddListRow(listname);
                    objCtrl.Update(info);
                }
            }
            catch (Exception ex)
            {
                // ignore
            }
        }


        public static void SystemSave(SimplisityInfo postInfo)
        {
            var selecteditemid = postInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            if (GeneralUtils.IsNumeric(selecteditemid))
            {
                var objCtrl = new DNNrocketController();
                var info = objCtrl.GetInfo(Convert.ToInt32(selecteditemid));
                info.XMLData = postInfo.XMLData;
                info.GUIDKey = postInfo.GetXmlProperty("genxml/textbox/ctrlkey");
                objCtrl.Update(info);

                // Capture existing SYSTEMLINK records, so we can selectivly delete. To protect the system during operation, so records are always there.
                var systemlinklist = objCtrl.GetList(info.PortalId, -1, "SYSTEMLINK");
                var systemlinkidxlist = objCtrl.GetList(info.PortalId, -1, "SYSTEMLINKIDX");
                var newsystemlinklist = new List<int>();
                var newsystemlinkidxlist = new List<int>();

                // make systemlink record
                var entityList = new List<string>();
                foreach (var i in info.GetList("idxfielddata"))
                {
                    var entitytypecode = i.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                    var xreftypecode = i.GetXmlProperty("genxml/dropdownlist/xreftypecode");
                    var entityguidkey = entitytypecode;

                    if (!entityList.Contains(entityguidkey))
                    {
                        //build xref list
                        var xmldata = "<genxml>";
                        foreach (XmlNode xrefnod in info.XMLDoc.SelectNodes("genxml/idxfielddata/genxml[dropdownlist/entitytypecode/text()='" + entitytypecode + "']"))
                        {
                            xmldata += xrefnod.OuterXml;
                        }
                        xmldata += "</genxml>";

                        var idxinfo = objCtrl.GetByGuidKey(info.PortalId, info.ItemID, "SYSTEMLINK", entityguidkey); // use system id as moduleid
                        if (idxinfo == null)
                        {
                            idxinfo = new SimplisityInfo();
                            idxinfo.PortalId = info.PortalId;
                            idxinfo.TypeCode = "SYSTEMLINK";
                            idxinfo.GUIDKey = entityguidkey;
                            idxinfo.XMLData = xmldata;
                            idxinfo.ParentItemId = info.ItemID;
                            idxinfo.ModuleId = info.ItemID;
                            var itemid = objCtrl.Update(idxinfo);
                            idxinfo.ItemID = itemid;
                        }
                        else
                        {
                            idxinfo.ParentItemId = info.ItemID;
                            idxinfo.XMLData = xmldata;
                            objCtrl.Update(idxinfo);
                        }
                        newsystemlinklist.Add(idxinfo.ItemID);
                        entityList.Add(entityguidkey);

                        // SYSTEMLINKIDX
                        foreach (XmlNode xrefnod in idxinfo.XMLDoc.SelectNodes("genxml/genxml"))
                        {
                            var i2 = new SimplisityRecord();
                            i2.XMLData = xrefnod.OuterXml;

                            var idxref = i2.GetXmlProperty("genxml/textbox/indexref");
                            var typecodeIdx = i2.GetXmlProperty("genxml/dropdownlist/xreftypecode");
                            if (typecodeIdx == "")
                            {
                                typecodeIdx = i2.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                            }
                            var idxinfo2 = objCtrl.GetByGuidKey(info.PortalId, info.ItemID, "SYSTEMLINK" + typecodeIdx, idxref);
                            if (idxinfo2 == null)
                            {
                                idxinfo2 = new SimplisityInfo();
                                idxinfo2.PortalId = info.PortalId;
                                idxinfo2.TypeCode = "SYSTEMLINK" + typecodeIdx;
                                idxinfo2.GUIDKey = idxref;
                                idxinfo2.ParentItemId = idxinfo.ItemID;
                                idxinfo2.ModuleId = info.ItemID;
                                idxinfo2.XMLData = i2.XMLData;
                                idxinfo2.TextData = typecodeIdx;
                                var itemid = objCtrl.Update(idxinfo2);
                                idxinfo2.ItemID = itemid;
                            }
                            else
                            {
                                idxinfo2.ParentItemId = idxinfo.ItemID;
                                idxinfo2.XMLData = i2.XMLData;
                                idxinfo2.TextData = typecodeIdx;
                                objCtrl.Update(idxinfo2);
                            }
                            newsystemlinkidxlist.Add(idxinfo2.ItemID);
                        }
                    }
                }

                // delete any that have been remove.
                foreach (var sl in systemlinklist)
                {
                    if (!newsystemlinklist.Contains(sl.ItemID))
                    {
                        objCtrl.Delete(sl.ItemID);
                    }
                }
                foreach (var sl in systemlinkidxlist)
                {
                    if (!newsystemlinkidxlist.Contains(sl.ItemID))
                    {
                        objCtrl.Delete(sl.ItemID);
                    }
                }




                CacheUtils.ClearAllCache();
            }
        }

        public static void SystemDelete(SimplisityInfo sInfo)
        {
            var itemid = sInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                objCtrl.Delete(Convert.ToInt32(itemid));

                CacheUtils.ClearCache();
            }
        }


        public static void RebuildIndex(SimplisityInfo postInfo, bool deleteindex )
        {
            var itemid = postInfo.GetXmlProperty("genxml/hidden/selecteditemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                var sysInfo = objCtrl.GetInfo(Convert.ToInt32(itemid));

                var entityList = new List<string>();
                // get interface data, the langauge needs indexing.
                foreach (var i in sysInfo.GetList("interfacedata"))
                {
                    var entityTypeCode = i.GetXmlProperty("genxml/textbox/entitytypecode");
                    if (!entityList.Contains(entityTypeCode) && entityTypeCode != "")
                    {
                        entityList.Add(entityTypeCode);
                    }
                }
                foreach (var i in sysInfo.GetList("idxfielddata"))
                {
                    var entityTypeCode = i.GetXmlProperty("genxml/dropdownlist/entitytypecode");
                    if (!entityList.Contains(entityTypeCode) && entityTypeCode != "")
                    {
                        entityList.Add(entityTypeCode);
                    }
                }
                foreach (var entityCode in entityList)
                {
                    if (deleteindex)
                    {
                        var l2 = objCtrl.GetList(-1, -1, entityCode);
                        foreach (var sInfo in l2)
                        {
                            objCtrl.DeleteIndex(sInfo);
                        }
                    }
                    var l = objCtrl.GetList(-1, -1, entityCode + "LANG");
                    foreach (var sInfo in l)
                    {
                        objCtrl.RebuildLangIndex(sInfo, sInfo.ItemID);
                    }
                }

                CacheUtils.ClearCache();
            }

        }


        public static string CopyInterface(SimplisityInfo postInfo, string templateControlRelPath)
        {
            var strOut = "";
            var info = new SimplisityInfo();
            info.SetXmlProperty("genxml/delay", "2000");
            var itemid = postInfo.GetXmlProperty("genxml/hidden/fromsystemid");
            if (GeneralUtils.IsNumeric(itemid))
            {
                var objCtrl = new DNNrocketController();
                var sysInfo = objCtrl.GetInfo(Convert.ToInt32(itemid));
                if (sysInfo != null)
                {
                    var interfacekey = postInfo.GetXmlProperty("genxml/hidden/interfacekey");
                    var tosystemid = postInfo.GetXmlProperty("genxml/hidden/tosystemid");
                    if (tosystemid == itemid)
                    {
                        info.SetXmlProperty("genxml/message", "Canont copy the interface to the same system.");
                        info.SetXmlProperty("genxml/color", "w3-pale-red");
                    }
                    else
                    {

                        var sysInfoTo = objCtrl.GetInfo(Convert.ToInt32(tosystemid));
                        if (sysInfoTo != null)
                        {
                            var interfaceToCopy = sysInfo.GetListItem("interfacedata", "genxml/textbox/interfacekey", interfacekey);
                            if (interfaceToCopy != null)
                            {
                                var interfaceExists = sysInfoTo.GetListItem("interfacedata", "genxml/textbox/interfacekey", interfacekey);
                                if (interfaceExists == null)
                                {
                                    sysInfoTo.AddListRow("interfacedata", interfaceToCopy);
                                    objCtrl.Update(sysInfoTo);
                                    info.SetXmlProperty("genxml/message", "Interface Copied");
                                    info.SetXmlProperty("genxml/color", "w3-pale-green");
                                    info.SetXmlProperty("genxml/delay", "750");
                                }
                                else
                                {
                                    info.SetXmlProperty("genxml/message", "Interface already exists in destination.  Delete the Interface in destination and try again.");
                                    info.SetXmlProperty("genxml/color", "w3-pale-red");
                                    info.SetXmlProperty("genxml/delay", "3000");
                                }

                            }
                            else
                            {
                                info.SetXmlProperty("genxml/message", "Interface does not exists.");
                                info.SetXmlProperty("genxml/color", "w3-pale-red");
                            }
                        }
                        else
                        {
                            info.SetXmlProperty("genxml/message", "System does not exists.");
                            info.SetXmlProperty("genxml/color", "w3-pale-red");
                        }
                    }
                }
                CacheUtils.ClearCache();
                var themeFolder = postInfo.GetXmlProperty("genxml/hidden/theme");
                var razortemplate = postInfo.GetXmlProperty("genxml/hidden/template");
                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());
                strOut = DNNrocketUtils.RazorDetail(razorTempl, info);

            }
            return strOut;
        }

    }
}

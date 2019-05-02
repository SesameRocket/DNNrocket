﻿using System;
using System.Web;
using Simplisity;
using DotNetNuke.Entities.Users;
using DNNrocketAPI.Interfaces;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security;
using DotNetNuke.Services.Mail;
using DotNetNuke.Entities.Users.Membership;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DNNrocketAPI.Componants;
using System.Collections.Specialized;
using System.Text;

namespace DNNrocketAPI
{
    public class ProcessAPI2 : IHttpHandler
    {
        private String _editlang = "";
        public static string TemplateRelPath = "/DesktopModules/DNNrocket/api";

        public void ProcessRequest(HttpContext context)
        {
            var strOut = "ERROR: Invalid.";
            var strJson = "";

            try
            {

                var objCtrl = new DNNrocketController();

                _editlang = DNNrocketUtils.GetEditCulture();

                var paramCmd = context.Request.QueryString["cmd"];


                if (paramCmd == "downloadfile")
                {
                    var msg = "";
                    var fileindex = context.Request.QueryString["fileindex"];
                    var itemid = context.Request.QueryString["itemid"];
                    var fieldid = context.Request.QueryString["fieldid"];
                    if (GeneralUtils.IsNumeric(itemid) && GeneralUtils.IsNumeric(fileindex) && !String.IsNullOrEmpty(fieldid))
                    {
                        var downloadname = context.Request.QueryString["downloadname"];
                        var listname = context.Request.QueryString["listname"];
                        if (String.IsNullOrEmpty(listname)) listname = "settingsdata";
                        var sInfo = objCtrl.GetInfo(Convert.ToInt32(itemid), DNNrocketUtils.GetCurrentCulture());
                        var sInfoItem = sInfo.GetListItem(listname, Convert.ToInt32(fileindex));
                        var fpath = sInfoItem.GetXmlProperty("genxml/lang/genxml/hidden/rel" + fieldid);
                        if (fpath == "") fpath = sInfoItem.GetXmlProperty("genxml/hidden/rel" + fieldid);
                        if (fpath != "")
                        {
                            fpath = DNNrocketUtils.MapPath(fpath);
                            if (String.IsNullOrEmpty(downloadname)) downloadname = sInfoItem.GetXmlProperty("genxml/lang/genxml/textbox/name" + fieldid);
                            if (String.IsNullOrEmpty(downloadname)) downloadname = sInfoItem.GetXmlProperty("genxml/textbox/name" + fieldid);
                            if (String.IsNullOrEmpty(downloadname)) downloadname = Path.GetFileName(fpath);
                            DNNrocketUtils.ForceDocDownload(fpath, downloadname, context.Response);
                            msg = " - Cannot find: " + fpath;
                        }
                        else
                        {
                            strOut = "File Download Error, no data found for '" + fieldid + "'";
                        }
                    }
                    strOut = "File Download Error, itemid: " + itemid + ", fileindex: " + fileindex + " ";

                    context.Response.ContentType = "text/plain";
                    context.Response.Write(strOut);
                    context.Response.End();
                }



                var postInfo = new SimplisityInfo();
                postInfo.PortalId = PortalSettings.Current.PortalId;
                postInfo.SetXmlProperty("genxml/hidden", "");

                postInfo.SetXmlProperty("genxml/hidden/url", context.Request.Url.ToString());

                // Add any url params (uncoded)
                foreach (String key in context.Request.QueryString.Keys)
                {
                    postInfo.SetXmlProperty("genxml/urlparams/" + key.Replace("_", "-"), context.Request.QueryString[key]);
                }
                foreach (string key in context.Request.Form)
                {
                    postInfo.SetXmlProperty("genxml/postform/" + key.Replace("_", "-"), context.Request.Form[key]); // remove '_' from xpath
                }


                var param = context.Request.BinaryRead(context.Request.ContentLength);
                var strRequest = Encoding.ASCII.GetString(param);
                postInfo.SetXmlProperty("genxml/requestcontent", strRequest);

                var interfacekey = "";
                var systemprovider = "";
                var dataid = postInfo.GetXmlPropertyInt("genxml/urlparams/ref");
                if (String.IsNullOrEmpty(paramCmd) && dataid > 0)
                {
                    // use the dataid to get the systemprovider, interface, tabid, moduleid.
                    var dataRecord = objCtrl.GetRecord(dataid);
                    paramCmd = dataRecord.GetXmlProperty("genxml/hidden/cmd");
                    systemprovider = dataRecord.GetXmlProperty("genxml/hidden/systemprovider");
                    interfacekey = dataRecord.GetXmlProperty("genxml/hidden/interfacekey");
                    postInfo.SetXmlProperty("genxml/hidden/moduleid", dataRecord.GetXmlProperty("genxml/hidden/moduleid"));
                    postInfo.SetXmlProperty("genxml/hidden/tabid", dataRecord.GetXmlProperty("genxml/hidden/tabid"));
                }
                else
                {
                    systemprovider = postInfo.GetXmlProperty("genxml/urlparams/systemprovider").Trim(' ');
                    interfacekey = postInfo.GetXmlProperty("genxml/urlparams/interfacekey");
                }
                if (systemprovider == "") systemprovider = postInfo.GetXmlProperty("genxml/systemprovider");
                if (systemprovider == "") systemprovider = "dnnrocket";
                if (interfacekey == "") interfacekey = paramCmd.Split('_')[0];


                postInfo.SetXmlProperty("genxml/systemprovider", systemprovider);

                var systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", systemprovider);
                var rocketInterface = new DNNrocketInterface(systemInfo, interfacekey);

                if (rocketInterface.Exists)
                {
                    var returnDictionary = DNNrocketUtils.GetProviderReturn(paramCmd, systemInfo, rocketInterface, postInfo, TemplateRelPath, _editlang);

                    if (returnDictionary.ContainsKey("outputhtml"))
                    {
                        strOut = returnDictionary["outputhtml"];
                    }
                    if (returnDictionary.ContainsKey("outputjson"))
                    {
                        strJson = returnDictionary["outputjson"];
                    }

                }
            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }


            #region "return results"

            context.Response.Clear();
            if (strJson != "")
            {
                //send back xml as plain text
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.Write(JsonConvert.SerializeObject(strJson));
            }
            else
            {
                //send back xml as plain text
                context.Response.ContentType = "text/plain";
                context.Response.Write(strOut);
            }
            context.Response.End();


            #endregion

        }

        public bool IsReusable
        {
            get
            {
                return false;

            }
        }

        public static string GetSideMenu(SimplisityInfo sInfo, string systemprovider)
        {
            try
            {
                var strOut = "";
                var themeFolder = sInfo.GetXmlProperty("genxml/hidden/theme");
                var razortemplate = sInfo.GetXmlProperty("genxml/hidden/template");
                var moduleid = sInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
                if (moduleid == 0) moduleid = -1;

                var passSettings = sInfo.ToDictionary();

                var systemData = new SystemData();
                var sInfoSystem = systemData.GetSystemByKey(systemprovider);
                var sidemenu = new Componants.SideMenu(sInfoSystem);
                var templateControlRelPath = sInfo.GetXmlProperty("genxml/hidden/relpath");
                sidemenu.ModuleId = moduleid;

                var razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, templateControlRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());

                if (razorTempl == "")
                {
                    // no razor template for sidemenu, so use default.
                    razorTempl = DNNrocketUtils.GetRazorTemplateData(razortemplate, TemplateRelPath, themeFolder, DNNrocketUtils.GetCurrentCulture());
                }

                strOut = DNNrocketUtils.RazorDetail(razorTempl, sidemenu, passSettings);

                return strOut;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string DownloadFile(HttpContext context, string filenamepath, string downloadname, string fileext)
        {
            var strOut = "";
            if (filenamepath != "")
            {
                strOut = filenamepath; // return this is error.
                if (downloadname == "") downloadname = Path.GetFileNameWithoutExtension(filenamepath) + fileext;
                try
                {
                    context.Response.Clear();
                    context.Response.AppendHeader("content-disposition", "attachment; filename=" + downloadname);
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.WriteFile(filenamepath);
                    context.Response.End();
                }
                catch (Exception ex)
                {
                    var errmsg = ex.ToString();
                    // ignore, robots can cause error on thread abort.
                }
            }
            return strOut;
        }



    }
}
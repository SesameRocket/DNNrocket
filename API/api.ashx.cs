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

namespace DNNrocketAPI
{
    public class ProcessAPI : IHttpHandler
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

                // Do file upload is this is a file upload request.
                if (context.Request.Files.Count > 0)
                {
                    var fileout = DNNrocketUtils.FileUpload(context);
                }
                else
                {
                    _editlang = DNNrocketUtils.GetEditCulture();


                    var paramCmd = context.Request.QueryString["cmd"];

                    if (paramCmd == "login_signout")
                    {
                        var ps = new PortalSecurity();
                        ps.SignOut();
                        strOut = LoginUtils.LoginForm(new SimplisityInfo(),"login", UserUtils.GetCurrentUserId());
                        context.Response.ContentType = "text/plain";
                        context.Response.Write(strOut);
                        context.Response.End();
                    }

                    var requestJson = "";
                    var postInfo = new SimplisityInfo();
                    postInfo.SetXmlProperty("genxml/hidden", "");
                    if (DNNrocketUtils.RequestParam(context, "inputjson") != "")
                    {
                        requestJson = HttpUtility.UrlDecode(DNNrocketUtils.RequestParam(context, "inputjson"));

                        // ---- START: DEBUG POST ------
                        var debugSystemProvider = DNNrocketUtils.GetCookieValue("s-current-systemprovider");
                        var debugSystemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", debugSystemProvider);
                        if (debugSystemInfo != null && debugSystemInfo.GetXmlPropertyBool("genxml/checkbox/debugmode"))
                        {
                            FileUtils.SaveFile(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_requestJson.json", requestJson);
                        }
                        // ---- END: DEBUG POST ------

                        postInfo = SimplisityJson.GetSimplisityInfoFromJson(requestJson, _editlang);

                        // ---- START: DEBUG POST ------
                        if (debugSystemInfo != null && debugSystemInfo.GetXmlPropertyBool("genxml/checkbox/debugmode"))
                        {
                            FileUtils.SaveFile(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_postInfo.xml", postInfo.XMLData);
                        }
                        // ---- END: DEBUG POST ------

                    }

                    // Add any url params
                    foreach (string key in context.Request.QueryString.Keys)
                    {
                        if (key != "cmd")
                        {
                            var values = context.Request.QueryString.GetValues(key);
                            foreach (string value in values)
                            {
                                postInfo.SetXmlProperty("genxml/hidden/" + key, GeneralUtils.DeCode(value));
                            }

                        }
                    }

                    var systemprovider = postInfo.GetXmlProperty("genxml/hidden/systemprovider").Trim(' ');
                    if (systemprovider == "") systemprovider = postInfo.GetXmlProperty("genxml/systemprovider");
                    if (systemprovider == "") systemprovider = "dnnrocket";

                    var interfacekey = postInfo.GetXmlProperty("genxml/hidden/interfacekey");
                    if (interfacekey == "") interfacekey = paramCmd.Split('_')[0];

                    postInfo.SetXmlProperty("genxml/systemprovider", systemprovider);

                    if (paramCmd == "login_login")
                    {
                        LoginUtils.DoLogin(postInfo, HttpContext.Current.Request.UserHostAddress);
                        strOut = ""; // the page will rteload after the call
                    }
                    else
                    {
                        switch (paramCmd)
                    {
                        case "getsidemenu":
                            strOut = GetSideMenu(postInfo, systemprovider);
                            break;
                        default:
                            var systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", systemprovider);
                            var rocketInterface = new DNNrocketInterface(systemInfo, interfacekey);

                            if (rocketInterface.Exists)
                            {
                                var returnDictionary = DNNrocketUtils.GetProviderReturn(paramCmd, systemInfo, rocketInterface, postInfo, TemplateRelPath, _editlang);

                                if (returnDictionary.ContainsKey("outputhtml"))
                                {
                                    strOut = returnDictionary["outputhtml"];
                                }
                                if (returnDictionary.ContainsKey("filenamepath"))
                                {
                                    if (!returnDictionary.ContainsKey("downloadname")) returnDictionary["downloadname"] = "";
                                    if (!returnDictionary.ContainsKey("fileext")) returnDictionary["fileext"] = "";
                                    DownloadFile(context, returnDictionary["filenamepath"], returnDictionary["downloadname"], returnDictionary["fileext"]);
                                }
                                if (returnDictionary.ContainsKey("outputjson"))
                                {
                                    strJson = returnDictionary["outputjson"];
                                }

                            }
                            else
                            {
                                // check for systemspi, does not exist.  It's used to create the systemprovders 
                                if (systemprovider == "" || systemprovider == "systemapi" || systemprovider == "login")
                                {
                                    var ajaxprov = APInterface.Instance("DNNrocketSystemData", "DNNrocket.SystemData.startconnect", TemplateRelPath);
                                    var returnDictionary = ajaxprov.ProcessCommand(paramCmd, systemInfo, null, postInfo, HttpContext.Current.Request.UserHostAddress, _editlang);
                                    strOut = returnDictionary["outputhtml"];
                                }
                                else
                                {
                                    strOut = "ERROR: Invalid SystemProvider: " + systemprovider + "  interfacekey: " + interfacekey + " - Check Database for SYSTEM,'" + systemprovider + "' (No spaces)";
                                }

                            }
                            break;
                    }
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
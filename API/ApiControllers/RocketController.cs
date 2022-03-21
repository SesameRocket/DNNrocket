﻿using DNNrocketAPI.Components;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Simplisity;
using System;  
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;  
using System.Net;  
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace DNNrocketAPI.ApiControllers
{

    public class RocketController : DnnApiController
    {
        private SessionParams _sessionParams;
        public static string TemplateRelPath = "/DesktopModules/DNNrocket/api";

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage GetTest()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, "Test API2");
        }

        /// <summary>
        /// Normal Endpoint for Simplisity
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage Action()
        {
            var context = HttpContext.Current;

            if (!context.Request.QueryString.AllKeys.Contains("cmd"))
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, "No 'cmd' parameter in url.  Unable to process action.");
            }

            var paramCmd = context.Request.QueryString["cmd"];

            var systemkey = "";
            if (context.Request.QueryString.AllKeys.Contains("systemkey")) systemkey = context.Request.QueryString["systemkey"];
            if (systemkey == "" && context.Request.QueryString.AllKeys.Contains("s")) systemkey = context.Request.QueryString["s"]; // reduce chars.
            if (systemkey == "" || systemkey == "undefined") systemkey = paramCmd.Split('_')[0];

            var paramInfo = BuildParamInfo();
            var postInfo = BuildPostInfo();

            var systemData = new SystemLimpet(systemkey);
            var interfacekey = paramCmd.Split('_')[0];
            var rocketInterface = new RocketInterface(systemData.SystemInfo, interfacekey);
            TrackCmd(ref paramCmd, systemData, ref rocketInterface, ref paramInfo);

            var rtn = new HttpResponseMessage();
            var strOut = ProcessAction(postInfo, paramInfo, paramCmd, systemkey);
            if (strOut == "process") // no admin actions processed, so do normal provider command.
            {
                rtn = ProcessProvider(paramCmd, postInfo, paramInfo, systemData, rocketInterface);
            }
            else
            {
                rtn = this.Request.CreateResponse(HttpStatusCode.OK, strOut, "text/plain");
            }


            if (rtn.Headers.Contains("Access-Control-Allow-Origin")) rtn.Headers.Remove("Access-Control-Allow-Origin");
            //rtn.Headers.Add("Access-Control-Allow-Origin", "*");
            // Access-Control-Allow-Origin must be setup of IIS, to allow iframes.  The header cannot be added more than once.
            return rtn;
        }
        /// <summary>
        /// Endpoint for Simplisity when we only want to deal with the Request Content
        /// Puts the content into a field "genxml/requestcontent".
        /// This is because we cannot pass the context to .Net Standard modules
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public HttpResponseMessage ActionContent()
        {
            HttpResponseMessage rtn;
            var context = HttpContext.Current;
            var key = "";
            var paramCmd = "";
            if (context.Request.QueryString.AllKeys.Contains("k")) key = context.Request.QueryString["k"];
            if (context.Request.QueryString.AllKeys.Contains("cmd")) paramCmd = context.Request.QueryString["cmd"];

            if (paramCmd == "" && key == "") return this.Request.CreateResponse(HttpStatusCode.OK, "No 'cmd' or 'key' parameter in url.  Unable to process action.");

            var paramInfo = BuildParamInfo(true);
            var postInfo = BuildPostInfo();

            if (paramCmd == "")
            {
                // if we do not have a "cmd" we may be passing a key to a data file.
                // This data file needs to keep the data required to process and should be created by a call to an external system that returns a POST. (like a Bank) 
                // This is to keep the URL smaller (for some payment providers) and also proves the call is initiated by this system. 
                var sRec = DNNrocketUtils.GetTempStorage(key, true);
                if (sRec == null)
                    return this.Request.CreateResponse(HttpStatusCode.OK, "No ACTIONRETURN data: " + key);
                else
                {
                    paramInfo.AddXmlNode(sRec.XMLData, sRec.RootNodeName, paramInfo.RootNodeName);
                    paramCmd = sRec.GetXmlProperty("genxml/cmd");
                    var systemkey = sRec.GetXmlProperty("genxml/systemkey");
                    var interfacekey = sRec.GetXmlProperty("genxml/interfacekey");
                    var systemData = new SystemLimpet(systemkey);
                    var rocketInterface = new RocketInterface(systemData.SystemInfo, interfacekey);
                    rtn = ProcessProvider(paramCmd, postInfo, paramInfo, systemData, rocketInterface);
                }
            }
            else
            {
                var systemkey = "";
                if (context.Request.QueryString.AllKeys.Contains("systemkey")) systemkey = context.Request.QueryString["systemkey"];
                if (systemkey == "" && context.Request.QueryString.AllKeys.Contains("s")) systemkey = context.Request.QueryString["s"]; // reduce chars.

                var systemData = new SystemLimpet(systemkey);
                var interfacekey = paramCmd.Split('_')[0];
                var rocketInterface = new RocketInterface(systemData.SystemInfo, interfacekey);
                rtn = ProcessProvider(paramCmd, postInfo, paramInfo, systemData, rocketInterface);
            }

            if (rtn.Headers.Contains("Access-Control-Allow-Origin")) rtn.Headers.Remove("Access-Control-Allow-Origin");
            //rtn.Headers.Add("Access-Control-Allow-Origin", "*");
            return rtn;
        }
        private SimplisityInfo BuildPostInfo()
        {
            var context = HttpContext.Current;
            var requestJson = "";
            var postInfo = new SimplisityInfo();
            if (DNNrocketUtils.RequestParam(context, "inputjson") != "")
            {
                requestJson = HttpUtility.UrlDecode(DNNrocketUtils.RequestParam(context, "inputjson"));
                postInfo = SimplisityJson.GetSimplisityInfoFromJson(requestJson, "");
                postInfo.PortalId = PortalUtils.GetPortalId();
            }
            return postInfo;
        }

        private SimplisityInfo BuildParamInfo(bool requestContent = false)
        {
            var context = HttpContext.Current;

            var paramJson = "";
            var paramInfo = new SimplisityInfo();
            if (DNNrocketUtils.RequestParam(context, "paramjson") != "")
            {
                paramJson = HttpUtility.UrlDecode(DNNrocketUtils.RequestParam(context, "paramjson"));
                paramInfo = SimplisityJson.GetSimplisityInfoFromJson(paramJson, "");
            }
            paramInfo.PortalId = PortalUtils.GetPortalId();

            // get all query string params
            foreach (string key in context.Request.QueryString.AllKeys)
            {
                var keyValue = context.Request.QueryString[key];
                paramInfo.SetXmlProperty("genxml/urlparams/" + key.ToLower(), keyValue);
            }
            // get all form data (drop the ones we already processed) 
            foreach (string key in context.Request.Form.AllKeys)
            {
                if (key.ToLower() != "paramjson" && key.ToLower() != "inputjson" && key.ToLower() != "remote")
                {
                    var keyValue = DNNrocketUtils.RequestParam(context, key);
                    paramInfo.SetXmlProperty("genxml/form/" + key.ToLower(), keyValue);
                }
                if (key.ToLower() == "remote")
                {
                    // add any remote data to paramsInfo
                    var keyValue = DNNrocketUtils.RequestParam(context, key);
                    var remoteData = new SimplisityInfo();
                    var remote = HttpUtility.UrlDecode(keyValue);
                    remote = GeneralUtils.Base64Decode(GeneralUtils.DeCode(remote)); // string uses decimal code, so it's not changed during post.
                    remoteData.FromXmlItem(remote);
                    paramInfo.AddXmlNode(remoteData.XMLData, "genxml/remote", "genxml");
                }
            }

            if (requestContent)
            {
                // put the content into a field. Usually used for validation of the request.
                // (WE CANNOT PASS CONTEXT TO .NET STANDARD)
                var requestBinaryContent = context.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                var requestStringContent = Encoding.ASCII.GetString(requestBinaryContent);
                paramInfo.SetXmlProperty("genxml/requestcontent", requestStringContent);
            }

            _sessionParams = new SessionParams(paramInfo);
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();

            return paramInfo;
        }

        private void TrackCmd(ref string paramCmd, SystemLimpet systemData, ref RocketInterface rocketInterface, ref SimplisityInfo paramInfo)
        {
            var sessionParams = new SessionParams(paramInfo);
            var userParams = new UserParams(sessionParams.BrowserSessionId);
            if (paramInfo.GetXmlPropertyBool("genxml/hidden/reload"))
            {
                var menucmd = userParams.GetCommand(systemData.SystemKey);
                if (menucmd != "")
                {
                    paramCmd = menucmd;
                    paramInfo = userParams.GetParamInfo(systemData.SystemKey);
                    var interfacekey = userParams.GetInterfaceKey(systemData.SystemKey);
                    rocketInterface = new RocketInterface(systemData.SystemInfo, interfacekey);

                    // it might be a change of langauge (reload)
                    // Change the langauge to the session language.
                    paramInfo.SetXmlProperty("genxml/hidden/culturecode", sessionParams.CultureCode);
                    paramInfo.SetXmlProperty("genxml/hidden/culturecodeedit", sessionParams.CultureCodeEdit);
                }
            }
            else
            {
                if (paramInfo.GetXmlPropertyBool("genxml/hidden/track")) userParams.Track(systemData.SystemKey, paramCmd, paramInfo, rocketInterface.InterfaceKey);
            }
        }
        /// <summary>
        /// Admin Actions for Action Endpoint [TODO: rewrite to be cleaner]
        /// </summary>
        /// <param name="postInfo"></param>
        /// <param name="paramInfo"></param>
        /// <param name="paramCmd"></param>
        /// <param name="systemkey"></param>
        /// <returns></returns>
        private string ProcessAction(SimplisityInfo postInfo, SimplisityInfo paramInfo, string paramCmd, string systemkey)
        {
            var strOut = "ERROR: Invalid.";
            var context = HttpContext.Current;

            try
            {
                var objCtrl = new DNNrocketController();

                if (paramCmd == "login_signout")
                {
                    var ps = new PortalSecurity();
                    ps.SignOut();
                    strOut = "";
                    //strOut = UserUtils.LoginForm("", new SimplisityInfo(), "login", UserUtils.GetCurrentUserId());
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(strOut);
                    context.Response.End();
                }

                systemkey = systemkey.Trim(' ');
                if (systemkey == "") paramInfo.GetXmlProperty("genxml/hidden/systemkey").Trim(' ');
                if (systemkey == "" && paramCmd.Contains("_")) systemkey = paramCmd.Split('_')[0];
                if (systemkey == "") systemkey = "dnnrocket";
                var systemData = new SystemLimpet(systemkey);

                var interfacekey = paramInfo.GetXmlProperty("genxml/hidden/interfacekey");
                if (interfacekey == "") interfacekey = paramInfo.GetXmlProperty("genxml/urlparams/interfacekey").Trim(' ');
                if (interfacekey == "") interfacekey = paramInfo.GetXmlProperty("genxml/hidden/i").Trim(' '); // reduce chars
                if (interfacekey == "") interfacekey = paramInfo.GetXmlProperty("genxml/urlparams/i").Trim(' ');
                if (interfacekey == "") interfacekey = paramCmd.Split('_')[0];
                if (interfacekey == "") interfacekey = systemkey;

                paramInfo.SetXmlProperty("genxml/hidden/systemkey", systemkey);
                paramInfo.SetXmlProperty("genxml/hidden/userhostaddress", HttpContext.Current.Request.UserHostAddress);

                switch (paramCmd)
                {
                    case "changeculture":
                        var lang = paramInfo.GetXmlProperty("genxml/hidden/culturecode");
                        DNNrocketUtils.SetCookieValue("language", lang);
                        strOut = lang; // the page will reload after the call
                        break;
                    case "login_login":
                        UserUtils.DoLogin(postInfo, paramInfo);
                        strOut = ""; // the page will reload after the call
                        break;
                    case "login_register":
                        strOut = UserUtils.RegisterForm(systemData.SystemInfo, postInfo, interfacekey, UserUtils.GetCurrentUserId());
                        break;
                    case "login_doregister":
                        strOut = UserUtils.RegisterUser(postInfo, DNNrocketUtils.GetCurrentCulture());
                        if (strOut == "") UserUtils.DoLogin(postInfo, paramInfo);
                        break;
                    case "getsidemenu":
                        strOut = GetSideMenu(paramInfo, systemkey);
                        break;
                    case "gettopbar":
                        strOut = GetTopBar(paramInfo, systemkey);
                        break;
                    case "rocketapi_clearallcache":
                        if (UserUtils.IsAdministrator()) strOut = ClearCache();
                        break;
                    case "rocketapi_cleartempdb":
                        if (UserUtils.IsSuperUser()) strOut = ClearTempDB();
                        break;
                    case "rocketapi_recycleapppool":
                        if (UserUtils.IsSuperUser()) strOut = RecycleAppPool();
                        break;
                    case "global_globaldetail":
                        if (UserUtils.IsSuperUser()) strOut = SystemGlobalDetail(paramInfo);
                        break;
                    case "global_globalsave":
                        if (UserUtils.IsSuperUser())
                        {
                            SystemGlobalSave(postInfo);
                            strOut = SystemGlobalDetail(paramInfo);
                        }
                        break;
                    case "global_defaultroles":
                        if (UserUtils.IsSuperUser())
                        {
                            DNNrocketUtils.CreateDefaultRocketRoles(PortalUtils.GetCurrentPortalId());
                            strOut = SystemGlobalDetail(paramInfo);
                        }
                        break;
                    case "global_installscheduler":
                        SchedulerUtils.SchedulerInstall();
                        strOut = SystemGlobalDetail(paramInfo);
                        break;
                    case "global_uninstallscheduler":
                        SchedulerUtils.SchedulerUnInstall();
                        strOut = SystemGlobalDetail(paramInfo);
                        break;
                    default:
                        strOut = "process"; // process the provider              
                        break;
                }
            }
            catch (Exception ex)
            {
                strOut = LogUtils.LogException(ex);
            }
            return strOut;
        }

        private HttpResponseMessage ProcessProvider(string paramCmd, SimplisityInfo postInfo, SimplisityInfo paramInfo, SystemLimpet systemData, RocketInterface rocketInterface)
        {
            var strOut = "ERROR: Invalid.";
            object jsonReturn = null;
            object xmlReturn = null;
            var returnDictionary = new Dictionary<string, object>();

            // before event
            var rtnDictInfo = DNNrocketUtils.EventProviderBefore(paramCmd, systemData, postInfo, paramInfo, "");
            if (rtnDictInfo.ContainsKey("post")) postInfo = (SimplisityInfo)rtnDictInfo["post"];
            if (rtnDictInfo.ContainsKey("param")) paramInfo = (SimplisityInfo)rtnDictInfo["param"];

            // command action
            var remoteCache = "false";
            var statusCode = "00";
            var errorMsg = "";
            if (rocketInterface.Exists)
            {

                returnDictionary = DNNrocketUtils.GetProviderReturn(paramCmd, systemData.SystemInfo, rocketInterface, postInfo, paramInfo, TemplateRelPath, "");

                if (returnDictionary.ContainsKey("outputhtml"))
                {
                    strOut = (string)returnDictionary["outputhtml"];
                }
                if (returnDictionary.ContainsKey("filenamepath"))
                {
                    var downloadname = "download.zip";
                    if (returnDictionary.ContainsKey("downloadname")) downloadname = (string)returnDictionary["downloadname"];
                    DownloadFile((string)returnDictionary["filenamepath"], downloadname);
                }
                if (returnDictionary.ContainsKey("downloadfiledata"))
                {
                    var downloadname = "download.zip";
                    if (returnDictionary.ContainsKey("downloadname")) downloadname = (string)returnDictionary["downloadname"];
                    DownloadStringAsFile((string)returnDictionary["downloadfiledata"], downloadname);
                }
                if (returnDictionary.ContainsKey("outputjson"))
                {
                    jsonReturn = returnDictionary["outputjson"];
                }
                if (returnDictionary.ContainsKey("outputxml"))
                {
                    xmlReturn = returnDictionary["outputxml"];
                }

                if (returnDictionary.ContainsKey("razor-statuscode")) statusCode = returnDictionary["razor-statuscode"].ToString();
                if (returnDictionary.ContainsKey("razor-errormsg")) errorMsg = returnDictionary["razor-errormsg"].ToString();

            }

            // after Event
            var returnDictionaryAfterEvent = DNNrocketUtils.EventProviderAfter(paramCmd, systemData, postInfo, paramInfo, "");
            if (returnDictionaryAfterEvent.ContainsKey("outputhtml")) strOut = (string)returnDictionaryAfterEvent["outputhtml"];
            if (returnDictionaryAfterEvent.ContainsKey("outputjson")) jsonReturn = returnDictionaryAfterEvent["outputjson"];
            if (returnDictionaryAfterEvent.ContainsKey("outputxml")) xmlReturn = returnDictionaryAfterEvent["outputxml"];

            #region "return results"

            HttpResponseMessage resp = null;
            if (jsonReturn != null)
            {
                resp = this.Request.CreateResponse(HttpStatusCode.OK, jsonReturn, System.Net.Http.Formatting.JsonMediaTypeFormatter.DefaultMediaType);
            }
            if (xmlReturn != null)
            {
                resp = this.Request.CreateResponse(HttpStatusCode.OK, xmlReturn, System.Net.Http.Formatting.XmlMediaTypeFormatter.DefaultMediaType);
            }
            if (resp == null)
            {
                resp = this.Request.CreateResponse(HttpStatusCode.OK, strOut, "text/plain");
            }

            resp.Headers.Add("razor-statuscode", statusCode);
            resp.Headers.Add("razor-errormsg", GeneralUtils.Base64Encode(errorMsg));

            // add headers for SEO page
            foreach (var h in returnDictionary)
            {
                if (h.Key.StartsWith("remote-")) resp.Headers.Add(h.Key, GeneralUtils.Base64Encode(h.Value.ToString()));
            }


            return resp;

            #endregion

        }



        private string GetSideMenu(SimplisityInfo sInfo, string systemkey)
        {
            try
            {
                var systemData = new SystemLimpet(systemkey);
                if (!systemData.Exists) return "ERROR: No SystemKey, Missing system.rules";

                var template = sInfo.GetXmlProperty("genxml/hidden/template");
                if (template == "") template = "SideMenu.cshtml";

                var appThemeSystem = new AppThemeSystemLimpet(systemkey);
                var razorTempl = appThemeSystem.GetTemplate(template);
                if (razorTempl == "")
                {
                    var appThemeDNNrocket = new AppThemeDNNrocketLimpet(systemkey);
                    razorTempl = appThemeDNNrocket.GetTemplate(template);
                }
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("systemdata",systemData);
                var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, dataObjects, null, _sessionParams, true);
                if (pr.ErrorMsg != "") return pr.ErrorMsg;
                return pr.RenderedText;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string GetTopBar(SimplisityInfo sInfo, string systemkey)
        {
            try
            {
                var template = sInfo.GetXmlProperty("genxml/hidden/template");
                if (template == "") template = "TopBar.cshtml";
                var passSettings = sInfo.ToDictionary();
                var appThemeSystem = new AppThemeSystemLimpet(systemkey);
                var razorTempl = appThemeSystem.GetTemplate(template);
                if (razorTempl == "")
                {
                    var appThemeDNNrocket = new AppThemeDNNrocketLimpet(systemkey);
                    razorTempl = appThemeDNNrocket.GetTemplate(template);
                }
                return RenderRazorUtils.RazorDetail(razorTempl, sInfo, passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private string DownloadFile(string filenamepath, string downloadname)
        {
            var strOut = "";
            if (filenamepath != "")
            {
                strOut = filenamepath; // return this is error.
                if (downloadname == "") downloadname = Path.GetFileName(filenamepath);
                try
                {
                    HttpResponse response = HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "text/plain";
                    response.AppendHeader("content-disposition", "attachment; filename=" + downloadname);
                    response.TransmitFile(filenamepath);
                    response.Flush();
                    response.End();
                }
                catch (Exception ex)
                {
                    var errmsg = ex.ToString();
                    // ignore, robots can cause error on thread abort.
                }
            }
            return strOut;
        }

        private string DownloadStringAsFile(string filedata, string downloadname = "")
        {
            var strOut = "";
            if (filedata != "")
            {
                if (downloadname == "") downloadname = "downloadfile.txt";
                try
                {
                    HttpResponse response = HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "text/plain";
                    response.AppendHeader("content-disposition", "attachment; filename=" + downloadname);
                    response.Write(filedata);
                    response.Flush();
                    response.End();
                }
                catch (Exception ex)
                {
                    var errmsg = ex.ToString();
                    // ignore, robots can cause error on thread abort.
                }
            }
            return strOut;
        }

        private string ClearCache()
        {
            CacheFileUtils.ClearFileCacheAllPortals();
            CacheUtils.ClearAllCache();
            CacheUtilsDNN.ClearAllCache();
            DNNrocketUtils.ClearAllCache();
            return "OK";
        }
        private string ClearTempDB()
        {
            ClearCache();
            DNNrocketUtils.ClearTempDB();
            return "OK";
        }
        private string RecycleAppPool()
        {
            ClearCache();
            DNNrocketUtils.RecycleApplicationPool();
            return "OK";
        }
        private String SystemGlobalDetail(SimplisityInfo paramInfo)
        {
            var passSettings = paramInfo.ToDictionary();
            var appThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = appThemeSystem.GetTemplate("GlobalDetail.cshtml");

            SchedulerUtils.SchedulerIsInstalled();
            var globalData = new SystemGlobalData();
            var strOut = RenderRazorUtils.RazorDetail(razorTempl, globalData, passSettings);
            return strOut;
        }

        private void SystemGlobalSave(SimplisityInfo postInfo)
        {
            var globalData = new SystemGlobalData();
            globalData.Save(postInfo);
            ClearCache();
        }



    }

}

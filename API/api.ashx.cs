﻿using System;
using System.Web;
using Simplisity;
using DotNetNuke.Entities.Users;
using DNNrocketAPI.Interfaces;

namespace DNNrocketAPI
{
    public class ProcessAPI : IHttpHandler
    {
        private String _editlang = "";

        public void ProcessRequest(HttpContext context)
        {
            var strOut = "ERROR: Invalid.";
            try
            {
                _editlang = DNNrocketUtils.GetCurrentCulture();

                var paramCmd = context.Request.QueryString["cmd"];

                var requestXml = HttpUtility.UrlDecode(DNNrocketUtils.RequestParam(context, "inputxml"));
                
                var sInfo = SimplisityUtils.GetSimplisityInfo(requestXml);

                var systemprovider = sInfo.GetXmlProperty("genxml/hidden/systemprovider");
                if (systemprovider == "") systemprovider = DNNrocketUtils.RequestQueryStringParam(context, "systemprovider");
                var interfacekey = sInfo.GetXmlProperty("genxml/hidden/interfacekey");
                if (interfacekey == "") interfacekey = paramCmd.Split('_')[0];


                if (systemprovider == "" || systemprovider == "systemapi")
                {
                    if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
                    {
                        // By default we prcess the DNNrocketAPI system api.
                        strOut = SystemFunction.ProcessCommand(paramCmd,sInfo, _editlang);
                    }
                    else
                    {
                        strOut = "ERROR: Invalid Security.";
                    }

                }
                else
                {
                    if (systemprovider != "")
                    {
                        // Run API Provider.
                        strOut = "API not found: " + systemprovider;
                        var objCtrl = new DNNrocketController();

                        var systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", systemprovider);
                        if (systemInfo != null)
                        {
                            var systemRecord = new SystemRecord(systemInfo);
                            var iface = systemRecord.GetInterface(interfacekey);
                            if (iface != null)
                            {

                                var assembly = iface.GetXmlProperty("genxml/textbox/assembly");
                                var namespaceclass = iface.GetXmlProperty("genxml/textbox/namespaceclass");
                                if (assembly == "" || namespaceclass == "")
                                {
                                    strOut = "No assembly or namespaceclass defined: " + systemprovider + " : " + assembly + "," + namespaceclass;
                                }
                                else
                                {
                                    try
                                    {
                                        var ajaxprov = APInterface.Instance(assembly, namespaceclass);
                                        strOut = ajaxprov.ProcessCommand(paramCmd, sInfo, _editlang);
                                    }
                                    catch (Exception ex)
                                    {
                                        //strOut = ex.ToString();
                                        strOut = "No valid assembly found: " + systemprovider + " : " + assembly + "," + namespaceclass;
                                    }
                                }
                            }
                            else
                            {
                                strOut = "interfacekey not found: " + interfacekey;
                            }
                        }
                        else
                        {
                            strOut = "No valid system found: " + systemprovider;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strOut = ex.ToString();
            }


            #region "return results"

            //send back xml as plain text
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
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


    }
}
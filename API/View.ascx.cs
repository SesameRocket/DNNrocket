// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2015 SARL Nevoweb.  www.Nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using Simplisity;
using System.Collections.Specialized;

namespace DNNrocketAPI
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase, IActionable
    {
        #region Event Handlers


        private bool _debugmode = false;
        private bool _activatedetail = false;

        private string _paramCmd;
        private string _systemprovider;

        private string _interfacekey;
        private string _templateRelPath;
        private string _entiytypecode;

        private DNNrocketInterface _rocketInterface;

        private SimplisityInfo _settingsInfo;
        private SimplisityInfo _configInfo;
        private SimplisityInfo _systemInfo;


        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);

            var objCtrl = new DNNrocketController();

            _settingsInfo = DNNrocketUtils.GetModuleSettings(ModuleId);
            _activatedetail = _settingsInfo.GetXmlPropertyBool("genxml/checkbox/activatedetail");
            _paramCmd = _settingsInfo.GetXmlProperty("genxml/hidden/command");
            _debugmode = _settingsInfo.GetXmlPropertyBool("genxml/checkbox/debugmode");


            var moduleInfo = ModuleController.Instance.GetModule(ModuleId, TabId, false);
            var desktopModule = moduleInfo.DesktopModule;

            _systemprovider = desktopModule.ModuleDefinitions.First().Key.ToLower(); // Use the First DNN Module definition as the DNNrocket systemprovider

            _interfacekey = desktopModule.ModuleName.ToLower();  // Use the module name as DNNrocket interface key.

            _systemInfo = objCtrl.GetByGuidKey(-1, -1, "SYSTEM", _systemprovider);

            _configInfo = objCtrl.GetData(_interfacekey + "_" + ModuleId, "CONFIG", DNNrocketUtils.GetEditCulture(), -1, ModuleId, true);

            _rocketInterface = new DNNrocketInterface(_systemInfo, _interfacekey);

            var clearmodcache = DNNrocketUtils.RequestParam(Context, "clearmodcache");
            if (clearmodcache != "")
            {
                CacheUtils.ClearCache(_interfacekey + clearmodcache);
            }
            var clearallcache = DNNrocketUtils.RequestParam(Context, "clearallcache");
            if (clearallcache != "")
            {
                DNNrocketUtils.ClearPortalCache(PortalId);
                CacheUtils.ClearAllCache();
            }

            if (_rocketInterface.Exists)
            {
                _templateRelPath = _rocketInterface.TemplateRelPath;
                _entiytypecode = _rocketInterface.EntityTypeCode;
                _paramCmd = _rocketInterface.DefaultCommand;
                if (String.IsNullOrEmpty(_templateRelPath)) _templateRelPath = base.ControlPath; // if we dont; define template path in the interface assume it's the control path.

                DNNrocketUtils.IncludePageHeaders(base.ModuleId, this.Page, _systemprovider, _templateRelPath, "pageheader.cshtml", _rocketInterface.DefaultTheme);
            }


        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {

                base.OnLoad(e);

                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void PageLoad()
        {
            var objCtrl = new DNNrocketController();

            var itemref = DNNrocketUtils.RequestQueryStringParam(Request, "refid");
            // check for detail page display
            if (GeneralUtils.IsNumeric(itemref))
            {
                var info = objCtrl.GetInfo(Convert.ToInt32(itemref), DNNrocketUtils.GetCurrentCulture());
                if (info != null)
                {
                    var pagename = info.GetXmlProperty("genxml/lang/genxml/textbox/pagename");
                    if (pagename == "") pagename = info.GetXmlProperty("genxml/textbox/pagename");
                    if (pagename == "") pagename = info.GetXmlProperty("genxml/lang/genxml/textbox/title");
                    if (pagename == "") pagename = info.GetXmlProperty("genxml/textbox/title");

                    var pagetitle = info.GetXmlProperty("genxml/lang/genxml/textbox/pagetitle");
                    if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/textbox/pagetitle");
                    if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/lang/genxml/textbox/title");
                    if (pagetitle == "") pagetitle = info.GetXmlProperty("genxml/textbox/title");

                    var pagekeywords = info.GetXmlProperty("genxml/lang/genxml/textbox/pagekeywords");

                    var pagedescription = info.GetXmlProperty("genxml/lang/genxml/textbox/pagedescription");

                    DotNetNuke.Framework.CDefault tp = (DotNetNuke.Framework.CDefault)this.Page;
                    if (pagetitle != "") tp.Title = pagetitle;
                    if (pagedescription != "") tp.Description = pagedescription;
                    if (pagekeywords != "") tp.KeyWords = pagekeywords;
                }
            }

            var postInfo = new SimplisityInfo();
            postInfo.PortalId = PortalSettings.Current.PortalId;
            postInfo.ModuleId = ModuleId;
            postInfo.SetXmlProperty("genxml/hidden/moduleid", ModuleId.ToString());
            postInfo.SetXmlProperty("genxml/hidden/tabid", PortalSettings.Current.ActiveTab.TabID.ToString());

            if (_rocketInterface.Exists)
            {
                // add parameters to postInfo and cachekey
                var paramString = "";
                foreach (String key in Request.QueryString.AllKeys)
                {
                    paramString += key + "=" + Request.QueryString[key];
                    // we need this if we need to process url parmas on the APInterface.  In the cshtml we can use (Model.GetUrlParam("????"))
                    postInfo.SetXmlProperty("genxml/urlparams/" + key.Replace("_","-"), Request.QueryString[key]);
                }
                foreach (string key in Request.Form)
                {
                    postInfo.SetXmlProperty("genxml/postform/" + key.Replace("_","-"), Request.Form[key]); // remove '_' from xpath
                }

                var strOut = "No Interface Found.";
                var cacheOutPut = "";
                var cacheKey = "view.ascx" + ModuleId + DNNrocketUtils.GetCurrentCulture() + paramString + DNNrocketUtils.GetCurrentCulture();
                if (_rocketInterface.IsCached) cacheOutPut = (string)CacheUtils.GetCache(cacheKey);

                if (cacheOutPut == null || cacheOutPut == "")
                {
                    var returnDictionary = DNNrocketUtils.GetProviderReturn(_paramCmd, _systemInfo, _rocketInterface, postInfo, _templateRelPath, DNNrocketUtils.GetCurrentCulture());

                    if (returnDictionary.ContainsKey("outputhtml"))
                    {
                        strOut = returnDictionary["outputhtml"];
                        if (_rocketInterface.IsCached) CacheUtils.SetCache(cacheKey, strOut, _interfacekey + ModuleId);
                    }
                }
                else
                {
                    strOut = cacheOutPut;
                }

                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);
            }
            else
            {
                var strOut = "Invalid Interface: systemprovider: " + _systemprovider + "  interfacekey: " + _interfacekey;
                var lit = new Literal();
                lit.Text = strOut;
                phData.Controls.Add(lit);
            }

        }


        #endregion


        #region Optional Interfaces

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                if (_configInfo != null && _configInfo.XMLDoc.SelectNodes("genxml/*").Count > 1 && !_configInfo.GetXmlPropertyBool("genxml/checkbox/noiframeedit"))
                {
                    actions.Add(GetNextActionID(), DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/API/App_LocalResources/", "DNNrocket.edit") , "", "", "register.gif", "javascript:" + _interfacekey + "editiframe_" + ModuleId + "()", false, SecurityAccessLevel.Edit, true, false);
                }

                var adminurl = _systemInfo.GetXmlProperty("genxml/textbox/adminurl");
                adminurl = adminurl.ToLower().Replace("[moduleid]", ModuleId.ToString());
                adminurl = adminurl.ToLower().Replace("[tabid]", TabId.ToString());
                if (adminurl != "")
                {
                    actions.Add(GetNextActionID(), DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/API/App_LocalResources/", "DNNrocket.rocketadmin"), "", "", "icon_dashboard_16px.gif", adminurl, false, SecurityAccessLevel.Edit, true, false);
                    actions.Add(GetNextActionID(), DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/API/App_LocalResources/", "DNNrocket.rocketadmintab"), "", "", "icon_dashboard_16px.gif", adminurl, false, SecurityAccessLevel.Edit, true, true);
                }
                actions.Add(GetNextActionID(), DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/API/App_LocalResources/", "DNNrocket.clearmodcache"), "", "", "action_refresh.gif", "?clearmodcache=" + ModuleId, false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocket/API/App_LocalResources/", "DNNrocket.clearallcache"), "", "", "action_refresh.gif", "?clearallcache=" + ModuleId, false, SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion



    }

}

using System;
using System.Web.UI.WebControls;
using System.Xml;
using DNNrocketAPI.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;
using Simplisity;

namespace SystemView
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase, IUpgradeable
    {
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
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
        }

        protected override void OnPreRender(EventArgs e)
        {

            var strOut = "";
            var appThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = appThemeSystem.GetTemplate("ActiveSystems.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, null, null, null, true);
            if (pr.StatusCode != "00") 
                strOut = pr.ErrorMsg;
            else
                strOut = pr.RenderedText;

            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);
        }

        public string UpgradeModule(string Version)
        {
            // See: DNNrocketModuleController.
            // We include here to action the upgrade in the module controller.
            return "";
        }


        #endregion

    }

}

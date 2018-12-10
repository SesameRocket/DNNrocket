﻿
using System.Linq;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Runtime.Remoting;
using System.Web;
using NBrightDNN;
using Simplisity;

namespace DNNrocketAPI
{

    public abstract class APInterface
	{

		#region "Shared/Static Methods"

		// singleton reference to the instantiated object 

	    private static Dictionary<string, APInterface> _providerList; 
        // constructor
        static APInterface()
		{
			CreateProvider();
		}

		// dynamically create provider
		private static void CreateProvider()
		{

			string providerName = null;

		    _providerList = new Dictionary<string, APInterface>();

            var pluginData = new PluginData(0);
		    var l = pluginData.GetAjaxProviders(false);

		    foreach (var p in l)
		    {
                try
                {
                    var prov = p.Value;
                    ObjectHandle handle = null;
                    handle = Activator.CreateInstance(prov.GetXmlProperty("genxml/textbox/assembly"), prov.GetXmlProperty("genxml/textbox/namespaceclass"));
                    var objProvider = (APInterface)handle.Unwrap();
                    var ctrlkey = prov.GetXmlProperty("genxml/textbox/ctrl");
                    var lp = 1;
                    while (_providerList.ContainsKey(ctrlkey))
                    {
                        ctrlkey = ctrlkey + lp.ToString("");
                        lp += 1;
                    }
                    objProvider.Ajaxkey = ctrlkey;
                    _providerList.Add(ctrlkey, objProvider);
                }
                catch (Exception ex)
                {
                    // ignore, we may possibly have plugin data without assembly.
                }
		    }
		}


		// return the provider
        public static APInterface Instance(string ctrlkey)
		{
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            CreateProvider(); // plugin not found, so reload and search again.
            if (_providerList.ContainsKey(ctrlkey)) return _providerList[ctrlkey];
            return null;
		}

		#endregion

        public abstract string Ajaxkey { get; set; }

        public abstract string ProcessCommand(string paramCmd, SimplisityInfo sInfo, string editlang = "");

        public abstract void Validate();

    }

}


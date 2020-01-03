﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;

namespace DNNrocket.Documents
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private string _appthemeRelPath;
        private string _appthemeMapPath;
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private CommandSecurity _commandSecurity;
        private DNNrocketInterface _rocketInterface;

        public override Dictionary<string, string> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = "ERROR"; // return ERROR if not matching commands.

            paramCmd = paramCmd.ToLower();

            _rocketInterface = new DNNrocketInterface(interfaceInfo);

            var appPath = _rocketInterface.TemplateRelPath;
            if (appPath == "") appPath = "/DesktopModules/DNNrocket/Documents";
            _appthemeRelPath = appPath;
            _appthemeMapPath = DNNrocketUtils.MapPath(_appthemeRelPath);
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _commandSecurity = new CommandSecurity(-1, -1, _rocketInterface);
            _commandSecurity.AddCommand("rocketdocs_upload", true);
            _commandSecurity.AddCommand("rocketdocs_delete", true);
            _commandSecurity.AddCommand("rocketdocs_list", false);

            if (!_commandSecurity.HasSecurityAccess(paramCmd))
            {
                strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, DNNrocketUtils.GetCurrentUserId());
                return ReturnString(strOut);
            }

            switch (paramCmd)
            {
                case "rocketdocs_upload":
                    UploadDocumentToFolder();
                    strOut = ListData();
                    break;
                case "rocketdocs_delete":
                    DeleteDocs();
                    strOut = ListData();
                    break;
                case "rocketdocs_list":
                    strOut = ListData();
                    break;
            }

            return ReturnString(strOut);
        }

        public Dictionary<string, string> ReturnString(string strOut, string jsonOut = "")
        {
            var rtnDic = new Dictionary<string, string>();
            rtnDic.Add("outputhtml", strOut);
            rtnDic.Add("outputjson", jsonOut);
            return rtnDic;
        }


        public String ListData()
        {
            try
            {
                return DNNrocketUtils.RenderDocumentSelect(_paramInfo.GetXmlPropertyBool("genxml/hidden/singleselect"), _paramInfo.GetXmlPropertyBool("genxml/hidden/autoreturn"), _paramInfo.GetXmlProperty("genxml/hidden/documentfolder"));
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string UploadDocumentToFolder()
        {
            var userid = DNNrocketUtils.GetCurrentUserId(); // prefix to filename on upload.

            var documentfolder = _paramInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (documentfolder == "") documentfolder = "docs";
            var docDirectory = DNNrocketUtils.HomeDNNrocketDirectoryMapPath() + "\\" + documentfolder;
            if (!Directory.Exists(docDirectory)) Directory.CreateDirectory(docDirectory);

            var strOut = "";
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(DNNrocketUtils.TempDirectoryMapPath() + "\\" + userfilename, docDirectory + "\\" + friendlyname,true);
                        File.Delete(DNNrocketUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }

            }

            return strOut;
        }

        public void DeleteDocs()
        {
            var docfolder = _postInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (docfolder == "") docfolder = "docs";
            var docDirectory = DNNrocketUtils.HomeDNNrocketDirectoryMapPath() + "\\" + docfolder;
            var docList = _postInfo.GetXmlProperty("genxml/hidden/dnnrocket-documentlist").Split(';');
            foreach (var i in docList)
            {
                if (i != "")
                {
                    var documentname = GeneralUtils.DeCode(i);
                    var docFile = docDirectory + "\\" + documentname;
                    if (File.Exists(docFile))
                    {
                        File.Delete(docFile);
                    }
                }
            }

        }



    }
}

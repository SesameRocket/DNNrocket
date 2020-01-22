﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNNrocketAPI.Componants
{
    public class OnlineAppThemeIndex
    {
        private const string AppSystemFolderRel = "/DesktopModules/DNNrocket/SystemThemes";
        private FtpConnect _ftpConnect;
        /// <summary>
        /// The Index is a list of XML file, which contain the dat from AppTheme.
        /// This is normally used to ensure we have an image on the fileystsem to display the appThemes from the Online AppTheme server.
        /// It is updated when the Online AppThemes are looked at, if no xml file exists for hte apptheme, it will be downlaoded from the online server.
        /// </summary>
        /// <param name="systemKey"></param>
        /// <param name="OnlineIndexType">"public" or "private"</param>
        public OnlineAppThemeIndex(string systemKey, string onlineIndexType)
        {
            OnlineIndexType = onlineIndexType.ToLower();
            IndexFolderMapPath = DNNrocketUtils.MapPath(AppSystemFolderRel) + "\\" + systemKey + "_" + OnlineIndexType + "Index";
            if (!Directory.Exists(IndexFolderMapPath)) Directory.CreateDirectory(IndexFolderMapPath);
            ListData = new Dictionary<string, SimplisityInfo>();
            LoadLocalIndex();
            _ftpConnect = new FtpConnect(systemKey);
        }
        private void LoadLocalIndex()
        {
            foreach (var f in Directory.GetFiles(IndexFolderMapPath, "*.xml"))
            {
                var xmlData = FileUtils.ReadFile(f);
                var sInfo = new SimplisityInfo();
                sInfo.FromXmlItem(xmlData);
                var fname = Path.GetFileName(f);
                if (ListData.ContainsKey(fname)) ListData.Remove(fname);
                ListData.Add(fname, sInfo);
            }
        }
        public void DeleteAll()
        {
            try
            {
                foreach (var f in ListData)
                {
                    var fMapPath = IndexFolderMapPath + "\\" + f.Key;
                    File.Delete(fMapPath);
                }
                Directory.Delete(IndexFolderMapPath, true);
            }
            catch (Exception exc)
            {
                DNNrocketUtils.LogException(exc);
            }

        }
        public void DeleteIndex(string appThemeFolder)
        {
            try
            {
                var fname = appThemeFolder + ".xml";
                var fMapPath = IndexFolderMapPath + "\\" + fname;
                ListData.Remove(fname);
                File.Delete(fMapPath);
            }
            catch (Exception exc)
            {
                DNNrocketUtils.LogException(exc);
            }

        }
        public SimplisityInfo GetIndexData(string appThemeFolder)
        {
            var filename = appThemeFolder + ".xml";
            if (ListData.ContainsKey(filename)) return ListData[filename];

            var fMapPath = IndexFolderMapPath + "\\" + filename;
            if (_ftpConnect.IsValid)
            {
                // download theme xml with image and save image
                _ftpConnect.DownloadAppThemeXmlToFile(appThemeFolder, fMapPath);
            }
            if (File.Exists(fMapPath))
            {
                var xmlData = FileUtils.ReadFile(fMapPath);
                var sInfo = new SimplisityInfo();
                sInfo.FromXmlItem(xmlData);
                var fname = Path.GetFileName(fMapPath);
                if (ListData.ContainsKey(fname)) ListData.Remove(fname);
                ListData.Add(fname, sInfo);
                return sInfo;
            }
            return new SimplisityInfo(); ;
        }

        public string GetLogoBase64String(string appThemeFolder)
        {
            var sInfo = GetIndexData(appThemeFolder);
            return sInfo.GetXmlProperty("genxml/hidden/logobase64");
        }

        public Dictionary<string, SimplisityInfo> ListData { set; get; }
        public string IndexFolderMapPath { set; get; }
        public string OnlineIndexType { set; get; }

    }
}

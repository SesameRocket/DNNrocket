﻿using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Rocket.AppThemes.Componants
{

    public class AppThemeDataList
    {

        public AppThemeDataList(string selectedsystemkey)
        {
            AppProjectFolderRel = "/DesktopModules/DNNrocket/AppThemes";
            AssignFolders();

            PopulateSystemFolderList();

            SelectedSystemKey = selectedsystemkey;
            
            if (List.Count == 0) PopulateAppThemeList();
        }
        private void AssignFolders()
        {
            AppProjectFolderMapPath = DNNrocketUtils.MapPath(AppProjectFolderRel);

            AppProjectThemesFolderRel = AppProjectFolderRel + "/Themes";
            AppProjectThemesFolderMapPath = DNNrocketUtils.MapPath(AppProjectThemesFolderRel);

            AppSystemThemeFolderRootRel = AppProjectFolderRel + "/SystemThemes";
            AppSystemThemeFolderRootMapPath = DNNrocketUtils.MapPath(AppSystemThemeFolderRootRel);
        }
        public void PopulateAppThemeList()
        {
            List = new List<AppTheme>();
            var dirlist = System.IO.Directory.GetDirectories(AppSystemThemeFolderRootMapPath + "\\" + SelectedSystemKey);
            foreach (var d in dirlist)
            {
                var dr = new System.IO.DirectoryInfo(d);
                var appTheme = new AppTheme(SelectedSystemKey, dr.Name, "");
                List.Add(appTheme);
            }
        }
        public void PopulateSystemFolderList()
        {
            SystemFolderList = new List<SystemInfoData>();
            var dirlist2 = System.IO.Directory.GetDirectories(AppSystemThemeFolderRootMapPath);
            foreach (var d in dirlist2)
            {
                var dr = new System.IO.DirectoryInfo(d);
                var systemInfoData = new SystemInfoData(dr.Name);
                if (systemInfoData.Exists) SystemFolderList.Add(systemInfoData);
            }
        }

        public void ClearCache()
        {
            var cachekey = "SelectedSystemKey*" + DNNrocketUtils.GetCurrentUserId();
            CacheUtils.RemoveCache(cachekey);
            cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
            CacheUtils.RemoveCache(cachekey);
            cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
            CacheUtils.RemoveCache(cachekey);
            PopulateSystemFolderList();
            PopulateAppThemeList();
        }

        public void RemoveSelectedSystemKey()
        {
            var cachekey = "SelectedSystemKey*" + DNNrocketUtils.GetCurrentUserId();
            CacheUtils.RemoveCache(cachekey);
        }

        public string AppProjectFolderRel { get; set; }
        public string AppProjectFolderMapPath { get; set; }
        public string AppSystemThemeFolderRootRel { get; set; }
        public string AppSystemThemeFolderRootMapPath { get; set; }
        public string AppProjectThemesFolderRel { get; set; }
        public string AppProjectThemesFolderMapPath { get; set; }
        public string SelectedSystemKey {
            get {
                var cachekey = "SelectedSystemKey*" + DNNrocketUtils.GetCurrentUserId();
                if (CacheUtils.GetCache(cachekey) == null) return "";
                return CacheUtils.GetCache(cachekey).ToString(); ;
            }
            set {
                if (value != "")
                {
                    var cachekey = "SelectedSystemKey*" + DNNrocketUtils.GetCurrentUserId();
                    CacheUtils.SetCache(cachekey, value);
                    PopulateSystemFolderList();
                    PopulateAppThemeList();
                }
            }
        }
        public List<AppTheme> List {
            get
            {
                var cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
                if (CacheUtils.GetCache(cachekey) == null) return new List<AppTheme>();
                return (List<AppTheme>)CacheUtils.GetCache(cachekey);
            }
            set
            {
                var cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
                CacheUtils.SetCache(cachekey, value);
            }
        }
        public List<SystemInfoData> SystemFolderList {
            get
            {
                var cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
                if (CacheUtils.GetCache(cachekey) == null) return new List<SystemInfoData>();
                return (List<SystemInfoData>)CacheUtils.GetCache(cachekey);
            }
            set
            {
                var cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
                CacheUtils.SetCache(cachekey, value);
            }
        }

    }

}

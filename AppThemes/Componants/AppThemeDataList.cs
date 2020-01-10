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

            AppSystemThemeFolderRootRel = "/DesktopModules/DNNrocket/SystemThemes";
            AppSystemThemeFolderRootMapPath = DNNrocketUtils.MapPath(AppSystemThemeFolderRootRel);

            if (!Directory.Exists(AppSystemThemeFolderRootMapPath))
            {
                Directory.CreateDirectory(AppSystemThemeFolderRootMapPath);
                Directory.CreateDirectory(AppSystemThemeFolderRootMapPath.TrimEnd('\\') + "\\dnnrocketmodule"); // included in default install
            }

        }
        public void PopulateAppThemeList()
        {
            List = new List<AppTheme>();
            if (SelectedSystemKey != "")
            {
                var themePath = AppSystemThemeFolderRootMapPath + "\\" + SelectedSystemKey;
                if (Directory.Exists(themePath))
                {
                    var dirlist = System.IO.Directory.GetDirectories(themePath);
                    foreach (var d in dirlist)
                    {
                        var dr = new System.IO.DirectoryInfo(d);
                        var appTheme = new AppTheme(SelectedSystemKey, dr.Name, "");
                        List.Add(appTheme);
                    }
                }
            }
        }
        public void PopulateSystemFolderList()
        {
            SystemFolderList = new List<SystemData>();
            var dirlist2 = System.IO.Directory.GetDirectories(AppSystemThemeFolderRootMapPath);
            foreach (var d in dirlist2)
            {
                var dr = new System.IO.DirectoryInfo(d);
                var systemData = new SystemData(dr.Name);
                if (systemData.Exists) SystemFolderList.Add(systemData);
            }
        }

        public void ClearCacheLists()
        {
            var cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
            CacheUtils.RemoveCache(cachekey, "apptheme");
            cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
            CacheUtils.RemoveCache(cachekey, "apptheme");
            PopulateSystemFolderList();
            PopulateAppThemeList();
        }

        public void ClearCache()
        {
            SelectedSystemKey = "";
            ClearCacheLists();
        }
        public string AppProjectFolderRel { get; set; }
        public string AppProjectFolderMapPath { get; set; }
        public string AppSystemThemeFolderRootRel { get; set; }
        public string AppSystemThemeFolderRootMapPath { get; set; }
        public string AppProjectThemesFolderRel { get; set; }
        public string AppProjectThemesFolderMapPath { get; set; }
        public string SelectedSystemKey { get; set; }
        public List<AppTheme> List {
            get
            {
                var cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
                if (CacheUtils.GetCache(cachekey, "apptheme") == null) return new List<AppTheme>();
                return (List<AppTheme>)CacheUtils.GetCache(cachekey, "apptheme");
            }
            set
            {
                var cachekey = "AppThemeDataList*" + AppProjectThemesFolderMapPath;
                CacheUtils.SetCache(cachekey, value, "apptheme");
            }
        }
        public List<SystemData> SystemFolderList {
            get
            {
                var cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
                if (CacheUtils.GetCache(cachekey, "apptheme") == null) return new List<SystemData>();
                return (List<SystemData>)CacheUtils.GetCache(cachekey, "apptheme");
            }
            set
            {
                var cachekey = "AppThemeDataList*" + AppSystemThemeFolderRootMapPath;
                CacheUtils.SetCache(cachekey, value, "apptheme");
            }
        }

    }

}

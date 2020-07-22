﻿using DotNetNuke.Services.Exceptions;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNNrocketAPI.Componants
{
    public class LogUtils
    {
        //  --------------------- Debug Log files ------------------------------
        public static void LogDebug(string message)
        {
            var mappath = PortalUtils.TempDirectoryMapPath().TrimEnd('\\') + "\\debug";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            FileUtils.AppendToLog(mappath, "debug", message);
        }
        public static void LogDebugClear()
        {
            var mappath = PortalUtils.TempDirectoryMapPath().TrimEnd('\\') + "\\debug";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            System.IO.DirectoryInfo di = new DirectoryInfo(mappath);
            foreach (System.IO.FileInfo file in di.GetFiles())
            {

                file.Delete();
            }
        }
        /// <summary>
        /// Output a data file, if the given name to the Portal \DNNrocketTemp\debug folder.
        /// </summary>
        /// <param name="outFileName">Name of file</param>
        /// <param name="content">content of file</param>
        public static void OutputDebugFile(string outFileName, string content)
        {
            var mappath = PortalUtils.TempDirectoryMapPath().TrimEnd('\\') + "\\debug";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            FileUtils.SaveFile(mappath + "\\" + outFileName, content);
        }

        public static void LogTracking(string message, string logName = "Log")
        {
            var mappath = PortalUtils.HomeDNNrocketDirectoryMapPath().TrimEnd('\\') + "\\logs";
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            FileUtils.AppendToLog(mappath, logName, message);
        }
        public static string LogException(Exception exc)
        {
            Exceptions.LogException(exc);
            return exc.ToString();
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayActive.Engine.App.Helpers
{
    public static class FileProcessing
    {
        public static string ReadFromFile(string path)
        {
            string jsonStringMousePort;
            try
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(@"C:\ProgramData\SteelSeries\SteelSeries Engine 3\coreProps.json");
                jsonStringMousePort = file.ReadLine();
            }
            catch (Exception ex)
            {
                jsonStringMousePort = null;
                string currentMethodName = Helpers.ErrorHandling.GetCurrentMethodName();
                Helpers.ErrorHandling.LogErrorToTxtFile(ex, currentMethodName);
            }
            return jsonStringMousePort;
        }
    }
}
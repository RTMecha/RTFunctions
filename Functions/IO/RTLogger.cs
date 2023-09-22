using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions.IO
{
    public static class RTLogger
    {
        public static string GetClassName(Type type)
        {
            string s = type.ToString();

            s = s.Substring(s.LastIndexOf('.') + 1, -(s.LastIndexOf('.') + 1 - s.Length));

            switch (s)
            {
                case "EditorPlugin":
                    {
                        string version = "1.0.0";
                        if (ModCompatibility.mods.ContainsKey("EditorManagement"))
                            version = ModCompatibility.mods["EditorManagement"].version;

                        return $"[<color=#F6AC1A>Editor</color><color=#2FCBD6>Management</color>] {version}\n";
                    }
                default:
                    {
                        return $"[<color=#0E36FD>RT<color=#4FBDD1>Functions</color>] {FunctionsPlugin.VersionNumber}";
                    }
            }
        }

        public static void LogFormat(Type type, string format, params object[] args) => Debug.LogFormat($"{GetClassName(type)}{format}", args);
    }
}

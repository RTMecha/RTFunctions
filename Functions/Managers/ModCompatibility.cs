using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using BeatmapObject = DataManager.GameData.BeatmapObject;
using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;
using Prefab = DataManager.GameData.Prefab;

namespace RTFunctions.Functions.Managers
{
    public class ModCompatibility : MonoBehaviour
    {
        //Move this to RTFunctions.Functions.Managers

        public static ModCompatibility inst;

        public static GameObject bepinex;

        public static Dictionary<string, object> sharedFunctions = new Dictionary<string, object>();

        void Awake()
        {
            inst = this;

            bepinex = GameObject.Find("BepInEx_Manager");

            if (bepinex.GetComponentByName("ExplorerBepInPlugin"))
            {
                try
                {
                    var ue = bepinex.GetComponentByName("ExplorerBepInPlugin");

                    var mod = new Mod(ue, ue.GetType());

                    //var consoleController = AccessTools.TypeByName("UnityExplorer.CSConsole.ConsoleController");

                    //if (consoleController != null)
                    //{
                    //    var csmod = new Mod(null, consoleController);
                    //    csmod.Methods.Add("Evaluate", consoleController.GetMethod("Evaluate", new Type[] { typeof(string), typeof(bool) }).CreateDelegate(consoleController));
                    //    mod.components.Add("ConsoleController", csmod);
                    //}

                    mods.Add("UnityExplorer", mod);
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("{0}Error.\nMessage: {1}\nStackTrace: {2}", FunctionsPlugin.className, ex.Message, ex.StackTrace);
                }
            }
        }

        public static Dictionary<string, Mod> mods = new Dictionary<string, Mod>();

        public class Mod
        {
            public Mod(object inst, Type type)
            {
                this.inst = inst;
                this.type = type;
            }

            public object inst;
            public Type type;

            public Dictionary<string, Mod> components = new Dictionary<string, Mod>();

            Dictionary<string, Delegate> methods = new Dictionary<string, Delegate>();
            public Dictionary<string, Delegate> Methods
            {
                get => methods;
                set => methods = value;
            }

            public string version = "1.0.0";
            string className;
            public string ClassName
            {
                get => className;
                private set
                {
                    className = $"[{value}] {version}\n";
                }
            }

            public void Invoke(string name, params object[] values)
            { if (Methods.ContainsKey(name)) Methods[name].DynamicInvoke(values); }
        }
    }
}

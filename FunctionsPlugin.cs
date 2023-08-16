using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using LSFunctions;
using SimpleJSON;

using RTFunctions.Functions;
using RTFunctions.Patchers;
using RTFunctions.Enums;

namespace RTFunctions
{
	[BepInPlugin("com.mecha.rtfunctions", "RT Functions", " 1.2.2")]
	[BepInProcess("Project Arrhythmia.exe")]
	public class FunctionsPlugin : BaseUnityPlugin
	{
		//EnumPatcher from https://github.com/SlimeRancherModding/SRML

		//Updates:

		public static FunctionsPlugin inst;
		public static string className = "[<color=#0E36FD>RT<color=#4FBDD1>Functions</color>] " + PluginInfo.PLUGIN_VERSION + "\n";
		private readonly Harmony harmony = new Harmony("rtfunctions");

		public static ConfigEntry<KeyCode> OpenPAFolder { get; set; }
		public static ConfigEntry<KeyCode> OpenPAPersistentFolder { get; set; }

		private static ConfigEntry<bool> DebugsOn { get; set; }
		public static ConfigEntry<bool> IncreasedClipPlanes { get; set; }
		private static ConfigEntry<string> DisplayName { get; set; }

		public static string displayName;

		private void Awake()
		{
			inst = this;

			DebugsOn = Config.Bind("Debugging", "Enabled", false, "If disabled, turns all Unity debug logs off. Might boost performance.");
			IncreasedClipPlanes = Config.Bind("Game", "Camera Clip Planes", true, "Increases the clip panes to a very high amount, allowing for object render depth to go really high or really low.");
			DisplayName = Config.Bind("User", "Display Name", "Player", "Sets the username to show in levels and menus.");
			OpenPAFolder = Config.Bind("File", "Open Project Arrhythmia Folder", KeyCode.F3, "Opens the folder containing the Project Arrhythmia application and all files related to it.");
			OpenPAPersistentFolder = Config.Bind("File", "Open LocalLow Folder", KeyCode.F4, "Opens the data folder all instances of PA share containing the log files and copied prefab (if you have EditorManagement installed)");

			displayName = DisplayName.Value;

			player.sprName = displayName;

			Config.SettingChanged += new EventHandler<SettingChangedEventArgs>(UpdateSettings);

			harmony.PatchAll(typeof(FunctionsPlugin));
			harmony.PatchAll(typeof(DataManagerPatch));
			harmony.PatchAll(typeof(DataManagerGameDataPatch));
			harmony.PatchAll(typeof(DataManagerBeatmapThemePatch));
			harmony.PatchAll(typeof(DataManagerBeatmapObjectPatch));
			harmony.PatchAll(typeof(DataManagerPrefabPatch));
			harmony.PatchAll(typeof(GameManagerPatch));
			harmony.PatchAll(typeof(ObjectManagerPatch));
			harmony.PatchAll(typeof(SaveManagerPatch));

			Logger.LogInfo($"Plugin RT Functions is loaded!");
		}

		private static void UpdateSettings(object sender, EventArgs e)
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;

			SetCameraRenderDistance();

			displayName = DisplayName.Value;
			DataManager.inst.UpdateSettingString("s_display_name", DisplayName.Value);

			player.sprName = displayName;

			if (SteamWrapper.inst != null)
            {
				SteamWrapper.inst.user.displayName = displayName;
            }

			if (EditorManager.inst != null)
            {
				EditorManager.inst.SetCreatorName(displayName);
            }

			SaveProfile();
		}

		private void Update()
        {
			RTHelpers.screenScale = (float)Screen.width / 1920f;
			RTHelpers.screenScaleInverse = 1f / RTHelpers.screenScale;

			if (!Application.runInBackground)
            {
				Application.runInBackground = true;
			}

			if (Input.GetKeyDown(OpenPAFolder.Value))
            {
				RTFile.OpenInFileBrowser.Open(RTFile.ApplicationDirectory);
            }

			if (Input.GetKeyDown(OpenPAPersistentFolder.Value))
            {
				RTFile.OpenInFileBrowser.Open(RTFile.PersistentApplicationDirectory);
            }
		}

		[HarmonyPatch(typeof(SystemManager), "Awake")]
		[HarmonyPostfix]
		private static void DisableLoggers()
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;
		}


		public static void SetCameraRenderDistance()
		{
			if (GameManager.inst == null)
				return;

			Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			if (IncreasedClipPlanes.Value)
			{
				camera.farClipPlane = 100000;
				camera.nearClipPlane = -100000;
			}
			else
			{
				camera.farClipPlane = 32f;
				camera.nearClipPlane = 0.1f;
			}
		}

		public static void SaveProfile()
        {
			JSONNode jn = JSON.Parse("{}");

			jn["user_data"]["name"] = player.sprName;
			jn["user_data"]["spr-id"] = player.sprID;

			if (!RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile"))
			{
				Directory.CreateDirectory(RTFile.ApplicationDirectory + "profile");
			}
			if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile") && jn != null)
			{
				RTFile.WriteToFile("profile/profile.sep", jn.ToString(3));
			}
		}

		public static void ParseProfile()
        {
			if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile"))
			{
				string rawProfileJSON = FileManager.inst.LoadJSONFile("profile/profile.sep");

				if (!string.IsNullOrEmpty(rawProfileJSON))
				{
					JSONNode jn = JSON.Parse(rawProfileJSON);

					if (!string.IsNullOrEmpty(jn["user_data"]["name"]))
					{
						player.sprName = jn["user_data"]["name"];
					}

					if (!string.IsNullOrEmpty(jn["user_data"]["spr-id"]))
					{
						player.sprID = jn["user_data"]["spr-id"];
					}
				}

				DisplayName.Value = player.sprName;
			}
		}

		public static List<Universe> universes = new List<Universe>
		{
			new Universe(Universe.UniDes.Chardax, "000"),
		};

		public static User player = new User(displayName, UnityEngine.Random.Range(0, ulong.MaxValue).ToString(), new Universe(Universe.UniDes.MUS));

		public class User
        {
			public User(string _sprName, string _sprID, Universe _universe)
            {
				sprName = _sprName;
				sprID = _sprID;
				universe = _universe;
			}

			public string sprName = "Null";
			public string sprID = "0";
			public Universe universe;
        }

		public class Universe
        {
			public Universe()
            {
				uniDes = (UniDes)UnityEngine.Random.Range(0, 3);
				uniNum = string.Format("{0:000}", UnityEngine.Random.Range(0, int.MaxValue));

				for (int i = 0; i < UnityEngine.Random.Range(0, 10); i++)
                {
					timelines.Add(new Timeline(UnityEngine.Random.Range(0f, 9999999f)));
                }
            }

			public Universe(UniDes uniDes)
            {
				this.uniDes = uniDes;
				uniNum = string.Format("{0:000}", UnityEngine.Random.Range(0, int.MaxValue));

				Debug.LogFormat("{0}UniNum: {1}", className, uniNum);

				timelines = new List<Timeline>();
				for (int i = 0; i < UnityEngine.Random.Range(0, 10); i++)
                {
					timelines.Add(new Timeline(UnityEngine.Random.Range(0f, 9999999f)));
				}

				Debug.LogFormat("{0}Timeline Count: {1}", className, timelines.Count);
			}

			public Universe(string name, UniDes uniDes, string uniNum)
            {
				this.name = name;
				this.uniDes = uniDes;
				this.uniNum = uniNum;
            }

			public Universe(UniDes _unidDes, string _uniNum)
            {
				uniDes = _unidDes;
				uniNum = _uniNum;
			}

			public string name;
			public UniDes uniDes;
			public string uniNum = "000";
			public List<Timeline> timelines;

			public enum UniDes
			{
				Chardax,
				Genark,
				Archmo,
				MUS
			}

			public class Timeline
            {
				public Timeline(float _frq)
                {
					fq = _frq;
                }

				public float fq;
            }
		}
	}
}
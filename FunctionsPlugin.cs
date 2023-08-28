﻿using System;
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
using DG.Tweening;

using RTFunctions.Functions.Managers;
using RTFunctions.Functions.IO;
using RTFunctions.Functions;
using RTFunctions.Patchers;
using RTFunctions.Enums;

using Application = UnityEngine.Application;
using Screen = UnityEngine.Screen;

namespace RTFunctions
{
	[BepInPlugin("com.mecha.rtfunctions", "RT Functions", " 1.3.0")]
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

		#region Fullscreen

		public static ConfigEntry<bool> Fullscreen { get; set; }

		static bool FullscreenProp
        {
			get
            {
				return DataManager.inst.GetSettingBool("FullScreen", false);
            }
			set
            {
				DataManager.inst.UpdateSettingBool("FullScreen", value);
				SaveManager.inst.ApplyVideoSettings();
				SaveManager.inst.UpdateSettingsFile(false);
			}
        }

		public static bool prevFullscreen;

        #endregion

        #region Resolution

		public static ConfigEntry<Resolutions> Resolution { get; set; }

		static Resolutions ResolutionProp
        {
			get
            {
				return (Resolutions)DataManager.inst.GetSettingInt("Resolution_i", 0);
			}
			set
			{
				DataManager.inst.UpdateSettingInt("Resolution_i", (int)value);

				var res = DataManager.inst.resolutions[(int)value];

				DataManager.inst.UpdateSettingFloat("Resolution_x", res.x);
				DataManager.inst.UpdateSettingFloat("Resolution_y", res.y);

				SaveManager.inst.ApplyVideoSettings();
				SaveManager.inst.UpdateSettingsFile(false);
			}
        }

		public static Resolutions prevResolution;

        #endregion

        #region MasterVol

		public static ConfigEntry<int> MasterVol { get; set; }

		static int MasterVolProp
        {
            get
            {
				return DataManager.inst.GetSettingInt("MasterVolume", 9);
			}
			set
            {
				DataManager.inst.UpdateSettingInt("MasterVolume", value);

				SaveManager.inst.UpdateSettingsFile(false);
			}
        }

		public static int prevMasterVol;

		#endregion

		#region MusicVol

		public static ConfigEntry<int> MusicVol { get; set; }

		static int MusicVolProp
		{
			get
			{
				return DataManager.inst.GetSettingInt("MusicVolume", 9);
			}
			set
			{
				DataManager.inst.UpdateSettingInt("MusicVolume", value);

				SaveManager.inst.UpdateSettingsFile(false);
			}
		}

		public static int prevMusicVol;

        #endregion

        #region SFXVol

        public static ConfigEntry<int> SFXVol { get; set; }

		static int SFXVolProp
		{
			get
			{
				return DataManager.inst.GetSettingInt("EffectsVolume", 9);
			}
			set
			{
				DataManager.inst.UpdateSettingInt("EffectsVolume", value);

				SaveManager.inst.UpdateSettingsFile(false);
			}
		}

		public static int prevSFXVol;

		#endregion

		#region Language
		
		public enum Lang
        {
			english,
			spanish,
			japanese,
			thai,
			russian,
			pirate
        }

		public static ConfigEntry<Lang> Language { get; set; }

		static Lang LanguageProp
		{
			get
			{
				return (Lang)DataManager.inst.GetCurrentLanguageEnum();
			}
			set
			{
				DataManager.inst.GetSettingInt("Language_i", (int)value);

				SaveManager.inst.UpdateSettingsFile(false);
			}
		}

		public static Lang prevLanguage;

		#endregion

		void Awake()
		{
			inst = this;

			DebugsOn = Config.Bind("Debugging", "Enabled", false, "If disabled, turns all Unity debug logs off. Might boost performance.");
			IncreasedClipPlanes = Config.Bind("Game", "Camera Clip Planes", true, "Increases the clip panes to a very high amount, allowing for object render depth to go really high or really low.");
			DisplayName = Config.Bind("User", "Display Name", "Player", "Sets the username to show in levels and menus.");
			OpenPAFolder = Config.Bind("File", "Open Project Arrhythmia Folder", KeyCode.F3, "Opens the folder containing the Project Arrhythmia application and all files related to it.");
			OpenPAPersistentFolder = Config.Bind("File", "Open LocalLow Folder", KeyCode.F4, "Opens the data folder all instances of PA share containing the log files and copied prefab (if you have EditorManagement installed)");
			Fullscreen = Config.Bind("Settings", "Fullscreen", false);
			Resolution = Config.Bind("Settings", "Resolution", Resolutions.p720);
			MasterVol = Config.Bind("Settings", "Volume Master", 9, new ConfigDescription("Total volume.", new AcceptableValueRange<int>(0, 9)));
			MusicVol = Config.Bind("Settings", "Volume Music", 9, new ConfigDescription("Music volume.", new AcceptableValueRange<int>(0, 9)));
			SFXVol = Config.Bind("Settings", "Volume SFX", 9, new ConfigDescription("SFX volume.", new AcceptableValueRange<int>(0, 9)));
			Language = Config.Bind("Settings", "Language", Lang.english, "This is currently here for testing purposes. This version of the game has not been translated yet.");

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

		static void UpdateSettings(object sender, EventArgs e)
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;

			SetCameraRenderDistance();

			//Display Name
			{
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
			}

			if (prevFullscreen != Fullscreen.Value)
			{
				prevFullscreen = Fullscreen.Value;
				FullscreenProp = Fullscreen.Value;
			}

			if (prevResolution != Resolution.Value)
            {
				prevResolution = Resolution.Value;
				ResolutionProp = Resolution.Value;
            }

			if (prevMasterVol != MasterVol.Value)
            {
				prevMasterVol = MasterVol.Value;
				MasterVolProp = MasterVol.Value;
            }

			if (prevMusicVol != MusicVol.Value)
            {
				prevMusicVol = MusicVol.Value;
				MusicVolProp = MusicVol.Value;
            }

			if (prevSFXVol != SFXVol.Value)
            {
				prevSFXVol = SFXVol.Value;
				SFXVolProp = SFXVol.Value;
            }

			if (prevLanguage != Language.Value)
            {
				prevLanguage = Language.Value;
				LanguageProp = Language.Value;
            }

			SaveProfile();
		}

		void Update()
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
		private static void DisableLoggers() => Debug.unityLogger.logEnabled = DebugsOn.Value;

		[HarmonyPatch(typeof(SystemManager), "Update")]
		[HarmonyPrefix]
		private static bool SystemManagerUpdatePrefix()
		{
			if ((Input.GetKeyDown(KeyCode.P) && !LSHelpers.IsUsingInputField()) || (Input.GetKeyDown(KeyCode.F9) && !LSHelpers.IsUsingInputField()))
			{
				TakeScreenshot();
			}
			if (Input.GetKeyDown(KeyCode.F11) && !LSHelpers.IsUsingInputField())
			{
				Fullscreen.Value = !Fullscreen.Value;

				//DataManager.inst.UpdateSettingBool("FullScreen", !DataManager.inst.GetSettingBool("FullScreen"));
				//SaveManager.inst.ApplyVideoSettings();
			}
			return false;
        }

        [HarmonyPatch(typeof(EditorManager), "Start")]
        [HarmonyPostfix]
        static void EditorStartPostfix(EditorManager __instance)
        {
            __instance.SetCreatorName(DisplayName.Value);
			if (SteamWrapper.inst != null)
				SteamWrapper.inst.user.displayName = DisplayName.Value;
		}

        public static void TakeScreenshot()
		{
			string directory = RTFile.ApplicationDirectory + "screenshots";
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var file = directory + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".png";
			ScreenCapture.CaptureScreenshot(file, 1);
			Debug.LogFormat("{0}Took Screenshot! - {1}", className, file);

			inst.StartCoroutine(ScreenshotNotification());
		}

		static IEnumerator ScreenshotNotification()
		{
			yield return new WaitForSeconds(0.1f);

			// In-Game Screenshot notification
			{
				var scr = ScreenCapture.CaptureScreenshotAsTexture();

				AudioManager.inst.PlaySound("glitch");

				var inter = new GameObject("Canvas");
				inter.transform.localScale = Vector3.one * RTHelpers.screenScale;
				//inter.AddComponent<SpriteManager>();
				//menuUI = inter;
				var interfaceRT = inter.AddComponent<RectTransform>();
				interfaceRT.anchoredPosition = new Vector2(960f, 540f);
				interfaceRT.sizeDelta = new Vector2(1920f, 1080f);
				interfaceRT.pivot = new Vector2(0.5f, 0.5f);
				interfaceRT.anchorMin = Vector2.zero;
				interfaceRT.anchorMax = Vector2.zero;

				var canvas = inter.AddComponent<Canvas>();
				canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
				canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
				canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Tangent;
				canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal;
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.scaleFactor = RTHelpers.screenScale;

				var canvasScaler = inter.AddComponent<CanvasScaler>();
				canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

				Debug.LogFormat("{0}Canvas Scale Factor: {1}\nResoultion: {2}", className, canvas.scaleFactor, new Vector2(Screen.width, Screen.height));

				inter.AddComponent<GraphicRaycaster>();

				var imageObj = new GameObject("image");
				imageObj.transform.SetParent(inter.transform);
				imageObj.transform.localScale = Vector3.one;


				var imageRT = imageObj.AddComponent<RectTransform>();
				imageRT.anchoredPosition = new Vector2(850f, -480f);
				imageRT.sizeDelta = new Vector2(scr.width / 10f, scr.height / 10f);

				var im = imageObj.AddComponent<Image>();
				im.sprite = Sprite.Create(scr, new Rect(0f, 0f, scr.width, scr.height), Vector2.zero);

				var textObj = new GameObject("text");
				textObj.transform.SetParent(imageObj.transform);
				textObj.transform.localScale = Vector3.one;

				var textRT = textObj.AddComponent<RectTransform>();
				textRT.anchoredPosition = new Vector2(0f, 20f);
				textRT.sizeDelta = new Vector2(200f, 100f);

				var text = textObj.AddComponent<Text>();
				text.font = Font.GetDefault();
				text.text = "Took Screenshot!";

				var tween = DOTween.To(delegate (float x)
				{
					im.color = new Color(1f, 1f, 1f, x);
					text.color = new Color(1f, 1f, 1f, x);
				}, 1f, 0f, 1.5f).SetEase(DataManager.inst.AnimationList[2].Animation);

				DOTween.To(delegate (float x)
				{
					imageRT.anchoredPosition = new Vector2(850f, x);
				}, -480f, -600f, 1.5f).SetEase(DataManager.inst.AnimationList[8].Animation);

				tween.OnComplete(delegate ()
				{
					scr = null;

					Destroy(inter);
				});
			}

			yield break;
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
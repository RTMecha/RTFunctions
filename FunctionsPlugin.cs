using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

using LSFunctions;
using SimpleJSON;

using RTFunctions.Functions;
using RTFunctions.Functions.Animation;
using RTFunctions.Functions.Animation.Keyframe;
using RTFunctions.Functions.Managers;
using RTFunctions.Functions.Optimization;
using RTFunctions.Functions.Optimization.Objects;
using RTFunctions.Functions.IO;
using RTFunctions.Patchers;

using Application = UnityEngine.Application;
using Screen = UnityEngine.Screen;
using Ease = RTFunctions.Functions.Animation.Ease;
using Version = RTFunctions.Functions.Version;
using System.Linq;

namespace RTFunctions
{
	/// <summary>
	/// Base plugin for initializing all the patches.
	/// </summary>
	[BepInPlugin("com.mecha.rtfunctions", "RT Functions", " 1.8.4")]
	[BepInProcess("Project Arrhythmia.exe")]
	public class FunctionsPlugin : BaseUnityPlugin
	{
		// Animation and object system from https://github.com/Reimnop/Catalyst

		/// <summary>
		/// Path where all the plugins are stored.
		/// </summary>
		public static string BepInExPluginsPath => "BepInEx/plugins/";

		/// <summary>
		/// Path where all the mod-specific assets are stored.
		/// </summary>
		public static string BepInExAssetsPath => $"{BepInExPluginsPath}Assets/";

		/// <summary>
		/// For future reference.
		/// </summary>
		public static Version CurrentVersion => new Version(PluginInfo.PLUGIN_VERSION);

		/// <summary>
		/// We'll need the instance as we don't want to use GetComponent.
		/// </summary>
		public static FunctionsPlugin inst;

		/// <summary>
		/// Since most PA classes have a "className" for logging I decided to give some of mine a unique one.
		/// </summary>
		public static string className = "[<color=#0E36FD>RT<color=#4FBDD1>Functions</color>] " + PluginInfo.PLUGIN_VERSION + "\n";
		public static readonly Harmony harmony = new Harmony("rtfunctions");

        #region Configs

        public static ConfigEntry<KeyCode> OpenPAFolder { get; set; }
		public static ConfigEntry<KeyCode> OpenPAPersistentFolder { get; set; }

		public static ConfigEntry<bool> DebugsOn { get; set; }
		public static ConfigEntry<bool> IncreasedClipPlanes { get; set; }
		private static ConfigEntry<string> DisplayName { get; set; }

		public static string displayName;

		public static ConfigEntry<bool> NotifyREPL { get; set; }

		public static ConfigEntry<bool> BGReactiveLerp { get; set; }

		public static ConfigEntry<bool> LDM { get; set; }

		public static ConfigEntry<bool> AntiAliasing { get; set; }

		public static ConfigEntry<KeyCode> ScreenshotKey { get; set; }

		public static ConfigEntry<string> ScreenshotsPath { get; set; }

		public static ConfigEntry<bool> DiscordShowLevel { get; set; }

		#endregion

		// PA Settings
		#region Fullscreen

		public static ConfigEntry<bool> Fullscreen { get; set; }

		static void SetFullscreen(bool value)
		{
			prevFullscreen = Fullscreen.Value;

			DataManager.inst.UpdateSettingBool("FullScreen", value);
			SaveManager.inst.ApplyVideoSettings();
			SaveManager.inst.UpdateSettingsFile(false);
		}

        //static bool FullscreenProp
        //{
        //    get
        //    {
        //        return DataManager.inst.GetSettingBool("FullScreen", false);
        //    }
        //    set
        //    {
        //        DataManager.inst.UpdateSettingBool("FullScreen", value);
        //        SaveManager.inst.ApplyVideoSettings();
        //        SaveManager.inst.UpdateSettingsFile(false);
        //    }
        //}

        public static bool prevFullscreen;

        #endregion

        #region Resolution

		public static ConfigEntry<Resolutions> Resolution { get; set; }

		static void SetResolution(Resolutions value)
		{
			prevResolution = Resolution.Value;

			DataManager.inst.UpdateSettingInt("Resolution_i", (int)value);

			var res = DataManager.inst.resolutions[(int)value];

			DataManager.inst.UpdateSettingFloat("Resolution_x", res.x);
			DataManager.inst.UpdateSettingFloat("Resolution_y", res.y);

			SaveManager.inst.ApplyVideoSettings();
			SaveManager.inst.UpdateSettingsFile(false);
		}

        //static Resolutions ResolutionProp
        //{
        //    get
        //    {
        //        return (Resolutions)DataManager.inst.GetSettingInt("Resolution_i", 0);
        //    }
        //    set
        //    {
        //        DataManager.inst.UpdateSettingInt("Resolution_i", (int)value);

        //        var res = DataManager.inst.resolutions[(int)value];

        //        DataManager.inst.UpdateSettingFloat("Resolution_x", res.x);
        //        DataManager.inst.UpdateSettingFloat("Resolution_y", res.y);

        //        SaveManager.inst.ApplyVideoSettings();
        //        SaveManager.inst.UpdateSettingsFile(false);
        //    }
        //}

        public static Resolutions prevResolution;

        #endregion

        #region MasterVol

		public static ConfigEntry<int> MasterVol { get; set; }

		static void SetMasterVol(int value)
		{
			prevMasterVol = MasterVol.Value;

			DataManager.inst.UpdateSettingInt("MasterVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

        //static int MasterVolProp
        //{
        //    get
        //    {
        //        return DataManager.inst.GetSettingInt("MasterVolume", 9);
        //    }
        //    set
        //    {
        //        DataManager.inst.UpdateSettingInt("MasterVolume", value);

        //        SaveManager.inst.UpdateSettingsFile(false);
        //    }
        //}

        public static int prevMasterVol;

		#endregion

		#region MusicVol

		public static ConfigEntry<int> MusicVol { get; set; }

		static void SetMusicVol(int value)
		{
			prevMusicVol = MusicVol.Value;

			DataManager.inst.UpdateSettingInt("MusicVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		//static int MusicVolProp
		//{
		//	get
		//	{
		//		return DataManager.inst.GetSettingInt("MusicVolume", 9);
		//	}
		//	set
		//	{
		//		DataManager.inst.UpdateSettingInt("MusicVolume", value);

		//		SaveManager.inst.UpdateSettingsFile(false);
		//	}
		//}

		public static int prevMusicVol;

        #endregion

        #region SFXVol

        public static ConfigEntry<int> SFXVol { get; set; }

		static void SetSFXVol(int value)
		{
			prevSFXVol = SFXVol.Value;

			DataManager.inst.UpdateSettingInt("EffectsVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		//static int SFXVolProp
		//{
		//	get
		//	{
		//		return DataManager.inst.GetSettingInt("EffectsVolume", 9);
		//	}
		//	set
		//	{
		//		DataManager.inst.UpdateSettingInt("EffectsVolume", value);

		//		SaveManager.inst.UpdateSettingsFile(false);
		//	}
		//}

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

		static void SetLanguage(Lang value)
		{
			prevLanguage = Language.Value;

			DataManager.inst.GetSettingInt("Language_i", (int)value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		//static Lang LanguageProp
		//{
		//	get
		//	{
		//		return (Lang)DataManager.inst.GetCurrentLanguageEnum();
		//	}
		//	set
		//	{
		//		DataManager.inst.GetSettingInt("Language_i", (int)value);

		//		SaveManager.inst.UpdateSettingsFile(false);
		//	}
		//}

		public static Lang prevLanguage;

		#endregion

		#region Controller Rumble

		public static ConfigEntry<bool> ControllerRumble { get; set; }

		static void SetControllerRumble(bool value)
		{
			prevControllerRumble = ControllerRumble.Value;

			DataManager.inst.UpdateSettingBool("ControllerVibrate", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		//static bool ControllerRumbleProp
		//{
		//	get
		//	{
		//		return DataManager.inst.GetSettingBool("ControllerVibrate", true);
		//	}
		//	set
		//	{
		//		DataManager.inst.UpdateSettingBool("ControllerVibrate", value);

		//		SaveManager.inst.UpdateSettingsFile(false);
		//	}
		//}

		public static bool prevControllerRumble;
		
		#endregion

		void Awake()
		{
			inst = this;

			DebugsOn = Config.Bind("Debugging", "Enabled", true, "If disabled, turns all Unity debug logs off. Might boost performance.");
			NotifyREPL = Config.Bind("Debugging", "Notify REPL", false, "If in editor, code ran will have their results be notified.");

			ScreenshotsPath = Config.Bind("Game", "Screenshot Path", "screenshots", "The path to save screenshots to.");
			ScreenshotKey = Config.Bind("Game", "Screenshot Key", KeyCode.P, "The key to press to take a screenshot.");
			AntiAliasing = Config.Bind("Game", "Anti-Aliasing", true, "If antialiasing is on or not.");
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
			ControllerRumble = Config.Bind("Settings", "Controller Vibrate", true, "If the controllers should vibrate or not.");
			BGReactiveLerp = Config.Bind("Level Backgrounds", "Reactive Color Lerp", true, "If on, reactive color will lerp from base color to reactive color. Otherwise, the reactive color will be added to the base color.");
			LDM = Config.Bind("Level", "Low Detail Mode", false, "If enabled, any objects with \"LDM\" on will not be rendered.");
			DiscordShowLevel = Config.Bind("Discord", "Show Level Status", true, "Level name is shown.");

			displayName = DisplayName.Value;

			player.sprName = displayName;

			Config.SettingChanged += new EventHandler<SettingChangedEventArgs>(UpdateSettings);

			// Patchers
			{
				//harmony.PatchAll();
				harmony.PatchAll(typeof(FunctionsPlugin));
				harmony.PatchAll(typeof(BackgroundManagerPatch));
				harmony.PatchAll(typeof(DataManagerPatch));
				harmony.PatchAll(typeof(DataManagerGameDataPatch));
				harmony.PatchAll(typeof(DataManagerBeatmapObjectPatch));
				harmony.PatchAll(typeof(DataManagerPrefabPatch));
				harmony.PatchAll(typeof(DiscordControllerPatch));
				harmony.PatchAll(typeof(GameManagerPatch));
				harmony.PatchAll(typeof(InputDataManagerPatch));
				harmony.PatchAll(typeof(InputSelectManagerPatch));
				harmony.PatchAll(typeof(MyGameActionsPatch));
				harmony.PatchAll(typeof(ObjectManagerPatch));
				harmony.PatchAll(typeof(PlayerPatch));
				harmony.PatchAll(typeof(SaveManagerPatch));
			}

			// Hooks
			{
				GameManagerPatch.LevelStart += Updater.OnLevelStart;
				GameManagerPatch.LevelEnd += Updater.OnLevelEnd;
				ObjectManagerPatch.LevelTick += Updater.OnLevelTick;
			}

			System.Windows.Forms.Application.ApplicationExit += delegate (object sender, EventArgs e)
			{
				if (EditorManager.inst && EditorManager.inst.hasLoadedLevel && !EditorManager.inst.loading)
				{
					string str = RTFile.BasePath;
					string modBackup = RTFile.ApplicationDirectory + str + "level-quit-backup.lsb";
					if (RTFile.FileExists(modBackup))
						File.Delete(modBackup);

					//string lvl = RTFile.ApplicationDirectory + str + "level.lsb";
					//if (RTFile.FileExists(lvl))
					//	File.Copy(lvl, modBackup);

					StartCoroutine(DataManager.inst.SaveData(modBackup));
				}
			};

			Application.quitting += delegate ()
			{
				if (EditorManager.inst && EditorManager.inst.hasLoadedLevel && !EditorManager.inst.loading)
				{
					string str = RTFile.BasePath;
					string modBackup = RTFile.ApplicationDirectory + str + "level-quit-unity-backup.lsb";
					if (RTFile.FileExists(modBackup))
						File.Delete(modBackup);

					//string lvl = RTFile.ApplicationDirectory + str + "level.lsb";
					//if (RTFile.FileExists(lvl))
					//	File.Copy(lvl, modBackup);

					StartCoroutine(DataManager.inst.SaveData(modBackup));
				}
			};

			//SequenceManager.Init();

			Logger.LogInfo($"Plugin RT Functions is loaded!");
		}

		static void UpdateSettings(object sender, EventArgs e)
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;

			SetCameraRenderDistance();
			SetAntiAliasing();

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
				SetFullscreen(Fullscreen.Value);
				//FullscreenProp = Fullscreen.Value;
			}

			if (prevResolution != Resolution.Value)
            {
				SetResolution(Resolution.Value);
				//ResolutionProp = Resolution.Value;
            }

			if (prevMasterVol != MasterVol.Value)
            {
				SetMasterVol(MasterVol.Value);
				//MasterVolProp = MasterVol.Value;
            }

			if (prevMusicVol != MusicVol.Value)
            {
				SetMusicVol(MusicVol.Value);
				//MusicVolProp = MusicVol.Value;
            }

			if (prevSFXVol != SFXVol.Value)
            {
				SetSFXVol(SFXVol.Value);
				//SFXVolProp = SFXVol.Value;
            }

			if (prevLanguage != Language.Value)
            {
				SetLanguage(Language.Value);
				//LanguageProp = Language.Value;
            }
			
			if (prevControllerRumble != ControllerRumble.Value)
            {
				SetControllerRumble(ControllerRumble.Value);
				//ControllerRumbleProp = ControllerRumble.Value;
            }

			UpdateDiscordStatus(discordLevel, discordDetails, discordIcon, discordArt);

			SaveProfile();
		}

		void Update()
        {
			RTHelpers.screenScale = (float)Screen.width / 1920f;
			RTHelpers.screenScaleInverse = 1f / RTHelpers.screenScale;

			if (!Application.runInBackground)
				Application.runInBackground = true;

			if (!LSHelpers.IsUsingInputField())
			{
				if (Input.GetKeyDown(OpenPAFolder.Value))
					RTFile.OpenInFileBrowser.Open(RTFile.ApplicationDirectory);

				if (Input.GetKeyDown(OpenPAPersistentFolder.Value))
					RTFile.OpenInFileBrowser.Open(RTFile.PersistentApplicationDirectory);

				if (Input.GetKeyDown(KeyCode.I))
					Debug.LogFormat("{0}Objects alive: {1}", className, DataManager.inst.gameData.beatmapObjects.FindAll(x => x.TimeWithinLifespan()).Count);
			}
		}

        #region Patchers

        [HarmonyPatch(typeof(SystemManager), "Awake")]
		[HarmonyPostfix]
		static void DisableLoggers()
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;
		}

        [HarmonyPatch(typeof(SystemManager), "Update")]
		[HarmonyPrefix]
		static bool SystemManagerUpdatePrefix()
		{
			if (Input.GetKeyDown(ScreenshotKey.Value) && !LSHelpers.IsUsingInputField())
				TakeScreenshot();

			if (Input.GetKeyDown(KeyCode.F11) && !LSHelpers.IsUsingInputField())
				Fullscreen.Value = !Fullscreen.Value;

			return false;
        }

        [HarmonyPatch(typeof(EditorManager), "Start")]
        [HarmonyPostfix]
        static void EditorStartPostfix(EditorManager __instance)
        {
            __instance.SetCreatorName(DisplayName.Value);
			if (SteamWrapper.inst)
				SteamWrapper.inst.user.displayName = DisplayName.Value;
		}

		[HarmonyPatch(typeof(LSText), "randomString")]
		[HarmonyPrefix]
		static bool randomStringPrefix(int length, ref string __result)
		{
			string text = "";
			char[] array = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*_+{}|:<>?,.;'[]▓▒░▐▆▉☰☱☲☳☴☵☶☷►▼◄▬▩▨▧▦▥▤▣▢□■¤ÿòèµ¶™ßÃ®¾ð¥œ⁕(◠‿◠✿)".ToCharArray();
			while (text.Length < length)
			{
				text += array[UnityEngine.Random.Range(0, array.Length)].ToString();
			}
			__result = text;
			return false;
		}

		[HarmonyPatch(typeof(EventManager), "updateTheme")]
		[HarmonyPrefix]
		static bool updateTheme(EventManager __instance, float _theme)
		{
			if (!ModCompatibility.mods.ContainsKey("EventsCore"))
			{
				var beatmapTheme = Functions.Data.BeatmapTheme.DeepCopy((Functions.Data.BeatmapTheme)GameManager.inst.LiveTheme);
				((Functions.Data.BeatmapTheme)GameManager.inst.LiveTheme).Lerp((Functions.Data.BeatmapTheme)DataManager.inst.GetTheme(__instance.LastTheme),
					(Functions.Data.BeatmapTheme)DataManager.inst.GetTheme(__instance.NewTheme), _theme);

				if (beatmapTheme != GameManager.inst.LiveTheme)
					GameManager.inst.UpdateTheme();
			}
			else
				EventsCoreUpdateThemePrefix?.Invoke(__instance, _theme);

			return false;
		}

		public static Action<GameManager> EventsCoreGameThemePrefix { get; set; }
		public static Action<EventManager, float> EventsCoreUpdateThemePrefix { get; set; }

		#endregion

		public static void TakeScreenshot()
		{
			string directory = RTFile.ApplicationDirectory + ScreenshotsPath.Value;
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

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
				text.font = FontManager.inst.Inconsolata;
				text.text = "Took Screenshot!";

				var animation = new AnimationManager.Animation("Screenshot Notification");
				animation.colorAnimations = new List<AnimationManager.Animation.AnimationObject<Color>>
				{
					new AnimationManager.Animation.AnimationObject<Color>(new List<IKeyframe<Color>>
					{
						new ColorKeyframe(0f, Color.white, Ease.Linear),
						new ColorKeyframe(1.5f, new Color(1f, 1f, 1f, 0f), Ease.SineIn),
					}, delegate (Color x)
					{
						if (im)
							im.color = x;
						if (text)
							text.color = x;
					}),
				};
				animation.vector2Animations = new List<AnimationManager.Animation.AnimationObject<Vector2>>
				{
					new AnimationManager.Animation.AnimationObject<Vector2>(new List<IKeyframe<Vector2>>
					{
						new Vector2Keyframe(0f, new Vector2(850f, -480f), Ease.Linear),
						new Vector2Keyframe(1.5f, new Vector2(850f, -600f), Ease.BackIn)
					}, delegate (Vector2 x)
					{
						imageRT.anchoredPosition = x;
					}, delegate ()
					{
						scr = null;

						Destroy(inter);

						AnimationManager.inst?.RemoveID(animation.id);
					}),
				};

				AnimationManager.inst?.Play(animation);
            }

			yield break;
        }

		/// <summary>
		/// For setting mostly unlimited render depth range.
		/// </summary>
        public static void SetCameraRenderDistance()
		{
			if (GameManager.inst == null)
				return;

			var camera = Camera.main;
			camera.farClipPlane = IncreasedClipPlanes.Value ? 100000 : 32f;
			camera.nearClipPlane = IncreasedClipPlanes.Value ? -100000 : 0.1f;
		}

		public static void SetAntiAliasing()
        {
			if (GameStorageManager.inst && GameStorageManager.inst.postProcessLayer)
			{
				GameStorageManager.inst.postProcessLayer.antialiasingMode
					= AntiAliasing.Value ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
			}
		}

		public static string discordLevel = "";
		public static string discordDetails = "";
		public static string discordIcon = "";
		public static string discordArt = "";
		public static void UpdateDiscordStatus(string level, string details, string icon, string art = "pa_logo_white")
        {
			DiscordController.inst.OnStateChange(DiscordShowLevel.Value ? level : "");
			DiscordController.inst.OnArtChange(art);
			DiscordController.inst.OnIconChange(icon);
			DiscordController.inst.OnDetailsChange(details);

			discordLevel = level;
			discordDetails = details;
			discordIcon = icon;
			discordArt = art;

			DiscordRpc.UpdatePresence(DiscordController.inst.presence);
		}

		public static void SaveProfile()
		{
			var jn = JSON.Parse("{}");

			jn["user_data"]["name"] = player.sprName;
			jn["user_data"]["spr-id"] = player.sprID;

			if (!RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile"))
				Directory.CreateDirectory(RTFile.ApplicationDirectory + "profile");
			RTFile.WriteToFile("profile/profile.sep", jn.ToString(3));
		}

		public static void ParseProfile()
        {
			if (RTFile.DirectoryExists(RTFile.ApplicationDirectory + "profile"))
			{
				string rawProfileJSON = RTFile.ReadFromFile(RTFile.ApplicationDirectory + "profile/profile.sep");

				if (!string.IsNullOrEmpty(rawProfileJSON))
				{
					var jn = JSON.Parse(rawProfileJSON);

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
			new Universe("Axiom Nexus", Universe.UniDes.Chardax, "000"),
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
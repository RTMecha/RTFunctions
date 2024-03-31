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
using System.IO.Compression;

namespace RTFunctions
{
	/// <summary>
	/// Base plugin for initializing all the patches.
	/// </summary>
	[BepInPlugin("com.mecha.rtfunctions", "RT Functions", " 1.11.2")]
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
		public static string className = "[<color=#0E36FD>RT</color><color=#4FBDD1>Functions</color>] " + PluginInfo.PLUGIN_VERSION + "\n";
		public static readonly Harmony harmony = new Harmony("rtfunctions");

		public static Material blur;
		public static Material GetBlur()
		{
			var assetBundle = AssetBundle.LoadFromFile(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/objectmaterials.asset");
			var assetToLoad = assetBundle.LoadAsset<Material>("blur.mat");
			var blurMat = Instantiate(assetToLoad);
			assetBundle.Unload(false);

			return blurMat;
		}

		#region Configs

		public static ConfigEntry<KeyCode> OpenPAFolder { get; set; }
		public static ConfigEntry<KeyCode> OpenPAPersistentFolder { get; set; }

		public static ConfigEntry<bool> DebugsOn { get; set; }
		public static ConfigEntry<bool> AllowControlsInputField { get; set; }
		public static ConfigEntry<bool> IncreasedClipPlanes { get; set; }
		private static ConfigEntry<string> DisplayName { get; set; }

		public static string displayName;

		public static ConfigEntry<bool> DebugInfo { get; set; }
		public static ConfigEntry<bool> DebugInfoStartup { get; set; }
		public static ConfigEntry<KeyCode> DebugInfoToggleKey { get; set; }
		public static ConfigEntry<bool> NotifyREPL { get; set; }

		public static ConfigEntry<bool> BGReactiveLerp { get; set; }

		public static ConfigEntry<bool> LDM { get; set; }

		public static ConfigEntry<bool> AntiAliasing { get; set; }

		public static ConfigEntry<KeyCode> ScreenshotKey { get; set; }

		public static ConfigEntry<string> ScreenshotsPath { get; set; }
		public static ConfigEntry<bool> UseNewUpdateMethod { get; set; }

		public static ConfigEntry<bool> DiscordShowLevel { get; set; }

		public static ConfigEntry<bool> EnableVideoBackground { get; set; }
		public static ConfigEntry<bool> RunInBackground { get; set; }

		public static ConfigEntry<bool> EvaluateCode { get; set; }
		public static ConfigEntry<bool> ReplayLevel { get; set; }
		public static ConfigEntry<bool> PrioritizeVG { get; set; }

		public static ConfigEntry<string> DiscordRichPresenceID { get; set; }

		#endregion

		#region Default Settings

		public static ConfigEntry<bool> Fullscreen { get; set; }

		public static ConfigEntry<Resolutions> Resolution { get; set; }

		public static ConfigEntry<int> MasterVol { get; set; }

		public static ConfigEntry<int> MusicVol { get; set; }

		public static ConfigEntry<int> SFXVol { get; set; }

		public static ConfigEntry<ModLanguage> Language { get; set; }

		public static ConfigEntry<bool> ControllerRumble { get; set; }

		static void SetFullscreen(bool value)
		{
			prevFullscreen = Fullscreen.Value;

			DataManager.inst.UpdateSettingBool("FullScreen", value);
			SaveManager.inst.ApplyVideoSettings();
			SaveManager.inst.UpdateSettingsFile(false);
		}

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

		static void SetMasterVol(int value)
		{
			prevMasterVol = MasterVol.Value;

			DataManager.inst.UpdateSettingInt("MasterVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		static void SetMusicVol(int value)
		{
			prevMusicVol = MusicVol.Value;

			DataManager.inst.UpdateSettingInt("MusicVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		static void SetSFXVol(int value)
		{
			prevSFXVol = SFXVol.Value;

			DataManager.inst.UpdateSettingInt("EffectsVolume", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		static void SetLanguage(ModLanguage value)
		{
			prevLanguage = Language.Value;

			DataManager.inst.UpdateSettingInt("Language_i", (int)value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		static void SetControllerRumble(bool value)
		{
			prevControllerRumble = ControllerRumble.Value;

			DataManager.inst.UpdateSettingBool("ControllerVibrate", value);

			SaveManager.inst.UpdateSettingsFile(false);
		}

		public static bool prevFullscreen;

		public static Resolutions prevResolution;

		public static int prevMasterVol;

		public static int prevMusicVol;

		public static int prevSFXVol;

		public static ModLanguage prevLanguage;

		public static bool prevControllerRumble;

		#endregion

		void Awake()
		{
			inst = this;

			DebugsOn = Config.Bind("Debugging", "Enabled", true, "If disabled, turns all Unity debug logs off. Might boost performance.");
			DebugInfo = Config.Bind("Debugging", "Show Debug Info", false, "Shows a helpful info overlay with some information about the current gamestate.");
			DebugInfoStartup = Config.Bind("Debugging", "Create Debug Info", false, "If the Debug Info menu should be created on game start. Requires restart to have this option take affect.");
			DebugInfoToggleKey = Config.Bind("Debugging", "Show Debug Info Toggle Key", KeyCode.F6, "Shows a helpful info overlay with some information about the current gamestate.");
			NotifyREPL = Config.Bind("Debugging", "Notify REPL", false, "If in editor, code ran will have their results be notified.");

			AllowControlsInputField = Config.Bind("Game", "Allow Controls While Using InputField", true, "If you have this off, the player will not move while an InputField is being used.");
			UseNewUpdateMethod = Config.Bind("Game", "Use New Update Method", true, "Possibly releases the fixed framerate of the game.");
            UseNewUpdateMethod.SettingChanged += UseNewUpdateMethodChanged;
			ScreenshotsPath = Config.Bind("Game", "Screenshot Path", "screenshots", "The path to save screenshots to.");
			ScreenshotKey = Config.Bind("Game", "Screenshot Key", KeyCode.F2, "The key to press to take a screenshot.");
			AntiAliasing = Config.Bind("Game", "Anti-Aliasing", true, "If antialiasing is on or not.");
			RunInBackground = Config.Bind("Game", "Run In Background", true, "If you want the game to continue playing when minimized.");
			IncreasedClipPlanes = Config.Bind("Game", "Camera Clip Planes", true, "Increases the clip panes to a very high amount, allowing for object render depth to go really high or really low.");
			EnableVideoBackground = Config.Bind("Game", "Video Backgrounds", false, "If on, the old video BG feature returns, though somewhat buggy. Requires a bg.mp4 file to exist in the level folder.");
			EvaluateCode = Config.Bind("Game", "Evaluate Custom Code", false, "If custom written code should evaluate. Turn this on if you're sure the level you're using isn't going to mess anything up with a code Modifier or custom player code.");
			ReplayLevel = Config.Bind("Game", "Replay Level in Background After Completion", true, "When completing a level, having this on will replay the level with no players in the background of the end screen.");
			PrioritizeVG = Config.Bind("Game", "Priotize VG format", true, "Due to LS file formats also being in level folders with VG formats, VG format will need to be prioritized, though you can turn this off if a VG level isn't working and it has a level.lsb file.");

			DisplayName = Config.Bind("User", "Display Name", "Player", "Sets the username to show in levels and menus.");

			OpenPAFolder = Config.Bind("File", "Open Project Arrhythmia Folder", KeyCode.F4, "Opens the folder containing the Project Arrhythmia application and all files related to it.");
			OpenPAPersistentFolder = Config.Bind("File", "Open LocalLow Folder", KeyCode.F5, "Opens the data folder all instances of PA share containing the log files and copied prefab (if you have EditorManagement installed)");

			Fullscreen = Config.Bind("Settings", "Fullscreen", false);
			Resolution = Config.Bind("Settings", "Resolution", Resolutions.p720);
			MasterVol = Config.Bind("Settings", "Volume Master", 8, new ConfigDescription("Total volume.", new AcceptableValueRange<int>(0, 9)));
			MusicVol = Config.Bind("Settings", "Volume Music", 9, new ConfigDescription("Music volume.", new AcceptableValueRange<int>(0, 9)));
			SFXVol = Config.Bind("Settings", "Volume SFX", 9, new ConfigDescription("SFX volume.", new AcceptableValueRange<int>(0, 9)));
			Language = Config.Bind("Settings", "Language", ModLanguage.English, "This is currently here for testing purposes. This version of the game has not been translated yet.");
			ControllerRumble = Config.Bind("Settings", "Controller Vibrate", true, "If the controllers should vibrate or not.");

			BGReactiveLerp = Config.Bind("Level Backgrounds", "Reactive Color Lerp", true, "If on, reactive color will lerp from base color to reactive color. Otherwise, the reactive color will be added to the base color.");

			LDM = Config.Bind("Level", "Low Detail Mode", false, "If enabled, any objects with \"LDM\" on will not be rendered.");
			DiscordShowLevel = Config.Bind("Discord", "Show Level Status", true, "Level name is shown.");
			DiscordRichPresenceID = Config.Bind("Discord", "Status ID (READ DESC)", "1176264603374735420", "Only change if you already have your own custom Discord app status setup.");

			displayName = DisplayName.Value;

			player.sprName = displayName;

			Updater.UseNewUpdateMethod = UseNewUpdateMethod.Value;

			DisplayName.SettingChanged += DisplayNameChanged;
            Fullscreen.SettingChanged += DefaultSettingsChanged;
			Resolution.SettingChanged += DefaultSettingsChanged;
			MasterVol.SettingChanged += DefaultSettingsChanged;
			MusicVol.SettingChanged += DefaultSettingsChanged;
			SFXVol.SettingChanged += DefaultSettingsChanged;
			Language.SettingChanged += DefaultSettingsChanged;
			ControllerRumble.SettingChanged += DefaultSettingsChanged;
            LDM.SettingChanged += LDMChanged;
            DiscordShowLevel.SettingChanged += DiscordChanged;
			Config.SettingChanged += new EventHandler<SettingChangedEventArgs>(UpdateSettings);

			blur = GetBlur();

			// Patchers
			{
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
				harmony.PatchAll(typeof(SoundLibraryPatch));
				harmony.PatchAll(typeof(SteamManagerPatch));
				harmony.PatchAll(typeof(SteamWrapperAchievementsPatch));
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
					string modBackup = str + "level-quit-backup.lsb";
					if (RTFile.FileExists(modBackup))
						File.Delete(modBackup);

					StartCoroutine(DataManager.inst.SaveData(modBackup));
				}
			};

			Application.quitting += delegate ()
			{
				if (EditorManager.inst && EditorManager.inst.hasLoadedLevel && !EditorManager.inst.loading)
				{
					string str = RTFile.BasePath;
					string modBackup = str + "level-quit-unity-backup.lsb";
					if (RTFile.FileExists(modBackup))
						File.Delete(modBackup);

					StartCoroutine(DataManager.inst.SaveData(modBackup));
				}
			};

			Logger.LogInfo($"Plugin RT Functions is loaded!");
		}

		void Update()
		{
			RTHelpers.screenScale = (float)Screen.width / 1920f;
			RTHelpers.screenScaleInverse = 1f / RTHelpers.screenScale;

			Application.runInBackground = RunInBackground.Value;

			if (!LSHelpers.IsUsingInputField())
			{
				if (Input.GetKeyDown(OpenPAFolder.Value))
					RTFile.OpenInFileBrowser.Open(RTFile.ApplicationDirectory);

				if (Input.GetKeyDown(OpenPAPersistentFolder.Value))
					RTFile.OpenInFileBrowser.Open(RTFile.PersistentApplicationDirectory);

				if (Input.GetKeyDown(DebugInfoToggleKey.Value))
					DebugInfo.Value = !DebugInfo.Value;
			}

			RTDebugger.Update();
		}

        #region Settings Changed

        void DiscordChanged(object sender, EventArgs e)
		{
			UpdateDiscordStatus(discordLevel, discordDetails, discordIcon, discordArt);
		}

		void LDMChanged(object sender, EventArgs e)
		{
			if (EditorManager.inst)
			{
				var list = Functions.Data.GameData.Current.BeatmapObjects.Where(x => x.LDM).ToList();
				for (int i = 0; i < list.Count; i++)
				{
					Updater.UpdateProcessor(list[i]);
				}
			}
		}

		void DefaultSettingsChanged(object sender, EventArgs e)
		{
			if (prevFullscreen != Fullscreen.Value)
				SetFullscreen(Fullscreen.Value);

			if (prevResolution != Resolution.Value)
				SetResolution(Resolution.Value);

			if (prevMasterVol != MasterVol.Value)
				SetMasterVol(MasterVol.Value);

			if (prevMusicVol != MusicVol.Value)
				SetMusicVol(MusicVol.Value);

			if (prevSFXVol != SFXVol.Value)
				SetSFXVol(SFXVol.Value);

			if (prevLanguage != Language.Value)
				SetLanguage(Language.Value);

			if (prevControllerRumble != ControllerRumble.Value)
				SetControllerRumble(ControllerRumble.Value);
		}

		void DisplayNameChanged(object sender, EventArgs e)
		{
			displayName = DisplayName.Value;
			DataManager.inst.UpdateSettingString("s_display_name", DisplayName.Value);

			player.sprName = displayName;

			if (SteamWrapper.inst != null)
				SteamWrapper.inst.user.displayName = displayName;

			EditorManager.inst?.SetCreatorName(displayName);

			SaveProfile();
		}

		void UseNewUpdateMethodChanged(object sender, EventArgs e)
		{
			Updater.UseNewUpdateMethod = UseNewUpdateMethod.Value;
		}

		static void UpdateSettings(object sender, EventArgs e)
		{
			Debug.unityLogger.logEnabled = DebugsOn.Value;

			SetCameraRenderDistance();
			SetAntiAliasing();

			if (RTVideoManager.inst)
			{
				if (RTVideoManager.inst.didntPlay && EnableVideoBackground.Value)
				{
					RTVideoManager.inst.Play(RTVideoManager.inst.currentURL, RTVideoManager.inst.currentAlpha);
				}
			}

			SaveProfile();
		}

        #endregion

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

		[HarmonyPatch(typeof(SceneManager), "DisplayLoadingScreen", new Type[] { typeof(string), typeof(bool) })]
		[HarmonyPrefix]
		static bool LoadScenePrefix(ref IEnumerator __result, string __0)
		{
			bool editor = __0 == "Editor" && !ModCompatibility.EditorManagementInstalled;
			bool arcade = __0 == "Input Select" && !ModCompatibility.ArcadiaCustomsInstalled;

			if (editor || arcade)
			{
				string sc = editor ? "editor" : "arcade";
				string mod = editor ? "EditorManagement" : "ArcadiaCustoms";

				Popup($"Cannot enter {sc} without {mod} installed!", new Color(0.8976f, 0.2f, 0.2f, 1f), "<b>Error!</b>", 3f);
				if (ArcadeManager.inst && ArcadeManager.inst.ic)
					ArcadeManager.inst.ic.SwitchBranch("main_menu");
				__result = Empty();
				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(FileManager), "LoadImageFileRaw", MethodType.Enumerator)]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> LoadImageFileRawTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var match = new CodeMatcher(instructions).Start();

			match = match.RemoveInstructionsInRange(108, 120);

			return match.InstructionEnumeration();
		}

		//[HarmonyPatch(typeof(ZipArchiveEntry), "GetDataCompressor")]
		//[HarmonyPrefix]
		//static bool GetDataCompressorPrefix(ref CheckSumAndSizeWriteStream __result, ZipArchiveEntry __instance, Stream __0, bool __1, EventHandler __2)
		//{
		//    __result = GetDataCompressor(__instance, __0, __1, __2);
		//    return false;
		//}

		[HarmonyPatch(typeof(ZipArchiveEntry), "OpenInWriteMode")]
		[HarmonyPrefix]
		static bool OpenInWriteModePrefix(ref Stream __result, ZipArchiveEntry __instance)
        {
			__result = OpenInWriteMode(__instance);
			return false;
        }

		static Stream OpenInWriteMode(ZipArchiveEntry __instance)
		{
			if (__instance._everOpenedForWrite)
			{
				throw new IOException("Ever opened for write");
			}
			__instance._everOpenedForWrite = true;
			var dataCompressor = GetDataCompressor(__instance, __instance._archive.ArchiveStream, true, delegate (object o, EventArgs e)
			{
				__instance._archive.ReleaseArchiveStream(__instance);
				__instance._outstandingWriteStream = null;
			});
			__instance._outstandingWriteStream = new ZipArchiveEntry.DirectToArchiveWriterStream(dataCompressor, __instance);
			return new WrappedStream(__instance._outstandingWriteStream, delegate (object o, EventArgs e)
			{
				__instance._outstandingWriteStream.Close();
			});
		}

		static CheckSumAndSizeWriteStream GetDataCompressor(ZipArchiveEntry __instance, Stream backingStream, bool leaveBackingStreamOpen, EventHandler onClose)
        {
            var stream = new DeflateStream(backingStream, CompressionMode.Compress, leaveBackingStreamOpen);

            return new CheckSumAndSizeWriteStream(stream, backingStream, leaveBackingStreamOpen && !true, delegate (long initialPosition, long currentPosition, uint checkSum)
            {
                __instance._crc32 = checkSum;
                __instance._uncompressedSize = currentPosition;
                __instance._compressedSize = backingStream.Position - initialPosition;
                onClose?.Invoke(__instance, EventArgs.Empty);
            });
        }

        public static Action<GameManager> EventsCoreGameThemePrefix { get; set; }
		public static Action<EventManager, float> EventsCoreUpdateThemePrefix { get; set; }

        #endregion

        #region Misc Functions

        public static IEnumerator Empty()
        {
			yield break;
        }

		public static string currentPopupID;
		public static GameObject currentPopup;
		public static void Popup(string dialogue, Color bar, string title, float time = 2f, bool destroyPrevious = true)
		{
			if (destroyPrevious && currentPopup)
            {
				if (AnimationManager.inst.animations.Has(x => x.id == currentPopupID))
					AnimationManager.inst.RemoveID(currentPopupID);
				Destroy(currentPopup);

			}

			var inter = new GameObject("Canvas");
			currentPopup = inter;
			inter.transform.localScale = Vector3.one * RTHelpers.screenScale;
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
			canvas.sortingOrder = 1000;

			var canvasScaler = inter.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

			inter.AddComponent<GraphicRaycaster>();

			var imageObj = new GameObject("image");
			imageObj.transform.SetParent(inter.transform);
			imageObj.transform.localScale = Vector3.zero;

			var imageRT = imageObj.AddComponent<RectTransform>();
			imageRT.anchoredPosition = new Vector2(0f, 0f);
			imageRT.sizeDelta = new Vector2(610f, 250f);

			var im = imageObj.AddComponent<Image>();
			im.color = new Color(0.2f, 0.2f, 0.2f, 1f);

			var textObj = new GameObject("text");
			textObj.transform.SetParent(imageObj.transform);
			textObj.transform.localScale = Vector3.one;

			var textRT = textObj.AddComponent<RectTransform>();
			textRT.anchoredPosition = new Vector2(0f, 0f);
			textRT.sizeDelta = new Vector2(590f, 250f);

			var text = textObj.AddComponent<Text>();
			text.font = FontManager.inst.Inconsolata;
			text.text = dialogue;
			text.fontSize = 20;
			text.alignment = TextAnchor.MiddleCenter;

			var top = new GameObject("top");
			top.transform.SetParent(imageRT);
			top.transform.localScale = Vector3.one;

			var topRT = top.AddComponent<RectTransform>();
			topRT.anchoredPosition = new Vector2(0f, 110f);
			topRT.sizeDelta = new Vector2(610f, 32f);

			var topImage = top.AddComponent<Image>();
			topImage.color = bar;

			var titleTextObj = new GameObject("text");
			titleTextObj.transform.SetParent(topRT);
			titleTextObj.transform.localScale = Vector3.one;

			var titleTextRT = titleTextObj.AddComponent<RectTransform>();
			titleTextRT.anchoredPosition = Vector2.zero;
			titleTextRT.sizeDelta = new Vector2(590f, 32f);

			var titleText = titleTextObj.AddComponent<Text>();
			titleText.alignment = TextAnchor.MiddleLeft;
			titleText.font = FontManager.inst.Inconsolata;
			titleText.fontSize = 20;
			titleText.text = title;
			titleText.color = RTHelpers.InvertColorHue(RTHelpers.InvertColorValue(bar));

			var animation = new AnimationManager.Animation("Popup Notification");
			currentPopupID = animation.id;
			animation.floatAnimations = new List<AnimationManager.Animation.AnimationObject<float>>
				{
					new AnimationManager.Animation.AnimationObject<float>(new List<IKeyframe<float>>
					{
						new FloatKeyframe(0f, 0f, Ease.Linear),
						new FloatKeyframe(0.2f, 1f, Ease.BackOut),
						new FloatKeyframe(time + 0.2f, 1f, Ease.Linear),
						new FloatKeyframe(time + 0.7f, 0f, Ease.BackIn),
						new FloatKeyframe(time + 0.8f, 0f, Ease.Linear),
					}, delegate (float x)
					{
						imageObj.transform.localScale = new Vector3(x, x, x);
					}),
				};
			animation.onComplete = delegate ()
			{
				Destroy(inter);

				AnimationManager.inst.RemoveID(animation.id);
			};

			AnimationManager.inst?.Play(animation);
		}

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

        #endregion

        #region Profile

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

        #endregion
    }
}
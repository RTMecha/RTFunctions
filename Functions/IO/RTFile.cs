using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace RTFunctions.Functions.IO
{
	public static class RTFile
	{
		public static string ApplicationDirectory => Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/")) + "/";

		public static string PersistentApplicationDirectory => Application.persistentDataPath;

		//F:/PA_Builds/PA Launcher App/bin/Debug/net6.0-windows/4.1.16-BepInEx-5.4.21/beatmaps/story\CA - Ahead of the Curve [PAA3]\level.ogg
		public static string basePath
		{
			get
			{
				if (GameManager.inst != null && !string.IsNullOrEmpty(GameManager.inst.basePath))
				{
					return GameManager.inst.basePath;
				}
				else
				{
					return SaveManager.inst.ArcadeQueue.AudioFileStr.Replace(ApplicationDirectory, "").Replace("\\", "/").Replace("/level.ogg", "/");
				}
			}
		}

		public static IEnumerator LoadImageFile(string _path, Action<Sprite> action, Action<string> onError)
		{
			if (!File.Exists(_path))
			{
				onError(_path);
			}
			else
			{
				var bytes = File.ReadAllBytes(_path);
				var tex = new Texture2D(256, 256, TextureFormat.RGBA32, true);
				tex.LoadImage(bytes);

				tex.wrapMode = TextureWrapMode.Clamp;
				tex.filterMode = FilterMode.Point;
				tex.Apply();

				action(Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f), 100f));
				tex = null;
			}
			yield break;
		}

		public static IEnumerator LoadMusicFile(string _path, Action<AudioClip> action, Action<string> onError)
		{
			if (!File.Exists(_path))
			{
				onError(_path);
			}
			else
			{
				AudioType audioType;

				string ext = Path.GetExtension(_path);

				if (ext.ToLower() == ".ogg")
				{
					audioType = AudioType.OGGVORBIS;
				}
				else if (ext.ToLower() == ".wav")
				{
					audioType = AudioType.WAV;
				}
				else
				{
					audioType = AudioType.UNKNOWN;
				}

				var www = UnityWebRequestMultimedia.GetAudioClip(_path, audioType);
				yield return www.SendWebRequest();
				if (www.isHttpError)
				{
					Debug.LogWarning("Audio error:" + www.error);
				}
				else
				{
					AudioClip audioClip = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
					action(audioClip);
				}
			}
		}

		public static bool FileExists(string _filePath)
		{
			return !string.IsNullOrEmpty(_filePath) && File.Exists(_filePath);
		}

		public static bool DirectoryExists(string _directoryPath)
		{
			return !string.IsNullOrEmpty(_directoryPath) && Directory.Exists(_directoryPath);
		}

		public static void WriteToFile(string path, string json)
		{
			StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.Write(json);
			streamWriter.Flush();
			streamWriter.Close();
		}

		public static class OpenInFileBrowser
		{
			public static bool IsInMacOS
			{
				get
				{
					return SystemInfo.operatingSystem.IndexOf("Mac OS") != -1;
				}
			}

			public static bool IsInWinOS
			{
				get
				{
					return SystemInfo.operatingSystem.IndexOf("Windows") != -1;
				}
			}

			public static void OpenInMac(string path)
			{
				bool flag = false;
				string text = path.Replace("\\", "/");
				if (Directory.Exists(text))
				{
					flag = true;
				}
				if (!text.StartsWith("\""))
				{
					text = "\"" + text;
				}
				if (!text.EndsWith("\""))
				{
					text += "\"";
				}
				string arguments = (flag ? "" : "-R ") + text;
				try
				{
					Process.Start("open", arguments);
				}
				catch (Win32Exception ex)
				{
					ex.HelpLink = "";
				}
			}

			public static void OpenInWin(string path)
			{
				bool flag = false;
				string text = path.Replace("/", "\\");
				if (Directory.Exists(text))
				{
					flag = true;
				}
				try
				{
					Process.Start("explorer.exe", (flag ? "/root," : "/select,") + text);
				}
				catch (Win32Exception ex)
				{
					ex.HelpLink = "";
				}
			}

			public static void Open(string path)
			{
				if (IsInWinOS)
				{
					OpenInWin(path);
					return;
				}
				if (IsInMacOS)
				{
					OpenInMac(path);
					return;
				}
				OpenInWin(path);
				OpenInMac(path);
			}
		}
	}
}

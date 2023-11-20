using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

using UnityEngine;
using UnityEngine.Networking;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace RTFunctions.Functions.IO
{
	public static class RTFile
	{
		static Assembly _compressionAssembly;
		static Assembly _zipFileAssembly;

		public static Assembly CompressionAssembly
		{
			get
			{
				if (FileExists(ApplicationDirectory + "Project Arrhythmia_Data/Managed/System.IO.Compression.dll") && _compressionAssembly == null)
					_compressionAssembly = Assembly.LoadFile(ApplicationDirectory + "Project Arrhythmia_Data/Managed/System.IO.Compression.dll");

				return _compressionAssembly;
			}
		}

		public static Assembly ZipFileAssembly
		{
			get
			{
				if (FileExists(ApplicationDirectory + "Project Arrhythmia_Data/Managed/System.IO.Compression.ZipFile.dll") && _zipFileAssembly == null)
					_zipFileAssembly = Assembly.LoadFile(ApplicationDirectory + "Project Arrhythmia_Data/Managed/System.IO.Compression.ZipFile.dll");

				return _zipFileAssembly;
			}
		}

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

		public static bool FileExists(string _filePath) => !string.IsNullOrEmpty(_filePath) && File.Exists(_filePath);

		public static bool DirectoryExists(string _directoryPath) => !string.IsNullOrEmpty(_directoryPath) && Directory.Exists(_directoryPath);

		public static void WriteToFile(string path, string json)
		{
			StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.Write(json);
			streamWriter.Flush();
			streamWriter.Close();
		}

		public static string ReadFromFile(string path)
		{
			if (!FileExists(path))
			{
				Debug.LogFormat("{0}Could not load JSON file [{1}]", FunctionsPlugin.className, path);
				return null;
			}
			StreamReader streamReader = new StreamReader(path);
			string result = streamReader.ReadToEnd().ToString();
			streamReader.Close();
			return result;
		}

		public static AudioType GetAudioType(string str)
		{
			var l = str.LastIndexOf('.');

			var fileType = str.Substring(l, -(l - str.Length)).ToLower();

			switch (fileType)
			{
				case ".wav":
					{
						return AudioType.WAV;
					}
				case ".ogg":
					{
						return AudioType.OGGVORBIS;
					}
				case ".mp3":
					{
						return AudioType.MPEG;
					}
			}

			return AudioType.UNKNOWN;
		}

		public static byte[] ReadBytes(Stream input)
		{
			byte[] buffer = new byte[16 * 1024];
			using (var ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		public static float[] ConvertByteToFloat(byte[] array, int length = 4)
		{
			float[] floatArr = new float[array.Length / length];
			for (int i = 0; i < floatArr.Length; i++)
			{
				if (BitConverter.IsLittleEndian)
				{
					Array.Reverse(array, i * 4, 4);
				}
				floatArr[i] = BitConverter.ToSingle(array, i * 4) / 0x80000000;
			}
			return floatArr;
		}

		public static float[] ConvertByteToFloat(byte[] array, BitType bitType, int length)
		{
			float[] floatArr = new float[array.Length / length];

			for (int i = 0; i < floatArr.Length; i++)
			{
				switch (bitType)
                {
					case BitType.Bit16:
                        {
                            floatArr[i] = ((float)BitConverter.ToInt16(array, i * length)) / 32768.0f;
							break;
                        }
					case BitType.Bit32:
                        {
                            floatArr[i] = ((float)BitConverter.ToInt32(array, i * length)) / 2147483648.0f;
							break;
                        }
                }
			}

			return floatArr;
		}

		public static float[] Convert16BitToFloat(byte[] input)
		{
			int inputSamples = input.Length / 2; // 16 bit input, so 2 bytes per sample
			float[] output = new float[inputSamples];
			int outputIndex = 0;
			for (int n = 0; n < inputSamples; n++)
			{
				short sample = BitConverter.ToInt16(input, n * 2);
				output[outputIndex++] = sample / 32768f;
			}
			return output;
		}

		public static float[] Convert24BitToFloat(byte[] input)
		{
			int inputSamples = input.Length / 3; // 24 bit input
			float[] output = new float[inputSamples];
			int outputIndex = 0;
			var temp = new byte[4];
			for (int n = 0; n < inputSamples; n++)
			{
				// copy 3 bytes in
				Array.Copy(input, n * 3, temp, 0, 3);
				int sample = BitConverter.ToInt32(temp, 0);
				output[outputIndex++] = sample / 16777216f;
			}
			return output;
		}
		
		public static float[] Convert32BitToFloat(byte[] input)
		{
			int inputSamples = input.Length / 4; // 24 bit input
			float[] output = new float[inputSamples];
			int outputIndex = 0;
			var temp = new byte[4];
			for (int n = 0; n < inputSamples; n++)
			{
				Array.Copy(input, n * 4, temp, 0, 4);
				int sample = BitConverter.ToInt32(temp, 0);
				output[outputIndex++] = sample / 2147483648f;
			}
			return output;
		}

		public enum BitType
        {
			Bit2,
			Bit4,
			Bit8,
			Bit12,
			Bit16,
			Bit24,
			Bit32,
			Bit64
        }

		public static class ZipUtil
		{
			/*RTFile.Zip("E:/SteamLibrary/steamapps/common/Project Arrhythmia/beatmaps/testzip.zip", new string[]
			 *	{
			 *		"E:/SteamLibrary/steamapps/common/Project Arrhythmia/beatmaps/arcade/test.cs",
			 *	};
			 */

			public static void Zip(string path, string[] files)
			{
				using (var memoryStream = new MemoryStream())
				{
					using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
					{
						foreach (var file in files)
						{
							archive.CreateEntryFromFile(file, Path.GetFileName(file));
						}
					}

					using (var fileStream = new FileStream(path, FileMode.Create))
					{
						memoryStream.Seek(0, SeekOrigin.Begin);
						memoryStream.CopyTo(fileStream);
					}
				}
			}

			// RTFile.UnZip("E:/SteamLibrary/steamapps/common/Project Arrhythmia/beatmaps/testzip.zip", "E:/SteamLibrary/steamapps/common/Project Arrhythmia/beatmaps/testzip output");

			// UPDATE THIS
			public static void UnZip(string path, string output)
			{
				var archive = ZipFile.OpenRead(path);
				archive.ExtractToDirectory(output);
			}

			public static string GetZipString(string path, int file)
			{
				var archive = ZipFile.OpenRead(path);
				var stream = archive.Entries[file].Open();

				var streamReader = new StreamReader(stream);
				string result = streamReader.ReadToEnd().ToString();
				streamReader.Close();

				return result;
			}

			public static Texture2D GetZipImage(string path, int file)
			{
				var bytes = GetZipData(path, file);

				var mem = new MemoryStream(bytes);

				System.Drawing.Image image = System.Drawing.Image.FromStream(mem);

				var texture2d = new Texture2D(image.Width, image.Height, TextureFormat.RGBA32, false);
				texture2d.LoadImage(bytes);

				texture2d.wrapMode = TextureWrapMode.Clamp;
				texture2d.filterMode = FilterMode.Point;
				texture2d.Apply();

				image.Dispose();
				image = null;

				mem.Dispose();
				mem = null;

				return texture2d;
			}

			//public static AudioClip GetZipAudioClip(string path, int file)
			//{
			//	var bytes = GetZipData(path, file);

			//	float[] samples = new float[bytes.Length / 4]; //size of a float is 4 bytes

			//	Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);

			//	int channels = 1; //Assuming audio is mono because microphone input usually is
			//	int sampleRate = 44100; //Assuming your samplerate is 44100 or change to 48000 or whatever is appropriate

			//	AudioClip clip = AudioClip.Create("ZipClip", samples.Length, channels, sampleRate, false);
			//	clip.SetData(samples, 0);

			//	return clip;
			//}

			// Only supports .zip for now
			public static AudioClip GetZipAudioClip(string path, int file)
			{
				var bytes = GetZipData(path, file);

				return GetAudioWAV(bytes, "ZipClip");

				//using (Stream s = new MemoryStream(bytes))
				//{
				//	AudioClip audioClip = AudioClip.Create("ZipClip", bytes.Length, 1, 48000, false);
				//	float[] f = ConvertByteToFloat(bytes);
				//	audioClip.SetData(f, 0);

				//	return audioClip;
				//}
			}

			public static byte[] GetZipData(string path, int file)
			{
				var archive = ZipFile.OpenRead(path);
				var stream = archive.Entries[file].Open();

				var bytes = ReadBytes(stream);

				stream.Close();

				return bytes;
			}
		}

		public static class OpenInFileBrowser
		{
			public static bool IsInMacOS => SystemInfo.operatingSystem.IndexOf("Mac OS") != -1;

			public static bool IsInWinOS => SystemInfo.operatingSystem.IndexOf("Windows") != -1;

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

		// Implemented from https://stackoverflow.com/questions/35228767/noisy-audio-clip-after-decoding-from-base64/68965193#68965193

		readonly struct PcmHeader
		{
			#region Public types & data

			public int BitDepth { get; }
			public int AudioSampleSize { get; }
			public int AudioSampleCount { get; }
			public ushort Channels { get; }
			public int SampleRate { get; }
			public int AudioStartIndex { get; }
			public int ByteRate { get; }
			public ushort BlockAlign { get; }

			#endregion

			#region Constructors & Finalizer

			PcmHeader(int bitDepth,
				int audioSize,
				int audioStartIndex,
				ushort channels,
				int sampleRate,
				int byteRate,
				ushort blockAlign)
			{
				BitDepth = bitDepth;
				_negativeDepth = Mathf.Pow(2f, BitDepth - 1f);
				_positiveDepth = _negativeDepth - 1f;

				AudioSampleSize = bitDepth / 8;
				AudioSampleCount = Mathf.FloorToInt(audioSize / (float)AudioSampleSize);
				AudioStartIndex = audioStartIndex;

				Channels = channels;
				SampleRate = sampleRate;
				ByteRate = byteRate;
				BlockAlign = blockAlign;
			}

			#endregion

			#region Public Methods

			public static PcmHeader FromBytes(byte[] pcmBytes)
			{
				using var memoryStream = new MemoryStream(pcmBytes);
				return FromStream(memoryStream);
			}

			public static PcmHeader FromStream(Stream pcmStream)
			{
				pcmStream.Position = SizeIndex;
				using BinaryReader reader = new BinaryReader(pcmStream);

				int headerSize = reader.ReadInt32();  // 16
				ushort audioFormatCode = reader.ReadUInt16(); // 20

				string audioFormat = GetAudioFormatFromCode(audioFormatCode);
				if (audioFormatCode != 1 && audioFormatCode == 65534)
				{
					// Only uncompressed PCM wav files are supported.
					throw new ArgumentOutOfRangeException(nameof(pcmStream),
														  $"Detected format code '{audioFormatCode}' {audioFormat}, but only PCM and WaveFormatExtensible uncompressed formats are currently supported.");
				}

				ushort channelCount = reader.ReadUInt16(); // 22
				int sampleRate = reader.ReadInt32();  // 24
				int byteRate = reader.ReadInt32();  // 28
				ushort blockAlign = reader.ReadUInt16(); // 32
				ushort bitDepth = reader.ReadUInt16(); //34

				pcmStream.Position = SizeIndex + headerSize + 2 * sizeof(int); // Header end index
				int audioSize = reader.ReadInt32();                            // Audio size index

				return new PcmHeader(bitDepth, audioSize, (int)pcmStream.Position, channelCount, sampleRate, byteRate, blockAlign); // audio start index
			}

			public float NormalizeSample(float rawSample)
			{
				float sampleDepth = rawSample < 0 ? _negativeDepth : _positiveDepth;
				return rawSample / sampleDepth;
			}

			#endregion

			#region Private Methods

			static string GetAudioFormatFromCode(ushort code)
			{
				switch (code)
				{
					case 1: return "PCM";
					case 2: return "ADPCM";
					case 3: return "IEEE";
					case 7: return "?-law";
					case 65534: return "WaveFormatExtensible";
					default: throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown wav code format.");
				}
			}

			#endregion

			#region Private types & Data

			const int SizeIndex = 16;

			readonly float _positiveDepth;
			readonly float _negativeDepth;

			#endregion
		}

		readonly struct PcmData
		{
			#region Public types & data

			public float[] Value { get; }
			public int Length { get; }
			public int Channels { get; }
			public int SampleRate { get; }

			#endregion

			#region Constructors & Finalizer

			PcmData(float[] value, int channels, int sampleRate)
			{
				Value = value;
				Length = value.Length;
				Channels = channels;
				SampleRate = sampleRate;
			}

			#endregion

			#region Public Methods

			public static PcmData FromBytes(byte[] bytes)
			{
				if (bytes == null)
				{
					throw new ArgumentNullException(nameof(bytes));
				}

				PcmHeader pcmHeader = PcmHeader.FromBytes(bytes);
				if (pcmHeader.BitDepth != 16 && pcmHeader.BitDepth != 32 && pcmHeader.BitDepth != 8)
				{
					throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
				}

				float[] samples = new float[pcmHeader.AudioSampleCount];
				for (int i = 0; i < samples.Length; ++i)
				{
					int byteIndex = pcmHeader.AudioStartIndex + i * pcmHeader.AudioSampleSize;
					float rawSample;
					switch (pcmHeader.BitDepth)
					{
						case 8:
							rawSample = bytes[byteIndex];
							break;

						case 16:
							rawSample = BitConverter.ToInt16(bytes, byteIndex);
							break;

						case 32:
							rawSample = BitConverter.ToInt32(bytes, byteIndex);
							break;

						default: throw new ArgumentOutOfRangeException(nameof(pcmHeader.BitDepth), pcmHeader.BitDepth, "Supported values are: 8, 16, 32");
					}

					samples[i] = pcmHeader.NormalizeSample(rawSample); // normalize sample between [-1f, 1f]
				}

				return new PcmData(samples, pcmHeader.Channels, pcmHeader.SampleRate);
			}

			#endregion
		}

		readonly struct VorbisHeader
		{
			#region Public types & data

			public int BitDepth { get; }
			public int AudioSampleSize { get; }
			public int AudioSampleCount { get; }
			public ushort Channels { get; }
			public int SampleRate { get; }
			public int AudioStartIndex { get; }
			public int ByteRate { get; }
			public ushort BlockAlign { get; }

			#endregion

			#region Constructors & Finalizer

			VorbisHeader(int bitDepth,
				int audioSize,
				int audioStartIndex,
				ushort channels,
				int sampleRate,
				int byteRate,
				ushort blockAlign)
			{
				BitDepth = bitDepth;
				_negativeDepth = Mathf.Pow(2f, BitDepth - 1f);
				_positiveDepth = _negativeDepth - 1f;

				AudioSampleSize = bitDepth / 8;
				AudioSampleCount = Mathf.FloorToInt(audioSize / (float)AudioSampleSize);
				AudioStartIndex = audioStartIndex;

				Channels = channels;
				SampleRate = sampleRate;
				ByteRate = byteRate;
				BlockAlign = blockAlign;
			}

			#endregion

			#region Private types & Data

			const int SizeIndex = 16;

			readonly float _positiveDepth;
			readonly float _negativeDepth;

			#endregion
		}

		readonly struct VorbisData
		{
			#region Public types & data

			public float[] Value { get; }
			public int Length { get; }
			public int Channels { get; }
			public int SampleRate { get; }

			#endregion

			#region Constructors & Finalizer

			VorbisData(float[] value, int channels, int sampleRate)
			{
				Value = value;
				Length = value.Length;
				Channels = channels;
				SampleRate = sampleRate;
            }

            #endregion

        }

        public static AudioClip GetAudioClip(string path)
        {
            var f = Convert16BitToFloat(File.ReadAllBytes(path));

            AudioClip audioClip = AudioClip.Create("testSound", f.Length, 2, 44100, false);
            audioClip.SetData(f, 0);

			return audioClip;
        }

        public static AudioClip GetAudioClip(byte[] bytes, string name)
		{
			switch (GetAudioType(name))
			{
				case AudioType.WAV: return GetAudioWAV(bytes, name);
				default: throw new Exception("Formats outside of .wav are not supported.");
			}
		}

		public static AudioClip GetAudioWAV(byte[] bytes, string name = "clip")
		{
			var pcmData = PcmData.FromBytes(bytes);
			var audioClip = AudioClip.Create(name, pcmData.Length, pcmData.Channels, pcmData.SampleRate, false);
			audioClip.SetData(pcmData.Value, 0);
			return audioClip;
		}

        // Having some issues with NVorbis.

        //public static AudioClip GetAudioOGG(byte[] bytes, string name = "clip")
        //{
        //    using (var vorbis = new NVorbis.VorbisReader(new MemoryStream(bytes, false)))
        //    {
        //        Debug.Log($"Found ogg ch={vorbis.Channels} freq={vorbis.SampleRate} samp={vorbis.TotalSamples}");

        //        float[] _audioBuffer = new float[vorbis.TotalSamples]; // Just dump everything
        //        int read = vorbis.ReadSamples(_audioBuffer, 0, (int)vorbis.TotalSamples);
        //        AudioClip audioClip = AudioClip.Create(name, (int)(vorbis.TotalSamples / vorbis.Channels), vorbis.Channels, vorbis.SampleRate, false);
        //        audioClip.SetData(_audioBuffer, 0);

        //        return audioClip;
        //    }
        //}
    }
}

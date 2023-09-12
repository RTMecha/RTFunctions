﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

using SimpleJSON;

using RTFunctions.Functions.IO;

namespace RTFunctions.Functions.Managers.Networking
{
    public class AlephNetworkManager : MonoBehaviour
    {
        public static AlephNetworkManager inst;
        public static string className = "[<color=#FC5F58>AlephNetworkManager</color>]\n";

        public static void Init()
        {
            var gameObject = new GameObject("NetworkManager");
            gameObject.transform.SetParent(SystemManager.inst.transform);
            gameObject.AddComponent<AlephNetworkEditorManager>();
        }

        void Awake()
        {
            inst = this;
        }

        public static IEnumerator DownloadFileOld(string path, Action<object> callback = null)
        {
            if (callback != null)
            {
                using (var www = new WWW(path))
                {
                    while (!www.isDone)
                        yield return null;
                    callback(www);
                }
            }

            yield break;
        }

        //Start(AlephNetworkManager.DownloadJSONFile("https://cdn.discordapp.com/attachments/811214540141363201/1151210119913287850/rhythmtech_player.lspl", delegate (string json)
        //{
        //	var jn = JSON.Parse(json);
        //  Log(jn["base"]["name"]);
        //}));

        public static void TestVersions()
        {
            var jn = JSON.Parse("{}");
            jn["versions"]["events_core"] = "1.5.0";
            jn["versions"]["creative_players"] = "2.3.2";

            RTFile.WriteToFile("E:/Project Arrhythmia mods/TestPlugin (bepinex)/4.1.16 Mods/RTFunctions/mod_info.lss", jn.ToString(3));
        }

        public static IEnumerator DownloadJSONFile(string path, Action<string> callback)
        {
            using (var www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogErrorFormat("{0}Error: {1}", className, www.error);
                }
                else
                {
                    callback(www.downloadHandler.text);
                }
            }

            yield break;
        }

        public static IEnumerator DownloadImageTexture(string path, Action<Texture2D> callback)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat("{0}Error: {1}", className, www.error);
            }
            else
            {
                Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                callback(tex);
            }
        }

        public static IEnumerator DownloadAudioClip(string path, AudioType audioType, Action<AudioClip> callback)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat("{0}Error: {1}", className, www.error);
            }
            else
            {
                AudioClip audioClip = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                callback(audioClip);
            }
        }

        public static IEnumerator DownloadAssetBundle(string path, Action<AssetBundle> callback)
        {
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(path);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogErrorFormat("{0}Error: {1}", className, www.error);
            }
            else
            {
                AssetBundle assetBundle = ((DownloadHandlerAssetBundle)www.downloadHandler).assetBundle;
                callback(assetBundle);
            }
        }
    }
}
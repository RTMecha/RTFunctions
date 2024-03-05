using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LSFunctions;
using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using SteamworksFacepunch;
using SteamworksFacepunch.Data;
using SteamworksFacepunch.Ugc;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RTFunctions.Functions.Managers.Networking
{
    public class SteamWorkshopManager : MonoBehaviour
    {
        public static SteamWorkshopManager inst;

        public static string className = "[<color=#e81e62>Steam</color>] \n";

        public static void Init(SteamManager steamManager) => steamManager.gameObject.AddComponent<SteamWorkshopManager>();

        public int levelCount;

        public List<Level> Levels { get; set; } = new List<Level>();

        public SteamUser steamUser;

        void Awake()
        {
            if (!inst)
                inst = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            try
            {
                SteamClient.Init(440310U);
                steamUser = new SteamUser(SteamClient.SteamId, SteamClient.SteamId.Value, SteamClient.Name);
                Debug.Log($"{className}Init Steam User: {SteamClient.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"{className}Steam Workshop Init failed.\nPlease replace the steam_api64.dll in Project Arrhythmia_Data/Plugins with the newer version!\n{ex}");
            }
        }

        void Update() => SteamClient.RunCallbacks();

        void OnApplicationQuit() => SteamClient.Shutdown();
        
        public bool hasLoaded;

        public bool loading;

        public PublishedFileId[] subscribedFiles;

        public uint LevelCount { get; set; }

        public IEnumerator GetSubscribedItems(Action<Level, int> onLoad = null)
        {
            hasLoaded = false;
            loading = true;
            Levels.Clear();

            uint numSubscribedItems = SteamUGC.Internal.GetNumSubscribedItems();
            subscribedFiles = new PublishedFileId[numSubscribedItems];
            LevelCount = numSubscribedItems;
            uint subscribedItems = SteamUGC.Internal.GetSubscribedItems(subscribedFiles, numSubscribedItems);
            for (int i = 0; i < subscribedFiles.Length; i++)
            {
                yield return GetItemContent(subscribedFiles[i], i, onLoad);
            }

            loading = false;
            hasLoaded = true;
        }

        public IEnumerator GetItemContent(PublishedFileId publishedFileID, int i = 0, Action<Level, int> onLoad = null)
        {
            if (SteamUGC.Internal.GetItemState(publishedFileID) == 8U)
            {
                if (SteamUGC.Internal.DownloadItem(publishedFileID, false))
                    Debug.Log("Downloaded File!");
                else
                    yield break;
            }
            else
            {
                ulong punSizeOnDisk = 0;
                string pchFolder;
                uint punTimeStamp = 0;
                SteamUGC.Internal.GetItemInstallInfo(publishedFileID, ref punSizeOnDisk, out pchFolder, ref punTimeStamp);

                if (!Level.Verify(pchFolder + "/"))
                    yield break;

                var level = new Level(pchFolder + "/");
                
                if (level.InvalidID)
                    yield break;

                onLoad?.Invoke(level, i);

                Levels.Add(level);

                yield break;
            }
        }

        public void CreateEntry(Item entry)
        {
            var level = new Level(entry.Directory.Replace("\\", "/") + "/");
            if (level.id == null || level.id == "0" || level.id == "-1")
                return;

            Levels.Add(level);
        }

        public void SearchTest(string search, int page = 1)
        {
            //Search(search, page);
            StartCoroutine(ISearch(search, page));
        }

        public IEnumerator ISearch(string search, int page = 1)
        {
            page = Mathf.Clamp(page, 1, int.MaxValue);

            var resultPage = Query.Items.WhereSearchText(search).RankedByTextSearch().GetPageAsync(page).Result;

            if (resultPage != null)
            {
                string str = $"{className}This page has {resultPage.Value.ResultCount} items\n";
                int num = 0;
                foreach (var item in resultPage.Value.Entries)
                {
                    str += $"Entry: {item.Title}";

                    if (num < resultPage.Value.ResultCount - 1)
                        str += "\n";

                    num++;
                }
                Debug.Log(str);
            }
            yield break;
        }

        public async void Search(string search, int page = 1)
        {
            ResultPage? resultPage = await Query.Items.WhereSearchText(search).RankedByTextSearch().GetPageAsync(page);

            if (resultPage != null)
            {
                string str = $"{className}This page has {resultPage.Value.ResultCount} items\n";
                int num = 0;
                foreach (var item in resultPage.Value.Entries)
                {
                    str += $"Entry: {item.Title}";

                    if (num < resultPage.Value.ResultCount - 1)
                        str += "\n";

                    num++;
                }
                Debug.Log(str);
            }
        }

        public static string LevelWorkshopLink(ulong _id) => _id == 0UL ? null : string.Format("https://steamcommunity.com/sharedfiles/filedetails/?id={0}", _id);

        public class SteamUser
        {
            public SteamId steamID;
            public ulong id;
            public string name = "No Steam User";

            public SteamUser()
            {
            }

            public SteamUser(SteamId steamID, ulong id, string name)
            {
                this.steamID = steamID;
                this.id = id;
                this.name = name;
            }
        }
    }


}

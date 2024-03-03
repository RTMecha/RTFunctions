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
                Debug.LogError($"{className}Steam Workshop Init failed.\n{ex}");
            }
        }

        void Update() => SteamClient.RunCallbacks();

        void OnApplicationQuit() => SteamClient.Shutdown();

        public async void LoadLevels()
        {
            Levels.Clear();
            Query q = Query.ItemsReadyToUse.WhereUserSubscribed().SortByCreationDate();
            int pageNum = 1;
            for (ResultPage? pageAsync = await q.GetPageAsync(pageNum); pageAsync.HasValue && pageAsync.Value.ResultCount > 0; pageAsync = await q.GetPageAsync(pageNum))
            {
                foreach (var entry in pageAsync.Value.Entries)
                {
                    if (entry.IsInstalled && !entry.NeedsUpdate && Level.Verify(entry.Directory + "/"))
                        CreateEntry(entry);
                    else if (Level.Verify(entry.Directory + "/") && await entry.DownloadAsync())
                        CreateEntry(entry);
                    else
                        Debug.LogError($"{className}{entry.Id} cannot be downloaded!");
                }
                ++pageNum;
            }
        }

        public void CreateEntry(Item entry)
        {
            Levels.Add(new Level(entry.Directory + "/"));
        }

        public async void Search(string search, int page = 1)
        {
            ResultPage? resultPage = await Query.Items.WhereSearchText(search).RankedByTextSearch().GetPageAsync(page);
            if (resultPage != null)
            {
                Debug.Log($"{className}This page has {resultPage.Value.ResultCount}");
                foreach (var item in resultPage.Value.Entries)
                {
                    Debug.Log($"{className}Entry: {item.Title}");
                }
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

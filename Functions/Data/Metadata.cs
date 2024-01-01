using System;
using System.Collections.Generic;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.IO;

using BaseMetadata = DataManager.MetaData;
using BaseArtist = DataManager.MetaData.Artist;
using BaseCreator = DataManager.MetaData.Creator;
using BaseSong = DataManager.MetaData.Song;
using BaseBeatmap = DataManager.MetaData.Beatmap;

namespace RTFunctions.Functions.Data
{
    public class Metadata : BaseMetadata
    {
		public Metadata() : base()
        {

        }

		public Metadata(LevelArtist artist, LevelCreator creator, LevelSong song, LevelBeatmap beatmap) : base(artist, creator, song, beatmap)
        {

        }

        public Metadata(BaseMetadata metadata)
        {

        }

        public string collectionID;
        public int index;
        public string id;

        #region Methods

        public static Metadata DeepCopy(Metadata orig) => new Metadata
        {
            artist = new LevelArtist
            {
                Link = orig.artist.Link,
                LinkType = orig.artist.LinkType,
                Name = orig.artist.Name
            },
            beatmap = new LevelBeatmap
            {
                date_edited = orig.beatmap.date_edited,
                game_version = orig.beatmap.game_version,
                version_number = orig.beatmap.version_number,
                workshop_id = orig.beatmap.workshop_id
            },
            creator = new LevelCreator
            {
                steam_id = orig.creator.steam_id,
                steam_name = orig.creator.steam_name,
                link = orig.LevelCreator.link,
                linkType = orig.LevelCreator.linkType
            },
            song = new LevelSong
            {
                BPM = orig.song.BPM,
                description = orig.song.description,
                difficulty = orig.song.difficulty,
                previewLength = orig.song.previewLength,
                previewStart = orig.song.previewStart,
                time = orig.song.time,
                title = orig.song.title,
                tags = orig.LevelSong.tags,
            },
            id = orig.id,
            index = orig.index,
            collectionID = orig.collectionID,
        };

        public static Metadata Parse(JSONNode jn)
        {
			Metadata result;
			try
			{
				string name = "Artist Name";
				int linkType = 0;
				string link = "kaixomusic";
				try
				{
					if (!string.IsNullOrEmpty(jn["artist"]["name"]))
						name = jn["artist"]["name"];
					if (!string.IsNullOrEmpty(jn["artist"]["linkType"]))
						linkType = jn["artist"]["linkType"].AsInt;
					if (!string.IsNullOrEmpty(jn["artist"]["link"]))
						link = jn["artist"]["link"];
				}
				catch (Exception ex)
				{
					Debug.LogError($"Artist Error: {ex}");
				}

				var artist = new LevelArtist(name, linkType, link);

				string steam_name = "Mecha";
				int steam_id = -1;
				string creatorLink = "";
				int creatorLinkType = 0;

                try
				{
					if (!string.IsNullOrEmpty(jn["creator"]["steam_name"]))
						steam_name = jn["creator"]["steam_name"];
					if (!string.IsNullOrEmpty(jn["creator"]["steam_id"]))
						steam_id = jn["creator"]["steam_id"].AsInt;
					if (!string.IsNullOrEmpty(jn["creator"]["link"]))
						creatorLink = jn["creator"]["link"];
					if (!string.IsNullOrEmpty(jn["creator"]["linkType"]))
						creatorLinkType = jn["creator"]["linkType"].AsInt;
				}
				catch (Exception ex)
				{
					Debug.LogError($"Creator Error: {ex}");
				}

				var creator = new LevelCreator(steam_name, steam_id, creatorLink, creatorLinkType);

				string title = "Pyrolysis";
				int difficulty = 2;
				string description = "This is the default description!";
				float bpm = 120f;
				float time = 60f;
				float previewStart = 0f;
				float previewLength = 30f;

				string[] tags = new string[]
				{
				};

				try
				{
					if (!string.IsNullOrEmpty(jn["song"]["title"]))
						title = jn["song"]["title"];
					if (!string.IsNullOrEmpty(jn["song"]["difficulty"]))
						difficulty = jn["song"]["difficulty"].AsInt;
					if (!string.IsNullOrEmpty(jn["song"]["description"]))
						description = jn["song"]["description"];
					if (!string.IsNullOrEmpty(jn["song"]["bpm"]))
						bpm = jn["song"]["bpm"].AsFloat;
					if (!string.IsNullOrEmpty(jn["song"]["t"]))
						time = jn["song"]["t"].AsFloat;
					if (!string.IsNullOrEmpty(jn["song"]["preview_start"]))
						previewStart = jn["song"]["preview_start"].AsFloat;
					if (!string.IsNullOrEmpty(jn["song"]["preview_length"]))
						previewLength = jn["song"]["preview_length"].AsFloat;

					if (jn["song"]["tags"] != null)
					{
						tags = new string[jn["song"]["tags"].Count];
						for (int i = 0; i < jn["song"]["tags"].Count; i++)
						{
							tags[i] = jn["song"]["tags"][i];
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"Song Error: {ex}");
				}


				var song = new LevelSong(title, difficulty, description, bpm, time, previewStart, previewLength, tags);

				string gameVersion = ProjectArrhythmia.GameVersion.ToString();
				string dateEdited = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
				string dateCreated = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
				int workshopID = -1;
				int num = 0;
				string beatmapID = LSFunctions.LSText.randomString(16);

                try
				{
					if (!string.IsNullOrEmpty(jn["beatmap"]["game_version"]))
						gameVersion = jn["beatmap"]["game_version"];
					if (!string.IsNullOrEmpty(jn["beatmap"]["date_edited"]))
						dateEdited = jn["beatmap"]["date_edited"];
					if (!string.IsNullOrEmpty(jn["beatmap"]["date_created"]))
						dateCreated = jn["beatmap"]["date_created"];
					if (!string.IsNullOrEmpty(jn["beatmap"]["version_number"]))
						num = jn["beatmap"]["version_number"].AsInt;
					if (!string.IsNullOrEmpty(jn["beatmap"]["workshop_id"]))
						workshopID = jn["beatmap"]["workshop_id"].AsInt;
					if (!string.IsNullOrEmpty(jn["beatmap"]["beatmap_id"]))
						beatmapID = jn["beatmap"]["beatmap_id"];
				}
                catch (Exception ex)
				{
					Debug.LogError($"Beatmap Error: {ex}");
				}

				var beatmap = new LevelBeatmap(dateEdited, dateCreated, gameVersion, num, workshopID.ToString());

				result = new Metadata(artist, creator, song, beatmap);
			}
			catch
			{
				var artist2 = new LevelArtist("Corrupted", 0, "");
				var creator2 = new LevelCreator(SteamWrapper.inst.user.displayName, SteamWrapper.inst.user.id, "", 0);
				var song2 = new LevelSong("Corrupt Metadata", 0, "", 140f, 100f, -1f, -1f, new string[] { "Corrupted" });
				var beatmap2 = new LevelBeatmap("", "", ProjectArrhythmia.GameVersion.ToString(), 0, "-1");
				result = new Metadata(artist2, creator2, song2, beatmap2);
				Debug.LogError($"{DataManager.inst.className}Something went wrong with parsing metadata!");
			}
			return result;
		}

		public JSONNode ToJSON()
		{
			var jn = JSON.Parse("{}");

			jn["artist"]["name"] = artist.Name;
			jn["artist"]["link"] = artist.Link;
			jn["artist"]["linkType"] = artist.LinkType.ToString();

			jn["creator"]["steam_name"] = creator.steam_name;
			jn["creator"]["steam_id"] = creator.steam_id.ToString();
			jn["creator"]["link"] = LevelCreator.link;
			jn["creator"]["linkType"] = LevelCreator.linkType.ToString();

			jn["song"]["title"] = song.title;
			jn["song"]["difficulty"] = song.difficulty.ToString();
			jn["song"]["description"] = song.description;
			jn["song"]["bpm"] = song.BPM.ToString();
			jn["song"]["t"] = song.time.ToString();
			jn["song"]["preview_start"] = song.previewStart.ToString();
			jn["song"]["preview_length"] = song.previewLength.ToString();
			for (int i = 0; i < LevelSong.tags.Length; i++)
				jn["song"]["tags"][i] = LevelSong.tags[i];

			jn["beatmap"]["date_created"] = LevelBeatmap.date_created;
			jn["beatmap"]["date_edited"] = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
			jn["beatmap"]["version_number"] = beatmap.version_number.ToString();
			jn["beatmap"]["game_version"] = beatmap.game_version;
			jn["beatmap"]["workshop_id"] = beatmap.workshop_id.ToString();
			jn["beatmap"]["beatmap_id"] = LevelBeatmap.beatmap_id;

			return jn;
		}

        #endregion

        public LevelArtist LevelArtist => (LevelArtist)artist;
        public LevelCreator LevelCreator => (LevelCreator)creator;
        public LevelSong LevelSong => (LevelSong)song;
		public LevelBeatmap LevelBeatmap => (LevelBeatmap)beatmap;

        #region Operators

        public static implicit operator bool(Metadata exists) => exists != null;

		public override bool Equals(object obj) => obj is Metadata && LevelBeatmap.beatmap_id == (obj as Metadata).LevelBeatmap.beatmap_id;

		public override string ToString() => $"{LevelBeatmap.beatmap_id}: {artist.Name} - {song.title}";

        #endregion
    }

    public class LevelArtist : BaseArtist
    {
        public LevelArtist() : base()
		{
            
        }

		public LevelArtist(string name, int linkType, string link) : base(name, linkType, link)
        {

        }

		public string URL
			=> LinkType < 0 || LinkType > DataManager.inst.linkTypes.Count - 1 || Link.Contains("http://") || Link.Contains("https://") ? null : string.Format(DataManager.inst.linkTypes[LinkType].linkFormat, Link);

        #region Operators

		public static implicit operator bool(LevelArtist exists) => exists != null;

		public override string ToString() => Name;

        #endregion
    }

    public class LevelCreator : BaseCreator
    {
        public LevelCreator() : base()
		{

        }

		public LevelCreator(string steam_name, int steam_id, string link, int linkType) : base(steam_name, steam_id)
        {
			this.link = link;
			this.linkType = linkType;
        }

		public string URL
			=> linkType < 0 || linkType > creatorLinkTypes.Count - 1 || link.Contains("http://") || link.Contains("https://") ? null : string.Format(creatorLinkTypes[linkType].linkFormat, link);


		public int linkType;
        public string link;

		public static List<DataManager.LinkType> creatorLinkTypes = new List<DataManager.LinkType>
		{
			new DataManager.LinkType("Youtube", "https://www.youtube.com/c/{0}"),
			new DataManager.LinkType("Newgrounds", "https://{0}.newgrounds.com/"),
			new DataManager.LinkType("Discord", "https://discord.gg/{0}"),
			new DataManager.LinkType("Patreon", "https://patreon.com/{0}"),
			new DataManager.LinkType("Twitter", "https://twitter.com/{0}"),
		};

		#region Operators

		public static implicit operator bool(LevelCreator exists) => exists != null;

		public override string ToString() => steam_name;

		#endregion
	}

	public class LevelSong : BaseSong
    {
        public LevelSong() : base()
		{

        }

		public LevelSong(string title, int difficulty, string description, float BPM, float time, float previewStart, float previewLength, string[] tags) : base(title, difficulty, description, BPM, time, previewStart, previewLength)
        {
			this.tags = tags;
        }

        public string[] tags;

		#region Operators

		public static implicit operator bool(LevelSong exists) => exists != null;

		public override string ToString() => title;

		#endregion
	}

	public class LevelBeatmap : BaseBeatmap
    {
		public LevelBeatmap() : base()
		{
			beatmap_id = workshop_id.ToString();
			date_created = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
		}

		public LevelBeatmap(string dateEdited, string gameVersion, int versionNumber, int workshopID) : base(dateEdited, gameVersion, versionNumber, workshopID)
        {
			beatmap_id = workshopID.ToString();
		}

		public LevelBeatmap(string dateEdited, string gameVersion, int versionNumber, string beatmapID) : base(dateEdited, gameVersion, versionNumber, Mathf.Clamp(int.Parse(beatmapID), 0, int.MaxValue))
		{
			beatmap_id = beatmapID;
			date_created = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
		}
		
		public LevelBeatmap(string dateEdited, string dateCreated, string gameVersion, int versionNumber, string beatmapID) : base(dateEdited, gameVersion, versionNumber, Mathf.Clamp(int.Parse(beatmapID), 0, int.MaxValue))
		{
			beatmap_id = beatmapID;
			date_created = dateCreated;
		}

		public string beatmap_id;
		public string date_created;

		public void RegisterMods()
        {
			foreach (var file in System.IO.Directory.GetFiles(RTFile.ApplicationDirectory + "BepInEx/plugins", "*.dll", System.IO.SearchOption.TopDirectoryOnly))
            {
				var name = file.Replace("\\", "/").Replace(RTFile.ApplicationDirectory + "BepInEx/plugins/", "");
				if (name != "ConfigurationManager.dll" && name != "EditorManagement.dll" && name != "EditorOnStartup.dll")
					requiredMods.Add(name);
            }
        }

		public List<string> requiredMods = new List<string>();

		#region Operators

		public static implicit operator bool(LevelBeatmap exists) => exists != null;

		public override string ToString() => beatmap_id;

		#endregion
	}
}

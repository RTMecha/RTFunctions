﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SimpleJSON;

using RTFunctions.Functions.Components;
using RTFunctions.Functions.Optimization;

using ObjectType = DataManager.GameData.BeatmapObject.ObjectType;
using AutoKillType = DataManager.GameData.BeatmapObject.AutoKillType;
using EventKeyframe = DataManager.GameData.EventKeyframe;

using BaseBeatmapObject = DataManager.GameData.BeatmapObject;
using BasePrefab = DataManager.GameData.Prefab;
using BasePrefabObject = DataManager.GameData.PrefabObject;
using BaseBackground = DataManager.GameData.BackgroundObject;
using BaseEditorData = DataManager.GameData.BeatmapObject.EditorData;
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
				if (!string.IsNullOrEmpty(jn["artist"]["name"]))
					name = jn["artist"]["name"];
				int linkType = 0;
				if (!string.IsNullOrEmpty(jn["artist"]["linkType"]))
					linkType = jn["artist"]["linkType"].AsInt;
				string link = "kaixo";
				if (!string.IsNullOrEmpty(jn["artist"]["link"]))
					link = jn["artist"]["link"];
				var artist = new LevelArtist(name, linkType, link);

				string steam_name = "Mecha";
				if (!string.IsNullOrEmpty(jn["creator"]["steam_name"]))
					steam_name = jn["creator"]["steam_name"];
				int steam_id = -1;
				if (!string.IsNullOrEmpty(jn["creator"]["steam_id"]))
					steam_id = jn["creator"]["steam_id"].AsInt;
				var creator = new LevelCreator(steam_name, steam_id);

				string title = "Pyrolysis";
				if (!string.IsNullOrEmpty(jn["song"]["title"]))
					title = jn["song"]["title"];
				int difficulty = 2;
				if (!string.IsNullOrEmpty(jn["song"]["difficulty"]))
					difficulty = jn["song"]["difficulty"].AsInt;
				string description = "This is the default description!";
				if (!string.IsNullOrEmpty(jn["song"]["description"]))
					description = jn["song"]["description"];
				float bpm = 120f;
				if (!string.IsNullOrEmpty(jn["song"]["bpm"]))
					bpm = jn["song"]["bpm"].AsFloat;
				float time = 60f;
				if (!string.IsNullOrEmpty(jn["song"]["t"]))
					time = jn["song"]["t"].AsFloat;
				float previewStart = 0f;
				if (!string.IsNullOrEmpty(jn["song"]["preview_start"]))
					previewStart = jn["song"]["preview_start"].AsFloat;
				float previewLength = 30f;
				if (!string.IsNullOrEmpty(jn["song"]["preview_length"]))
					previewLength = jn["song"]["preview_length"].AsFloat;
				var song = new LevelSong(title, difficulty, description, bpm, time, previewStart, previewLength);

				string gameVersion = ProjectArrhythmia.GameVersion.ToString();
				if (!string.IsNullOrEmpty(jn["beatmap"]["game_version"]))
					gameVersion = jn["beatmap"]["game_version"];
				string dateEdited = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
				if (!string.IsNullOrEmpty(jn["beatmap"]["date_edited"]))
					dateEdited = jn["beatmap"]["date_edited"];
				string dateCreated = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
				if (!string.IsNullOrEmpty(jn["beatmap"]["date_created"]))
					dateCreated = jn["beatmap"]["date_created"];
				int num = 0;
				if (!string.IsNullOrEmpty(jn["beatmap"]["version_number"]))
					num = jn["beatmap"]["version_number"].AsInt;
				int workshopID = -1;
				if (!string.IsNullOrEmpty(jn["beatmap"]["workshop_id"]))
					workshopID = jn["beatmap"]["workshop_id"].AsInt;

				string beatmapID = LSFunctions.LSText.randomString(16);
				if (!string.IsNullOrEmpty(jn["beatmap"]["beatmap_id"]))
					beatmapID = jn["beatmap"]["beatmap_id"];

				var beatmap = new LevelBeatmap(dateEdited, dateCreated, gameVersion, num, workshopID.ToString());

				result = new Metadata(artist, creator, song, beatmap);
			}
			catch
			{
				var artist2 = new LevelArtist("Corrupted", 0, "");
				var creator2 = new LevelCreator(SteamWrapper.inst.user.displayName, SteamWrapper.inst.user.id);
				var song2 = new LevelSong("Corrupt Metadata", 0, "", 140f, 100f, -1f, -1f);
				var beatmap2 = new LevelBeatmap("", ProjectArrhythmia.GameVersion.ToString(), 0, -1);
				result = new Metadata(artist2, creator2, song2, beatmap2);
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
    }

    public class LevelArtist : BaseArtist
    {
        public LevelArtist() : base()
		{
            
        }

		public LevelArtist(string name, int linkType, string link) : base(name, linkType, link)
        {

        }
    }

    public class LevelCreator : BaseCreator
    {
        public LevelCreator() : base()
		{

        }

		public LevelCreator(string steam_name, int steam_id) : base(steam_name, steam_id)
        {

        }

        public int linkType;
        public string link;
    }

    public class LevelSong : BaseSong
    {
        public LevelSong() : base()
		{

        }

		public LevelSong(string title, int difficulty, string description, float BPM, float time, float previewStart, float previewLength) : base(title, difficulty, description, BPM, time, previewStart, previewLength)
        {

        }

        public string[] tags;
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
    }
}

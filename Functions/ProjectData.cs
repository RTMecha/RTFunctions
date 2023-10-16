using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace RTFunctions.Functions
{
    public enum TitleFormat
    {
        ArtistTitle,
        TitleArtist
    }

    public class ProjectData
    {
        public static List<Level> levels;
        public static List<Collection> collections;

        public class Level
        {
            public Song song;
            public Beatmap beatmap;
        }

        public class Song
        {
            public Song(string[] artists, string title, bool remix, string[] remixArtists)
            {
                this.artists = artists;
                this.title = title;
                this.remix = remix;
                this.remixArtists = remixArtists;
            }

            public string[] artists;
            public string title;
            public bool remix;
            public string[] remixArtists;

            public string genre;

            public override string ToString() => title;
        }

        public class Beatmap
        {
            public Beatmap(string[] creators, string name)
            {
                this.creators = creators;
                this.name = name;
            }

            public string[] creators;
            public string name;
            public string id;
            public string tags;

            public string refCollectionID;

            #region Difficulty

            public static List<string> DifficultyNames = new List<string>
            {
                "Animation",
                "Easy",
                "Normal",
                "Hard",
                "Expert",
                "Expert+",
                "Master",
            };
            public static List<Color> DifficultyColors = new List<Color>
            {

            };
            public static int MaxDifficulty => 6;

            int difficulty;
            public int Difficulty
            {
                get => Mathf.Clamp(difficulty, 0, MaxDifficulty);
                set => difficulty = Mathf.Clamp(value, 0, MaxDifficulty);
            }

            public string DifficultyName => DifficultyNames[Difficulty];
            public Color DifficultyColor => DifficultyColors[Difficulty];

            #endregion

            public Beatmap GetNextLevel()
            {
                if (collections.Find(x => x.id == refCollectionID) != null)
                {
                    var collection = collections.Find(x => x.id == refCollectionID);
                    int index = collection.levels.IndexOf(this) + 1;
                    if (index > 0 && index < collection.levels.Count)
                        return collection.levels[index];
                }

                return null;
            }
            
            public Beatmap GetPrevLevel()
            {
                if (collections.Find(x => x.id == refCollectionID) != null)
                {
                    var collection = collections.Find(x => x.id == refCollectionID);
                    int index = collection.levels.IndexOf(this) - 1;
                    if (index > 0 && index < collection.levels.Count)
                        return collection.levels[index];
                }

                return null;
            }

            public override string ToString() => name;
        }

        public class Collection
        {
            public Collection(List<Beatmap> levels, string name, string id)
            {
                this.levels = levels;
                this.name = name;
                this.id = id;
            }

            public List<Beatmap> levels;
            public string name;
            public string id;

            public override string ToString() => name;
        }
    }
}

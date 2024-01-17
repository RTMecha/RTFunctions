using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers.Networking;
using RTFunctions.Functions.Optimization;
using RTFunctions.Functions.Optimization.Objects;
using RTFunctions.Functions.Optimization.Objects.Visual;

namespace RTFunctions.Functions.Managers
{
    /// <summary>
    /// This class is used to store fonts from the customfonts.asset file.
    /// </summary>
    public class FontManager : MonoBehaviour
    {
        public static FontManager inst;
        public static string className = "[<color=#A100FF>FontManager</color>] \n";

        public Dictionary<string, Font> allFonts = new Dictionary<string, Font>();
        public Dictionary<string, TMP_FontAsset> allFontAssets = new Dictionary<string, TMP_FontAsset>();
        public bool loadedFiles = false;

        public Font Inconsolata
        {
            get
            {
                if (allFonts.ContainsKey("Inconsolata Variable"))
                    return allFonts["Inconsolata Variable"];
                Debug.Log($"{className}Inconsolata Font doesn't exist for some reason.");
                return Font.GetDefault();
            }
        }

        void Awake()
        {
            inst = this;
            StartCoroutine(SetupCustomFonts());
        }

        void Update()
        {

            if (DataManager.inst.gameData != null && DataManager.inst.gameData.beatmapObjects.Count > 0)
            {
                foreach (var beatmapObject in DataManager.inst.gameData.beatmapObjects)
                {
                    if (beatmapObject.shape == 4 && beatmapObject.TimeWithinLifespan() && Updater.TryGetObject(beatmapObject, out LevelObject levelObject) && ((TextObject)levelObject.visualObject).TextMeshPro)
                    {
                        var tmp = ((TextObject)levelObject.visualObject).TextMeshPro;

                        var currentAudioTime = AudioManager.inst.CurrentAudioSource.time;
                        var currentAudioLength = AudioManager.inst.CurrentAudioSource.clip.length;

                        var str = beatmapObject.text;

                        #region Audio

                        if (beatmapObject.text.Contains("<msAudio000>"))
                        {
                            str = str.Replace("<msAudio000>", TextTranslater.PreciseToMilliSeconds(currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<msAudio00>"))
                        {
                            str = str.Replace("<msAudio00>", TextTranslater.PreciseToMilliSeconds(currentAudioTime, "{0:00}"));
                        }

                        if (beatmapObject.text.Contains("<msAudio0>"))
                        {
                            str = str.Replace("<msAudio0>", TextTranslater.PreciseToMilliSeconds(currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<sAudio00>"))
                        {
                            str = str.Replace("<sAudio00>", TextTranslater.PreciseToSeconds(currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<sAudio0>"))
                        {
                            str = str.Replace("<sAudio0>", TextTranslater.PreciseToSeconds(currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<mAudio00>"))
                        {
                            str = str.Replace("<mAudio00>", TextTranslater.PreciseToMinutes(currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<mAudio0>"))
                        {
                            str = str.Replace("<mAudio0>", TextTranslater.PreciseToMinutes(currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<hAudio00>"))
                        {
                            str = str.Replace("<hAudio00>", TextTranslater.PreciseToHours(currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<hAudio0>"))
                        {
                            str = str.Replace("<hAudio0>", TextTranslater.PreciseToHours(currentAudioTime, "{0:0}"));
                        }

                        #endregion

                        #region Audio Left

                        if (beatmapObject.text.Contains("<msAudioLeft000>"))
                        {
                            str = str.Replace("<msAudioLeft000>", TextTranslater.PreciseToMilliSeconds(currentAudioLength - currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<msAudioLeft00>"))
                        {
                            str = str.Replace("<msAudioLeft00>", TextTranslater.PreciseToMilliSeconds(currentAudioLength - currentAudioTime, "{0:00}"));
                        }

                        if (beatmapObject.text.Contains("<msAudioLeft0>"))
                        {
                            str = str.Replace("<msAudioLeft0>", TextTranslater.PreciseToMilliSeconds(currentAudioLength - currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<sAudioLeft00>"))
                        {
                            str = str.Replace("<sAudioLeft00>", TextTranslater.PreciseToSeconds(currentAudioLength - currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<sAudioLeft0>"))
                        {
                            str = str.Replace("<sAudioLeft0>", TextTranslater.PreciseToSeconds(currentAudioLength - currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<mAudioLeft00>"))
                        {
                            str = str.Replace("<mAudioLeft00>", TextTranslater.PreciseToMinutes(currentAudioLength - currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<mAudioLeft0>"))
                        {
                            str = str.Replace("<mAudioLeft0>", TextTranslater.PreciseToMinutes(currentAudioLength - currentAudioTime, "{0:0}"));
                        }

                        if (beatmapObject.text.Contains("<hAudioLeft00>"))
                        {
                            str = str.Replace("<hAudioLeft00>", TextTranslater.PreciseToHours(currentAudioLength - currentAudioTime));
                        }

                        if (beatmapObject.text.Contains("<hAudioLeft0>"))
                        {
                            str = str.Replace("<hAudioLeft0>", TextTranslater.PreciseToHours(currentAudioLength - currentAudioTime, "{0:0}"));
                        }

                        #endregion

                        #region Real Time

                        if (beatmapObject.text.Contains("<sRTime00>"))
                        {
                            str = str.Replace("<sRTime00>", DateTime.Now.ToString("ss"));
                        }

                        if (beatmapObject.text.Contains("<sRTime0>"))
                        {
                            str = str.Replace("<sRTime0>", DateTime.Now.ToString("s"));
                        }

                        if (beatmapObject.text.Contains("<mRTime00>"))
                        {
                            str = str.Replace("<mRTime00>", DateTime.Now.ToString("mm"));
                        }

                        if (beatmapObject.text.Contains("<mRTime0>"))
                        {
                            str = str.Replace("<mRTime0>", DateTime.Now.ToString("m"));
                        }

                        if (beatmapObject.text.Contains("<hRTime0012>"))
                        {
                            str = str.Replace("<hRTime0012>", DateTime.Now.ToString("hh"));
                        }

                        if (beatmapObject.text.Contains("<hRTime012>"))
                        {
                            str = str.Replace("<hRTime012>", DateTime.Now.ToString("h"));
                        }

                        if (beatmapObject.text.Contains("<hRTime0024>"))
                        {
                            str = str.Replace("<hRTime0024>", DateTime.Now.ToString("HH"));
                        }

                        if (beatmapObject.text.Contains("<hRTime024>"))
                        {
                            str = str.Replace("<hRTime024>", DateTime.Now.ToString("H"));
                        }

                        if (beatmapObject.text.Contains("<domRTime00>"))
                        {
                            str = str.Replace("<domRTime00>", DateTime.Now.ToString("dd"));
                        }

                        if (beatmapObject.text.Contains("<domRTime0>"))
                        {
                            str = str.Replace("<domRTime0>", DateTime.Now.ToString("d"));
                        }

                        if (beatmapObject.text.Contains("<dowRTime00>"))
                        {
                            str = str.Replace("<dowRTime00>", DateTime.Now.ToString("dddd"));
                        }

                        if (beatmapObject.text.Contains("<dowRTime0>"))
                        {
                            str = str.Replace("<dowRTime0>", DateTime.Now.ToString("ddd"));
                        }

                        if (beatmapObject.text.Contains("<mmRTime00>"))
                        {
                            str = str.Replace("<mnRTime00>", DateTime.Now.ToString("MM"));
                        }

                        if (beatmapObject.text.Contains("<mnRTime0>"))
                        {
                            str = str.Replace("<mnRTime0>", DateTime.Now.ToString("M"));
                        }

                        if (beatmapObject.text.Contains("<mmRTime00>"))
                        {
                            str = str.Replace("<mmRTime00>", DateTime.Now.ToString("MMMM"));
                        }

                        if (beatmapObject.text.Contains("<mmRTime0>"))
                        {
                            str = str.Replace("<mmRTime0>", DateTime.Now.ToString("MMM"));
                        }

                        if (beatmapObject.text.Contains("<yRTime0000>"))
                        {
                            str = str.Replace("<yRTime0000>", DateTime.Now.ToString("yyyy"));
                        }

                        if (beatmapObject.text.Contains("<yRTime00>"))
                        {
                            str = str.Replace("<yRTime00>", DateTime.Now.ToString("yy"));
                        }

                        #endregion

                        #region Players

                        var phRegex = new Regex(@"<playerHealth=(.*?)>");
                        var phMatch = phRegex.Match(beatmapObject.text);

                        if (phMatch.Success && int.TryParse(phMatch.Groups[1].ToString(), out int num))
                        {
                            if (InputDataManager.inst.players.Count > num)
                            {
                                str = str.Replace("<playerHealth=" + num.ToString() + ">", InputDataManager.inst.players[num].health.ToString());
                            }
                            else
                            {
                                str = str.Replace("<playerHealth=" + num.ToString() + ">", "");
                            }
                        }

                        if (beatmapObject.text.Contains("<playerHealthAll>"))
                        {
                            var ph = 0;

                            for (int i = 0; i < InputDataManager.inst.players.Count; i++)
                            {
                                ph += InputDataManager.inst.players[i].health;
                            }

                            str = str.Replace("<playerHealthAll>", ph.ToString());
                        }

                        var pdRegex = new Regex(@"<playerDeaths=(.*?)>");
                        var pdMatch = pdRegex.Match(beatmapObject.text);

                        if (pdMatch.Success && int.TryParse(pdMatch.Groups[1].ToString(), out int numDeath))
                        {
                            if (InputDataManager.inst.players.Count > numDeath)
                            {
                                str = str.Replace("<playerDeaths=" + numDeath.ToString() + ">", InputDataManager.inst.players[numDeath].PlayerDeaths.Count.ToString());
                            }
                            else
                            {
                                str = str.Replace("<playerDeaths=" + numDeath.ToString() + ">", "");
                            }
                        }

                        if (beatmapObject.text.Contains("<playerDeathsAll>"))
                        {
                            var pd = 0;

                            for (int i = 0; i < InputDataManager.inst.players.Count; i++)
                            {
                                pd += InputDataManager.inst.players[i].PlayerDeaths.Count;
                            }

                            str = str.Replace("<playerDeathsAll>", pd.ToString());
                        }

                        var phiRegex = new Regex(@"<playerHits=(.*?)>");
                        var phiMatch = phiRegex.Match(beatmapObject.text);

                        if (phiMatch.Success && int.TryParse(phiMatch.Groups[1].ToString(), out int numHit))
                        {
                            if (InputDataManager.inst.players.Count > numHit)
                            {
                                str = str.Replace("<playerHits=" + numHit.ToString() + ">", InputDataManager.inst.players[numHit].PlayerHits.Count.ToString());
                            }
                            else
                            {
                                str = str.Replace("<playerHits=" + numHit.ToString() + ">", "");
                            }
                        }

                        if (beatmapObject.text.Contains("<playerHitsAll>"))
                        {
                            var pd = 0;

                            for (int i = 0; i < InputDataManager.inst.players.Count; i++)
                            {
                                pd += InputDataManager.inst.players[i].PlayerHits.Count;
                            }

                            str = str.Replace("<playerHitsAll>", pd.ToString());
                        }

                        #endregion

                        #region QuickElement

                        var qeRegex = new Regex(@"<quickElement=(.*?)>");
                        var qeMatch = qeRegex.Match(beatmapObject.text);

                        if (qeMatch.Success)
                        {
                            str = str.Replace("<quickElement=" + qeMatch.Groups[1].ToString() + ">", QuickElementManager.ConvertQuickElement(beatmapObject, qeMatch.Groups[1].ToString()));
                        }

                        #endregion

                        #region Random

                        {
                            var ratRegex = new Regex(@"<randomText=(.*?)>");
                            var ratMatch = ratRegex.Match(beatmapObject.text);

                            if (ratMatch.Success && int.TryParse(ratMatch.Groups[1].ToString(), out int ratInt))
                            {
                                str = str.Replace("<randomText=" + ratMatch.Groups[1].ToString() + ">", LSFunctions.LSText.randomString(ratInt));
                            }

                            var ranRegex = new Regex(@"<randomNumber=(.*?)>");
                            var ranMatch = ranRegex.Match(beatmapObject.text);

                            if (ranMatch.Success && int.TryParse(ranMatch.Groups[1].ToString(), out int ranInt))
                            {
                                str = str.Replace("<randomNumber=" + ranMatch.Groups[1].ToString() + ">", LSFunctions.LSText.randomNumString(ranInt));
                            }
                        }

                        #endregion

                        #region Theme

                        //for (int i = 0; i < GameManager.inst.LiveTheme.objectColors.Count; i++)
                        //{
                        //    if (str.Contains("<themeObject=" + i.ToString() + ">"))
                        //    {
                        //        str = str.Replace("<themeObject=" + i.ToString() + ">", "<#" + LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.objectColors[i]) + ">");
                        //    }
                        //}

                        //{
                        //    var regex = new Regex(@"<themeObject=(.*?)>");
                        //    var match = regex.Match(beatmapObject.text);

                        //    if (match.Success && int.TryParse(match.Groups[1].ToString(), out int theme))
                        //    {
                        //        theme = Mathf.Clamp(theme, 0, GameManager.inst.LiveTheme.objectColors.Count - 1);
                        //        str = str.Replace("<themeObject=" + match.Groups[1].ToString() + ">", "<#" + LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.objectColors[theme]) + ">");
                        //    }
                        //}

                        //{
                        //    var regex = new Regex(@"<themeBGs=(.*?)>");
                        //    var match = regex.Match(beatmapObject.text);

                        //    if (match.Success && int.TryParse(match.Groups[1].ToString(), out int theme))
                        //    {
                        //        theme = Mathf.Clamp(theme, 0, GameManager.inst.LiveTheme.backgroundColors.Count - 1);
                        //        str = str.Replace("<themeBGs=" + match.Groups[1].ToString() + ">", "<#" + LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.backgroundColors[theme]) + ">");
                        //    }
                        //}

                        //{
                        //    var regex = new Regex(@"<themePlayers=(.*?)>");
                        //    var match = regex.Match(beatmapObject.text);

                        //    if (match.Success && int.TryParse(match.Groups[1].ToString(), out int theme))
                        //    {
                        //        theme = Mathf.Clamp(theme, 0, GameManager.inst.LiveTheme.playerColors.Count - 1);
                        //        str = str.Replace("<themePlayers=" + match.Groups[1].ToString() + ">", "<#" + LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.playerColors[theme]) + ">");
                        //    }
                        //}

                        //if (beatmapObject.text.Contains("<themeBG>"))
                        //{
                        //    str = str.Replace("<themeBG>", LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.backgroundColor));
                        //}

                        //if (beatmapObject.text.Contains("<themeGUI>"))
                        //{
                        //    str = str.Replace("<themeGUI>", LSFunctions.LSColors.ColorToHex(GameManager.inst.LiveTheme.guiColor));
                        //}

                        #endregion

                        #region Mod stuff

                        {
                            var regex = new Regex(@"<modifierVariable=(.*?)>");
                            var match = regex.Match(beatmapObject.text);

                            if (match.Success)
                            {
                                str = str.Replace("<modifierVariable=" + match.Groups[1].ToString() + ">", ((BeatmapObject)DataManager.inst.gameData.beatmapObjects.Find(x => x.name == match.Groups[1].ToString())).integerVariable.ToString());
                            }
                        }

                        #endregion

                        if (tmp)
                            tmp.text = str;
                    }
                }
            }
        }

        public void ChangeAllFontsInEditor()
        {
            var fonts = (from x in Resources.FindObjectsOfTypeAll<Text>()
                         where x.font.name == "Inconsolata-Regular"
                         select x).ToList();

            var inconsolata = Inconsolata;
            foreach (var font in fonts)
            {
                font.font = inconsolata;
            }
        }

        public IEnumerator SetupCustomFonts()
        {
            var refer = MaterialReferenceManager.instance;
            var dictionary = (Dictionary<int, TMP_FontAsset>)refer.GetType().GetField("m_FontAssetReferenceLookup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(refer);

            if (RTFile.FileExists(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets/customfonts.asset"))
            {
                var assetBundle = GetAssetBundle(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets", "customfonts.asset");
                foreach (var asset in assetBundle.GetAllAssetNames())
                {
                    string str = asset.Replace("assets/font/", "");
                    var font = assetBundle.LoadAsset<Font>(str);
                    if (font != null)
                    {
                        var fontCopy = Instantiate(font);
                        fontCopy.name = ChangeName(str);
                        if (!allFonts.ContainsKey(str))
                        {
                            allFonts.Add(fontCopy.name, fontCopy);
                        }
                        else
                        {
                            Debug.LogErrorFormat("{0}There was an error in adding the {1} font to the Dictionary.", className, str);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("{0}There was an error in loading the {1} font.", className, str);
                    }
                }
                assetBundle.Unload(false);
            }
            else
            {
                Debug.LogErrorFormat("{0}There was an error in loading the custom fonts. Make sure you have the customfonts file in the Assets folder!", className);

                try
                {
                    inst.StartCoroutine(AlephNetworkManager.DownloadAssetBundle("https://cdn.discordapp.com/attachments/1151231196022452255/1151231743580442765/customfonts", delegate (AssetBundle assetBundle)
                    {
                        foreach (var asset in assetBundle.GetAllAssetNames())
                        {
                            string str = asset.Replace("assets/font/", "");
                            var font = assetBundle.LoadAsset<Font>(str);
                            if (font != null)
                            {
                                var fontCopy = Instantiate(font);
                                fontCopy.name = ChangeName(str);
                                if (!allFonts.ContainsKey(str))
                                {
                                    allFonts.Add(fontCopy.name, fontCopy);
                                }
                                else
                                {
                                    Debug.LogErrorFormat("{0}There was an error in adding the {1} font to the Dictionary.", className, str);
                                }
                            }
                            else
                            {
                                Debug.LogErrorFormat("{0}There was an error in loading the {1} font.", className, str);
                            }
                        }
                        assetBundle.Unload(false);
                    }));
                }
                catch (Exception ex)
                {
                    Debug.LogFormat("{0}There was an error in getting the AssetBundle.\nMESSAGE: {1}\nSTACKTRACE: {2}", className, ex.Message, ex.StackTrace);
                }
            }

            foreach (var font in allFonts)
            {
                var e = TMP_FontAsset.CreateFontAsset(font.Value);
                e.name = font.Key;

                var random1 = TMP_TextUtilities.GetSimpleHashCode(e.name);
                e.hashCode = random1;
                e.materialHashCode = random1;

                if (!dictionary.ContainsKey(e.hashCode))
                {
                    MaterialReferenceManager.AddFontAsset(e);
                }
                else
                {
                    Debug.LogErrorFormat("{0}There was an error in adding the {1} font asset to the MaterialReferenceManager Font Asset Dictionary.\nHashcode: {2}", className, font.Key, e.hashCode);
                }

                if (!allFontAssets.ContainsKey(font.Key))
                {
                    allFontAssets.Add(font.Key, e);
                }
                else
                {
                    Debug.LogErrorFormat("{0}There was an error in adding the {1} font asset to the Dictionary.", className, font);
                }
            }

            yield break;
        }

        public Font GetCustomFont(string _font)
        {
            var assetBundle = GetAssetBundle(RTFile.ApplicationDirectory + "BepInEx/plugins/Assets", "customfonts");
            var fontToLoad = assetBundle.LoadAsset<Font>(_font);
            var font = Instantiate(fontToLoad);
            assetBundle.Unload(false);
            return font;
        }

        public AssetBundle GetAssetBundle(string _filepath, string _bundle) => AssetBundle.LoadFromFile(Path.Combine(_filepath, _bundle));

        public TMP_FontAsset GetCustomFontAsset(string _font)
        {
            var fontAsset = TMP_FontAsset.CreateFontAsset(GetCustomFont(_font));
            fontAsset.name = _font;
            MaterialReferenceManager.AddFontAsset(fontAsset);
            return fontAsset;
        }

        public void GetAsset(string _filepath, string _bundle, string _filename, Action<GameObject> callback)
        {
            var assetBundle = GetAssetBundle(_filepath, _bundle);
            var prefab = assetBundle.LoadAsset<GameObject>(_filename);
            callback(Instantiate(prefab));

            assetBundle.Unload(false);
        }

        public void GetAsset(string _filepath, string _bundle, string _filename, Action<Font> callback)
        {
            var assetBundle = GetAssetBundle(_filepath, _bundle);
            var prefab = assetBundle.LoadAsset<Font>(_filename);
            callback(Instantiate(prefab));

            assetBundle.Unload(false);
        }

        public string ChangeName(string _name1)
        {
            switch (_name1)
            {
                case "adamwarrenpro-bold.ttf":
                    {
                        return "Adam Warren Pro Bold";
                    }
                case "adamwarrenpro-bolditalic.ttf":
                    {
                        return "Adam Warren Pro BoldItalic";
                    }
                case "adamwarrenpro.ttf":
                    {
                        return "Adam Warren Pro";
                    }
                case "arrhythmia-font.ttf":
                    {
                        return "Arrhythmia";
                    }
                case "badabb__.ttf":
                    {
                        return "BadaBoom BB";
                    }
                case "bionicle language.ttf":
                    {
                        return "Matoran Language 1";
                    }
                case "dtm-mono.otf":
                    {
                        return "Determination Mono";
                    }
                case "dtm-sans.otf":
                    {
                        return "determination sans";
                    }
                case "flowcircular-regular.ttf":
                    {
                        return "Flow Circular";
                    }
                case "fredokaone-regular.ttf":
                    {
                        return "Fredoka One";
                    }
                case "hachicro.ttf":
                    {
                        return "Hachicro";
                    }
                case "inconsolata-variablefont_wdth,wght.ttf":
                    {
                        return "Inconsolata Variable";
                    }
                case "komikah_.ttf":
                    {
                        return "Komika Hand";
                    }
                case "komikahb.ttf":
                    {
                        return "Komika Hand Bold";
                    }
                case "komikask.ttf":
                    {
                        return "Komika Slick";
                    }
                case "komikasl.ttf":
                    {
                        return "Komika Slim";
                    }
                case "komikhbi.ttf":
                    {
                        return "Komika Hand BoldItalic";
                    }
                case "komikhi_.ttf":
                    {
                        return "Komika Hand Italic";
                    }
                case "komikj__.ttf":
                    {
                        return "Komika Jam";
                    }
                case "komikji_.ttf":
                    {
                        return "Komika Jam Italic";
                    }
                case "komikski.ttf":
                    {
                        return "Komika Slick Italic";
                    }
                case "komiksli.ttf":
                    {
                        return "Komika Slim Italic";
                    }
                case "mata nui.ttf":
                    {
                        return "Matoran Language 2";
                    }
                case "minecraftbold-nmk1.otf":
                    {
                        return "Minecraft Text Bold";
                    }
                case "minecraftbolditalic-1y1e.otf":
                    {
                        return "Minecraft Text BoldItalic";
                    }
                case "minecraftitalic-r8mo.otf":
                    {
                        return "Minecraft Text Italic";
                    }
                case "minecraftregular-bmg3.otf":
                    {
                        return "Minecraft Text";
                    }
                case "minercraftory.ttf":
                    {
                        return "Minecraftory";
                    }
                case "monsterfriendback.otf":
                    {
                        return "Monster Friend Back";
                    }
                case "monsterfriendfore.otf":
                    {
                        return "Monster Friend Fore";
                    }
                case "oxygene1.ttf":
                    {
                        return "Oxygene";
                    }
                case "piraka theory gf.ttf":
                    {
                        return "Piraka Theory";
                    }
                case "piraka.ttf":
                    {
                        return "Piraka";
                    }
                case "pusab___.otf":
                    {
                        return "Pusab";
                    }
                case "rahkshi font.ttf":
                    {
                        return "Rahkshi";
                    }
                case "revuebt-regular 1.otf":
                    {
                        return "Revue 1";
                    }
                case "revuebt-regular.otf":
                    {
                        return "Revue";
                    }
                case "transdings-waoo.ttf":
                    {
                        return "Transdings";
                    }
                case "transformersmovie-y9ad.ttf":
                    {
                        return "Transformers Movie";
                    }
                case "giedi ancient autobot.otf":
                    {
                        return "Ancient Autobot";
                    }
                case "nexa bold.otf":
                    {
                        return "Nexa Bold";
                    }
                case "nexabook.otf":
                    {
                        return "Nexa Book";
                    }
                case "angsa.ttf":
                    {
                        return "Angsana";
                    }
                case "angsab.ttf":
                    {
                        return "Angsana Bold";
                    }
                case "angsai.ttf":
                    {
                        return "Angsana Italic";
                    }
                case "angsananewbolditalic.ttf":
                    {
                        return "Angsana Bold Italic";
                    }
                case "angsaz.ttf":
                    {
                        return "Angsana Z";
                    }
                case "about_friend_extended_v2_by_matthewtheprep_ddribq5.otf":
                    {
                        return "About Friend";
                    }
                case "arial.ttf":
                    {
                        return "Arial";
                    }
                case "arialbd.ttf":
                    {
                        return "Arial Bold";
                    }
                case "arialbi.ttf":
                    {
                        return "Arial Bold Italic";
                    }
                case "ariali.ttf":
                    {
                        return "Arial Italic";
                    }
                case "ariblk.ttf":
                    {
                        return "Arial Black";
                    }
                case "calibri.ttf":
                    {
                        return "Calibri";
                    }
                case "calibrii.ttf":
                    {
                        return "Calibri Italic";
                    }
                case "calibril.ttf":
                    {
                        return "Calibri Light";
                    }
                case "calibrili.ttf":
                    {
                        return "Calibri Light Italic";
                    }
                case "calibriz.ttf":
                    {
                        return "Calibri Bold Italic";
                    }
                case "cambria.ttc":
                    {
                        return "Cambria";
                    }
                case "cambriab.ttf":
                    {
                        return "Cambria Bold";
                    }
                case "cambriaz.ttf":
                    {
                        return "Cambria Bold Italic";
                    }
                case "candara.ttf":
                    {
                        return "Candara";
                    }
                case "candarab.ttf":
                    {
                        return "Candara Bold";
                    }
                case "candarai.ttf":
                    {
                        return "Candara Italic";
                    }
                case "candaral.ttf":
                    {
                        return "Candara Light";
                    }
                case "candarali.ttf":
                    {
                        return "Candara Light Italic";
                    }
                case "candaraz.ttf":
                    {
                        return "Candara Bold Italic";
                    }
                case "comic.ttf":
                    {
                        return "Comic Sans";
                    }
                case "comicbd.ttf":
                    {
                        return "Comic Sans Bold";
                    }
                case "comici.ttf":
                    {
                        return "Comic Sans Italic";
                    }
                case "comicz.ttf":
                    {
                        return "Comic Sans Bold Italic";
                    }
                case "impact.ttf":
                    {
                        return "Impact";
                    }
                case "micross.ttf":
                    {
                        return "Sans Serif";
                    }
                case "times.ttf":
                    {
                        return "Times New Roman";
                    }
                case "timesbd.ttf":
                    {
                        return "Times New Roman Bold";
                    }
                case "timesbi.ttf":
                    {
                        return "Times New Roman Bold Italic";
                    }
                case "timesi.ttf":
                    {
                        return "Times New Roman Italic";
                    }
                case "webdings.ttf":
                    {
                        return "Webdings";
                    }
                case "wingding.ttf":
                    {
                        return "Wingding";
                    }
            }
            return _name1;
        }

        public Font GetDefaultFont(string _font, int _size)
        {
            if (Font.GetOSInstalledFontNames().Contains(_font))
            {
                return Font.CreateDynamicFontFromOSFont(_font, _size);
            }
            Debug.LogFormat("{0}Font {1} does not exist on OS!", className, _font);
            return Font.GetDefault();
        }

        public static class TextTranslater
        {
            public static string PreciseToMilliSeconds(float seconds, string format = "{0:000}") => string.Format(format, TimeSpan.FromSeconds(seconds).Milliseconds);

            public static string PreciseToSeconds(float seconds, string format = "{0:00}") => string.Format(format, TimeSpan.FromSeconds(seconds).Seconds);

            public static string PreciseToMinutes(float seconds, string format = "{0:00}") => string.Format(format, TimeSpan.FromSeconds(seconds).Minutes);

            public static string PreciseToHours(float seconds, string format = "{0:00}") => string.Format(format, TimeSpan.FromSeconds(seconds).Hours);

            public static string SecondsToTime(float seconds)
            {
                var timeSpan = TimeSpan.FromSeconds(seconds);
                return string.Format("{0:D0}:{1:D1}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }

            public static string Percentage(float t, float length) => string.Format("{0:000}", ((int)RTMath.Percentage(t, length)).ToString());

            public static string ConvertHealthToEquals(int _num, int _max = 3)
            {
                string str = "[";
                for (int i = 0; i < _num; i++)
                {
                    str += "=";
                }

                int e = -_num + _max;
                if (e > 0)
                {
                    for (int i = 0; i < e; i++)
                    {
                        str += " ";
                    }
                }

                return str += "]";
            }

            public static string ArrayToStringParams(params object[] vs) => ArrayToString(vs);

            public static string ArrayToString(object[] vs)
            {
                string s = "";
                if (vs.Length > 0)
                    for (int i = 0; i < vs.Length; i++)
                    {
                        s += vs[i].ToString();
                        if (i != vs.Length - 1)
                            s += ", ";
                    }
                return s;
            }

            public static string AlphabetBinaryEncrypt(string c)
            {
                var t = c;
                var str = "";

                foreach (var ch in t)
                {
                    var pl = ch.ToString();
                    pl = AlphabetBinaryEncryptChar(ch.ToString());
                    str += pl + " ";
                }
                return str;
            }

            public static string AlphabetBinaryEncryptChar(string c) => alphabetLowercase.Contains(c.ToLower()) ? binary[alphabetLowercase.IndexOf(c.ToLower())] : c;

            public static string AlphabetByteEncrypt(string c)
            {
                var t = c;
                var str = "";

                foreach (var ch in c.ToLower())
                {
                    var pl = ch.ToString();
                    pl = AlphabetByteEncryptChar(ch);
                    str += pl + " ";
                }
                return str;
            }

            public static string AlphabetByteEncryptChar(char c) => ((byte)c).ToString();

            public static string AlphabetKevinEncrypt(string c)
            {
                var t = c;
                var str = "";

                foreach (var ch in t)
                {
                    var pl = ch.ToString();
                    pl = AlphabetKevinEncryptChar(ch.ToString());
                    str += pl;
                }
                return str;
            }

            public static string AlphabetKevinEncryptChar(string c) => alphabetLowercase.Contains(c.ToLower()) ? kevin[alphabetLowercase.IndexOf(c.ToLower())] : c;

            public static string AlphabetA1Z26Encrypt(string c)
            {
                var t = c;
                var str = "";
                foreach (var ch in t)
                {
                    var pl = ch.ToString();
                    pl = AlphabetA1Z26EncryptChar(ch.ToString());
                    str += pl + " ";
                }
                return str;
            }

            public static string AlphabetA1Z26EncryptChar(string c) => alphabetLowercase.Contains(c.ToLower()) ? (alphabetLowercase.IndexOf(c.ToLower()) + 1).ToString() : c;

            public static string AlphabetCaesarEncrypt(string c, int offset = 3)
            {
                var t = c;
                var str = "";
                foreach (var ch in t)
                {
                    var pl = ch.ToString();
                    pl = AlphabetCaesarEncryptChar(ch.ToString(), offset);
                    str += pl;
                }
                return str;
            }

            public static string AlphabetCaesarEncryptChar(string c, int offset = 3)
            {
                var t = c;

                if (alphabetLowercase.Contains(t))
                {
                    var index = alphabetLowercase.IndexOf(t) - offset;
                    if (index < 0)
                        index += 26;

                    if (index < alphabetLowercase.Count && index >= 0)
                    {
                        return alphabetLowercase[index];
                    }
                }

                if (alphabetUppercase.Contains(t))
                {
                    var index = alphabetUppercase.IndexOf(t) - offset;
                    if (index < 0)
                        index += 26;

                    if (index < alphabetUppercase.Count && index >= 0)
                    {
                        return alphabetUppercase[index];
                    }
                }

                return t;
            }

            public static string AlphabetAtbashEncrypt(string c)
            {
                var t = c;
                var str = "";
                foreach (var ch in t)
                {
                    var pl = ch.ToString();
                    pl = AlphabetAtbashEncryptChar(ch.ToString());
                    str += pl;
                }
                return str;
            }

            public static string AlphabetAtbashEncryptChar(string c)
            {
                var t = c;

                if (alphabetLowercase.Contains(t))
                {
                    var index = -(alphabetLowercase.IndexOf(t) - alphabetLowercase.Count + 1);
                    if (index < alphabetLowercase.Count && index >= 0)
                    {
                        return alphabetLowercase[index];
                    }
                }

                if (alphabetUppercase.Contains(t))
                {
                    var index = -(alphabetUppercase.IndexOf(t) - alphabetUppercase.Count + 1);
                    if (index < alphabetUppercase.Count && index >= 0)
                    {
                        return alphabetUppercase[index];
                    }
                }

                return t;
            }

            public static List<string> alphabetLowercase = new List<string>
            {
                "a",
                "b",
                "c",
                "d",
                "e",
                "f",
                "g",
                "h",
                "i",
                "j",
                "k",
                "l",
                "m",
                "n",
                "o",
                "p",
                "q",
                "r",
                "s",
                "t",
                "u",
                "v",
                "w",
                "x",
                "y",
                "z"
            };

            public static List<string> alphabetUppercase = new List<string>
            {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J",
                "K",
                "L",
                "M",
                "N",
                "O",
                "P",
                "Q",
                "R",
                "S",
                "T",
                "U",
                "V",
                "W",
                "X",
                "Y",
                "Z"
            };

            public static List<string> kevin = new List<string>
            {
                "@",
                "|}",
                "(",
                "|)",
                "[-",
                "T-",
                "&",
                "|-|",
                "!",
                "_/",
                "|<",
                "|",
                "^^",
                "^",
                "0",
                "/>",
                "\\<",
                "|-",
                "5",
                "-|-",
                "(_)",
                "\\/",
                "\\/\\/",
                "*",
                "-/",
                "-/_"
            };

            public static List<string> binary = new List<string>
            {
                "01100001", // a
			    "01100010", // b
			    "01100011", // c
			    "01100100", // d
			    "01100101", // e
			    "01100110", // f
			    "01100111", // g
			    "01101000", // h
			    "01001001", // i
			    "01001001", // i
			    "01001010", // j
			    "01001011", // k
			    "01001100", // l
			    "01001101", // m
			    "01001110", // n
			    "01001111", // o
			    "01010000", // p
			    "01010001", // q
			    "01010010", // r
			    "01010011", // s
			    "01010100", // t
			    "01010101", // u
			    "01010110", // v
			    "01010111", // w
			    "01011000", // x
			    "01011001", // y
			    "01011010", // z
		    };
        }
    }
}

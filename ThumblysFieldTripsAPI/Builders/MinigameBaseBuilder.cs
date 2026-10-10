using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PicnicPanic;
using System.Collections.Generic;
using System.Reflection;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Registers;
using UnityEngine;
using UnityEngine.UI;

namespace ThumblysFieldTripsAPI.Builders
{
    public class MinigameBaseBuilder<T> where T : Minigame
    {
        private LoopingSoundObject _music = AssetFinder.FindOfTypeWithName<LoopingSoundObject>("Mus_Party_BGM", true);
        private string _midi = "CampMinigame_1_1";

        private string _minigameName = "Minigame_Unknown";
        private string _minigameTitle = "Unknown Minigame";
        private string _minigameDesc = "Unnamed and unloved, the developer forgot to give this minigame a description.";

        private SoundObject _startAudio = AssetFinder.FindOfTypeWithName<SoundObject>("BAL_LetsGo", true);
        private SoundObject _winAudio = AssetFinder.FindOfTypeWithName<SoundObject>("BAL_YouWon", true);
        private SoundObject _loseAudio = AssetFinder.FindOfTypeWithName<SoundObject>("BAL_GameOver", true);
        private SoundObject _highAudio = AssetFinder.FindOfTypeWithName<SoundObject>("BAL_HighScore", true);

        private string _winText = "Mgm_Base_Win";
        private string _loseText = "Mgm_Base_Lose";

        private int _bonus = 250;
        private bool _usesCursor = false;
        private bool _skipsTitle = false;

        private MinigameName _type;
        private string _enumName = "Unknown";
        
        private static FieldInfo _minigame = AccessTools.Field(typeof(MinigameBase), "minigame");
        private static FieldInfo _minigameController = AccessTools.Field(typeof(MinigameBase), "minigameController");
        private static FieldInfo _minigameNameKey = AccessTools.Field(typeof(MinigameBase), "minigameNameKey");
        private static FieldInfo _musicObject = AccessTools.Field(typeof(MinigameBase), "musicObject");
        private static FieldInfo _midiSong = AccessTools.Field(typeof(MinigameBase), "midiSong");
        private static FieldInfo _disableOnPause = AccessTools.Field(typeof(MinigameBase), "disableOnPause");
        private static FieldInfo _winKey = AccessTools.Field(typeof(MinigameBase), "winKey");
        private static FieldInfo _loseKey = AccessTools.Field(typeof(MinigameBase), "loseKey");
        private static FieldInfo _perfectBonus = AccessTools.Field(typeof(MinigameBase), "perfectBonus");
        private static FieldInfo _useCursor = AccessTools.Field(typeof(MinigameBase), "useCursor");
        private static FieldInfo _skipTitle = AccessTools.Field(typeof(MinigameBase), "skipTitle");
        private static FieldInfo _scoreTable = AccessTools.Field(typeof(MinigameBase), "scoreTable");

        private static FieldInfo _audStart = AccessTools.Field(typeof(MinigameBase), "audStart");
        private static FieldInfo _audWin = AccessTools.Field(typeof(MinigameBase), "audWin");
        private static FieldInfo _audLose = AccessTools.Field(typeof(MinigameBase), "audLose");
        private static FieldInfo _audHighScore = AccessTools.Field(typeof(MinigameBase), "audHighScore");

        /// <summary>
        /// Builds the MinigameBase.
        /// </summary>
        public MinigameBase Build()
        {

            MinigameBase minigame = Object.Instantiate(FieldTripsAPI.assetMan.Get<MinigameBase>("MinigameBase"), MTM101BaldiDevAPI.prefabTransform);
            minigame.gameObject.name = _minigameName;

            Object.DestroyImmediate(minigame.GetComponent<Minigame_Picnic>());
            T minigameComponent = minigame.gameObject.AddComponent<T>();

            Transform game = minigame.transform.Find("GameCanvas").Find("Game");
            Transform TitleChalkboard = minigame.transform.Find("GameCanvas").Find("Overlay").Find("Title");
            TitleChalkboard.Find("Title").GetComponent<TextLocalizer>().key = _minigameTitle;
            TitleChalkboard.Find("Description").GetComponent<TextLocalizer>().key = _minigameDesc;

            Object.DestroyImmediate(game.Find("Thoughts").gameObject);
            Object.DestroyImmediate(game.Find("BaldiBehindParent").gameObject);
            Object.DestroyImmediate(game.Find("Plates").gameObject);
            Object.DestroyImmediate(game.Find("BaldiOverParent").gameObject);
            Object.DestroyImmediate(game.Find("FuseAudioManager").gameObject);
            Object.DestroyImmediate(game.Find("MinigameHUD").gameObject);
            game.Find("BG").GetComponent<Image>().sprite = null;

            MinigameName minigameType = _type;
            if (_enumName != "")
            {
                minigameType = EnumExtensions.ExtendEnum<MinigameName>(_enumName);
            }

            ((MinigameHighScoreTable)_scoreTable.GetValue(minigame)).gameObject.AddComponent<ModdedMinigameHighScoreTable>().Setup((MinigameHighScoreTable)_scoreTable.GetValue(minigame));

            _minigame.SetValue(minigame, minigameType);
            _minigameController.SetValue(minigame, minigameComponent);
            _minigameNameKey.SetValue(minigame, _minigameTitle);
            _musicObject.SetValue(minigame, _music);
            _midiSong.SetValue(minigame, _midi);
            _winKey.SetValue(minigame, _winText);
            _loseKey.SetValue(minigame, _loseText);
            _perfectBonus.SetValue(minigame, _bonus);
            _useCursor.SetValue(minigame, _usesCursor);
            _skipTitle.SetValue(minigame, _skipsTitle);

            _disableOnPause.SetValue(minigame, new GameObject[0]);

            _audStart.SetValue(minigame, _startAudio);
            _audWin.SetValue(minigame, _winAudio);
            _audLose.SetValue(minigame, _loseAudio);
            _audHighScore.SetValue(minigame, _highAudio);

            return minigame;
        }

        /// <summary>
        /// Sets the name of the GameObject.
        /// </summary>
        public MinigameBaseBuilder<T> SetName(string name)
        {
            _minigameName = name;
            return this;
        }

        /// <summary>
        /// Sets the title and description key of the minigame Title Screen.
        /// </summary>
        public MinigameBaseBuilder<T> SetTitle(string title, string desc)
        {
            _minigameTitle = title;
            _minigameDesc = desc;
            return this;
        }

        /// <summary>
        /// Set the FieldTrips enum that this field trip will use.
        /// </summary>
        public MinigameBaseBuilder<T> SetEnum(MinigameName type)
        {
            _type = type;
            _enumName = "";
            return this;
        }

        /// <summary>
        /// Create a FieldTrips enum using EnumExtensions with the specified name.
        /// </summary>
        public MinigameBaseBuilder<T> SetEnum(string enumToRegister)
        {
            _type = MinigameName.CampfireFrenzy;
            _enumName = enumToRegister;
            return this;
        }

        /// <summary>
        /// Sets the midi music that the minigame will play.
        /// </summary>
        public MinigameBaseBuilder<T> SetMusic(string midi)
        {
            _midi = midi;
            return this;
        }

        /// <summary>
        /// Sets the music that the minigame will play.
        /// </summary>
        public MinigameBaseBuilder<T> SetMusic(LoopingSoundObject music)
        {
            _music = music;
            _midi = "";
            return this;
        }

        /// <summary>
        /// Sets win and lose keys for the minigame.
        /// </summary>
        public MinigameBaseBuilder<T> SetEndText(string winText, string loseText)
        {
            _winText = winText;
            _loseText = loseText;
            return this;
        }

        /// <summary>
        /// Sets the win bonus if the player does no mistakes.
        /// </summary>
        public MinigameBaseBuilder<T> SetWinBonus(int points)
        {
            _bonus = points;
            return this;
        }

        /// <summary>
        /// Sets the voicelines when Starting, Winning, Losing and High Score.
        /// </summary>
        public MinigameBaseBuilder<T> SetAudios(SoundObject start, SoundObject win, SoundObject lose, SoundObject highscore)
        {
            _startAudio = start;
            _winAudio = win;
            _loseAudio = lose;
            _highAudio = highscore;
            return this;
        }

        /// <summary>
        /// Sets if the minigame requires a cursor.
        /// </summary>
        public MinigameBaseBuilder<T> NeedsCursor()
        {
            _usesCursor = true;
            return this;
        }

        /// <summary>
        /// Sets if you want it to skip the title.
        /// </summary>
        public MinigameBaseBuilder<T> SkipsTitle()
        {
            _skipsTitle = true;
            return this;
        }
    }
}

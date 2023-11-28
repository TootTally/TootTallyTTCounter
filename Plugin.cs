using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using TootTallyCore.APIServices;
using TootTallyCore.Graphics;
using TootTallyCore.Utils.Helpers;
using TootTallyCore.Utils.TootTallyModules;
using TrombLoader.CustomTracks;
using UnityEngine;

namespace TootTallyTTCounter
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("TootTallyLeaderboard", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin, ITootTallyModule
    {
        public static Plugin Instance;

        private Harmony _harmony;
        public ConfigEntry<bool> ModuleConfigEnabled { get; set; }
        public bool IsConfigInitialized { get; set; }

        //Change this name to whatever you want
        public string Name { get => "TTCounter"; set => Name = value; }

        public static void LogInfo(string msg) => Instance.Logger.LogInfo(msg);
        public static void LogError(string msg) => Instance.Logger.LogError(msg);

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
            _harmony = new Harmony(Info.Metadata.GUID);

            GameInitializationEvent.Register(Info, TryInitialize);
        }

        private void TryInitialize()
        {
            // Bind to the TTModules Config for TootTally
            ModuleConfigEnabled = TootTallyCore.Plugin.Instance.Config.Bind("Modules", "TTCounter", true, "Enables TT Counter display while you are playing");
            TootTallyModuleManager.AddModule(this);
            TootTallySettings.Plugin.Instance.AddModuleToSettingPage(this);
        }

        public void LoadModule()
        {
            _harmony.PatchAll(typeof(TTCounterPatches));
            LogInfo($"Module loaded!");
        }

        public void UnloadModule()
        {
            _harmony.UnpatchSelf();
            LogInfo($"Module unloaded!");
        }



        public static class TTCounterPatches
        {
            private static SerializableClass.SongDataFromDB _songData;
            private static TTCounter _ttCounter;


            [HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
            [HarmonyPostfix]
            public static void OnGameControllerStartPostfix(GameController __instance)
            {
                var counterText = GameObjectFactory.CreateSingleText(__instance.ui_score_shadow.transform.parent.parent, "TTCounter", "0.00tt", Color.white);
                _ttCounter = counterText.gameObject.AddComponent<TTCounter>();
                _ttCounter.levelData = __instance.leveldata;

                if (_songData != null && _songData.track_ref == GlobalVariables.chosen_track_data.trackref)
                {
                    _ttCounter.SetChartData(_songData);
                    return;
                }

                GetSongDataFromServer(songData =>
                {
                    _songData = songData;
                    if (songData != null)
                        _ttCounter.SetChartData(_songData);
                });
            }

            [HarmonyPatch(typeof(LevelSelectController), nameof(LevelSelectController.clickPlay))]
            [HarmonyPostfix]
            public static void TryPreloadSongData()
            {
                if (_songData != null && _songData.track_ref == GlobalVariables.chosen_track_data.trackref) return;

                GetSongDataFromServer(songData => _songData = songData);
            }

            [HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
            [HarmonyPostfix]
            public static void OnScoreAveragePostfix(int ___totalscore, int ___currentnoteindex)
            {
                if (_ttCounter != null)
                    _ttCounter.OnScoreChanged(___totalscore, ___currentnoteindex);
            }

            public static void GetSongDataFromServer(Action<SerializableClass.SongDataFromDB> callback)
            {
                var track = TrackLookup.lookup(GlobalVariables.chosen_track_data.trackref);
                var songHash = SongDataHelper.GetSongHash(track);

                Plugin.Instance.StartCoroutine(TootTallyAPIService.GetHashInDB(songHash, track is CustomTrack, songHashInDB =>
                {
                    if (songHashInDB == 0) return;

                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetSongDataFromDB(songHashInDB, callback));
                }));
            }
        }

    }
}
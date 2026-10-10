using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThumblysFieldTripsAPI.Registers;
using UnityEngine;

namespace ThumblysFieldTripsAPI
{

    [BepInPlugin("thumbly.baldiplus.fieldtripAPI", "Thumbly's Field Trip API", "1.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class FieldTripsAPI : BaseUnityPlugin
    {
        public static FieldTripsAPI Instance { get; private set; }
        internal static AssetManager assetMan = new AssetManager();

        internal ConfigEntry<bool> removeVanillaTrips;

        private static List<TripAdditionSettings> TripsToAdd = new List<TripAdditionSettings>();

        private static FieldInfo _tierOneTrips = AccessTools.Field(typeof(PitstopGameManager), "tierOneTrips");
        private static FieldInfo _tierTwoTrips = AccessTools.Field(typeof(PitstopGameManager), "tierTwoTrips");
        private static FieldInfo _tierThreeTrips = AccessTools.Field(typeof(PitstopGameManager), "tierThreeTrips");

        public static MinigameHUD minigameHudPref {  get; private set; }

        /// <summary>
        /// Please add them in the LoadingEventOrder.Pre phase.
        /// </summary>
        public static void AddFieldTrip(FieldTripObject trip, int weight, TripTiers tier)
        {
            TripsToAdd.Add(new TripAdditionSettings(trip, weight, tier));
        }

        private IEnumerator RegisterFieldTrips()
        {
            yield return 2;
            yield return "FTAPI | Adding Field Trips to Generator...";

            PitstopGameManager pitstop = assetMan.Get<PitstopGameManager>("Pitstop");

            List<WeightedFieldTrip> tierOne = new List<WeightedFieldTrip>();
            List<WeightedFieldTrip> tierTwo = new List<WeightedFieldTrip>();
            List<WeightedFieldTrip> tierThree = new List<WeightedFieldTrip>();

            if (!removeVanillaTrips.Value || TripsToAdd.Count <= 0)
            {
                tierOne.AddRange(((WeightedFieldTrip[])_tierOneTrips.GetValue(pitstop)).ToList());
                tierTwo.AddRange(((WeightedFieldTrip[])_tierTwoTrips.GetValue(pitstop)).ToList());
                tierThree.AddRange(((WeightedFieldTrip[])_tierThreeTrips.GetValue(pitstop)).ToList());
            }
            if (TripsToAdd.Count >= 1)
            {
                foreach (TripAdditionSettings trip in TripsToAdd)
                {
                    switch (trip.Tier)
                    {
                        case TripTiers.TierOne:
                            tierOne.Add(trip.Object);
                            break;
                        case TripTiers.TierTwo:
                            tierTwo.Add(trip.Object);
                            break;
                        case TripTiers.TierThree:
                            tierThree.Add(trip.Object);
                            break;
                    }
                }
            }


            _tierOneTrips.SetValue(pitstop, tierOne.ToArray());
            _tierTwoTrips.SetValue(pitstop, tierTwo.ToArray());
            _tierThreeTrips.SetValue(pitstop, tierThree.ToArray());

            yield return "FTAPI | Loading Field trip High Scores...";

            Debug.Log("Loading Custom field trips High scores");
            Singleton<ModdedFieldTripScoreManager>.Instance.Load();
            Debug.Log("Successfully loaded High Scores!");
        }

        private IEnumerator GatherAssets()
        {
            yield return 1;
            yield return "FTAPI | Gathering Resources";


            assetMan.Add<PitstopGameManager>("Pitstop", AssetFinder.FindOfTypeWithName<PitstopGameManager>("PitstopManager", true));
            assetMan.Add<Cubemap>("DefaultSky", AssetFinder.FindOfTypeWithName<Cubemap>("Cubemap_DayStandard", true));

            MinigameBase minigameBase = AssetFinder.FindOfTypeWithName<MinigameBase>("Minigame_PicnicPanic", true);
            MinigameTutorial TutorialBase = AssetFinder.FindOfTypeWithName<MinigameTutorial>("MinigameTutorial_CampfireFrenzy", true);

            minigameHudPref = minigameBase.transform.Find("GameCanvas").Find("Game").Find("MinigameHUD").GetComponent<MinigameHUD>();

            assetMan.Add<MinigameBase>("MinigameBase", minigameBase);
            assetMan.Add<MinigameTutorial>("TutorialBase", TutorialBase);
            assetMan.Add<GameObject>("TutorialScreen", TutorialBase.gameObject.transform.Find("GameCanvas").Find("Game").Find("Page_0").gameObject);


            FieldTripBaseRoomFunction CampingRoomBase = AssetFinder.FindOfTypeWithName<FieldTripBaseRoomFunction>("CampHubRoomFunction", true);
            Transform ObjectBase = CampingRoomBase.transform.Find("ObjectBase");

            assetMan.Add<Transform>("Bus", ObjectBase.Find("Bus"));
            assetMan.Add<Transform>("BusOccupied", ObjectBase.Find("Bus_Occupied"));
            assetMan.Add<Transform>("ExitTrigger", ObjectBase.Find("ExitTrigger"));
            assetMan.Add<GameObject>("MinigameTrigger", ObjectBase.Find("Minigame1Trigger").gameObject);
            assetMan.Add<GameObject>("MinigameTutorialTrigger", ObjectBase.Find("Minigame1Tutorial").gameObject);
            assetMan.Add<GameObject>("IntroBaldi", ObjectBase.Find("AnimatedTripBaldi").gameObject);
        }

        private void Awake()
        {
            Harmony harmony = new Harmony("thumbly.baldiplus.farmcomeback");

            ModdedSaveGame.AddSaveHandler(Info);
            gameObject.AddComponent<ModdedFieldTripScoreManager>();

            harmony.PatchAllConditionals();

            removeVanillaTrips = Config.Bind("Vanilla",
                "Remove Vanilla Field Trips",
                false,
                @"Removes the Vanilla Field Trips.
WARNING: ONLY CHECK TO TRUE IF THERE ARE MODDED FIELD TRIPS OR THIS WILL BREAK THE GAME!");

            LoadingEvents.RegisterOnAssetsLoaded(Info, GatherAssets(), LoadingEventOrder.Start);
            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterFieldTrips(), LoadingEventOrder.Post);
        }
    }

    public enum TripTiers
    {
        TierOne,
        TierTwo,
        TierThree
    }

    internal struct TripAdditionSettings
    {
        public WeightedFieldTrip Object;
        public TripTiers Tier;

        public TripAdditionSettings(FieldTripObject trip, int weight, TripTiers tier = TripTiers.TierOne)
        {
            Object = new WeightedFieldTrip() { selection = trip, weight = weight };
            Tier = tier;
        }
    }
}

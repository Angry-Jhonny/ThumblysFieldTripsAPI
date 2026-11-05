using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ThumblysFieldTripsAPI.Builders;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Helpers;
using ThumblysFieldTripsAPI.Registers;
using UnityEngine;
using UnityEngine.UI;

namespace ThumblysFieldTripsAPI
{

    [BepInPlugin("thumbly.baldiplus.fieldtripAPI", "Thumbly's Field Trip API", "1.4.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class FieldTripsAPI : BaseUnityPlugin
    {
        public static FieldTripsAPI Instance { get; private set; }
        internal static AssetManager assetMan = new AssetManager();

        internal ConfigEntry<bool> removeVanillaTrips;

        private static List<TripAdditionSettings> TripsToAdd = new List<TripAdditionSettings>();
        internal static List<TripMenuSettings> TripScenes = new List<TripMenuSettings>();

        private static FieldInfo _tierOneTrips = AccessTools.Field(typeof(PitstopGameManager), "tierOneTrips");
        private static FieldInfo _tierTwoTrips = AccessTools.Field(typeof(PitstopGameManager), "tierTwoTrips");
        private static FieldInfo _tierThreeTrips = AccessTools.Field(typeof(PitstopGameManager), "tierThreeTrips");

        public static MinigameMetaStorage minigamesMetadata = new MinigameMetaStorage();

        public static List<CustomSpriteRendererAnimator> fieldTripEntranceAnimators = new List<CustomSpriteRendererAnimator>();

        internal static string TimeScoreFormat = "{0:00}:{1:00}.{2:000}";
        internal static bool loadingMods = true;

        public static MinigameHUD minigameHudPref {  get; private set; }

        /// <summary>
        /// Adds a field trip to the Pitstop game manager
        /// Please add them in the LoadingEventOrder.Pre phase.
        /// </summary>
        public static void AddFieldTrip(FieldTripObject trip, int weight, TripTiers tier)
        {
            TripsToAdd.Add(new TripAdditionSettings(trip, weight, tier));
        }

        /// <summary>
        /// Creates a SceneObject and Manager and button to the trip pick screen.
        /// </summary>
        public static void AddTripToMenu(FieldTripObject trip, string nameKey, string descKey)
        {
            string tripString = EnumExtensions.GetExtendedName<FieldTrips>((int)trip.trip);
            PitstopGameManager manager = new FieldTripGameManagerBuilder()
                .SetName("FieldTripManager_" + tripString)
                .SetFieldTrip(trip)
                .Build();
            SceneObject scene = new FieldTripSceneObjectBuilder()
                .SetName(tripString)
                .SetManager(manager)
                .Build();

            TripScenes.Add(new TripMenuSettings(scene, nameKey, descKey));
        }

        private IEnumerator RegisterFinal()
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

            yield return "FTAPI | Loading Final Changes...";

            fieldTripEntranceAnimators.ForEach(x => x.AddAnimation("CampIntro", new SpriteAnimation(new Sprite[]
            {
                AssetFinder.FindOfTypeWithName<Sprite>("CampingTalkSheet_0", true),
                AssetFinder.FindOfTypeWithName<Sprite>("CampingTalkSheet_1", true)
            }, 1)));

            Singleton<ModdedFieldTripScoreManager>.Instance.Load();
        }

        private IEnumerator GatherAssets()
        {
            yield return 1;
            yield return "FTAPI | Gathering Resources";

            MinigameMetadata CampfireMetadata = AssetFinder.FindOfTypeWithName<MinigameBase>("Minigame_CampfireFrenzy", true).GetComponent<Minigame>().AddMeta(this, MinigameFlags.None);
            MinigameMetadata PicnicMetadata = AssetFinder.FindOfTypeWithName<MinigameBase>("Minigame_PicnicPanic", true).GetComponent<Minigame>().AddMeta(this, MinigameFlags.None);

            FieldTripEntranceRoomFunction fieldTripEntrance = AssetFinder.FindOfTypeWithName<FieldTripEntranceRoomFunction>("FieldTripEntranceRoomFunction", true);
            Animator animatorObject = ((Animator)fieldTripEntrance.ReflectionGetVariable("animator"));
            FakeAnimatorSimple fakeAnimator = animatorObject.gameObject.AddFakeAnimatorComponent<FakeAnimatorSimple>();
            CustomSpriteRendererAnimator animator = fakeAnimator.gameObject.AddComponent<CustomSpriteRendererAnimator>();
            animator.renderer = fakeAnimator.GetComponent<SpriteRenderer>();
            fakeAnimator.simpleAnimator = animator;
            fieldTripEntranceAnimators.Add(animator);

            TripScenes.Add(new TripMenuSettings(AssetFinder.FindOfTypeWithName<SceneObject>("Camping", true), "But_Trip_Camp", "Men_CampTrip"));
            TripScenes.Add(new TripMenuSettings(AssetFinder.FindOfTypeWithName<SceneObject>("Farm", true), "But_Trip_Farm", "Men_FarmTrip"));

            assetMan.Add<PitstopGameManager>("Pitstop", AssetFinder.FindOfTypeWithName<PitstopGameManager>("PitstopManager", true));
            assetMan.Add<PitstopGameManager>("FieldTripGameManager", AssetFinder.FindOfTypeWithName<PitstopGameManager>("FieldTripManager_Camping", true));
            assetMan.Add<SceneObject>("CampingScene", AssetFinder.FindOfTypeWithName<SceneObject>("Camping", true));
            assetMan.Add<Cubemap>("DefaultSky", AssetFinder.FindOfTypeWithName<Cubemap>("Cubemap_DayStandard", true));

            MinigameBase minigameBase = AssetFinder.FindOfTypeWithName<MinigameBase>("Minigame_PicnicPanic", true);
            MinigameTutorial TutorialBase = AssetFinder.FindOfTypeWithName<MinigameTutorial>("MinigameTutorial_CampfireFrenzy", true);

            Sprite[] dialSprite = AssetLoader.SpritesFromSpritesheet(8, 4, 1, Vector2.one / 2, AssetLoader.TextureFromMod(this, "TimerSheet.png"));
            minigameHudPref = Instantiate<MinigameHUD>(minigameBase.transform.Find("GameCanvas").Find("Game").Find("MinigameHUD").GetComponent<MinigameHUD>(), MTM101BaldiDevAPI.prefabTransform);
            minigameHudPref.gameObject.name = "ModdedMinigameHUD";
            minigameHudPref.gameObject.AddComponent<MinigameCountdown>();

            Image dialImage = UIHelpers.CreateImage(dialSprite[0], minigameHudPref.transform, new Vector3(16, 24, 0), true);
            minigameHudPref.GetComponent<MinigameCountdown>().dialImage = dialImage;
            minigameHudPref.GetComponent<MinigameCountdown>().dialSprite = dialSprite;
            dialImage.transform.localPosition = new Vector3(15, 320, 0);
            dialImage.enabled = false;
            dialImage.raycastTarget = false;

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
            Harmony harmony = new Harmony("thumbly.baldiplus.fieldtripAPI");

            ModdedSaveGame.AddSaveHandler(Info);
            gameObject.AddComponent<ModdedFieldTripScoreManager>();

            harmony.PatchAllConditionals();

            removeVanillaTrips = Config.Bind("Vanilla",
                "Remove Vanilla Field Trips",
                false,
                @"Removes the Vanilla Field Trips.
WARNING: ONLY CHECK TO TRUE IF THERE ARE MODDED FIELD TRIPS OR THIS WILL BREAK THE GAME!");

            LoadingEvents.RegisterOnAssetsLoaded(Info, GatherAssets(), LoadingEventOrder.Start);
            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterFinal(), LoadingEventOrder.Post);
            AssetLoader.LocalizationFromMod(this);
        }
    }

    public enum TripTiers
    {
        TierOne,
        TierTwo,
        TierThree
    }

    public struct TripAdditionSettings
    {
        public WeightedFieldTrip Object;
        public TripTiers Tier;

        public TripAdditionSettings(FieldTripObject trip, int weight, TripTiers tier = TripTiers.TierOne)
        {
            Object = new WeightedFieldTrip() { selection = trip, weight = weight };
            Tier = tier;
        }
    }

    public struct TripMenuSettings
    {
        public SceneObject trip;
        public string nameKey;
        public string descKey;

        public TripMenuSettings(SceneObject trip, string name, string desc)
        {
            this.trip = trip;
            nameKey = name;
            descKey = desc;
        }
    }
}

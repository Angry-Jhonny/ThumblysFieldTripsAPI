using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using MTM101BaldAPI.UI;
using MTM101BaldAPI.Reflection;

using ThumblysFieldTripsAPI;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Builders;

using FarmComeback.Editor;
using FarmComeback.Haystack;
using MTM101BaldAPI.Components.Animation;

namespace FarmComeback
{
    [BepInPlugin("thumbly.baldiplus.farmcomeback", "Farm Comeback", "1.0.0")]

    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("thumbly.baldiplus.fieldtripAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]

    public class FarmComebackPlugin : BaseUnityPlugin
    {
        public static FarmComebackPlugin Instance { get; internal set; }

        public static Dictionary<string, Sprite> EditorIcons = new Dictionary<string, Sprite>();

        public static string MinigameMidi = "Level_1_End";

        private SoundObject CreateSoundobject(Color color, SoundType type, string key, string name)
        {
            AudioClip clip = AssetLoader.AudioClipFromMod(this, name + ".wav");

            SoundObject sound = ObjectCreators.CreateSoundObject(clip, key, type, color);
            sound.subtitle = key != "";
            return sound;
        }

        private IEnumerator RegisterEverything()
        {
            yield return 5;
            yield return "FarmComeback : Loading Assets...";

            MinigameMidi = AssetLoader.MidiFromMod("FarmMinigame_1_1", this, "FarmMinigame_1_1.mid");

            Texture2D barnBody = AssetLoader.TextureFromMod(this, "BarnRedWood.png");
            Texture2D BarnDoors = AssetLoader.TextureFromMod(this, "BarnDoors.png");
            Texture2D barnBeams = AssetLoader.TextureFromMod(this, "BarnWhiteWood.png");
            Texture2D barnRoof = AssetLoader.TextureFromMod(this, "BarnRoof.png");

            Sprite FarmFlag = AssetLoader.SpriteFromMod(this, Vector2.right * 0.09375f, 16, "FarmFlag.png");

            Sprite HaystackBG0 = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "BackgroundAnimated00.png");
            Sprite HaystackBG1 = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "BackgroundAnimated01.png");
            Sprite HaystackBG2 = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "BackgroundAnimated02.png");
            Sprite HaystackBG3 = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "BackgroundAnimated03.png");
            Sprite HaystackBG4 = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "BackgroundAnimated04.png");
            Sprite PlayerImage = AssetLoader.SpriteFromMod(this, Vector2.one / 2, 1, "Haystack", "PlayerHaystack.png");

            Sprite[] HayImages = AssetLoader.SpritesFromSpritesheet(1, 4, 1, Vector2.one / 2, AssetLoader.TextureFromMod(this, "Haystack", "HayBales_Sheet.png"));
            Sprite[] HayRotate = AssetLoader.SpritesFromSpritesheet(4, 4, 32, Vector2.one / 2, AssetLoader.TextureFromMod(this, "HayRotate_Sheet.png"));

            Sprite[] FarmSheet = AssetLoader.SpritesFromSpritesheet(8, 5, 1, Vector2.one / 2, AssetLoader.TextureFromMod(this, "FarmSheet.png"));
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                EditorIcons.Add("Editor_FarmFlag", AssetLoader.SpriteFromMod(this, Vector2.zero, 1, "Editor", "object_farmflag.png"));
                EditorIcons.Add("Editor_Barn", AssetLoader.SpriteFromMod(this, Vector2.zero, 1, "Editor", "object_barn.png"));
                EditorIcons.Add("Editor_Hay", AssetLoader.SpriteFromMod(this, Vector2.zero, 1, "Editor", "object_hay.png"));
                EditorIcons.Add("Editor_Sign", AssetLoader.SpriteFromMod(this, Vector2.zero, 1, "Editor", "object_sign.png"));
            }

            SoundObject farmIntro = CreateSoundobject(Color.green, SoundType.Voice, "Vfx_BAL_Intro_Farm", "BAL_Intro_Farm");
            SoundObject farmEnterIntro = CreateSoundobject(Color.green, SoundType.Voice, "Vfx_BAL_FarmIntro_1", "BAL_FarmIntro");

            Sprite[] farmTalkSprites = new Sprite[]
            {
                AssetFinder.FindOfTypeWithName<Sprite>("FarmTalkSheet_0", true),
                AssetFinder.FindOfTypeWithName<Sprite>("FarmTalkSheet_1", true)
            };

            List<Sprite> HaystackTutorialSprites = new List<Sprite>();
            foreach (string path in Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(this), "Haystack_TutPics")))
            {
                HaystackTutorialSprites.Add(AssetLoader.SpriteFromFile(path, Vector2.one / 2, 1));
            }

            yield return "FarmComeback : Loading Objects...";

            GameObject FarmFlagObj = Instantiate(AssetFinder.FindOfTypeWithName<GameObject>("Plant", true));
            FarmFlagObj.gameObject.name = "FarmFlag";
            FarmFlagObj.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            FarmFlagObj.GetComponentInChildren<SpriteRenderer>().sprite = FarmFlag;

            GameObject HayObject = Instantiate(AssetFinder.FindOfTypeWithName<GameObject>("Plant", true));
            HayObject.gameObject.name = "HayObject";
            HayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            SpriteRenderer HayRenderer = HayObject.GetComponentInChildren<SpriteRenderer>();
            HayRenderer.sprite = HayRotate[0];
            HayRenderer.transform.localPosition = Vector3.up * 4f;
            SpriteRotator rotator = HayRenderer.gameObject.AddComponent<SpriteRotator>();
            rotator.ReflectionSetVariable("spriteRenderer", HayRenderer);
            rotator.ReflectionSetVariable("sprites", HayRotate);

            Material tileBaseMat = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "TileBase" && x.GetInstanceID() >= 0);
            Shader standardShader = Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Shader Graphs/Standard");

            Material BarnBodyMat = new Material(tileBaseMat) { name = "BarnBodyMat", shader = standardShader };
            BarnBodyMat.SetMainTexture(barnBody);
            Material BarnBeamsMat = new Material(tileBaseMat) { name = "BarnBeamsMat", shader = standardShader };
            BarnBeamsMat.SetMainTexture(barnBeams);
            Material BarnRoofMat = new Material(tileBaseMat) { name = "BarnRoofMat", shader = standardShader };
            BarnRoofMat.SetMainTexture(barnRoof);
            Material BarnDoorMat = new Material(tileBaseMat) { name = "BarnDoorMat", shader = standardShader };
            BarnDoorMat.SetMainTexture(BarnDoors);

            GameObject BarnModel = AssetLoader.ModelFromModManualMaterials(this, new Dictionary<string, Material>()
                {
                    { "BarnBody", BarnBodyMat },
                    { "BarnBeams", BarnBeamsMat },
                    { "BarnRoof", BarnRoofMat },
                    { "BarnDoor", BarnDoorMat }
                }, "Barn.obj");
            BarnModel.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            BarnModel.transform.localScale = Vector3.one * 3;
            BoxCollider collider = BarnModel.AddComponent<BoxCollider>();
            collider.center = Vector3.up * 4f;
            collider.size = new Vector3(8.4f, 8, 7.2f);
            NavMeshObstacle navMesh = BarnModel.AddComponent<NavMeshObstacle>();
            navMesh.shape = NavMeshObstacleShape.Box;
            navMesh.center = collider.center;
            navMesh.size = collider.size;
            navMesh.carving = true;


            LevelLoaderPlugin.Instance.basicObjects.Add("Barn", BarnModel);
            LevelLoaderPlugin.Instance.basicObjects.Add("FarmFlag", FarmFlagObj);
            LevelLoaderPlugin.Instance.basicObjects.Add("HayObject", HayObject);
            LevelLoaderPlugin.Instance.basicObjects.Add("SoonSign", AssetFinder.FindOfTypeWithName<Transform>("Sign0", true).gameObject);

            yield return "FarmComeback : Loading Room Asset...";

            Debug.Log("Loading Hub Room...");

            BinaryReader reader = new BinaryReader(File.OpenRead(Path.Combine(AssetLoader.GetModPath(this), "Room_FarmHub.rbpl")));
            BaldiRoomAsset formatAsset = BaldiRoomAsset.Read(reader);
            reader.Close();
            RoomAsset farmHubRoom = LevelImporter.CreateRoomAsset(formatAsset);
            farmHubRoom.lightPre = LevelLoaderPlugin.Instance.lightTransforms["null"];
            farmHubRoom.category = RoomCategory.FieldTrip;
            farmHubRoom.type = RoomType.Null;
            farmHubRoom.name = "Room_FarmHubNew";
            farmHubRoom.name = "Room_FarmHubNew";

            Debug.Log("Instantiating Farm Function Container...");

            RoomFunctionContainer farmContainer = Instantiate<RoomFunctionContainer>(farmHubRoom.roomFunctionContainer);
            farmContainer.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            farmContainer.name = "FarmHubRoomFunction";
            farmHubRoom.roomFunctionContainer = farmContainer;

            Debug.Log("Building Room Function...");

            FieldTripLoot loot = new FieldTripLoot();

            loot.guaranteedItems = new List<ItemObject>()
            {
                AssetFinder.FindOfTypeWithName<ItemObject>("YTP_250", true),
                AssetFinder.FindOfTypeWithName<ItemObject>("YTP_250", true),
                AssetFinder.FindOfTypeWithName<ItemObject>("YTP_250", true)
            };

            loot.potentialItems = new List<WeightedItemObject>()
            {
                new WeightedItemObject()
                {
                    selection = ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = ItemMetaStorage.Instance.FindByEnum(Items.InvisibilityElixir).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = ItemMetaStorage.Instance.FindByEnum(Items.Teleporter).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value,
                    weight = 100
                },
                new WeightedItemObject()
                {
                    selection = ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value,
                    weight = 100
                }
            };

            List<Sprite> BAL_Farm_Talk1 = new List<Sprite>();
            for (int i = 6; i <= 16; i++)
            {
                BAL_Farm_Talk1.Add(FarmSheet[i]);
            }
            for (int i = 15; i >= 6; i--)
            {
                BAL_Farm_Talk1.Add(FarmSheet[i]);
            }

            List<Sprite> BAL_Farm_Talk2 = new List<Sprite>();
            for (int i = 17; i <= 27; i++)
            {
                BAL_Farm_Talk2.Add(FarmSheet[i]);
            }
            for (int i = 26; i >= 17; i--)
            {
                BAL_Farm_Talk2.Add(FarmSheet[i]);
            }
            List<Sprite> BAL_Farm_Talk3 = new List<Sprite>();
            for (int i = 28; i <= 38; i++)
            {
                BAL_Farm_Talk3.Add(FarmSheet[i]);
            }

            MinigameTutorial HaystackTutorial = new MinigameTutorialBuilder()
                .SetName("MinigameTutorial_Haystack")
                .SetMidi(MinigameMidi)
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[0], new Vector3(0, -32, 0.75f), "Mgm_Haystack_Tut1"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[1], new Vector3(-60, 110, 0.5f), "Mgm_Haystack_Tut2"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[2], new Vector3(-100, -160, 1f), "Mgm_Haystack_Tut3"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[3], new Vector3(128, -32, 0.5f), "Mgm_Haystack_Tut4"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[4], new Vector3(-128, 16, 0.75f), "Mgm_Haystack_Tut5"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[5], new Vector3(80, 120, 0.5f), "Mgm_Haystack_Tut6"))
                .AddScreen(new TutorialScreenInfo(HaystackTutorialSprites[6], new Vector3(140, 150, 0.25f), "Mgm_Haystack_Tut7"))
                .AddTutorAnimation("BAL_Farm_Talk1", new SpriteAnimation(BAL_Farm_Talk1.ToArray(), 1.4f))
                .AddTutorAnimation("BAL_Farm_Talk2", new SpriteAnimation(BAL_Farm_Talk2.ToArray(), 1.4f))
                .AddTutorAnimation("BAL_Farm_Talk3", new SpriteAnimation(BAL_Farm_Talk3.ToArray(), 0.75f))
                .Build();

            MinigameBase HaystackMinigame = new MinigameBaseBuilder<Minigame_Haystack>()
                .SetName("Minigame_Haystack")
                .SetTitle("Mgm_Haystack_Title", "Mgm_Haystack_Desc")
                .SetMusic(MinigameMidi)
                .SetEndText("Mgm_Haystack_Win", "Mgm_Haystack_Lose")
                .SetEnum("Haystack")
                .Build();
            Minigame_Haystack Haystack = HaystackMinigame.GetComponent<Minigame_Haystack>();
            Transform gameRect = HaystackMinigame.transform.Find("GameCanvas").Find("Game");
            MinigameHUD hud = Instantiate(FieldTripsAPI.minigameHudPref, gameRect);
            HaystackMinigame.GetComponent<Minigame_Haystack>().minigameHud = hud;
            Image playerImage = UIHelpers.CreateImage(PlayerImage, gameRect, Vector3.zero, false);
            playerImage.name = "Student";
            SpriteController hayPrefab = UIHelpers.CreateImage(HayImages[0], MTM101BaldiDevAPI.prefabTransform, Vector3.zero, false).gameObject.AddComponent<SpriteController>();
            hayPrefab.name = "FallingHay";

            Haystack.BackgroundSprites.Add(HaystackBG0);
            Haystack.BackgroundSprites.Add(HaystackBG1);
            Haystack.BackgroundSprites.Add(HaystackBG2);
            Haystack.BackgroundSprites.Add(HaystackBG3);
            Haystack.BackgroundSprites.Add(HaystackBG4);
            Haystack.BGLoopStart = 3;
            Haystack.BG = gameRect.Find("BG").GetComponent<Image>();
            Haystack.BG.sprite = HaystackBG0;
            Haystack.HaySprites = HayImages;
            Haystack.playerRect = playerImage.rectTransform;
            Haystack.hayPrefab = hayPrefab;
            Haystack.gameRect = gameRect;
            Haystack.audToss = AssetFinder.FindOfTypeWithName<SoundObject>("CampfireToss", true);
            Haystack.audTossLands = AssetFinder.FindOfTypeWithName<SoundObject>("CampfireLand", true);

            ExtendedFieldTripBaseRoomFunction farmFunction = new FieldTripRoomFunctionBuilder()
                .SetBusTransform(new Vector3(100, 0, 15), Direction.South)
                .SetFunctionGameobject(farmContainer.gameObject) // Required
                .SetIntroTutor(new Vector3(105, 4, 35), farmEnterIntro) // Required
                .SetTutorialBaldiSprite(farmTalkSprites[0]) // Required (if there is a AddMinigameTutorialBaldi)
                .AddMinigameTrigger(new MinigameSettings(HaystackMinigame, new Vector3(116, 5, 150)))
                .AddMinigameTutorialBaldi(HaystackTutorial, new Vector3(125, 5, 144))
                .SetLootTable(loot) // Required
                .SetItemSettings(8, 3) // Required
                .SetPitstopItemOffset(new Vector3(-225.25f, 0, 13.5f)) // Required (if there is a Minigame)
                .Build();

            farmContainer.AddFunction(farmFunction);

            RoomAssetPlacementData farmHubRoomPlacementData = new RoomAssetPlacementData()
            {
                room = farmHubRoom,
                direction = Direction.North,
                position = new IntVector2(0, 16)
            };

            yield return "FarmComeback : Loading Field Trip...";

            ExtendedFieldTripObject farmTrip = new FieldTripBuilder()
                .SetName("Farm")
                .SetEnum(FieldTrips.Farm)
                .SetBaldiSprites(farmTalkSprites)
                .SetSound(farmIntro)
                .SetSkybox(AssetFinder.FindOfTypeWithName<Cubemap>("Cubemap_Twilight", true))
                .SetHubRoom(farmHubRoomPlacementData)
                .SetSpawn(new IntVector2(9, 18), Direction.North)
                .Build();

            FieldTripsAPI.AddFieldTrip(farmTrip, 100, TripTiers.TierOne);

            AssetFinder.FindOfTypeWithName<PitstopGameManager>("FieldTripManager_Camping", true).ReflectionSetVariable("forcedTrip", farmTrip);

            yield return "FarmComeback : Loading Level Editor...";
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio")) { EditorSupport.AddEditorContent(); }
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("thumbly.baldiplus.farmcomeback");

            ModdedSaveGame.AddSaveHandler(Info);

            harmony.PatchAllConditionals();

            LoadingEvents.RegisterOnAssetsLoaded(base.Info, RegisterEverything(), LoadingEventOrder.Pre);
            AssetLoader.LocalizationFromMod(this);
        }
    }
}


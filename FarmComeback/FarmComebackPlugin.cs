using BepInEx;
using BepInEx.Bootstrap;
using BunchOfFieldTrips.Editor;
using BunchOfFieldTrips.Haystack;
using BunchOfFieldTrips.Herding;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThumblysFieldTripsAPI;
using ThumblysFieldTripsAPI.Builders;
using ThumblysFieldTripsAPI.Extentions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace BunchOfFieldTrips
{
    [BepInPlugin("thumbly.baldiplus.bunchoffieldtrips", "Bunch of field trips", "1.3.0")]

    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("thumbly.baldiplus.fieldtripAPI", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]

    public class BunchOfTripsPlugin : BaseUnityPlugin
    {
        public static BunchOfTripsPlugin Instance { get; internal set; }
        public static AssetManager assetMan = new AssetManager();
        public static Dictionary<string, Sprite> EditorIcons = new Dictionary<string, Sprite>();

        public static string FarmMidi = "Level_1_End";
        public static string WildWestMidi = "Level_1_End";

        private readonly Vector2 vectorCenter = Vector2.one / 2;

        private SoundObject CreateSoundobject(Color color, SoundType type, string key, params string[] paths)
        {
            AudioClip clip = AssetLoader.AudioClipFromMod(this, paths);

            SoundObject sound = ObjectCreators.CreateSoundObject(clip, key, type, color);
            sound.subtitle = key != "";
            return sound;
        }

        private Sprite CreateSprite(Vector2 center, float pixelsPerUnit, params string[] paths)
        {
            return AssetLoader.SpriteFromMod(this, center, pixelsPerUnit, paths);
        }

        private Sprite[] CreateSprites(int width, int height, float pixelsPerUnit, Vector2 center, params string[] paths) => CreateSprites(new Vector2Int(width, height), pixelsPerUnit, center, paths);

        private Sprite[] CreateSprites(Vector2Int tiling, float pixelsPerUnit, Vector2 center, params string[] paths)
        {
            return AssetLoader.SpritesFromSpritesheet(tiling.x, tiling.y, pixelsPerUnit, center, AssetLoader.TextureFromMod(this, paths));
        }

        private IEnumerator RegisterFarm()
        {
            yield return 6;
            yield return "BOFT : Loading Farm Assets...";
            string root = "Farm";

            FarmMidi = AssetLoader.MidiFromMod("FarmMinigame_1_1", this, root, "FarmMinigame_1_1.mid");

            Texture2D barnBody = AssetLoader.TextureFromMod(this, root, "BarnRedWood.png");
            Texture2D BarnDoors = AssetLoader.TextureFromMod(this, root, "BarnDoors.png");
            Texture2D barnBeams = AssetLoader.TextureFromMod(this, root, "BarnWhiteWood.png");
            Texture2D barnRoof = AssetLoader.TextureFromMod(this, root, "BarnRoof.png");

            Texture2D fenceTex = AssetLoader.TextureFromMod(this, root, "AnimalFence.png");

            Sprite FarmFlag = CreateSprite(Vector2.right * 0.09375f, 16, root, "FarmFlag.png");
            Sprite AnimalFenceSign = CreateSprite(Vector2.right / 2, 32, root, "AnimalFenceSign.png");

            Sprite HaystackBG0 = CreateSprite(vectorCenter, 1, root, "Haystack", "BackgroundAnimated00.png");
            Sprite HaystackBG1 = CreateSprite(vectorCenter, 1, root, "Haystack", "BackgroundAnimated01.png");
            Sprite HaystackBG2 = CreateSprite(vectorCenter, 1, root, "Haystack", "BackgroundAnimated02.png");
            Sprite HaystackBG3 = CreateSprite(vectorCenter, 1, root, "Haystack", "BackgroundAnimated03.png");
            Sprite HaystackBG4 = CreateSprite(vectorCenter, 1, root, "Haystack", "BackgroundAnimated04.png");
            Sprite PlayerImage = CreateSprite(vectorCenter, 1, root, "Haystack", "PlayerHaystack.png");
            Sprite[] HayImages = CreateSprites(1, 4, 1, vectorCenter, root, "Haystack", "HayBales_Sheet.png");

            Sprite HerdingBG = CreateSprite(vectorCenter, 1, root, "Herding", "HerdingBG.png");
            Sprite HerdingFence = CreateSprite(vectorCenter, 1, root, "Herding", "AnimalFences.png");
            Sprite[] HerdingAnimals = CreateSprites(5, 2, 1, vectorCenter, root, "Herding", "AnimalsSheet.png");

            Sprite[] HayRotate = CreateSprites(4, 4, 32, new Vector2(0.5f, 0.11f), root, "HayRotate_Sheet.png");
            Sprite[] AnimalsBig = CreateSprites(5, 1, 16, Vector2.right / 2, root, "PenAnimals.png");

            Sprite[] FarmSheet = CreateSprites(8, 5, 1, Vector2.one / 2, root, "FarmSheet.png");
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                EditorIcons.Add("Editor_FarmFlag", CreateSprite(Vector2.zero, 1, root, "Editor", "object_farmflag.png"));
                EditorIcons.Add("Editor_Barn", CreateSprite(Vector2.zero, 1, root, "Editor", "object_barn.png"));
                EditorIcons.Add("Editor_Hay", CreateSprite(Vector2.zero, 1, root, "Editor", "object_hay.png"));
                EditorIcons.Add("Editor_Sign", CreateSprite(Vector2.zero, 1, root, "Editor", "object_sign.png"));
                EditorIcons.Add("Editor_Fence", CreateSprite(Vector2.zero, 1, root, "Editor", "object_fence.png"));
                EditorIcons.Add("Editor_AnimalSign", CreateSprite(Vector2.zero, 1, root, "Editor", "object_animalsign.png"));
            }

            SoundObject farmIntro = CreateSoundobject(Color.green, SoundType.Voice, "Vfx_BAL_Intro_Farm", root, "BAL_Intro_Farm.wav");
            SoundObject farmEnterIntro = CreateSoundobject(Color.green, SoundType.Voice, "Vfx_BAL_FarmIntro_1", root, "BAL_FarmIntro.wav");

            SoundObject AnimalsLoop = CreateSoundobject(Color.white, SoundType.Effect, "", root, "AnimalsLoop.wav");
            SoundObject Animal_Chicken = CreateSoundobject(Color.white, SoundType.Effect, "", root, "Herding", "Animal_Chicken.wav");
            SoundObject Animal_Cow = CreateSoundobject(Color.white, SoundType.Effect, "", root, "Herding", "Animal_Cow.wav");
            SoundObject Animal_Pig = CreateSoundobject(Color.white, SoundType.Effect, "", root, "Herding", "Animal_Pig.wav");
            SoundObject Animal_Sheep = CreateSoundobject(Color.white, SoundType.Effect, "", root, "Herding", "Animal_Sheep.wav");
            SoundObject Animal_Gorilla = CreateSoundobject(Color.white, SoundType.Effect, "", root, "Herding", "Animal_Gorilla.wav");

            Sprite[] farmTalkSprites = new Sprite[]
            {
                AssetFinder.FindOfTypeWithName<Sprite>("FarmTalkSheet_0", true),
                AssetFinder.FindOfTypeWithName<Sprite>("FarmTalkSheet_1", true)
            };

            List<Sprite> HaystackTutorialSprites = new List<Sprite>();
            foreach (string path in Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(this), root, "Haystack_TutPics")))
            {
                HaystackTutorialSprites.Add(AssetLoader.SpriteFromFile(path, Vector2.one / 2, 1));
            }

            yield return "BOFT : Loading Farm Objects...";

            GameObject FarmFlagObj = Instantiate(AssetFinder.FindOfTypeWithName<GameObject>("Plant", true));
            FarmFlagObj.gameObject.name = "FarmFlag";
            FarmFlagObj.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            FarmFlagObj.GetComponentInChildren<SpriteRenderer>().sprite = FarmFlag;

            GameObject FenceSignObj = Instantiate(AssetFinder.FindOfTypeWithName<GameObject>("Plant", true));
            FenceSignObj.gameObject.name = "AnimalFenceSign";
            FenceSignObj.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            FenceSignObj.GetComponentInChildren<SpriteRenderer>().sprite = AnimalFenceSign;

            GameObject HayObject = Instantiate(AssetFinder.FindOfTypeWithName<GameObject>("Plant", true).GetComponentInChildren<SpriteRenderer>().gameObject);
            HayObject.gameObject.name = "HayObject";
            HayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            SpriteRenderer HayRenderer = HayObject.GetComponent<SpriteRenderer>();
            HayRenderer.sprite = HayRotate[0];
            HayRenderer.transform.position = Vector3.zero;
            AnimatedSpriteRotator rotator = HayRenderer.gameObject.AddComponent<AnimatedSpriteRotator>();
            rotator.ReflectionSetVariable("renderer", HayRenderer);
            SpriteRotationMap rotationMap = new SpriteRotationMap();
            rotationMap.angleCount = 16;
            rotationMap.ReflectionSetVariable("spriteSheet", HayRotate);
            rotator.ReflectionSetVariable("spriteMap", new SpriteRotationMap[1] { rotationMap });

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
                }, "Farm", "Barn.obj");
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

            Material FenceMat = new Material(tileBaseMat) { name = "AnimalFence", shader = standardShader };
            FenceMat.SetMainTexture(fenceTex);

            GameObject FenceObject = new GameObject("AnimalFence");
            FenceObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject FenceMesh1 = new GameObject("FenceModel");
            FenceMesh1.transform.SetParent(FenceObject.transform);
            FenceMesh1.transform.localPosition = new Vector3(0, 5, 5);
            FenceMesh1.transform.localScale = Vector3.one * 10;
            FenceMesh1.AddComponent<MeshCollider>().sharedMesh = AssetFinder.FindOfTypeWithName<Mesh>("Quad", true);
            FenceMesh1.AddComponent<MeshFilter>().mesh = AssetFinder.FindOfTypeWithName<Mesh>("Quad", true);
            FenceMesh1.AddComponent<MeshRenderer>().sharedMaterial = FenceMat;
            GameObject FenceMesh2 = Instantiate<GameObject>(FenceMesh1, FenceObject.transform, true);
            FenceMesh2.transform.localRotation = Quaternion.Euler(0, 180, 0);

            AssetFinder.FindOfTypeWithName<Transform>("Sign0", true).GetComponentInChildren<SpriteRenderer>().material = AssetFinder.FindOfTypeWithName<Material>("SpriteStandard_NoBillboard", true);

            LevelLoaderPlugin.Instance.basicObjects.Add("Barn", BarnModel);
            LevelLoaderPlugin.Instance.basicObjects.Add("FarmFlag", FarmFlagObj);
            LevelLoaderPlugin.Instance.basicObjects.Add("AnimalSign", FenceSignObj);
            LevelLoaderPlugin.Instance.basicObjects.Add("HayObject", HayObject);
            LevelLoaderPlugin.Instance.basicObjects.Add("AnimalFence", FenceObject);
            LevelLoaderPlugin.Instance.basicObjects.Add("SoonSign", AssetFinder.FindOfTypeWithName<Transform>("Sign0", true).gameObject);

            yield return "BOFT : Loading Farm Hub Asset...";

            BinaryReader reader = new BinaryReader(File.OpenRead(Path.Combine(AssetLoader.GetModPath(this), "Farm", "Room_FarmHub.rbpl")));
            BaldiRoomAsset formatAsset = BaldiRoomAsset.Read(reader);
            reader.Close();
            RoomAsset farmHubRoom = LevelImporter.CreateRoomAsset(formatAsset);
            farmHubRoom.lightPre = LevelLoaderPlugin.Instance.lightTransforms["null"];
            farmHubRoom.category = RoomCategory.FieldTrip;
            farmHubRoom.type = RoomType.Null;

            RoomFunctionContainer farmContainer = Instantiate<RoomFunctionContainer>(farmHubRoom.roomFunctionContainer);
            farmContainer.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            farmContainer.name = "FarmHubRoomFunction";
            farmHubRoom.roomFunctionContainer = farmContainer;

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

            yield return "BOFT : Loading Farm Minigames...";

            MinigameTutorial HaystackTutorial = new MinigameTutorialBuilder()
                .SetName("MinigameTutorial_Haystack")
                .SetMidi(FarmMidi)
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

            MinigameBase HaystackMinigame = new MinigameBaseBuilder<Minigame_Haystack>(Info)
                .SetName("Minigame_Haystack")
                .SetTitle("Mgm_Haystack_Title", "Mgm_Haystack_Desc")
                .SetMusic(FarmMidi)
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
            Haystack.audWrong = AssetFinder.FindOfTypeWithName<SoundObject>("Activity_Incorrect", true);



            MinigameBase HerdingMinigame = new MinigameBaseBuilder<Minigame_Herding>(Info)
                .SetName("Minigame_Herding")
                .SetTitle("Mgm_Herding_Title", "Mgm_Herding_Desc")
                .SetMusic(FarmMidi)
                .SetEndText("Mgm_Herding_Win", "Mgm_Herding_Lose")
                .SetEnum("Herding")
                .NeedsCursor()
                .Build();
            Minigame_Herding Herding = HerdingMinigame.GetComponent<Minigame_Herding>();
            gameRect = HerdingMinigame.transform.Find("GameCanvas").Find("Game");
            gameRect.Find("BG").GetComponent<Image>().sprite = HerdingBG;
            Herding.gameRect = gameRect;
            GameObject animalsRectObject  = new GameObject("AnimalsRect");
            RectTransform animalsRect = animalsRectObject.AddComponent<RectTransform>();
            animalsRect.SetParent(gameRect);
            animalsRect.gameObject.transform.localScale = Vector3.one;
            Herding.animalRect = animalsRect;
            Image fenceImage = new GameObject("FenceImage").AddComponent<Image>();
            fenceImage.gameObject.layer = LayerMask.NameToLayer("UI");
            fenceImage.transform.SetParent(gameRect);
            fenceImage.sprite = HerdingFence;
            fenceImage.gameObject.transform.localScale = Vector3.one;
            fenceImage.rectTransform.sizeDelta = new Vector2(480, 360);
            fenceImage.rectTransform.anchoredPosition = new Vector2(240, 180);
            fenceImage.raycastTarget = false;
            hud = Instantiate(FieldTripsAPI.minigameHudPref, gameRect);
            Herding.minigameHud = hud;
            Image sector1Sign = UIHelpers.CreateImage(HerdingAnimals[0], fenceImage.transform, new Vector3(-21, 162, 0), false);
            Image sector2Sign = UIHelpers.CreateImage(HerdingAnimals[0], fenceImage.transform, new Vector3(172, 162, 0), false);
            Image sector3Sign = UIHelpers.CreateImage(HerdingAnimals[0], fenceImage.transform, new Vector3(-21, 29, 0), false);
            Image sector4Sign = UIHelpers.CreateImage(HerdingAnimals[0], fenceImage.transform, new Vector3(172, 29, 0), false);
            sector1Sign.raycastTarget = false;
            sector2Sign.raycastTarget = false;
            sector3Sign.raycastTarget = false;
            sector4Sign.raycastTarget = false;
            Herding.sectorSignImages.Add(sector1Sign);
            Herding.sectorSignImages.Add(sector2Sign);
            Herding.sectorSignImages.Add(sector3Sign);
            Herding.sectorSignImages.Add(sector4Sign);
            GameObject animalsSFX = new GameObject("AnimalsSFX");
            animalsSFX.transform.SetParent(gameRect.transform);
            AudioSource source = animalsSFX.AddComponent<AudioSource>();
            source.dopplerLevel = 0;
            source.playOnAwake = false;
            AudioManager animalAmbiance = animalsSFX.AddComponent<AudioManager>();
            animalAmbiance.useUnscaledPitch = true;
            animalAmbiance.ignoreListenerPause = true;
            animalAmbiance.positional = false;
            animalAmbiance.ReflectionSetVariable("disableSubtitles", true);
            animalAmbiance.volumeModifier = 0.5f;
            animalAmbiance.audioDevice = source;
            Herding.animalAmbiance = animalAmbiance;
            Herding.aud_animals = AnimalsLoop;
            HerdingMinigame.AddDisableOnPauseObject(animalsSFX);
            Herding.aud_correct = AssetFinder.FindOfTypeWithName<SoundObject>("YTPPickup_0", true);
            Herding.aud_incorrect = AssetFinder.FindOfTypeWithName<SoundObject>("Activity_Incorrect", true);


            Image animalPref = new GameObject("FarmAnimal").AddComponent<Image>();
            animalPref.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            animalPref.gameObject.tag = "Button";
            animalPref.gameObject.layer = LayerMask.NameToLayer("UI");
            animalPref.rectTransform.sizeDelta = Vector2.one * 20;
            animalPref.gameObject.transform.localScale = Vector3.one;
            animalPref.sprite = HerdingAnimals[2];
            BaseAnimalButton animalButton = animalPref.gameObject.AddComponent<BaseAnimalButton>();
            animalButton.image = animalPref;
            Herding.farmAnimalPrefab = animalButton;

            Minigame_Herding.possibleAnimals.AddRange(new AnimalInfo[]
            {
                new AnimalInfo()
                {
                    type = AnimalType.Chicken,
                    animal = HerdingAnimals[0],
                    animalFake = HerdingAnimals[5],
                    sound = Animal_Chicken
                },
                new AnimalInfo()
                {
                    type = AnimalType.Cow,
                    animal = HerdingAnimals[1],
                    animalFake = HerdingAnimals[6],
                    sound = Animal_Cow
                },
                new AnimalInfo()
                {
                    type = AnimalType.Pig,
                    animal = HerdingAnimals[2],
                    animalFake = HerdingAnimals[7],
                    sound = Animal_Pig
                },
                new AnimalInfo()
                {
                    type = AnimalType.Sheep,
                    animal = HerdingAnimals[3],
                    animalFake = HerdingAnimals[8],
                    sound = Animal_Sheep
                },
                new AnimalInfo()
                {
                    type = AnimalType.Gorilla,
                    animal = HerdingAnimals[4],
                    animalFake = HerdingAnimals[9],
                    sound = Animal_Gorilla
                }
            });

            GameObject plantPref = AssetFinder.FindOfTypeWithName<GameObject>("Plant", true);

            GameObject farmAnimalsObject = new GameObject("FarmAnimals");
            farmAnimalsObject.AddComponent<SphereCollider>().radius = 5f;
            farmAnimalsObject.transform.position = new Vector3(170, 0, 100);
            source = farmAnimalsObject.AddComponent<AudioSource>();
            source.dopplerLevel = 0;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.maxDistance = 70;
            source.minDistance = 25;
            source.playOnAwake = false;
            source.spatialBlend = 1;
            PropagatedAudioManager farmAnimalsAudio = farmAnimalsObject.AddComponent<PropagatedAudioManager>();
            farmAnimalsAudio.audioDevice = source;
            farmAnimalsAudio.ReflectionSetVariable("disableSubtitles", true);
            farmAnimalsAudio.ReflectionSetVariable("soundOnStart", new SoundObject[] { AnimalsLoop });
            farmAnimalsAudio.ReflectionSetVariable("loopOnStart", true);
            GameObject animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(3, 0, 10);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[1];
            animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(-12, 0, 15);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[0];
            animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(-15, 0, 5);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[3];
            animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(-7, 0, -5);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[2];
            animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(10, 0, -10);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[3];
            animal = Instantiate(plantPref, farmAnimalsObject.transform);
            animal.transform.localPosition = new Vector3(-15, 0, -15);
            animal.GetComponentInChildren<SpriteRenderer>().sprite = AnimalsBig[4];
            farmAnimalsObject.SetActive(false);

            FieldTripBaseRoomFunction farmFunction = new FieldTripRoomFunctionBuilder()
                .SetBusTransform(new Vector3(100, 0, 15), Direction.South)
                .SetFunctionGameobject(farmContainer.gameObject) // Required
                .SetIntroTutor(new Vector3(105, 4, 35), farmEnterIntro) // Required
                .SetTutorBaldiAnimation(new SpriteAnimation(new Sprite[] { farmTalkSprites[0], farmTalkSprites[1] }, 1)) // Required
                .AddMinigameTrigger(new MinigameSettings(HaystackMinigame, new Vector3(116, 5, 150)))
                .AddMinigameTutorialBaldi(HaystackTutorial, new Vector3(125, 5, 144))
                .AddMinigameTrigger(new MinigameSettings(HerdingMinigame, new Vector3(145, 5, 100)))
                .AddMinigameTutorialBaldi(HaystackTutorial, new Vector3(145, 5, 84))
                .SetLootTable(loot) // Required
                .SetItemSettings(8, 3) // Required
                .SetPitstopItemOffset(new Vector3(-225.25f, 0, 13.5f)) // Required (if there is a Minigame)
                .AddObjectToToggle(farmAnimalsObject)
                .Build();

            farmContainer.AddFunction(farmFunction);

            RoomAssetPlacementData farmHubRoomPlacementData = new RoomAssetPlacementData()
            {
                room = farmHubRoom,
                direction = Direction.North,
                position = new IntVector2(0, 16)
            };

            yield return "BOFT : Creating Farm Field Trip...";

            ExtendedFieldTripObject farmTrip = new FieldTripBuilder()
                .SetName("Farm")
                .SetEnum(FieldTrips.Farm)
                .SetBaldiAnimation("FarmIntro", farmTalkSprites[0], farmTalkSprites[1])
                .SetSound(farmIntro)
                .SetSkybox(AssetFinder.FindOfTypeWithName<Cubemap>("Cubemap_Twilight", true))
                .SetHubRoom(farmHubRoomPlacementData)
                .SetSpawn(new IntVector2(9, 18), Direction.North)
                .Build();

            FieldTripsAPI.AddFieldTrip(farmTrip, 100, TripTiers.TierOne);

            PitstopGameManager farmTripGameManager = new FieldTripGameManagerBuilder()
                .SetName("FieldTripManager_Farm")
                .SetFieldTrip(farmTrip)
                .Build();

            SceneObject vanillaFarmScene = AssetFinder.FindOfTypeWithName<SceneObject>("Farm", true);
            vanillaFarmScene.levelTitle = "FT";
            vanillaFarmScene.storeUsesNextLevelData = true;
            vanillaFarmScene.skippable = true;
            vanillaFarmScene.usesMap = false;
            vanillaFarmScene.manager = farmTripGameManager;
            vanillaFarmScene.levelAsset = AssetFinder.FindOfTypeWithName<LevelAsset>("Pitstop", true);
            vanillaFarmScene.extraAsset = AssetFinder.FindOfTypeWithName<ExtraLevelDataAsset>("PitstopExtraData", true);
            vanillaFarmScene.extraAsset = AssetFinder.FindOfTypeWithName<ExtraLevelDataAsset>("PitstopExtraData", true);
            vanillaFarmScene.skybox = AssetFinder.FindOfTypeWithName<Cubemap>("Cubemap_DayStandard", true);


            yield return "BOFT : Adding Farm to Level Editor...";
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio")) { EditorSupport.AddEditorContent(); }
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("thumbly.baldiplus.bunchoffieldtrips");

            ModdedSaveGame.AddSaveHandler(Info);

            harmony.PatchAllConditionals();

            LoadingEvents.RegisterOnAssetsLoaded(base.Info, RegisterFarm(), LoadingEventOrder.Pre);
            AssetLoader.LocalizationFromMod(this);
        }
    }
}


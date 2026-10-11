
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThumblysFieldTripsAPI.Extentions;
using ThumblysFieldTripsAPI.Patches;
using ThumblysFieldTripsAPI.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace ThumblysFieldTripsAPI.Builders
{
    public class FieldTripRoomFunctionBuilder
    {
        private GameObject _Gameobject = null;

        private Vector3 _IntroPosition = Vector3.zero;
        private SoundObject _IntroAudio = null;

        private Vector3 _busPos = Vector3.zero;
        private Direction _busDirection = Direction.North;

        private List<MinigameSettings> _MinigamesToAdd = new List<MinigameSettings>();
        private List<MinigameSettings> _TutorialsToAdd = new List<MinigameSettings>();
        private Sprite _TutorialBaldiSprite;
        private List<GameObject> _ObjectsToAdd = new List<GameObject>();
        private List<GameObject> _ObjectsToToggle = new List<GameObject>();

        private FieldTripLoot _loot = new FieldTripLoot()
        {
            potentialItems = new List<WeightedItemObject>(),
            guaranteedItems = new List<ItemObject>()
        };
        private float _PickupLimit = 3;
        private float _PickupCount = 8;
        private Vector3 _PitstopItmOffset = new Vector3(125, 0, -240);

        private TimeScaleModifier _timescale = new TimeScaleModifier(0f, 0f, 0f);

        private static readonly FieldInfo _onPress = AccessTools.Field(typeof(ClickableEventTrigger), "OnPress");

        private static readonly FieldInfo _potentialItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "potentialItems");
        private static readonly FieldInfo _guaranteedItems = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "guaranteedItems");
        private static readonly FieldInfo _objectBase = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "objectBase");
        private static readonly FieldInfo _objectToToggleAfterMinigame = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "objectToToggleAfterMinigame");
        private static readonly FieldInfo _introAudioManager = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "introAudioManager");
        private static readonly FieldInfo _audIntro = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "audIntro");
        private static readonly FieldInfo _itemCount = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "itemCount");
        private static readonly FieldInfo _itemLimit = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "itemLimit");
        private static readonly FieldInfo _pickupsToPitOffset = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "pickupsToPitOffset");
        private static readonly FieldInfo _minigameTimeScale = AccessTools.Field(typeof(FieldTripBaseRoomFunction), "minigameTimeScale");

        /// <summary>
        /// Builds the ExtendedFieldTripBaseRoomFunction.
        /// </summary>
        public ExtendedFieldTripBaseRoomFunction Build()
        {
            ExtendedFieldTripBaseRoomFunction Function = _Gameobject.AddComponent<ExtendedFieldTripBaseRoomFunction>();

            GameObject objectBase = new GameObject("ObjectBase");
            objectBase.transform.SetParent(_Gameobject.transform, objectBase.transform);

            Transform bus = Object.Instantiate(FieldTripsAPI.assetMan.Get<Transform>("Bus"), objectBase.transform);
            bus.localPosition = _busPos;
            bus.localRotation = _busDirection.ToRotation();
            Transform busOccupied = Object.Instantiate(FieldTripsAPI.assetMan.Get<Transform>("BusOccupied"), objectBase.transform);
            busOccupied.localPosition = bus.localPosition;
            busOccupied.localRotation = bus.rotation;
            Transform exitTrigger = Object.Instantiate(FieldTripsAPI.assetMan.Get<Transform>("ExitTrigger"), bus);
            exitTrigger.localPosition = new Vector3(6, 5, 0);
            exitTrigger.localRotation = Quaternion.Euler(0, 180, 0);
            exitTrigger.SetParent(objectBase.transform, true);

            foreach (GameObject obj in _ObjectsToToggle)
            {
                obj.transform.SetParent(objectBase.transform, true);
            }

            int i = 1;
            foreach (MinigameSettings minigame in _MinigamesToAdd)
            {
                GameObject Trigger = Object.Instantiate(FieldTripsAPI.assetMan.Get<GameObject>("MinigameTrigger"), objectBase.transform);
                _ObjectsToToggle.Add(Trigger);
                Trigger.transform.position = minigame.TriggerPosition;
                Trigger.GetComponent<CapsuleCollider>().radius = minigame.TriggerRadius;
                Trigger.GetComponent<CapsuleCollider>().height = minigame.TriggerHeight;
                Trigger.name = "Minigame" + i + "Trigger";

                Object.Destroy(Trigger.GetComponent<ClickableEventTrigger>());
                MinigameStartClickable newTrigger = Trigger.AddComponent<MinigameStartClickable>();
                newTrigger.roomFunction = Function;
                newTrigger.minigame = minigame.minigame;
                i++;
            }

            i = 1;
            foreach (MinigameSettings tutorial in _TutorialsToAdd)
            {
                GameObject Trigger = Object.Instantiate(FieldTripsAPI.assetMan.Get<GameObject>("MinigameTutorialTrigger"), objectBase.transform);
                _ObjectsToToggle.Add(Trigger);
                Trigger.transform.position = tutorial.TriggerPosition;
                Trigger.name = "Minigame" + i + "Tutorial";
                Trigger.GetComponentInChildren<SpriteRenderer>().sprite = _TutorialBaldiSprite;

                Object.Destroy(Trigger.GetComponent<ClickableEventTrigger>());
                MinigameStartClickable newTrigger = Trigger.AddComponent<MinigameStartClickable>();
                newTrigger.roomFunction = Function;
                newTrigger.minigame = tutorial.minigame;
                i++;
            }

            GameObject IntroBaldi = Object.Instantiate(FieldTripsAPI.assetMan.Get<GameObject>("IntroBaldi"), objectBase.transform);
            IntroBaldi.name = FieldTripsAPI.assetMan.Get<GameObject>("IntroBaldi").name;
            IntroBaldi.transform.position = _IntroPosition;
            IntroBaldi.GetComponent<Animator>().enabled = true;
            _introAudioManager.SetValue(Function, IntroBaldi.GetComponent<AudioManager>());
            _audIntro.SetValue(Function, _IntroAudio);
            _ObjectsToToggle.Add(IntroBaldi.gameObject);
            Function.introBaldiSwap = FieldTripSpriteSwap.AddTripSpriteSwap(IntroBaldi);

            _objectBase.SetValue(Function, objectBase.transform);
            _objectToToggleAfterMinigame.SetValue(Function, _ObjectsToToggle.ToArray());
            _potentialItems.SetValue(Function, _loot.potentialItems.ToArray());
            _guaranteedItems.SetValue(Function, _loot.guaranteedItems.ToList());
            _itemCount.SetValue(Function, _PickupCount);
            _itemLimit.SetValue(Function, _PickupLimit);
            _pickupsToPitOffset.SetValue(Function, _PitstopItmOffset);
            _minigameTimeScale.SetValue(Function, _timescale);

            return Function;
        }

        /// <summary>
        /// Gives the Object the baseObjects and the RoomFunction.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetFunctionGameobject(GameObject gameObject)
        {
            _Gameobject = gameObject;
            return this;
        }

        /// <summary>
        /// Sets the position and rotation of the bus (on the X and Z axis).
        /// </summary>
        public FieldTripRoomFunctionBuilder SetBusTransform(Vector3 busPosition, Direction direction)
        {
            _busPos = busPosition;
            _busDirection = direction;
            return this;
        }

        /// <summary>
        /// Set the introduction baldi spawn location and audio.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetIntroTutor(Vector3 baldiPosition, SoundObject introAudio)
        {
            _IntroPosition = baldiPosition;
            _IntroAudio = introAudio;
            return this;
        }

        /// <summary>
        /// Sets the timescale when a minigame is Initialized.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetTimescale(TimeScaleModifier timescale)
        {
            _timescale = timescale;
            return this;
        }

        /// <summary>
        /// Creates a trigger for the minigame and sets it up.
        /// </summary>
        public FieldTripRoomFunctionBuilder AddMinigameTrigger(MinigameSettings minigame)
        {
            _MinigamesToAdd.Add(minigame);
            return this;
        }

        /// <summary>
        /// Creates a baldi that will explain the given tutorial.
        /// </summary>
        public FieldTripRoomFunctionBuilder AddMinigameTutorialBaldi(MinigameTutorial tutorial, Vector3 baldiPosition)
        {
            _TutorialsToAdd.Add(new MinigameSettings(tutorial.GetComponent<MinigameBase>(), baldiPosition));
            return this;
        }

        /// <summary>
        /// Set the introduction baldi spawn location and audio.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetTutorialBaldiSprite(Sprite baldiSprite)
        {
            _TutorialBaldiSprite = baldiSprite;
            return this;
        }

        /// <summary>
        /// Sets the field trip loot table.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetLootTable(FieldTripLoot loot)
        {
            _loot = loot;
            return this;
        }

        /// <summary>
        /// Sets the Loot items Settings.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetItemSettings(int ItemCount = 8, int PickupLimit = 3)
        {
            
            return this;
        }

        /// <summary>
        /// Sets the pitstop Items offset.
        /// </summary>
        public FieldTripRoomFunctionBuilder SetPitstopItemOffset(Vector3 offset)
        {
            
            return this;
        }

        /// <summary>
        /// Adds an object to disable or enable when you beat a minigame.
        /// </summary>
        public FieldTripRoomFunctionBuilder AddObjectToToggle(GameObject gameObject)
        {
            _ObjectsToToggle.Add(gameObject);
            return this;
        }
    }

    public struct MinigameSettings
    {
        public Vector3 TriggerPosition;
        public float TriggerRadius;
        public float TriggerHeight;

        public MinigameBase minigame;

        public MinigameSettings(MinigameBase minigame, Vector3 position, float Radius = 4, float Height = 10)
        {
            TriggerRadius = Radius;
            TriggerHeight = Height;
            TriggerPosition = position;
            this.minigame = minigame;
        }
    }
}

using ThumblysFieldTripsAPI.Extentions;
using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Builders
{
    public class FieldTripBuilder
    {
        private RoomAssetPlacementData _fieldTripHub;

        private FieldTrips _type;
        private string _enumName = "";

        private SoundObject _introAudio;
        private Cubemap _skybox = FieldTripsAPI.assetMan.Get<Cubemap>("DefaultSky");
        private Direction _spawnDirection;
        private Vector3 _spawnPoint;
        private string _nameKey = "Unnamed";
        private Sprite _baldiIdle;
        private Sprite _baldiTalk;

        /// <summary>
        /// Builds the ExtendedFieldTripObject.
        /// </summary>
        public ExtendedFieldTripObject Build()
        {
            ExtendedFieldTripObject fieldTripObject = ScriptableObject.CreateInstance<ExtendedFieldTripObject>();

            fieldTripObject.name = _nameKey + "Trip";

            fieldTripObject.tripHub = _fieldTripHub;
            FieldTrips fieldTripType = _type;
            if (_enumName != "")
            {
                fieldTripType = EnumExtensions.ExtendEnum<FieldTrips>(_enumName);
            }
            fieldTripObject.trip = fieldTripType;
            fieldTripObject.intro = _introAudio;
            fieldTripObject.skybox = _skybox;
            fieldTripObject.spawnDirection = _spawnDirection;
            fieldTripObject.spawnPoint = _spawnPoint;
            fieldTripObject.animationName = "CampIntro";
            fieldTripObject.nameKey = "Level_Trip_" + _nameKey;
            fieldTripObject.idleSprite = _baldiIdle;
            fieldTripObject.talkSprite = _baldiTalk;

            return fieldTripObject;
        }

        /// <summary>
        /// Set the field trip name.
        /// </summary>
        public FieldTripBuilder SetName(string name)
        {
            _nameKey = name;
            return this;
        }

        /// <summary>
        /// Set the player spawn location.
        /// </summary>
        public FieldTripBuilder SetSpawn(Vector3 position, Direction direction)
        {
            _spawnPoint = position;
            _spawnPoint.y = 5f;
            _spawnDirection = direction;
            return this;
        }

        /// <summary>
        /// Set the player spawn location.
        /// </summary>
        public FieldTripBuilder SetSpawn(Vector2 position, Direction direction)
        {
            _spawnPoint = new Vector3(position.x, 5f, position.y);
            _spawnDirection = direction;
            return this;
        }

        /// <summary>
        /// Set the player spawn location in cells.
        /// </summary>
        public FieldTripBuilder SetSpawn(IntVector2 position, Direction direction)
        {
            _spawnPoint = new Vector3((position.x * 10) + 5, 5f, (position.z * 10) + 5);
            _spawnDirection = direction;
            return this;
        }

        /// <summary>
        /// Sets the sound that Baldi will "say" when you go near the bus.
        /// </summary>
        public FieldTripBuilder SetSound(SoundObject sound)
        {
            _introAudio = sound;
            return this;
        }

        /// <summary>
        /// Set the FieldTrips enum that this field trip will use.
        /// </summary>
        public FieldTripBuilder SetEnum(FieldTrips type)
        {
            _type = type;
            _enumName = "";
            return this;
        }

        /// <summary>
        /// Create a FieldTrips enum using EnumExtensions with the specified name.
        /// </summary>
        public FieldTripBuilder SetEnum(string enumToRegister)
        {
            _type = FieldTrips.Camp;
            _enumName = enumToRegister;
            return this;
        }

        /// <summary>
        /// Sets the Cubemap for the field trip hub.
        /// </summary>
        public FieldTripBuilder SetSkybox(Cubemap skybox)
        {
            _skybox = skybox;
            return this;
        }

        /// <summary>
        /// Sets the Hub RoomAssetPlacementData.
        /// </summary>
        public FieldTripBuilder SetHubRoom(RoomAssetPlacementData room)
        {
            _fieldTripHub = room;
            return this;
        }

        /// <summary>
        /// Sets the Baldi idle and talk sprites.
        /// </summary>
        public FieldTripBuilder SetBaldiSprites(Sprite idle, Sprite talk)
        {
            _baldiIdle = idle;
            _baldiTalk = talk;
            return this;
        }

        /// <summary>
        /// Sets the Baldi idle and talk sprites.
        /// </summary>
        public FieldTripBuilder SetBaldiSprites(Sprite[] sprites)
        {
            _baldiIdle = sprites[0];
            _baldiTalk = sprites[1];
            return this;
        }
    }
}

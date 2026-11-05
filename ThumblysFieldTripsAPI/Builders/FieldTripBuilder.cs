using ThumblysFieldTripsAPI.Extentions;
using MTM101BaldAPI;
using System;
using UnityEngine;
using MTM101BaldAPI.Components.Animation;

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
        private string _animName = "";
        private SpriteAnimation _baldiAnimation;

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
            if (_animName == string.Empty)
            {
                _animName = fieldTripType.ToStringExtended() + "Intro_" + ((int)fieldTripType).ToString();
            }
            fieldTripObject.trip = fieldTripType;
            fieldTripObject.intro = _introAudio;
            fieldTripObject.skybox = _skybox;
            fieldTripObject.spawnDirection = _spawnDirection;
            fieldTripObject.spawnPoint = _spawnPoint;
            fieldTripObject.animationName = _animName;
            fieldTripObject.nameKey = "Level_Trip_" + _nameKey;
            fieldTripObject.talkAnimation = _baldiAnimation;

            FieldTripsAPI.fieldTripEntranceAnimators.ForEach(x => x.AddAnimation(fieldTripObject.animationName, _baldiAnimation));

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
        [Obsolete("Use SetBaldiAnimation instead.")]
        public FieldTripBuilder SetBaldiSprites(Sprite idle, Sprite talk) => SetBaldiAnimation(idle, talk);

        /// <summary>
        /// Sets the Baldi idle and talk sprites.
        /// </summary>
        [Obsolete("Use SetBaldiAnimation instead.")]
        public FieldTripBuilder SetBaldiSprites(Sprite[] sprites) => SetBaldiAnimation(sprites[0], sprites[1]);

        /// <summary>
        /// Sets the Baldi talk animation.
        /// </summary>
        public FieldTripBuilder SetBaldiAnimation(Sprite idle, Sprite talk)
        {
            _baldiAnimation = new SpriteAnimation(new Sprite[] { idle, talk }, 1);
            return this;
        }

        /// <summary>
        /// Sets the Baldi talk animation.
        /// /// NOTE: Setting a custom animation name could cause other mods with the same key to break.
        /// </summary>
        public FieldTripBuilder SetBaldiAnimation(string animName, Sprite idle, Sprite talk)
        {
            _animName = animName;
            _baldiAnimation = new SpriteAnimation(new Sprite[] { idle, talk }, 1);
            return this;
        }

        /// <summary>
        /// Sets the Baldi talk animation.
        /// </summary>
        public FieldTripBuilder SetBaldiAnimation(SpriteAnimation animation)
        {
            _baldiAnimation = animation;
            return this;
        }

        /// <summary>
        /// Sets the Baldi talk animation.
        /// NOTE: Setting a custom animation name could cause other mods with the same key to break.
        /// </summary>
        public FieldTripBuilder SetBaldiAnimation(string animName, SpriteAnimation animation)
        {
            _animName = animName;
            _baldiAnimation = animation;
            return this;
        }
    }
}

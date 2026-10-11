using HarmonyLib;
using MTM101BaldAPI;
using System.Reflection;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Builders
{
    public class FieldTripGameManagerBuilder
    {
        private string _name = "FieldTripManager_Unnamed";
        private FieldTripObject _fieldTrip;

        private static FieldInfo _forcedTrip = AccessTools.Field(typeof(PitstopGameManager), "forcedTrip");

        /// <summary>
        /// Builds the PitstopGameManager with the given field trip.
        /// </summary>
        public PitstopGameManager Build()
        {
            PitstopGameManager gameManager = Object.Instantiate(FieldTripsAPI.assetMan.Get<PitstopGameManager>("FieldTripGameManager"), MTM101BaldiDevAPI.prefabTransform);
            _forcedTrip.SetValue(gameManager, _fieldTrip);
            gameManager.gameObject.name = _name;

            return gameManager;
        }

        /// <summary>
        /// Sets the name for the gameobject.
        /// </summary>
        public FieldTripGameManagerBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the forced field trip for the manager.
        /// </summary>
        public FieldTripGameManagerBuilder SetFieldTrip(FieldTripObject trip)
        {
            _fieldTrip = trip;
            return this;
        }
    }
}

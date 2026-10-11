using MTM101BaldAPI;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Builders
{
    public class FieldTripSceneObjectBuilder
    {
        private string _name = "UnnamedTrip";
        private PitstopGameManager _gameManager;

        /// <summary>
        /// Returns a SceneObject with the given FieldTripManager.
        /// </summary>
        public SceneObject Build()
        {
            SceneObject scene = Object.Instantiate<SceneObject>(FieldTripsAPI.assetMan.Get<SceneObject>("CampingScene"), MTM101BaldiDevAPI.prefabTransform);

            scene.manager = _gameManager;
            scene.nextLevel = scene;

            return scene;
        }

        /// <summary>
        /// Sets the name for the SceneObject.
        /// </summary>
        public FieldTripSceneObjectBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets what GameManager it needs to use.
        /// </summary>
        public FieldTripSceneObjectBuilder SetManager(PitstopGameManager gameManager)
        {
            _gameManager = gameManager;
            return this;
        }
    }
}

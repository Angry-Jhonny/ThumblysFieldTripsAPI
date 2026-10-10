using ThumblysFieldTripsAPI.Patches;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Extentions
{
    public class ExtendedFieldTripBaseRoomFunction : FieldTripBaseRoomFunction
    {
        public FieldTripSpriteSwap introBaldiSwap;

        public override void Initialize(RoomController room)
        {
            base.Initialize(room);
            introBaldiSwap.Initialize(
                introBaldiSwap.transform.parent.GetComponent<SpriteRenderer>(),
                Singleton<CoreGameManager>.Instance.currentFieldTrip as ExtendedFieldTripObject);
        }
    }
}

using HarmonyLib;
using UnityEngine;
using ThumblysFieldTripsAPI.Extentions;
using MTM101BaldAPI.AssetTools;

using Object = UnityEngine.Object;

namespace ThumblysFieldTripsAPI.Patches
{
    [HarmonyPatch(typeof(FieldTripEntranceRoomFunction))]
    [HarmonyPatch("Initialize")]
    internal class FieldTripPatches
    {
        static void Postfix(MainMenu __instance, ref SoundObject ___audCampIntro, ref Animator ___animator)
        {
            FieldTripObject trip = Singleton<CoreGameManager>.Instance.currentFieldTrip;
            ___audCampIntro = trip.intro;

            if (trip.GetType().FullName == "ThumblysFieldTripsAPI.Extentions.ExtendedFieldTripObject")
            {
                ExtendedFieldTripObject moddedTrip = (ExtendedFieldTripObject)trip;

                Debug.Log("Custom field trip sprites found!");
                FieldTripSpriteSwap.AddTripSpriteSwap(___animator.gameObject).Initialize(___animator.GetComponent<SpriteRenderer>(), moddedTrip);
            }
        }
    }

    public class FieldTripSpriteSwap : MonoBehaviour
    {
        private SpriteRenderer originalRenderer;
        private SpriteRenderer spriteRenderer;

        private Sprite originalTalk = AssetFinder.FindOfTypeWithName<Sprite>("CampingTalkSheet_1", true);

        private Sprite idle;
        private Sprite talk;

        private bool initialized = false;

        public void Initialize(SpriteRenderer original, ExtendedFieldTripObject fieldTrip)
        {
            originalRenderer = original;
            spriteRenderer = transform.GetComponent<SpriteRenderer>();
            idle = fieldTrip.idleSprite;
            talk = fieldTrip.talkSprite;
            initialized = true;
        }

        public static FieldTripSpriteSwap AddTripSpriteSwap(GameObject baldi)
        {
            GameObject spriteOverride = Object.Instantiate(baldi);
            baldi.GetComponent<SpriteRenderer>().enabled = false;
            spriteOverride.transform.SetParent(baldi.transform, false);
            spriteOverride.transform.localPosition = Vector3.zero;
            Object.Destroy(spriteOverride.GetComponent<Animator>());
            Object.Destroy(spriteOverride.GetComponent<PropagatedAudioManager>());
            Object.Destroy(spriteOverride.GetComponent<AudioSource>());
            Object.Destroy(spriteOverride.GetComponent<VolumeAnimator>());
            return spriteOverride.AddComponent<FieldTripSpriteSwap>();
        }

        private void Update()
        {
            if (!initialized) return;

            if (originalRenderer.sprite == originalTalk)
            {
                spriteRenderer.sprite = talk;
            }
            else
            {
                spriteRenderer.sprite = idle;
            }
        }
    }
}
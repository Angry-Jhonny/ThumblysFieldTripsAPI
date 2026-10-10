using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Components.Animation;
using System.Collections.Generic;
using System.Reflection;
using ThumblysFieldTripsAPI.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace ThumblysFieldTripsAPI.Builders
{
    public class MinigameTutorialBuilder
    {
        private List<TutorialScreenInfo> _screens = new List<TutorialScreenInfo>();
        private Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
        private List<string> _animNames = new List<string>();
        private string _midi = "CampMinigame_1_1";

        private string _name = "MinigameTutorial_Unknown";

        private static readonly FieldInfo _screen = AccessTools.Field(typeof(MinigameTutorial), "screen");
        private static readonly FieldInfo _tooltipKey = AccessTools.Field(typeof(MinigameTutorial), "tooltipKey");
        private static readonly FieldInfo _audVoice = AccessTools.Field(typeof(MinigameTutorial), "audVoice");
        private static readonly FieldInfo _positionAndScale = AccessTools.Field(typeof(MinigameTutorial), "positionAndScale");
        private static readonly FieldInfo _animation = AccessTools.Field(typeof(MinigameTutorial), "animation");
        private static readonly FieldInfo _tutorBaldiAnimator = AccessTools.Field(typeof(MinigameTutorial), "tutorBaldiAnimator");
        private static readonly FieldInfo _tutorBaldi = AccessTools.Field(typeof(MinigameTutorial), "tutorBaldi");

        /// <summary>
        /// Builds the MinigameTutorial.
        /// </summary>
        public MinigameTutorial Build()
        {
            GameObject baseObject = Object.Instantiate(FieldTripsAPI.assetMan.Get<MinigameTutorial>("TutorialBase").gameObject, MTM101BaldiDevAPI.prefabTransform);
            baseObject.name = _name;

            Transform gameTransform = baseObject.transform.Find("GameCanvas").Find("Game");

            foreach (Image img in gameTransform.GetComponentsInChildren<Image>())
            {
                if (img.name != "BG") { Object.DestroyImmediate(img.gameObject); }
            }

            List<GameObject> screens = new List<GameObject>();
            List<string> tips = new List<string>();
            List<SoundObject> clips = new List<SoundObject>();
            List<Vector3> posscale = new List<Vector3>();

            int i = 0;
            foreach (TutorialScreenInfo info in _screens)
            {
                GameObject screen = Object.Instantiate(FieldTripsAPI.assetMan.Get<GameObject>("TutorialScreen"), gameTransform);
                screen.GetComponent<RectTransform>().anchoredPosition = new Vector2(240, 180);
                screen.name = "Page_" + i;
                screen.transform.SetParent(gameTransform, true);
                screen.GetComponent<Image>().sprite = info.ScreenSprite;
                screens.Add(screen);
                tips.Add(info.TooltipKey);
                clips.Add(info.Audio);
                posscale.Add(info.PositionScale);
                i++;
            }

            MinigameTutorial minigameTutorial = baseObject.GetComponent<MinigameTutorial>();

            FakeAnimatorSimple fakeAnimator = ((Animator)_tutorBaldiAnimator.GetValue(minigameTutorial)).gameObject.AddFakeAnimatorComponent<FakeAnimatorSimple>();
            TutorialCustomImageAnimator animator = fakeAnimator.gameObject.AddComponent<TutorialCustomImageAnimator>();
            animator.animationEventTriggerCall = fakeAnimator.GetComponent<AnimationEventTriggerCallFunctionInParentObjectWhyIsThisNecessaryHahaha>();
            animator.image = ((SpriteController)_tutorBaldi.GetValue(minigameTutorial)).GetComponent<Image>();
            animator.LoadAnimations(_animations);
            fakeAnimator.simpleAnimator = animator;

            _animation.SetValue(minigameTutorial, _animNames.ToArray());
            _screen.SetValue(minigameTutorial, screens.ToArray());
            _tooltipKey.SetValue(minigameTutorial, tips.ToArray());
            _audVoice.SetValue(minigameTutorial, clips.ToArray());
            _positionAndScale.SetValue(minigameTutorial, posscale.ToArray());
            minigameTutorial.music = _midi;

            return minigameTutorial;
        }

        /// <summary>
        /// Sets the name of the Gameobject.
        /// </summary>
        public MinigameTutorialBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Adds a screen for the tutorial.
        /// </summary>
        public MinigameTutorialBuilder AddScreen(TutorialScreenInfo tutorialScreen)
        {
            _screens.Add(tutorialScreen);
            return this;
        }

        /// <summary>
        /// Adds a possible animation for the Tutor Baldi.
        /// </summary>
        public MinigameTutorialBuilder AddTutorAnimation(string animationName, SpriteAnimation animation)
        {
            _animations.Add(animationName, animation);
            _animNames.Add(animationName);
            return this;
        }

        /// <summary>
        /// Sets the possible animation for the Tutor Baldi.
        /// </summary>
        public MinigameTutorialBuilder AddTutorAnimations(Dictionary<string, SpriteAnimation> animations)
        {
            _animations = animations;
            _animNames.AddRange(animations.Keys);
            return this;
        }

        /// <summary>
        /// Sets the midi for the Tutorial (NOTE: the midi will be slowed down ingame).
        /// </summary>
        public MinigameTutorialBuilder SetMidi(string midiName)
        {
            _midi = midiName;
            return this;
        }
    }

    public struct TutorialScreenInfo
    {
        private static SoundObject Silence = AssetFinder.FindOfTypeWithName<SoundObject>("Silence", true);

        public string TooltipKey;
        public Sprite ScreenSprite;
        public Vector3 PositionScale;
        public SoundObject Audio;

        public TutorialScreenInfo(Sprite ScreenSprite, Vector3 PositionScale, string TooltipKey = "")
        {
            this.TooltipKey = TooltipKey;
            this.ScreenSprite = ScreenSprite;
            this.PositionScale = PositionScale;
            this.Audio = Silence;
        }

        public TutorialScreenInfo(Sprite ScreenSprite, Vector3 PositionScale, SoundObject Audio, string TooltipKey = "")
        {
            this.TooltipKey = TooltipKey;
            this.ScreenSprite = ScreenSprite;
            this.PositionScale = PositionScale;
            this.Audio = Audio;
        }
    }
}

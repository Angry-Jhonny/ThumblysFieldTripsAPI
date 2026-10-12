using MTM101BaldAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThumblysFieldTripsAPI;
using UnityEngine;
using UnityEngine.UI;

namespace FarmComeback.Haystack
{
    public class Minigame_Haystack : Minigame
    {
        public MinigameHUD minigameHud;

        public SoundObject audToss;
        public SoundObject audTossLands;

        public SoundObject audError;

        public Sprite[] HaySprites = new Sprite[0];

        public List<Sprite> BackgroundSprites = new List<Sprite>();
        public int BGLoopStart = 0;
        public Image BG;
        private int CurrentBG;

        public Transform gameRect;
        public RectTransform playerRect;
        public SpriteController hayPrefab;

        protected AnalogInputData MoveAnalogData = new AnalogInputData()
        {
            steamAnalogId = "CursorAnalog",
            steamDeltaId = "CursorDelta",
            xAnalogId = "CursorX",
            xDeltaId = "CursorXDelta",
            yAnalogId = "CursorY",
            yDeltaId = "CursorYDelta"
        };

        private Vector2 analogThisFrame;
        private Vector2 deltaThisFrame;
        private Vector2 Range = new Vector2(40, 440);
        protected Vector3 localPosition = new Vector3(240, 30, 0);

        private float position;

        protected float playerWobble;

        protected int remaining = 40;
        protected int remainingBeforeScroll = 4;

        protected int HayFallSpeed { get; private set; }

        protected List<SpriteController> fallingHays = new List<SpriteController>();

        protected List<SpriteController> stackedHays = new List<SpriteController>();

        public RectTransform currentSupport;

        protected float MinigameSpeed = 1;
        protected float MaxMinigameSpeed = 2.95f;

        public override void Initialize(MinigameBase minigameBase, bool scoringMode)
        {
            base.Initialize(minigameBase, scoringMode);

            VirtualReset();
        }

        public override void VirtualStart()
        {
            base.VirtualStart();
            SpawnHay(getRandomHayPosition());
        }

        public override void VirtualReset()
        {
            base.VirtualReset();
            minigameHud.Reinitialize();
            currentSupport = playerRect;
            BG.sprite = BackgroundSprites[0];
            remaining = 40;
            remainingBeforeScroll = 4;
            HayFallSpeed = 120;
            playerWobble = 0;
            SetSpeed(1);

            localPosition = new Vector3(240, 30, 0);
            position = 240;
            UpdatePlayerPosition();

            if (scoringMode)
            {
                minigameHud.SetScore(score);
            }
            else
            {
                minigameHud.SetRemaining(remaining);
            }

            foreach (SpriteController hay in  fallingHays)
            {
                Destroy(hay.gameObject);
            }

            foreach (SpriteController hay in stackedHays)
            {
                Destroy(hay.gameObject);
            }
            fallingHays.Clear();
            stackedHays.Clear();
        }

        public override void VirtualUpdate()
        {
            base.VirtualUpdate();
            playerWobble += minigameBase.DeltaTime;
            Singleton<InputManager>.Instance.GetAnalogInput(MoveAnalogData, out analogThisFrame, out deltaThisFrame);
            float offset = playerRect.position.x - currentSupport.position.x;
            position = Mathf.Clamp(position + deltaThisFrame.x + analogThisFrame.x * Time.unscaledDeltaTime * Singleton<PlayerFileManager>.Instance.controllerCursorSensitivity * 1.25f * MinigameSpeed, Range.x + offset, Range.y + offset);
            UpdatePlayerPosition();

            if (fallingHays.Count > 0)
            {
                List<SpriteController> hays = new List<SpriteController>();
                hays.AddRange(fallingHays);
                foreach (SpriteController hay in hays)
                {
                    hay.SetPosition(hay.Position - new Vector2(0, HayFallSpeed * minigameBase.DeltaTime * MinigameSpeed));

                    int YSupportRange = Mathf.RoundToInt(currentSupport.position.y + currentSupport.sizeDelta.y) - 24;
                    int YValue = Mathf.RoundToInt(hay.RectTransform.position.y);

                    float distance = Mathf.Abs(currentSupport.position.x - hay.RectTransform.position.x);

                    IEnumerable<int> Yrange = Enumerable.Range(YSupportRange, Mathf.CeilToInt(5 * MinigameSpeed));

                    if (Yrange.Contains(YValue) && distance < 42f)
                    {
                        HayCollected(hay);
                        SetSpeed(MinigameSpeed >= MaxMinigameSpeed ? 1f : MinigameSpeed + 0.05f);
                    }
                    else if (hay.RectTransform.anchoredPosition.y <= -40)
                    {
                        HayFell(hay);
                        SetSpeed(MinigameSpeed >= MaxMinigameSpeed ? 1f : MinigameSpeed + 0.05f);
                    }
                }
            }
        }

        private IEnumerator ScrollUpAnim()
        {
            int delay = 0;
            while (currentSupport.position.y > -80)
            {
                localPosition.y -= 2;
                delay++;
                if (delay >= 20)
                {
                    delay = 0;
                    CurrentBG++;
                    if (CurrentBG >= BackgroundSprites.Count) { CurrentBG = BGLoopStart; }
                    BG.sprite = BackgroundSprites[CurrentBG];
                }
                yield return new WaitForSecondsMinigameTimescale(minigameBase, 0.005f / MinigameSpeed);
            }
        }

        private IEnumerator QueueHayLaunch()
        {
            yield return new WaitForSecondsMinigameTimescale(minigameBase, 0.5f / MinigameSpeed);
            SpawnHay(getRandomHayPosition());
        }

        protected void SetSpeed(float speed)
        {
            MinigameSpeed = speed;
            Singleton<MusicManager>.Instance.SetSpeed(0.75f + speed / 4);
        }

        protected void UpdatePlayerPosition()
        {
            localPosition.x = Mathf.RoundToInt(position);
            playerRect.localPosition = localPosition + new Vector3(Mathf.Sin(playerWobble * 5) * Mathf.Clamp(score / 2, 0, 32), 0, 0);
        }

        protected float getRandomHayPosition() => Random.Range(Range.x, Range.y);

        protected virtual void SpawnHay(float position)
        {
            SpriteController newHay = Instantiate<SpriteController>(hayPrefab, gameRect);
            float offset = newHay.RectTransform.sizeDelta.x / 2;
            newHay.SetPosition(new Vector2(Mathf.Clamp(position, Range.x + offset, Range.y - offset), 380));
            fallingHays.Add(newHay);
            minigameBase.AudioManager.PlaySingle(audToss);
            newHay.SetSprite(HaySprites[Random.Range(0, HaySprites.Length)]);
        }

        protected virtual void HayFell(SpriteController HaySprite)
        {
            minigameHud.LoseLife();
            fallingHays.Remove(HaySprite);

            if (minigameHud.livesLost == 3)
            {
                if (scoringMode)
                {
                    minigameBase.SetScore(base.score);
                }
                else
                {
                    minigameBase.Lose();
                }
            }
            else
            {
                StartCoroutine(QueueHayLaunch());
            }
            Destroy(HaySprite.gameObject);
        }

        protected virtual void HayCollected(SpriteController HaySprite)
        {
            currentSupport = HaySprite.RectTransform;
            fallingHays.Remove(HaySprite);
            stackedHays.Add(HaySprite);
            HaySprite.transform.SetParent(playerRect, true);
            minigameBase.AudioManager.PlaySingle(audTossLands);
            AddScore(1);
            remainingBeforeScroll--;
            if (remainingBeforeScroll <= 0)
            {
                remainingBeforeScroll = 3;
                StartCoroutine(ScrollUpAnim());
            }
            if (stackedHays.Count >= 9)
            {
                SpriteController hay = stackedHays[0];
                stackedHays.Remove(hay);
                Destroy(hay.gameObject);
            }
            StartCoroutine(QueueHayLaunch());
        }

        protected override void AddScore(int amount)
        {
            base.AddScore(amount);
            if (scoringMode)
            {
                minigameHud.SetScore(score);
            }
            else
            {
                remaining --;
                minigameHud.SetRemaining(remaining);

                if (remaining <= 0)
                {
                    minigameBase.Win();
                }
            }
        }
    }
}

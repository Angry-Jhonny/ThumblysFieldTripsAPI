using CampfireFrenzy;
using System.Collections;
using System.Collections.Generic;
using ThumblysFieldTripsAPI.Extentions;
using UnityEngine;
using UnityEngine.UI;

namespace BunchOfFieldTrips.Herding
{
    public class Minigame_Herding : Minigame
    {
        public MinigameHUD minigameHud;

        public static List<AnimalInfo> possibleAnimals = new List<AnimalInfo>();

        public int maxRoundsNormal = 8;
        public int maxRoundsScoring = 12;

        protected List<BaseAnimalButton> animals = new List<BaseAnimalButton>();
        public BaseAnimalButton farmAnimalPrefab;

        public Transform gameRect;
        public Transform animalRect;
        public List<Image> sectorSignImages = new List<Image>();

        public Vector2 minMoveSpace = new Vector2(47, 12);
        public Vector2 maxMoveSpace = new Vector2(433, 51);

        public Vector2 startFenceOffset = new Vector2(47, 70);
        public Vector2 FenceSize = new Vector2(193, 133);
        public AudioManager animalAmbiance;
        public SoundObject aud_animals;

        public SoundObject aud_correct;
        public SoundObject aud_incorrect;

        protected List<AnimalType> sectorTypes = new List<AnimalType>();
        public List<AnimalType> SectorTypes => sectorTypes;

        protected float maxTimer;
        protected float timer;
        protected float scoreTimer;

        protected bool sorting;

        public override void Initialize(MinigameBase minigameBase, bool scoringMode)
        {
            base.Initialize(minigameBase, scoringMode);
            SetScore(0);
            minigameHud.SetCountdown(0, 0);
            if (!scoringMode)
            {
                minigameHud.HideScore();
                minigameBase.SetFinalRound(maxRoundsNormal);
            }
            else
            {
                minigameBase.SetFinalRound(maxRoundsScoring);
            }
            sorting = false;

            VirtualReset();
        }

        public override void VirtualUpdate()
        {
            base.VirtualUpdate();
            if (!sorting) return;
            if (timer > 0)
            {
                timer -= minigameBase.DeltaTime;
                minigameHud.SetCountdown(timer, maxTimer);
            }
            else
            {
                TimerEnd();
            }
        }

        public override void VirtualStart()
        {
            base.VirtualStart();
            SetScore(0);
            minigameBase.AdvanceRound(1f);
            perfect = true;
        }

        public override void VirtualReset()
        {
            animalAmbiance.FlushQueue(true);
            animals.ForEach(x => Destroy(x.gameObject));
            animals.Clear();
        }

        public override void StartRound()
        {
            base.StartRound();
            maxTimer = 3 + 7 * minigameBase.CurrentRound;
            timer = maxTimer;
            sorting = true;
            VirtualReset();
            List<AnimalInfo> animalList = new List<AnimalInfo>(possibleAnimals);
            animalList.Shuffle();
            sectorTypes.Clear();
            animalAmbiance.QueueAudio(aud_animals);
            animalAmbiance.SetLoop(true);
            for (int i = 0; i < 4; i++)
            {
                sectorTypes.Add(animalList[i].type);
                AnimalInfo info = animalList[i];
                sectorSignImages[i].sprite = info.animal;
                for (int j = 0; j < Mathf.RoundToInt(Random.Range(2, 4) * minigameBase.CurrentRound / 2); j++)
                {
                    SpawnAnimal(info, false);
                }
                for (int j = 0; j < Mathf.RoundToInt(Random.Range(0, 2) * minigameBase.CurrentRound / 4); j++)
                {
                    SpawnAnimal(info, true);
                }
            }
        }

        public virtual bool IsSorted()
        {
            bool flag = true;
            foreach (BaseAnimalButton animal in animals)
            {
                if (animal.animalSector == -1 && !animal.IsDecoy)
                {
                    flag = false;
                    break;
                }
                else
                {
                    if (animal.IsDecoy)
                    {
                        if (animal.animalSector == -1) continue;
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                    else
                    {
                        if (sectorTypes[animal.animalSector] == animal.Animal.type) continue;
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        public void SetTimeScore()
        {
            scoreTimer = timer;
        }

        public virtual void TimerEnd()
        {
            sorting = false;
            bool sorted = IsSorted();
            minigameBase.AudioManager.PlaySingle(sorted ? aud_correct : aud_incorrect);
            if (sorted)
            {
                AddScore(Mathf.RoundToInt(700f * Mathf.Clamp01(scoreTimer / maxTimer)));
                if (!scoringMode)
                {
                    if (perfect) minigameBase.AwardPerfectBonus();

                    base.minigameBase.Win();
                }
                StartCoroutine(waitEnumerator());
            }
            else
            {
                perfect = false;
                minigameHud.LoseLife();
                if (minigameHud.livesLost >= 3)
                {
                    if (scoringMode)
                    {
                        base.minigameBase.SetScore(score);
                    }
                    else
                    {
                        base.minigameBase.Lose();
                    }

                    Singleton<MusicManager>.Instance.StopFile();
                    Singleton<MusicManager>.Instance.StopMidi();
                    return;
                }
                StartCoroutine(waitEnumerator());
            }
        }

        private IEnumerator waitEnumerator()
        {
            CursorController.Instance.DisableClick(true);
            yield return new WaitForSeconds(3);
            CursorController.Instance.DisableClick(false);
            VirtualReset();
            minigameBase.AdvanceRound(1f);
        }

        public virtual void SpawnAnimal(AnimalInfo info, bool decoy)
        {
            Vector2 pos = new Vector2(Random.Range(minMoveSpace.x, maxMoveSpace.x), Random.Range(minMoveSpace.y, maxMoveSpace.y));
            BaseAnimalButton newAnimal = Instantiate<BaseAnimalButton>(farmAnimalPrefab, animalRect);
            newAnimal.Initialize(this, info, decoy);
            newAnimal.image.rectTransform.anchoredPosition = pos;
            animals.Add(newAnimal);
        }

        protected override void AddScore(int amount)
        {
            base.AddScore(amount);
            minigameHud.AddScore(score);
        }

        protected void SetScore(int amount)
        {
            score = amount;
            minigameHud.SetScore(score);
        }
    }

    public class AnimalInfo
    {
        public AnimalType type;
        public Sprite animal;
        public Sprite animalFake;
        public SoundObject sound;
    }
}
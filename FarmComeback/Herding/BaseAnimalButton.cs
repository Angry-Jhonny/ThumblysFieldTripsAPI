using MTM101BaldAPI;
using UnityEngine;
using UnityEngine.UI;

namespace BunchOfFieldTrips.Herding
{
    [DefaultExecutionOrder(-91)]
    public class BaseAnimalButton : MenuButton
    {
        private Minigame_Herding minigame;

        public Image image;
        private RectTransform rect;

        private AnimalInfo animal;

        protected float unhoverSize = 20;
        protected float hoverSize = 22;
        protected float holdSize = 26;

        public bool canMove;
        private float timeNextDir;
        private Vector2 currentDir;

        private Vector2 currentMin;
        private Vector2 currentMax;

        public int animalSector { get; protected set; }

        public AnimalInfo Animal => animal;
        public bool IsDecoy { get; private set; }

        public void Initialize(Minigame_Herding minigame, AnimalInfo animal, bool isDecoy)
        {
            rect = image.rectTransform;
            rect.sizeDelta = Vector3.one * unhoverSize;
            this.minigame = minigame;
            gameObject.name = "Animal_" + EnumExtensions.GetExtendedName<AnimalType>((int)animal.type);
            image.sprite = isDecoy ? animal.animalFake : animal.animal;
            audConfirmOverride = animal.sound;
            this.animal = animal;
            timeNextDir = 0;
            IsDecoy = isDecoy;
            held = false;
            canMove = true;
            animalSector = -1;
            currentMin = minigame.minMoveSpace;
            currentMax = minigame.maxMoveSpace;
        }

        private void Update()
        {
            if (canMove)
            {
                rect.anchoredPosition += currentDir * minigame.minigameBase.DeltaTime * 20;
                rect.anchoredPosition = new Vector2(Mathf.Clamp(rect.anchoredPosition.x, currentMin.x, currentMax.x), Mathf.Clamp(rect.anchoredPosition.y, currentMin.y, currentMax.y));
            }
            if (timeNextDir > 0)
            {
                timeNextDir -= minigame.minigameBase.DeltaTime;
            }
            else
            {
                timeNextDir = Random.Range(1.5f, 2.5f);
                currentDir = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                if (currentDir.x > 0) transform.localScale = Vector3.one;
                else  transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        private void LateUpdate()
        {
            if (held)
            {
                rect.position = CursorController.Instance.cursorTransform.position;
            }

            if (!highlighted && wasHighlighted)
            {
                wasHighlighted = false;
                UnHighlight();
            }

            highlighted = false;
        }

        protected virtual void UnHighlight()
        {
            rect.sizeDelta = Vector3.one * unhoverSize;
        }

        public override void Highlight()
        {
            base.Highlight();
            rect.sizeDelta = held ? Vector3.one * holdSize : Vector3.one * hoverSize;
            wasHighlighted = true;
            highlighted = true;
        }

        public override void Press()
        {
            held = true;
            canMove = false;
            transform.SetParent(minigame.gameRect, true);
            rect.sizeDelta = Vector3.one * holdSize;
        }

        public override void UnHold()
        {
            base.UnHold();
            rect.sizeDelta = highlighted ? Vector3.one * hoverSize : Vector3.one * unhoverSize;
            held = false;
            if (rect.anchoredPosition.y < 70)
            {
                animalSector = -1;
                currentMin = minigame.minMoveSpace;
                currentMax = minigame.maxMoveSpace;
            }
            else
            {
                int sectorX = 1;
                int sectorY = 1;
                if (rect.anchoredPosition.y > 203)
                {
                    sectorY = 2;
                }
                if (rect.anchoredPosition.x > 240)
                {
                    sectorX = 2;
                }
                if (sectorX == 1 && sectorY == 2)
                {
                    animalSector = 0;
                }
                else if (sectorX == 2 && sectorY == 2)
                {
                    animalSector = 1;
                }
                else if (sectorX == 1 && sectorY == 1)
                {
                    animalSector = 2;
                }
                else if (sectorX == 2 && sectorY == 1)
                {
                    animalSector = 3;
                }
                currentMin = new Vector2(minigame.startFenceOffset.x + 12 + (minigame.FenceSize.x * (sectorX - 1)), minigame.startFenceOffset.y + 12 + (minigame.FenceSize.y * (sectorY - 1)));
                currentMax = new Vector2(minigame.startFenceOffset.x - 12 + (minigame.FenceSize.x * sectorX), minigame.startFenceOffset.y - 12 + (minigame.FenceSize.y * sectorY));
            }
            transform.SetParent(minigame.animalRect, true);
            canMove = true;
            if (!IsDecoy)
            {
                minigame.SetTimeScore();
            }
        }

    }
}

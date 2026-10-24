using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThumblysFieldTripsAPI.Helpers
{
    public class PickFieldTripManager : MonoBehaviour
    {
        private TextLocalizer descText;
        private TMP_Text pagesCount;
        private int currentPage = 0;
        private List<PickPage> pages = new List<PickPage>();
        public TextLocalizer Desc_Text => descText;
        public TMP_Text PagesCountText => pagesCount;


        internal StandardMenuButton CreateArrowButton(Sprite unhover, Sprite hover, Transform parent, int indexChange)
        {
            Image image = UIHelpers.CreateImage(unhover, parent, Vector3.zero, false);
            image.rectTransform.offsetMin = new Vector2(0.5f, 0);
            image.rectTransform.offsetMax = new Vector2(0.5f, 0);
            image.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.sizeDelta = new Vector2(unhover.rect.width, unhover.rect.height);
            StandardMenuButton button = image.gameObject.AddComponent<StandardMenuButton>();
            button.image = image;
            button.name = "NavButton";
            button.tag = "Button";
            button.swapOnHigh = true;
            button.unhighlightedSprite = unhover;
            button.highlightedSprite = hover;
            button.gameObject.layer = LayerMask.NameToLayer("UI");
            button.OnPress = new UnityEvent();
            button.OnPress.AddListener(() =>
            {
                ChangePage(indexChange);
            });
            button.OnRelease = new UnityEvent();

            return button;
        }

        internal StandardMenuButton CreateTripButton(TripMenuSettings settings, Transform parent)
        {
            TMP_Text buttonText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, "", parent, new Vector3(0, 0, 0), false);
            buttonText.color = Color.black;
            buttonText.alignment = TextAlignmentOptions.Top;
            buttonText.gameObject.AddComponent<TextLocalizer>().key = settings.nameKey;
            buttonText.raycastTarget = true;
            buttonText.rectTransform.sizeDelta = Vector2.one * 100;
            StandardMenuButton button = buttonText.gameObject.AddComponent<StandardMenuButton>();
            button.text = buttonText;
            button.eventOnHigh = true;
            button.underlineOnHigh = true;
            button.name = settings.nameKey;
            button.tag = "Button";
            button.gameObject.layer = LayerMask.NameToLayer("UI");
            button.OnPress = new UnityEvent();
            button.OnPress.AddListener(() =>
            {
                LoadFieldTrip(settings);
            });
            button.OnHighlight = new UnityEvent();
            button.OnHighlight.AddListener(() =>
            {
                Desc_Text.GetLocalizedText(settings.descKey);
            });
            button.OffHighlight = new UnityEvent();
            button.OnRelease = new UnityEvent();

            return button;
        }

        public void BuildScreen()
        {
            transform.Find("Camping").gameObject.SetActive(false);
            transform.Find("Farm").gameObject.SetActive(false);
            descText = transform.Find("ModeText").GetComponent<TextLocalizer>();
            descText.GetComponent<TMP_Text>().rectTransform.anchoredPosition = new Vector2(0, -64);
            pagesCount = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans36, "1/1", transform, new Vector3(0, 155, 0), false);
            pagesCount.rectTransform.sizeDelta = new Vector2(100, 50);
            pagesCount.color = Color.black;
            pagesCount.alignment = TextAlignmentOptions.Top;
            pagesCount.name = "PagesCount";

            int pagesC = Mathf.CeilToInt((float)FieldTripsAPI.TripScenes.Count / 6f);
            int addedButtons = 0;
            Vector2 pos;

            print(pagesC);
            for (int page = 0; page < pagesC; page++)
            {
                pos = new Vector2(-90, 80);
                PickPage currentPage = new PickPage();
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        StandardMenuButton button = CreateTripButton(FieldTripsAPI.TripScenes[addedButtons], transform);
                        button.GetComponent<RectTransform>().anchoredPosition = pos;
                        currentPage.buttons.Add(button.gameObject);
                        pos.x += 180;
                        addedButtons++;
                        if (addedButtons == FieldTripsAPI.TripScenes.Count) break;
                    }
                    pos.y -= 60;
                    pos.x = -90;
                    if (addedButtons == FieldTripsAPI.TripScenes.Count) break;
                }
                pages.Add(currentPage);
                if (addedButtons == FieldTripsAPI.TripScenes.Count) break;
            }
            SetPage(0);

            if (pagesC > 1)
            {
                Sprite arrowLeft = AssetFinder.FindOfTypeWithName<Sprite>("MenuArrowSheet_2", true);
                Sprite arrowLeftHover = AssetFinder.FindOfTypeWithName<Sprite>("MenuArrowSheet_0", true);
                Sprite arrowRight = AssetFinder.FindOfTypeWithName<Sprite>("MenuArrowSheet_3", true);
                Sprite arrowRightHover = AssetFinder.FindOfTypeWithName<Sprite>("MenuArrowSheet_1", true);

                StandardMenuButton navButton = CreateArrowButton(arrowLeft, arrowLeftHover, transform, -1);
                navButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180, 64);
                navButton = CreateArrowButton(arrowRight, arrowRightHover, transform, 1);
                navButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(180, 64);
            }

            transform.Find("Bottom").SetAsLastSibling();
        }


        public void SetPage(int page)
        {
            currentPage = Mathf.Clamp(page, 0, pages.Count - 1);
            Change_Internal();
        }

        public void ChangePage(int index)
        {
            currentPage = Mathf.Clamp(currentPage + index, 0, pages.Count - 1);
            Change_Internal();
        }

        private void Change_Internal()
        {
            pages.ForEach(page => page.SetActive(false));
            pages[currentPage].SetActive(true);

            UpdateCountText();
        }

        public void UpdateCountText()
        {
            pagesCount.text = string.Format("{0}/{1}", (currentPage + 1).ToString(), pages.Count);
        }

        public void LoadFieldTrip(TripMenuSettings settings)
        {
            try
            {
                GameLoader loader = Resources.FindObjectsOfTypeAll<GameLoader>().First(x => x.GetInstanceID() > 0 && x.name == "GameLoader");
                gameObject.SetActive(false);
                loader.CheckSeed();
                loader.Initialize(2);
                loader.LoadLevel(settings.trip);
                loader.SetMode(1);
                loader.SetSave(false);
            }
            catch (Exception e)
            {
                descText.GetLocalizedText("<color=red>ERROR: " + e.Message + " Read BepInEx Console for more details.</color>");
                Debug.LogError(e);
            }
        }

        private struct PickPage
        {
            public List<GameObject> buttons;

            public PickPage()
            {
                buttons = new List<GameObject>();
            }

            public void SetActive(bool active)
            {
                buttons.ForEach(button => button.SetActive(active));
            }
        }
    }
}

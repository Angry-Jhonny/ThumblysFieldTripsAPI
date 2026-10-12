using MTM101BaldAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThumblysFieldTripsAPI;
using UnityEngine;
using UnityEngine.UI;

namespace FarmComeback.Haystack
{
    public class Minigame_Herding : Minigame
    {
        public MinigameHUD minigameHud;

        public static List<AnimalInfo> possibleAnimals = new List<AnimalInfo>();
    }

    public class AnimalInfo
    {
        public string name = "Animal";
        public Sprite animal;
        public Sprite animalBig;
        public Sprite animalFake;
        public SoundObject sound;
    }
}
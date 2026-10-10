using System;
using System.Collections.Generic;
using System.IO;
using UnityCipher;
using UnityEngine;
using Newtonsoft.Json;
using MTM101BaldAPI.Registers;

namespace ThumblysFieldTripsAPI.Registers
{
    [Serializable]
    public class ModdedFieldTripData
    {
        public List<FieldTripScore> tripScores;
    }

    [Serializable]
    public class FieldTripScore
    {
        public string minigame;
        public int score;
        public string name;

        public FieldTripScore(string minigame, int score, string name)
        {
            this.minigame = minigame;
            this.score = score;
            this.name = name;
        }
    }

    public class ModdedFieldTripScoreManager : Singleton<ModdedFieldTripScoreManager>
    {
        public List<FieldTripScore> tripScores = new List<FieldTripScore>() { };

        public List<FieldTripScore> GetSortedScoresForMinigame(string minigameName)
        {
            List<FieldTripScore> list = new List<FieldTripScore>();
            for (int i = 0; i < tripScores.Count; i++)
            {
                if (string.Equals(tripScores[i].minigame, minigameName))
                {
                    list.Add(tripScores[i]);
                }
            }

            SortMinigameScores(list);
            return list;
        }

        private void SortMinigameScores(List<FieldTripScore> scores)
        {
            List<FieldTripScore> list = new List<FieldTripScore>();
            int count = scores.Count;
            while (list.Count < count)
            {
                int index = 0;
                int num = 0;
                for (int i = 0; i < scores.Count; i++)
                {
                    if (scores[i].score > num)
                    {
                        num = scores[i].score;
                        index = i;
                    }
                }

                list.Add(scores[index]);
                scores.RemoveAt(index);
            }

            scores.Clear();
            scores.AddRange(list);
        }

        public void SetDefaultsTrips()
        {
            tripScores.Clear();
        }

        public void AddScore(string minigame, int score, string name, out int rank)
        {
            bool flag = false;
            rank = -1;
            FieldTripScore tripScore = new FieldTripScore(minigame, score, name);
            tripScores.Add(tripScore);
            List<FieldTripScore> list = new List<FieldTripScore>(GetSortedScoresForMinigame(minigame));
            for (int i = 0; i < list.Count && i < 5; i++)
            {
                if (list[i] == tripScore)
                {
                    rank = i;
                    flag = true;
                    break;
                }
            }

            for (int i = 5; i < list.Count; i++)
            {
                tripScores.Remove(list[i]);
            }

            if (flag)
            {
                Save();
            }
            else
            {
                tripScores.Remove(tripScore);
            }
        }

        public void Load()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Modded"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Modded");
            }
            string path = Application.persistentDataPath + "/Modded/FieldTripHighScores.hgh";
            SetDefaultsTrips();
            if (File.Exists(path))
            {
                bool flag = true;
                ModdedFieldTripData moddedFieldTripData = new ModdedFieldTripData();
                try
                {
                    moddedFieldTripData = JsonConvert.DeserializeObject<ModdedFieldTripData>(RijndaelEncryption.Decrypt(File.ReadAllText(path), "99"));
                }
                catch
                {
                    flag = false;
                }

                if (flag)
                {
                    tripScores = moddedFieldTripData.tripScores;
                    if (tripScores == null)
                    {
                        tripScores = new List<FieldTripScore>();
                    }
                }
                else
                {
                    Debug.LogWarning("High scores file is corrupted. Setting defaults.");
                    Save();
                }
            }
            else
            {
                Debug.Log("High scores do not exist. Setting defaults.");
                Save();
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Modded"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Modded");
            }
            string path = Application.persistentDataPath + "/Modded/FieldTripHighScores.hgh";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            ModdedFieldTripData moddedFieldTripData = new ModdedFieldTripData();
            moddedFieldTripData.tripScores = tripScores;
            File.WriteAllText(path, RijndaelEncryption.Encrypt(JsonConvert.SerializeObject(moddedFieldTripData), "99"));
            Debug.Log("High scores saved.");
        }
    }
}

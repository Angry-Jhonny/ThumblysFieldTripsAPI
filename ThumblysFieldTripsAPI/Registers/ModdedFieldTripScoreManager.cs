using System;
using System.Collections.Generic;
using System.IO;
using UnityCipher;
using UnityEngine;
using Newtonsoft.Json;

namespace ThumblysFieldTripsAPI.Registers
{
    #region New Save Structure
    [Serializable]
    public class ModdedFieldTripData
    {
        public List<FieldTripScore> tripScores = new List<FieldTripScore>();
        public int version = 2;
    }

    [Serializable]
    public class FieldTripScore
    {
        public string minigame;
        public double score;
        public string name;

        public FieldTripScore(string minigame, double score, string name)
        {
            this.minigame = minigame;
            this.score = score;
            this.name = name;
        }
    }
    #endregion

    #region Old Save Structure
    [Serializable]
    public class ModdedFieldTripDataOld
    {
        public List<FieldTripScoreOld> tripScores = new List<FieldTripScoreOld>();
    }

    [Serializable]
    public class FieldTripScoreOld
    {
        public string minigame;
        public int score;
        public string name;

        public FieldTripScoreOld(string minigame, int score, string name)
        {
            this.minigame = minigame;
            this.score = score;
            this.name = name;
        }

        public static FieldTripScore ConvertTo(FieldTripScoreOld score)
        {
            return new FieldTripScore(score.minigame, Convert.ToDouble(score.score), score.name);
        }
    }
    #endregion

    public class ModdedFieldTripScoreManager : Singleton<ModdedFieldTripScoreManager>
    {
        public List<FieldTripScore> tripScores = new List<FieldTripScore>() { };

        public List<FieldTripScore> GetSortedScoresForMinigame(string minigameName, bool greaterWins)
        {
            List<FieldTripScore> list = new List<FieldTripScore>();
            for (int i = 0; i < tripScores.Count; i++)
            {
                if (string.Equals(tripScores[i].minigame, minigameName))
                {
                    list.Add(tripScores[i]);
                }
            }

            SortMinigameScores(list, greaterWins);
            return list;
        }

        private void SortMinigameScores(List<FieldTripScore> scores, bool greaterWins)
        {
            List<FieldTripScore> list = new List<FieldTripScore>();
            int count = scores.Count;
            while (list.Count < count)
            {
                int index = 0;
                double num = 0;
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
            if (!greaterWins)
            {
                scores.Reverse();
            }
        }

        public void SetDefaultsTrips()
        {
            tripScores.Clear();
        }

        public void AddScore(string minigame, double score, string name, bool greaterWins, out int rank)
        {
            bool flag = false;
            rank = -1;
            FieldTripScore tripScore = new FieldTripScore(minigame, score, name);
            tripScores.Add(tripScore);
            List<FieldTripScore> list = new List<FieldTripScore>(GetSortedScoresForMinigame(minigame, greaterWins));
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
            string oldPath = Application.persistentDataPath + "/Modded/FieldTripHighScores.hgh";
            string path = Application.persistentDataPath + "/Modded/ModdedTripHighScores.hgh";
            SetDefaultsTrips();
            ModdedFieldTripData moddedFieldTripData = new ModdedFieldTripData();
            if (File.Exists(oldPath))
            {
                Debug.Log("Old version of high scores found! trying to convert");
                bool flag = true;
                ModdedFieldTripDataOld oldModdedFieldTripData = new ModdedFieldTripDataOld();
                try
                {
                    oldModdedFieldTripData = JsonConvert.DeserializeObject<ModdedFieldTripDataOld>(RijndaelEncryption.Decrypt(File.ReadAllText(oldPath), "99"));
                }
                catch
                {
                    flag = false;
                }

                if (flag)
                {
                    List<FieldTripScore> converted = new List<FieldTripScore>();
                    foreach (FieldTripScoreOld tripScore in oldModdedFieldTripData.tripScores)
                    {
                        converted.Add(FieldTripScoreOld.ConvertTo(tripScore));
                    }
                    tripScores = converted;
                    File.Delete(oldPath);
                    Save();
                    Debug.Log("Successfully loaded from old High Scores!");
                }
                else
                {
                    Debug.LogWarning("Old High scores file is corrupted. Trying to find new TripScores.");
                    File.Delete(oldPath);
                    Load();
                }
            }
            else if (File.Exists(path))
            {
                bool flag = true;
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
                    Debug.Log("Successfully loaded High Scores!");
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
            string path = Application.persistentDataPath + "/Modded/ModdedTripHighScores.hgh";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            ModdedFieldTripData moddedFieldTripData = new ModdedFieldTripData();
            moddedFieldTripData.tripScores = tripScores;
            moddedFieldTripData.version = 2;
            File.WriteAllText(path, RijndaelEncryption.Encrypt(JsonConvert.SerializeObject(moddedFieldTripData), "99"));
            Debug.Log("High scores saved.");
        }
    }
}

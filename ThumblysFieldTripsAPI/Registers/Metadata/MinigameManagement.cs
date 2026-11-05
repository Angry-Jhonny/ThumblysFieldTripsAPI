using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThumblysFieldTripsAPI.Extentions;
using UnityEngine;

namespace ThumblysFieldTripsAPI.Registers
{
    [Flags]
    public enum MinigameFlags // some might be useful while others might not be.
    {
        /// <summary>
        /// This Minigame has no applicable flags.
        /// </summary>
        None,
        /// <summary>
        /// This Minigame uses ElapsedTime and uses ExtendedMinigame.
        /// </summary>
        TimeScoring,
        /// <summary>
        /// You can use the cursor for playing the minigame.
        /// </summary>
        UsesCursor,
        /// <summary>
        /// Starts the minigame immediatly, used only on tutorials in Vanilla plus.
        /// </summary>
        SkipsTitle,
    }

    public class MinigameMetadata : IMetadata<Minigame>
    {
        private static FieldInfo _minigame = AccessTools.Field(typeof(MinigameBase), "minigame");

        public MinigameFlags flags;
        public MinigameName type => value.GetMinigameEnum();

        public Dictionary<string, Minigame> prefabs;
        private string defaultKey;
        public Minigame value => prefabs[defaultKey];
        public HashSet<string> tags => _tags;
        private HashSet<string> _tags = new HashSet<string>();

        public ScoringType scoreType = ScoringType.HighestScore;

        public PluginInfo info => _info;
        private PluginInfo _info;

        public MinigameMetadata(PluginInfo info, Minigame[] prefabs, string defKey, MinigameFlags flags = MinigameFlags.None)
        {
            _info = info;
            this.prefabs = new Dictionary<string, Minigame>();
            for (int i = 0; i < prefabs.Length; i++)
            {
                this.prefabs[prefabs[i].name] = prefabs[i];
            }
            defaultKey = defKey;
            this.flags = flags;
        }

        public MinigameMetadata(PluginInfo info, Minigame[] prefabs, string defKey, MinigameFlags flags, string[] tags) : this(info, prefabs, defKey, flags)
        {
            _tags.UnionWith(tags);
        }
    }

    public class MinigameMetaStorage : MetaStorage<MinigameName, MinigameMetadata, Minigame>
    {
        private static FieldInfo _minigame = AccessTools.Field(typeof(MinigameBase), "minigame");

        public static MinigameMetaStorage Instance => FieldTripsAPI.minigamesMetadata;

        public override void Add(MinigameMetadata toAdd)
        {
            metas.Add(toAdd.type, toAdd);
        }

        public bool AddPrefab(Minigame toAdd)
        {
            MinigameMetadata md = Get(toAdd);
            if (md == null) return false;
            md.prefabs[toAdd.name] = toAdd;
            return true;
        }

        public bool RemovePrefab(Minigame toRemove)
        {
            MinigameMetadata md = Get(toRemove);
            if (md == null) return false;
            return md.prefabs.Remove(md.prefabs.FirstOrDefault(x => x.Value == toRemove).Key);
        }

        public override MinigameMetadata Get(Minigame minigame)
        {
            return Get(minigame.GetMinigameEnum());
        }
    }

    public static class MetaExtentions
    {
        public static MinigameMetadata AddMeta(this Minigame me, BaseUnityPlugin plugin, MinigameFlags flags)
        {
            MinigameMetadata minigameMetadata = MinigameMetaStorage.Instance.Find((MinigameMetadata x) => x.type == me.GetMinigameEnum());
            if (minigameMetadata != null)
            {
                Debug.Log("Minigame " + EnumExtensions.GetExtendedName<Character>((int)me.GetMinigameEnum()) + " already has meta! Adding prefab instead...");
                minigameMetadata.prefabs.Add(me.name, me);
                return minigameMetadata;
            }

            MinigameMetadata minigameMetadata2 = new MinigameMetadata(plugin.Info, new Minigame[1] { me }, me.name, flags);
            MinigameMetaStorage.Instance.Add(minigameMetadata2);
            return minigameMetadata2;
        }
    }
}

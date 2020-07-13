using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Harmony;

namespace WaterproofItems
{
    public partial class ModEntry : Mod
    {
        public static Mod Instance { get; set; } = null;
        public static ModConfig Config { get; set; } = null;

        public override void Entry(IModHelper helper)
        {
            Instance = this; //provide a global reference to this mod's SMAPI utilities

            try
            {
                Config = Helper.ReadConfig<ModConfig>(); //attempt to load (or create) config.json
            }
            catch (Exception ex)
            {
                Monitor.Log($"Encountered an error while loading the config.json file. Default settings will be used instead. Full error message:\n-----\n{ex.ToString()}", LogLevel.Error);
                Config = new ModConfig(); //use the default settings
            }

            ApplyHarmonyPatches();

            Helper.Events.GameLoop.GameLaunched += EnableGMCM;
        }

        /// <summary>Applies any Harmony patches used by this mod.</summary>
        private void ApplyHarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(ModManifest.UniqueID); //create this mod's Harmony instance

            HarmonyPatch_FloatingItemBehavior.ApplyPatch(harmony);

            HarmonyPatch_FloatingItemVisualEffect.Instance = harmony; //pass the harmony instance to this patch (handled differently to support reuse after launch)
            if (Config?.EnableCosmeticFloatingEffect == true) //if the cosmetic effect is enabled
                HarmonyPatch_FloatingItemVisualEffect.ApplyPatch();
        }
    }

    /// <summary>A static class containing any extensions shared by separate parts of this mod.</summary>
    public static class Extensions
    {
        /// <summary>Indicates whether a <see cref="Debris"/> instance contains or represents an item.</summary>
        /// <param name="debris">The debris instance.</param>
        /// <returns>True if the debris contains or represents an item; otherwise false.</returns>
        public static bool IsAnItem(this Debris debris)
        {
            if
            (
                debris.debrisType == Debris.DebrisType.OBJECT
                || debris.debrisType == Debris.DebrisType.ARCHAEOLOGY
                || debris.debrisType == Debris.DebrisType.RESOURCE
                || debris.item != null
            )
                return true; //this is an item
            else
                return false; //this is NOT an item
        }
    }
}

﻿using System;
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
        /// <summary>A SMAPI GameLaunched event that enables GMCM support if that mod is available.</summary>
        public void EnableGMCM(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                GenericModConfigMenuAPI api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu"); //attempt to get GMCM's API instance

                if (api == null) //if the API is not available
                    return;

                api.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config)); //register "revert to default" and "write" methods for this mod's config

                //register an option for each of this mod's config settings
                api.RegisterSimpleOption
                (
                    ModManifest,
                    "Enable cosmetic floating effect",
                    "Check this box to enable the cosmetic \"floating on waves\" effect on items in water.\nUncheck this box to disable the effect.\nDisabling this may improve performance on some systems.",
                    () => Config.EnableCosmeticFloatingEffect,
                    (bool val) =>
                    {
                        if (val) //if this is being set to true
                            HarmonyPatch_FloatingItemEffect.ApplyPatch(); //apply this patch if necessary
                        else //if this is being set to false
                            HarmonyPatch_FloatingItemEffect.RemovePatch(); //remove this patch if necessary

                        Config.EnableCosmeticFloatingEffect = val;
                    }
                );

                api.RegisterSimpleOption
                (
                    ModManifest,
                    "Floating items move toward players",
                    "Check this box to make items in water slowly move toward the nearest player, making them easier to retrieve.\nIn multiplayer, this setting will only wor",
                    () => Config.FloatingItemsMoveTowardPlayers,
                    (bool val) => Config.FloatingItemsMoveTowardPlayers = val
                );


            }
            catch (Exception ex)
            {
                Monitor.Log($"An error happened while loading this mod's GMCM options menu. Its menu might be missing or fail to work. The auto-generated error message has been added to the log.", LogLevel.Warn);
                Monitor.Log($"----------", LogLevel.Trace);
                Monitor.Log($"{ex.ToString()}", LogLevel.Trace);
            }
        }
    }

    /// <summary>Generic Mod Config Menu's API interface. Used to recognize & interact with the mod's API when available.</summary>
    public interface GenericModConfigMenuAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
    }
}

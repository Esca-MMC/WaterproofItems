using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;

namespace WaterproofItems
{
    /// <summary>Handles this mod's option menu for Generic Mod Config Menu (GMCM).</summary>
    public static class GMCM
    {
        /// <summary>This mod's SMAPI monitor.</summary>
        private static IMonitor Monitor => ModEntry.Instance.Monitor;
        /// <summary>This mod's SMAPI helper.</summary>
        private static IModHelper Helper => ModEntry.Instance.Helper;
        /// <summary>This mod's SMAPI manifest.</summary>
        private static IManifest ModManifest => ModEntry.Instance.ModManifest;
        /// <summary>This mod's "config.json" class instance.</summary>
        private static ModConfig Config
        {
            get => ModEntry.Config;
            set => ModEntry.Config = value;
        }

        /// <summary>True if the method <see cref="Enable"/> has already run once.</summary>
        /// <remarks>This does NOT indicate whether GMCM is installed, nor whether this mod's menu is actually enabled.</remarks>
        private static bool InitializedGMCM { get; set; } = false;

        /// <summary>A SMAPI event that initializes this mod's GMCM option menu if possible.</summary>
        /// <remarks>This SMAPI event type was used to avoid issues with GMCM readiness timing. Another event may be more appropriate with later GMCM versions.</remarks>
        public static void Enable(object sender, RenderedActiveMenuEventArgs e)
        {
            if (InitializedGMCM)
                return; //do nothing
            InitializedGMCM = true; //prevent this method running again after this

            try
            {
                var api = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu"); //attempt to get GMCM's API instance

                if (api == null)
                    return;

                //create this mod's menu
                api.Register
                (
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config),
                    titleScreenOnly: false
                );

                //register an option for each config setting
                api.AddBoolOption
                (
                    mod: ModManifest,
                    getValue: () => ModEntry.Config.EnableCosmeticFloatingEffect,
                    setValue: (bool val) =>
                    {
                        if (val) //if this is being set to true
                            HarmonyPatch_FloatingItemVisualEffect.ApplyPatch(); //apply this patch if necessary
                        else //if this is being set to false
                            HarmonyPatch_FloatingItemVisualEffect.RemovePatch(); //remove this patch if necessary

                        Config.EnableCosmeticFloatingEffect = val;
                    },
                    name: () => "Enable floating animation",
                    tooltip: () => "If enabled, items on water will have a cosmetic floating animation.\n\nIf floating items cause your game to slow down, try disabling this."
                );

                api.AddBoolOption
                (
                    mod: ModManifest,
                    getValue: () => Config.TeleportItemsOutOfWater,
                    setValue: (bool val) => Config.TeleportItemsOutOfWater = val,
                    name: () => "Teleport floating items to player",
                    tooltip: () => "If enabled, items on water will immediately teleport to the nearest player."
                    
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
}

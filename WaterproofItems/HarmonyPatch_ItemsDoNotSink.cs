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
    /// <summary>A Harmony patch that prevents item-containing debris sinking in water.</summary>
    public static class HarmonyPatch_ItemsDoNotSink
    {
        /// <summary>Applies this Harmony patch to the game through the provided instance.</summary>
        /// <param name="harmony">This mod's Harmony instance.</param>
        public static void ApplyPatch(HarmonyInstance harmony)
        {
            ModEntry.Instance.Monitor.Log($"Applying Harmony patch \"{nameof(HarmonyPatch_ItemsDoNotSink)}\": prefixing SDV method \"GameLocation.sinkDebris(Debris, Vector2, Vector2)\".", LogLevel.Trace);
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.sinkDebris), new[] { typeof(Debris), typeof(Vector2), typeof(Vector2) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatch_ItemsDoNotSink), nameof(sinkDebris_Prefix))
            );
        }

        /// <summary>Prevents debris sinking if it contains or represents an item.</summary>
        /// <param name="debris">The sinking debris.</param>
        /// <param name="__result">The original method's return value. True if the debris should sink, false otherwise.</param>
        public static bool sinkDebris_Prefix(Debris debris, ref bool __result)
        {
            try
            {
                if (debris != null) //if the debris exists
                {
                    if (debris.debrisType == Debris.DebrisType.OBJECT || debris.debrisType == Debris.DebrisType.ARCHAEOLOGY || debris.debrisType == Debris.DebrisType.RESOURCE || debris.item != null) //if this debris contains any kind of item
                    {
                        __result = false; //return false instead of the original result
                        return false; //skip the rest of the original method (note: this also skips any other patches on the method, depending on order)
                    }
                }
                
                return true; //run the original method

            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.LogOnce($"Harmony patch \"{nameof(sinkDebris_Prefix)}\" has encountered an error:\n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }
    }
}

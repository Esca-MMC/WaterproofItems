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
    public static class HarmonyPatch_FloatingItemsMoveTowardPlayer
    {
        /// <summary>Applies this Harmony patch to the game through the provided instance.</summary>
        /// <param name="harmony">This mod's Harmony instance.</param>
        public static void ApplyPatch(HarmonyInstance harmony)
        {
            ModEntry.Instance.Monitor.Log($"Applying Harmony patch \"{nameof(HarmonyPatch_FloatingItemsMoveTowardPlayer)}\": postfixing SDV method \"Debris.playerInRange(Vector2, Farmer)\".", LogLevel.Trace);
            harmony.Patch(
                original: AccessTools.Method(typeof(Debris), "playerInRange", new[] { typeof(Vector2), typeof(Farmer) }), //note: Debris.playerInRange access level is private
                postfix: new HarmonyMethod(typeof(HarmonyPatch_FloatingItemsMoveTowardPlayer), nameof(playerInRange_Postfix))
            );
        }

        /// <summary>Causes item debris in water to ignore distance when finding nearby players.</summary>
        /// <param name="__instance">The <see cref="Debris"/> instance on which this method was called.</param>
        /// <param name="__result">The original method's return value. True if the provided player is in range.</param>
        public static void playerInRange_Postfix(Debris __instance, ref bool __result)
        {
            try
            {
                if (ModEntry.Config?.FloatingItemsMoveTowardPlayers == true //if this patch is enabled
                    && __result == false //AND the player was NOT in range
                    && __instance?.IsAnItem() == true //AND this debris represents an item
                    && __instance.Chunks.Count > 0) //AND it has at least 1 chunk
                {
                    Vector2 tilePosition = new Vector2((int)(__instance.Chunks[0].position.X / 64.0) + 1, (int)(__instance.Chunks[0].position.Y / 64.0) + 1); //get the first chunk's tile position
                    if (Game1.player.currentLocation.doesTileSinkDebris((int)tilePosition.X, (int)tilePosition.Y, __instance.debrisType)) //if this chunk is floating (i.e. should sink on its current tile)
                    {
                        __result = true; //return true (player is in range)
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.LogOnce($"Harmony patch \"{nameof(playerInRange_Postfix)}\" has encountered an error:\n{ex.ToString()}", LogLevel.Error);
            }
        }
    }
}

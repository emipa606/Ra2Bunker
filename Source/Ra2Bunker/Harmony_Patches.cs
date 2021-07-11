using HarmonyLib;
using RimWorld;
using Verse;

namespace Ra2Bunker
{
    [StaticConstructorOnStartup]
    internal class Harmony_Patches
    {
        static Harmony_Patches()
        {
            //Log.Message("Hello World!");
            var harmony = new Harmony("rimworld.scarjit.ra2bunker");

            var original = typeof(GameEnder).GetMethod("CheckOrUpdateGameOver");
            var postfix = typeof(Patches).GetMethod("CheckOrUpdateGameOver_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(postfix));
        }
    }

    internal class Patches
    {
        public static void CheckOrUpdateGameOver_Postfix(GameEnder __instance)
        {
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                var thingList = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
                foreach (var thing in thingList)
                {
                    if (!(thing is Building_Bunker {HasAnyContents: true}))
                    {
                        continue;
                    }

                    __instance.gameEnding = false;
                    return;
                }
            }
        }
    }
}
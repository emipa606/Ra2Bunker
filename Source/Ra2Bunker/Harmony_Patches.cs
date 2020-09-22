using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Ra2Bunker
{
    [StaticConstructorOnStartup]
    class Harmony_Patches
    {
        static Harmony_Patches()
        {
            //Log.Message("Hello World!");
            Harmony harmony = new Harmony(id: "rimworld.scarjit.ra2bunker");

            var original = typeof(GameEnder).GetMethod("CheckOrUpdateGameOver");
            var postfix = typeof(Patches).GetMethod("CheckOrUpdateGameOver_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(postfix));

        }

    }

    class Patches
    {
        public static void CheckOrUpdateGameOver_Postfix(GameEnder __instance)
        {
            List<Map> maps = Find.Maps;
            foreach (Map map in maps)
            {
                List<Thing> thingList = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
                foreach (Thing thing in thingList)
                {
                    if (!(thing is Building_Bunker bunker) || !bunker.HasAnyContents)
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

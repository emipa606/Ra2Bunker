using HarmonyLib;
using RimWorld;
using Verse;

namespace Ra2Bunker;

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
        harmony.PatchAll();
    }
}
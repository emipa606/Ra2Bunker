using HarmonyLib;
using RimWorld;

namespace Ra2Bunker
{
    [HarmonyPatch(typeof(Building_TurretGun), "CanSetForcedTarget", MethodType.Getter)]
    public static class Building_TurretGun_CanSetForcedTarget
    {
        public static void Postfix(Building_TurretGun __instance, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            __result = __instance.def.defName == "Ra2_Bunker";
        }
    }
}

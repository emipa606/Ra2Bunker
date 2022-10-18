using UnityEngine;
using Verse;

namespace Ra2Bunker;

public class PlaceWorker_ShowBunkerRadius : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        _ = Find.CurrentMap;
        GenDraw.DrawFieldEdges(Toils_bunker.GetAllEnterOutLoc(center), Color.magenta);
    }
}
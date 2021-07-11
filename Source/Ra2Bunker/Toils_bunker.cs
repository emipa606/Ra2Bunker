using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Ra2Bunker
{
    public static class Toils_bunker
    {
        public static Toil GotoThing(TargetIndex ind, PathEndMode peMode)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                actor.pather.StartPath(GetBunkerNearCell(actor.jobs.curJob.GetTarget(ind)), peMode);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedOrNull(ind);
            return toil;
        }


        private static LocalTargetInfo GetBunkerNearCell(LocalTargetInfo bunker)
        {
            var map = bunker.Thing.Map;
            var center = bunker.Cell;
            var bunk = (Building_Bunker) bunker.Thing;
            if (bunk == null)
            {
                return null;
            }

            var direc = bunk.direc;
            var cells = new List<IntVec3>();
            for (var i = -2; i < 3; i++)
            {
                for (var j = -2; j < 3; j++)
                {
                    if (Math.Abs(i) <= 1 && Math.Abs(j) <= 1 || Math.Abs(i * j) != 2)
                    {
                        continue;
                    }

                    if ((direc != 0 || j <= 0) && (direc != 1 || i <= 0) && (direc != 2 || j >= 0) &&
                        (direc != 3 || i >= 0))
                    {
                        continue;
                    }

                    var cel = new IntVec3(center.x + i, center.y, center.z + j);

                    if (CanOut(cel, map))
                    {
                        cells.Add(cel);
                    }
                }
            }

            return cells.RandomElement();
        }

        public static IntVec3 GetEnterOutLoc(Building_Bunker bunker)
        {
            var map = bunker.Map;
            var center = bunker.Position;
            var direc = bunker.direc;
            var cells = new List<IntVec3>();
            for (var i = -2; i < 3; i++)
            {
                for (var j = -2; j < 3; j++)
                {
                    if (Math.Abs(i) <= 1 && Math.Abs(j) <= 1 || Math.Abs(i * j) == 4)
                    {
                        continue;
                    }

                    if ((direc != 0 || j <= 0) && (direc != 1 || i <= 0) && (direc != 2 || j >= 0) &&
                        (direc != 3 || i >= 0))
                    {
                        continue;
                    }

                    var cel = new IntVec3(center.x + i, center.y, center.z + j);
                    if (CanOut(cel, map))
                    {
                        cells.Add(cel);
                    }
                }
            }

            return cells.RandomElement();
        }

        public static List<IntVec3> GetAllEnterOutLoc(IntVec3 bunker)
        {
            var center = bunker;

            var cells = new List<IntVec3>();
            for (var i = -2; i < 3; i++)
            {
                for (var j = -2; j < 3; j++)
                {
                    if (Math.Abs(i) <= 1 && Math.Abs(j) <= 1 || Math.Abs(i * j) == 4)
                    {
                        continue;
                    }

                    var cel = new IntVec3(center.x + i, center.y, center.z + j);
                    if (CanOut(cel, Find.CurrentMap))
                    {
                        cells.Add(cel);
                    }
                }
            }

            return cells;
        }

        private static bool CanOut(IntVec3 cell, Map map)
        {
            return map.thingGrid.ThingsListAt(cell).FindAll(x => x.def.passability == Traversability.Impassable)
                .Count == 0;
        }
    }
}
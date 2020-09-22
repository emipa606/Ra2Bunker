using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Ra2Bunker
{
    public class JobDriver_EnterRa2Bunker : JobDriver
    {
        // Token: 0x060002E5 RID: 741 RVA: 0x0001C3C4 File Offset: 0x0001A7C4
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        // Token: 0x060002E6 RID: 742 RVA: 0x0001C3FC File Offset: 0x0001A7FC
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_bunker.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);//Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
 
            Toil enter = new Toil();
            enter.initAction = delegate
            {
                Pawn actor = enter.actor;
                Building_Bunker pod = (Building_Bunker)actor.CurJob.targetA.Thing;
                void action()
                {
                    if (pod.GetInner().InnerListForReading.Count >= pod.maxCount)
                    {
                        return;
                    }
                    actor.DeSpawn(DestroyMode.Vanish);
                    pod.TryAcceptThing(actor, true);
                }

                action();
                
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
            yield break;
        }
    }
}

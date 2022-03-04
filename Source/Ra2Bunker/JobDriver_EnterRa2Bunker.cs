using System.Collections.Generic;
using Verse.AI;

namespace Ra2Bunker;

public class JobDriver_EnterRa2Bunker : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return
            Toils_bunker.GotoThing(TargetIndex.A,
                PathEndMode.ClosestTouch); //Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

        var enter = new Toil();
        enter.initAction = delegate
        {
            var actor = enter.actor;
            var pod = (Building_Bunker)actor.CurJob.targetA.Thing;

            void action()
            {
                if (pod.GetInner().InnerListForReading.Count >= pod.maxCount)
                {
                    return;
                }

                actor.DeSpawn();
                pod.TryAcceptThing(actor);
            }

            action();
        };
        enter.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return enter;
    }
}
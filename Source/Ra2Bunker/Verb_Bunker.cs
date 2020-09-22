using System.Collections.Generic;
using Verse;

namespace Ra2Bunker
{
    public class Verb_Bunker : Verb_LaunchProjectile
    {
        private Building_Bunker bunker;
        private List<Verb> verbss;
        public override void Reset()
        {
            base.Reset();
            bunker = (Building_Bunker)caster;
        }

        public void ResetVerb()
        {
            if (bunker == null)
            {
                bunker = (Building_Bunker)caster;
            }
            foreach (Pawn pawn in bunker.GetInner().InnerListForReading)
            {

                if (pawn.TryGetAttackVerb(currentTarget.Thing) == null)
                {
                    continue;
                }
                //Log.Warning(pawn.TryGetAttackVerb(this.currentTarget.Thing).TryStartCastOn(this.currentTarget) + "OH" + pawn.Name);
                pawn.TryGetAttackVerb(currentTarget.Thing).caster = pawn;

            }
        }
        protected override bool TryCastShot()
        {
            verbss = new List<Verb>();

            if (bunker == null)
            {
                bunker = (Building_Bunker)caster;
            }
            foreach (Pawn pawn in bunker.GetInner().InnerListForReading)
            {
                if (pawn.TryGetAttackVerb(currentTarget.Thing) != null)
                {
                    //Log.Warning(pawn.TryGetAttackVerb(this.currentTarget.Thing).TryStartCastOn(this.currentTarget) + "OH" + pawn.Name);
                    verbss.Add(pawn.TryGetAttackVerb(currentTarget.Thing));

                }
                else
                {
                    //  Log.Warning(pawn.Name+" no weapon");
                }
            }

            foreach (Verb vb in verbss)
            {

                // Thing tmpCaster = vb.caster;
                vb.caster = caster;
                bool fired = vb.TryStartCastOn(currentTarget);

                //vb.caster = tmpCaster;
            }
            return true;
        }
    }
}

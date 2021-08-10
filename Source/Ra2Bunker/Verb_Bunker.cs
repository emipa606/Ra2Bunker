using System.Collections.Generic;
using Verse;

namespace Ra2Bunker
{
    public class Verb_Bunker : Verb_Shoot
    {
        private Building_Bunker bunker;
        private List<Verb> verbss;

        public override void Reset()
        {
            base.Reset();
            bunker = (Building_Bunker) caster;
        }

        public void ResetVerb()
        {
            if (bunker == null)
            {
                bunker = (Building_Bunker) caster;
            }

            foreach (var pawn in bunker.GetInner().InnerListForReading)
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
                bunker = (Building_Bunker) caster;
            }

            foreach (var pawn in bunker.GetInner().InnerListForReading)
            {
                if (pawn.TryGetAttackVerb(currentTarget.Thing) != null)
                {
                    //Log.Warning(pawn.TryGetAttackVerb(this.currentTarget.Thing).TryStartCastOn(this.currentTarget) + "OH" + pawn.Name);
                    verbss.Add(pawn.TryGetAttackVerb(currentTarget.Thing));
                }
            }

            foreach (var vb in verbss)
            {
                // Thing tmpCaster = vb.caster;
                vb.caster = caster;
                var unused = vb.TryStartCastOn(currentTarget);

                //vb.caster = tmpCaster;
            }

            return true;
        }
    }
}
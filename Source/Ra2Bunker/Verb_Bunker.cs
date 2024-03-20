using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Ra2Bunker;

public class Verb_Bunker : Verb_Shoot
{
    private Building_Bunker bunker;
    private List<Verb> verbss;

    private Dictionary<Pawn, int> warmupDictionary;

    public override void Reset()
    {
        base.Reset();
        bunker = (Building_Bunker)caster;
        warmupDictionary = new Dictionary<Pawn, int>();
    }

    public void ResetVerb()
    {
        if (bunker == null)
        {
            bunker = (Building_Bunker)caster;
        }

        warmupDictionary = new Dictionary<Pawn, int>();
        ((Building_Bunker)caster).UpdateRange();

        foreach (var pawn in bunker.GetInner().InnerListForReading)
        {
            if (pawn.TryGetAttackVerb(currentTarget.Thing) == null)
            {
                continue;
            }

            pawn.TryGetAttackVerb(currentTarget.Thing).caster = pawn;
        }
    }


    protected override bool TryCastShot()
    {
        verbss = [];

        if (bunker == null)
        {
            bunker = (Building_Bunker)caster;
        }

        var pawns = bunker.GetInner().InnerListForReading;
        var newDictionary = new Dictionary<Pawn, int>();
        if (warmupDictionary == null)
        {
            warmupDictionary = new Dictionary<Pawn, int>();
        }
        else
        {
            foreach (var pawn in pawns)
            {
                if (!warmupDictionary.TryGetValue(pawn, out var value))
                {
                    continue;
                }

                newDictionary[pawn] = value;
            }
        }

        warmupDictionary = newDictionary;

        foreach (var pawn in pawns)
        {
            if (pawn.TryGetAttackVerb(currentTarget.Thing) == null)
            {
                continue;
            }

            var verb = pawn.TryGetAttackVerb(currentTarget.Thing);
            if (checkWarmup(pawn, verb))
            {
                verbss.Add(verb);
            }
        }

        //foreach (var pair in warmupDictionary)
        //{
        //    Log.Message($"{GenTicks.TicksGame} - {pair.Key}: {pair.Value}");
        //}

        if (!verbss.Any())
        {
            return false;
        }

        //Log.Message($"Found {verbss.Count} verbs");
        foreach (var vb in verbss)
        {
            //Log.Message($"{vb}");
            vb.caster = caster;
            //vb.WarmupComplete();
            vb.TryStartCastOn(currentTarget);
        }

        return true;
    }

    private bool checkWarmup(Pawn shooter, Verb attackVerb)
    {
        if (attackVerb.IsMeleeAttack)
        {
            return false;
        }

        if (warmupDictionary == null)
        {
            warmupDictionary = new Dictionary<Pawn, int>();
        }

        if (warmupDictionary.ContainsKey(shooter) && warmupDictionary[shooter] > 0)
        {
            warmupDictionary[shooter] -= 10;
            return false;
        }

        var returnValue = warmupDictionary.ContainsKey(shooter);

        var statValue = shooter.GetStatValue(StatDefOf.AimingDelayFactor);
        warmupDictionary[shooter] = (attackVerb.verbProps.warmupTime * statValue).SecondsToTicks() +
                                    attackVerb.verbProps.AdjustedCooldownTicks(attackVerb, shooter);

        return returnValue;
    }
}
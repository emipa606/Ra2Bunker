using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Ra2Bunker;

public class Building_Bunker : Building_TurretGun, IThingHolder
{
    public readonly int maxCount = 6;
    public int direc;


    protected ThingOwner<Pawn> innerContainer;

    public Building_Bunker()
    {
        innerContainer = new ThingOwner<Pawn>(this, false);
    }

    public bool HasAnyContents => innerContainer.Count > 0;

    public Thing ContainedThing => innerContainer.Count != 0 ? innerContainer[0] : null;

    public bool CanOpen => HasAnyContents;

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        UpdateRange();
    }

    public ThingOwner<Pawn> GetInner()
    {
        return innerContainer;
    }

    public override void TickRare()
    {
        base.TickRare();
        innerContainer.ThingOwnerTickRare();
    }


    public override void Tick()
    {
        if (innerContainer.Count < 1)
        {
            return;
        }

        base.Tick();
        innerContainer.ThingOwnerTick();
    }

    public virtual void Open()
    {
        if (!HasAnyContents)
        {
            return;
        }

        EjectAllContents();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref direc, "direc");
        Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
    }

    public override bool ClaimableBy(Faction fac, StringBuilder reason = null)
    {
        if (!innerContainer.Any)
        {
            return base.ClaimableBy(fac, reason);
        }

        foreach (var pawn in innerContainer)
        {
            if (pawn.Faction != fac)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    public virtual bool Accepts(Thing thing)
    {
        return innerContainer.CanAcceptAnyOf(thing);
    }

    public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!Accepts(thing))
        {
            return false;
        }

        bool transfer;
        if (thing.holdingOwner != null)
        {
            thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
            transfer = true;
        }
        else
        {
            transfer = innerContainer.TryAdd(thing);
        }

        UpdateRange();
        return transfer;
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
        {
            EjectAllContents();
        }

        innerContainer.ClearAndDestroyContents();
        base.Destroy(mode);
    }

    public virtual void EjectAllContents()
    {
        (AttackVerb as Verb_Bunker)?.ResetVerb();
        innerContainer.TryDropAll(Toils_bunker.GetEnterOutLoc(this), Map, ThingPlaceMode.Near);
    }

    public override string GetInspectString()
    {
        var text = base.GetInspectString();

        //   str = this.innerContainer.ContentsString;
        var str = $"{innerContainer.Count}/{maxCount}";

        if (!text.NullOrEmpty())
        {
            text += "\n";
        }

        return text + "CasketContains".Translate() + ": " + str.CapitalizeFirst() +
               (innerContainer.Count == maxCount ? "(Full)" : "");
    }

    public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
    {
        foreach (var o in base.GetMultiSelectFloatMenuOptions(selPawns))
        {
            yield return o;
        }

        if (innerContainer.Count == maxCount)
        {
            yield break;
        }

        var assignedPawns = innerContainer.Count;
        var pawnList = new List<Pawn>();
        foreach (var pawn in selPawns)
        {
            if (assignedPawns >= maxCount)
            {
                yield break;
            }

            pawnList.Add(pawn);
        }

        yield return new FloatMenuOption("EnterRa2Bunker".Translate(), jobAction);
        yield break;

        void jobAction()
        {
            MultiEnter(pawnList);
        }
    }

    private void MultiEnter(List<Pawn> pawnsToEnter)
    {
        var jobDef = DefDatabase<JobDef>.GetNamed("EnterRa2Bunker");
        foreach (var pawn in pawnsToEnter)
        {
            var job = new Job(jobDef, this);
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        foreach (var o in base.GetFloatMenuOptions(myPawn))
        {
            yield return o;
        }

        if (innerContainer.Count >= maxCount)
        {
            yield break;
        }

        var jobDef = DefDatabase<JobDef>.GetNamed("EnterRa2Bunker"); //JobDefOf.EnterCryptosleepCasket;
        string jobStr = "EnterRa2Bunker".Translate();

        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction),
            myPawn, this);
        yield break;

        void jobAction()
        {
            var job = new Job(jobDef, this);
            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var c in base.GetGizmos())
        {
            yield return c;
        }

        if (Faction == Faction.OfPlayer && innerContainer.Count > 0)
        {
            var eject = new Command_Action
            {
                action = SelectColonist,
                defaultLabel = "ExitBunker".Translate(),
                defaultDesc = "ExitBunkerDesc".Translate()
            };
            if (innerContainer.Count == 0)
            {
                eject.Disable("CommandPodEjectFailEmpty".Translate());
            }

            eject.hotKey = KeyBindingDefOf.Misc1;
            eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
            yield return eject;
        }

        string[] direcs = ["North", "East", "South", "West"];
        var direction = new Command_Action
        {
            defaultLabel = $"{"NowDirection".Translate()}\n{direcs[direc]}",
            defaultDesc = "ClickToChangeEnterDirection".Translate(),
            icon = TexCommand.GatherSpotActive,
            action = delegate
            {
                if (direc > 2)
                {
                    direc = 0;
                }
                else
                {
                    direc++;
                }
            }
        };

        yield return direction;
    }

    private void SelectColonist()
    {
        var list = new List<FloatMenuOption>();
        if (innerContainer.Count == 0)
        {
            return;
        }

        foreach (var pawn in innerContainer)
        {
            var postfix = new TaggedString();
            if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
            {
                postfix = $" ({pawn.equipment.Primary.def.label})";
            }

            var textToAdd = $"{pawn.NameFullColored}{postfix}";
            var pawnToEject = pawn;
            list.Add(new FloatMenuOption(textToAdd,
                delegate
                {
                    innerContainer.TryDrop(pawnToEject, Toils_bunker.GetEnterOutLoc(this), Map, ThingPlaceMode.Near,
                        out _);
                    UpdateRange();
                }, MenuOptionPriority.Default, null, null, 29f));
        }

        var sortedList = list.OrderBy(option => option.Label).ToList();
        sortedList.Add(new FloatMenuOption("Everyone".Translate(), EjectAllContents,
            MenuOptionPriority.Default, null, null, 29f));
        Find.WindowStack.Add(new FloatMenu(sortedList));
    }

    public void UpdateRange()
    {
        var maxRange = 0f;
        foreach (var pawn in innerContainer)
        {
            if (pawn.equipment.Primary == null || !pawn.equipment.Primary.def.IsRangedWeapon)
            {
                continue;
            }

            foreach (var defVerb in pawn.equipment.Primary.def.Verbs)
            {
                if (defVerb.range > maxRange)
                {
                    maxRange = defVerb.range;
                }
            }
        }

        AttackVerb.verbProps.range = maxRange;
    }
}
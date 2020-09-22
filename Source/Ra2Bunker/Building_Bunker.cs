

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Ra2Bunker
{
    public class Building_Bunker : Building_TurretGun, IThingHolder
    {

        public Building_Bunker()
        {
            innerContainer = new ThingOwner<Pawn>(this, false, LookMode.Deep);
        }

        public bool HasAnyContents
        {
            get
            {
                return innerContainer.Count > 0;
            }
        }

        public Thing ContainedThing
        {
            get
            {
                return (innerContainer.Count != 0) ? innerContainer[0] : null;
            }
        }

        public bool CanOpen
        {
            get
            {
                return HasAnyContents;
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public ThingOwner<Pawn> GetInner()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void TickRare()
        {
            base.TickRare();
            innerContainer.ThingOwnerTickRare(true);
        }


        public override void Tick()
        {
            if (innerContainer.Count < 1) return;
            base.Tick();
            innerContainer.ThingOwnerTick(true);
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
            Scribe_Values.Look<int>(ref direc, "direc", 0, false);
            Scribe_Deep.Look<ThingOwner<Pawn>>(ref innerContainer, "innerContainer", new object[]
            {
                this
            });

        }

        public override bool ClaimableBy(Faction fac)
        {
            if (innerContainer.Any)
            {
                for (int i = 0; i < innerContainer.Count; i++)
                {
                    if (innerContainer[i].Faction != fac)
                    {
                        continue;
                    }
                    return true;
                }
                return false;
            }
            return base.ClaimableBy(fac);
        }
        public virtual bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = innerContainer.TryAdd(thing, true);
            }
            return flag;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                EjectAllContents();
            }
            innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public virtual void EjectAllContents()
        {
            (AttackVerb as Verb_Bunker).ResetVerb();
            innerContainer.TryDropAll(Toils_bunker.GetEnterOutLoc(this), Map, ThingPlaceMode.Near, null, null);
        }

        // Token: 0x060024FE RID: 9470 RVA: 0x00116EF0 File Offset: 0x001152F0
        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            string str;

            //   str = this.innerContainer.ContentsString;
            str = $"{innerContainer.Count}/{maxCount}";

            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            return text + "CasketContains".Translate() + ": " + str.CapitalizeFirst() + ((innerContainer.Count == maxCount) ? "(Full)" : "");
        }

        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            foreach (FloatMenuOption o in base.GetMultiSelectFloatMenuOptions(selPawns))
            {
                yield return o;
            }
            if (innerContainer.Count == maxCount)
            {
                yield break;
            }
            if (Toils_bunker.GetEnterOutLoc(this) == null)
            {
                FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return failer;
            }
            else
            {
                var assignedPawns = innerContainer.Count;
                var pawnList = new List<Pawn>();
                foreach (Pawn pawn in selPawns)
                {
                    if (assignedPawns >= maxCount)
                    {
                        yield break;
                    }
                    pawnList.Add(pawn);
                }
                void jobAction()
                {
                    MultiEnter(pawnList);
                }
                yield return new FloatMenuOption("EnterRa2Bunker".Translate(), jobAction);
            }
        }

        private void MultiEnter(List<Pawn> pawnsToEnter)
        {
            JobDef jobDef = DefDatabase<JobDef>.GetNamed("EnterRa2Bunker", true);
            foreach (Pawn pawn in pawnsToEnter)
            {
                Job job = new Job(jobDef, this);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }
            if (innerContainer.Count < maxCount)
            {
                if (Toils_bunker.GetEnterOutLoc(this) == null)//!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return failer;
                }
                else
                {
                    JobDef jobDef = DefDatabase<JobDef>.GetNamed("EnterRa2Bunker", true);//JobDefOf.EnterCryptosleepCasket;
                    string jobStr = "EnterRa2Bunker".Translate();
                    void jobAction()
                    {
                        Job job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                    }
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn, this, "ReservedBy");
                }
            }
            yield break;
        }

        // Token: 0x06002515 RID: 9493 RVA: 0x00116FCC File Offset: 0x001153CC
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            if (Faction == Faction.OfPlayer && innerContainer.Count > 0)
            {
                Command_Action eject = new Command_Action
                {
                    action = delegate ()
                    {
                        SelectColonist();
                    },
                    defaultLabel = "ExitBunker".Translate(),
                    defaultDesc = "ExitBunkerDesc".Translate()
                };
                if (innerContainer.Count == 0)
                {
                    eject.Disable("CommandPodEjectFailEmpty".Translate());
                }
                eject.hotKey = KeyBindingDefOf.Misc1;
                eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return eject;
            }
            string[] direcs = { "North", "East", "South", "West" };
            Command_Action direction = new Command_Action
            {
                defaultLabel = $"{"NowDirection".Translate()}\n{direcs[direc]}",
                defaultDesc = "ClickToChangeEnterDirection".Translate(),
                icon = TexCommand.GatherSpotActive,
                action = delegate ()
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
            yield break;
        }

        private void SelectColonist()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (innerContainer.Count == 0)
            {
                return;
            }
            foreach (var pawn in innerContainer)
            {
                TaggedString postfix = new TaggedString();
                if(pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    postfix = $" ({pawn.equipment.Primary.def.label})";
                }
                var textToAdd = $"{pawn.NameFullColored}{postfix}";
                var pawnToEject = pawn;
                list.Add(new FloatMenuOption(textToAdd, delegate ()
                {
                    innerContainer.TryDrop(pawnToEject, Toils_bunker.GetEnterOutLoc(this), Map, ThingPlaceMode.Near, out _);
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            var sortedList = list.OrderBy(option => option.Label).ToList();
            sortedList.Add(new FloatMenuOption("Everyone".Translate(), delegate ()
            {
                EjectAllContents();
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            Find.WindowStack.Add(new FloatMenu(sortedList));
        }


        // Token: 0x040014D5 RID: 5333
        protected ThingOwner<Pawn> innerContainer;

        public int maxCount = 6;

        public int direc = 0;
    }
}

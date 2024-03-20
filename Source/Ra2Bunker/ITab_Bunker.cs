using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Ra2Bunker;

public class ITab_Bunker : ITab_ContentsBase
{
    private readonly List<Thing> listInt = [];

    public ITab_Bunker()
    {
        labelKey = "TabBunkerContents";
        containedItemsKey = "ContainedItems";
        canRemoveThings = false;
    }

    public override IList<Thing> container
    {
        get
        {
            var building_Casket = SelThing as Building_Bunker;
            listInt.Clear();

            if (building_Casket is not { HasAnyContents: true })
            {
                return listInt;
            }

            foreach (var pawn in building_Casket.GetInner())
            {
                listInt.Add(pawn);
            }

            return listInt;
        }
    }
}
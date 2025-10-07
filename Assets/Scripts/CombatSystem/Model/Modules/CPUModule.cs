using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class CPUModule : AModule
{
    public CPUModule()
    {
        // TODO decision making and storage of those "brains"
    }

    public ActionData MakeAction(ICombatModel _)
    {
        var owner = GetOwner();
        bool has_abilities = owner.TryGetModule<AbilityModule>(out var module);

        // if no abilities, they can't take a turn in combat.
        if (!has_abilities) return default;

        IAbility chosen = null;
        foreach (var ability in module.GetAbilities())
        {
            // TODO check decision criteria and pick a move
            // a map of abilities to their precondition "brains"
            chosen = ability;
        }

        var data = new ActionData() { Action = chosen, UserTeamUnitIndex = owner.GetIndices() }; // stub
        // TODO fill out data using the RequiredTargets list. Make sure it's done from their perspective!
        // e.g. "0" means allies of the enemies, so... keep that in mind

        return data;
    }
}

using UnityEngine;

class ReferenceModule : AModule
{
    public ACombatUnitSO CombatUnit { get; private set; }
    public ReferenceModule(ACombatUnitSO combatUnit)
    {
        this.CombatUnit = combatUnit;   
    }
}
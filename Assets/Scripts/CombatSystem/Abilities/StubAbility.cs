using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StubAbility : IAbility
{
    public IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view)
    {
        yield return null;
    }
}

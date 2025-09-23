using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StubCombatView : MonoBehaviour, ICombatView
{
    public IEnumerator NextPhase()
    {
        Debug.Log("Next Phase stub.");

        yield return null;
    }

    public void ProcessUnit(CombatUnit selected_unit)
    {
        Debug.Log("Process unit stub.");
    }
}

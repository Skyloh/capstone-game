using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StubCombatView : MonoBehaviour, ICombatView
{
    public void ProcessUnit(CombatUnit selected_unit)
    {
        Debug.Log("Process unit stub.");
    }
}

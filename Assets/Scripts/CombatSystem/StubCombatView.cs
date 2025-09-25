using System.Collections;
using UnityEngine;

public class StubCombatView : MonoBehaviour, ICombatView
{
    public void BeginUnitSelection()
    {
        Debug.Log("Begin unit selection stub.");
    }

    public IEnumerator NextPhase(int phase_turn_number)
    {
        Debug.Log($"Next Phase stub. Turn count is {phase_turn_number}.");

        yield return null;
    }

    public void ProcessUnit(CombatUnit selected_unit)
    {
        Debug.Log("Process unit stub.");
    }
}

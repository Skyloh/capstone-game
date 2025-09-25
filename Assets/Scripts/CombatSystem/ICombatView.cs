using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatView
{
    void BeginUnitSelection();

    void ProcessUnit(CombatUnit selected_unit);

    IEnumerator NextPhase(int phase_turn_number); // IEnumerator for the presumed delay between phases
}

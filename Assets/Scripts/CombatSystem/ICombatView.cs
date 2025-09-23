using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatView
{
    void ProcessUnit(CombatUnit selected_unit);
}

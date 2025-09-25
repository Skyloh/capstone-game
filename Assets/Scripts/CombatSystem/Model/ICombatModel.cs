using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatModel
{
    Team GetTeam(int team_index);

    int CurrentActiveTeamIndex();
    int IncActiveTeamIndex(); // returns the turn number of the current phase

    CombatUnit GetUnitByIndex(int team_index, int unit_index);
}

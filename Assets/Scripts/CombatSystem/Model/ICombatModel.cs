using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatModel
{
    Team GetTeam(int team_index);

    int CurrentActiveTeamIndex();

    CombatUnit GetUnitByIndex(int team_index, int unit_index);
}

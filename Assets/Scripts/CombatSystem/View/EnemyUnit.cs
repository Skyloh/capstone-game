using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CombatSystem.View{
public class EnemyUnit : Unit
{

 void UpdateWeaknessBar(IList<AffinityType> current, IList<AffinityType> previous)
    {
       Debug.Log("UpdateWeaknessBar " + name); 
    }
}
}
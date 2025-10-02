using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatView : MonoBehaviour
{
    // void InitializeCombat(ICombatModel model)
    // {
    //     
    // }
    //
    // void OnPartyMemberHovered(int id){}
    // void OnPartyMemberSelected(int id){}
    //
    // void OnActionSelected(int id)
    // {
    //     
    // }
    // void OnConfirmAction()
    
    /* UI will probably contain a few prefabs;
     Player Character and enemy character display.
     will have functions to show attack effects update health 
     and weakness bars on the top leel of prefab
     */
    /* UI PSEUDO CODE / structure
     struct Unit {
         image portrait
         image battlesprite
         ui healthbar
         ui? weaknessbar
         playOffenseAction();
     }
     class weaknessBar{
        swap(length, start index 1, start index 2)
        destroy(indices[]) // preferably sorted)
     }
     class Healthbar {
        float maxHealth;
        setHealth();
        getHealth();
        previewDamage(floor, ceiling);
     }
     
     class EnemyInfo {
        Healthbar
        Weaknessbar
        portrait
        setEnemy();
     }
     
    class PlayerInfo {
        portrait
        Healthbar
        weakness
    }
     
     */
}

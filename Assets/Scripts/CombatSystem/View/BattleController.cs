using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleController : MonoBehaviour, ICombatView
{
    // Start is called before the first frame update
    private VisualElement ui;

    private Button attackButton1;
    private Button attackButton2;
    private Button attackButton3;
    private Button attackButton4;
    private Label actionDescription;

    public CombatModel model;
    private void Awake()
    {
       ui = GetComponent<UIDocument>().rootVisualElement;
    }

    private void OnEnable()
    {
        attackButton1 = ui.Q<Button>("Action1");
        attackButton2 = ui.Q<Button>("Action2");
        attackButton3 = ui.Q<Button>("Action3");
        attackButton4 = ui.Q<Button>("Action4");
        actionDescription = ui.Q<Label>("ActionDescription");
        
        attackButton1.RegisterCallback<MouseOverEvent>((moe)=>UpdateAttackDescription(moe, 0));
        attackButton2.RegisterCallback<MouseOverEvent>((moe)=>UpdateAttackDescription(moe, 1));
        attackButton3.RegisterCallback<MouseOverEvent>((moe)=>UpdateAttackDescription(moe, 2));
        attackButton4.RegisterCallback<MouseOverEvent>((moe)=>UpdateAttackDescription(moe, 3));
    }

    private EventCallback<MouseOverEvent> attackButton1HoverCallback;

    private void UpdateAttackDescription(MouseOverEvent e, int index)
    {
       actionDescription.text = abilityCache.GetAbilities()[index].GetAbilityData().Description; 
    }

    
    // Update is called once per frame
    private void Update()
    {
        attackButton1.text = " Hello";
    }

    private AbilityModule abilityCache;
    public void BeginUnitSelection()
    {
        throw new NotImplementedException();
    }

    public void ProcessUnit(CombatUnit selected_unit)
    {
        selected_unit.TryGetModule(out abilityCache);
        var abilities = abilityCache.GetAbilities();
        attackButton1.text = abilities[0].GetAbilityData().Name; // abilities[1].name
        attackButton1.text = abilities[1].GetAbilityData().Name; // abilities[1].name
        attackButton1.text = abilities[2].GetAbilityData().Name; // abilities[1].name
        attackButton1.text = abilities[3].GetAbilityData().Name; // abilities[1].name
    }

    public IEnumerator NextPhase(int phase_turn_number)
    {
        throw new NotImplementedException();
    }

    public void UpdateView(CombatUnit new_unit, int team_id, int unit_index)
    {
        throw new NotImplementedException();
    }
}

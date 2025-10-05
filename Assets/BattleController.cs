using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    
    // Update is called once per frame
    private void Update()
    {
        attackButton1.text = " Hello";
    }

    public void BeginUnitSelection()
    {
        throw new NotImplementedException();
    }

    public void ProcessUnit(CombatUnit selected_unit)
    {
        AbilityModule abilityModule;
        selected_unit.TryGetModule(out abilityModule);
        var abilities = abilityModule.GetAbilities();
        attackButton1.text = ""; // abilities[1].name
        attackButton1.text = ""; // abilities[1].name
        attackButton1.text = ""; // abilities[1].name
        attackButton1.text = ""; // abilities[1].name
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

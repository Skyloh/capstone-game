using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace CombatSystem.View
{
    public class BattleInterface : MonoBehaviour, ICombatView
    {
        // Start is called before the first frame update
        public ViewSprites battleSprites;
        public BrainSO cpuBrain;
        private VisualElement ui;
        private VisualElement playerWeaknessIcon;
        private CombatManager combatManager;
        private Button attackButton1;
        private Button attackButton2;
        private Button attackButton3;
        private Button attackButton4;
        private Label playerCharacterName;
        private Label playerTotalHp;
        private Button backToUnits;
        private Label actionDescription;

        private IUnitSelector unitSelector;

        //TODO remove class and replace with non stub in own file

        public ICombatModel model => combatManager.CombatModel;

        private void Awake()
        {
            ui = GetComponent<UIDocument>().rootVisualElement;
            combatManager = GetComponent<CombatManager>();
            unitSelector = GetComponent<UnitSelector>();
            combatManager.InitCombat(cpuBrain);
            BeginUnitSelection();
        }

        private void OnPlayerHovered(int index, IUnit unit)
        {
            Debug.Log(index);
            DisplayUnit(model.GetTeam(0).GetUnit(index));
        }

        private void OnEnable()
        {
            attackButton1 = ui.Q<Button>("Action1");
            attackButton2 = ui.Q<Button>("Action2");
            attackButton3 = ui.Q<Button>("Action3");
            attackButton4 = ui.Q<Button>("Action4");
            playerCharacterName = ui.Q<Label>("PlayerName");
            playerWeaknessIcon =  ui.Q<VisualElement>("PlayerWeaknessIcon");
            playerTotalHp = ui.Q<Label>("PlayerTotalHp");
            backToUnits = ui.Q<Button>("BackToUnits");
            actionDescription = ui.Q<Label>("ActionDescription");

            attackButton1.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 0));

            attackButton2.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 1));
            attackButton3.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 2));

            attackButton4.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 3));
            backToUnits.RegisterCallback<ClickEvent>(OnBackToUnits);
        }

        private void OnBackToUnits(ClickEvent e)
        {
            Debug.Log("AHHHHHHHHH");
            unitSelector.ClearSelection();
            Debug.Log(Enum.GetName(typeof(BattleStates),currentState));
            if (currentState == BattleStates.ActionSelection)
            {
                TriggerState(BattleStates.UnitSelection);
                Debug.Log("Back to units");
            } 
        }
        private EventCallback<MouseOverEvent> attackButton1HoverCallback;

        private void UpdateAttackDescription(MouseOverEvent e, int index)
        {
            actionDescription.text = abilityCache.GetAbilities()[index].GetAbilityData().Description;
        }

        private AbilityModule abilityCache;

        public void BeginUnitSelection()
        {
            TriggerState(BattleStates.UnitSelection);
        }

        public IEnumerator NextPhase(int phase_turn_number)
        {
            throw new NotImplementedException();
        }

        public void UpdateView(CombatUnit new_unit, int team_id, int unit_index)
        {
            if (new_unit.TryGetModule(out HealthModule healthbar))
            {
                if (team_id == 0)
                {
                    healthbar.OnHealthChanged += unitSelector.Players[unit_index].UpdateHp;
                }
                else if (team_id == 1)
                {
                    healthbar.OnHealthChanged += unitSelector.Enemies[unit_index].UpdateHp;
                }
            }
        }

        #region StateManagement

        private enum BattleStates
        {
            Setup,
            UnitSelection,
            AffinityTargeting,
            TargetSelection,
            ActionSelection,
            EnemyTurn,
        }

        private BattleStates currentState = BattleStates.Setup;

        private void StartState(BattleStates previous)
        {
            switch (currentState)
            {
                case BattleStates.Setup:
                    StartSetup(previous);
                    break;
                case BattleStates.UnitSelection:
                    StartUnitSelection(previous);
                    break;
                case BattleStates.TargetSelection:
                    StartTargetSelection(previous);
                    break;
                case BattleStates.ActionSelection:
                    StartActionSelection(previous);
                    break;
                case BattleStates.EnemyTurn:
                    StartEnemyTurn(previous);
                    break;
                default:
                    throw new Exception("Invalid battle state: " + currentState);
            }
        }

        private void CleanupState(BattleStates next)
        {
            switch (currentState)
            {
                case BattleStates.Setup:
                    CleanUpSetup(next);
                    break;
                case BattleStates.UnitSelection:
                    CleanUpUnitSelection(next);
                    break;
                case BattleStates.TargetSelection:
                    CleanUpTargetSelection(next);
                    break;
                case BattleStates.ActionSelection:
                    CleanUpActionSelection(next);
                    break;
                case BattleStates.EnemyTurn:
                    CleanUpEnemyTurn(next);
                    break;
                default:
                    throw new Exception("Invalid battle state: " + currentState);
            }
        }

        private BattleStates triggeredState = BattleStates.Setup;

        private void TriggerState(BattleStates next)
        {
            triggeredState = next;
        }

        public void FixedUpdate()
        {
            BattleStates nextState = triggeredState;
            BattleStates oldState = currentState;
            if (nextState != currentState)
            {
                CleanupState(nextState);
                currentState = nextState;
                StartState(oldState);
            }

            switch (currentState)
            {
                case BattleStates.Setup:
                    HandleSetup();
                    break;
                case BattleStates.UnitSelection:
                    HandleUnitSelection();
                    break;
                case BattleStates.TargetSelection:
                    HandleTargetSelection();
                    break;
                case BattleStates.ActionSelection:
                    HandleActionSelection();
                    break;
                case BattleStates.EnemyTurn:
                    HandleEnemyTurn();
                    break;
            }
        }

        #endregion

        #region Setup

        private void StartSetup(BattleStates previous)
        {
        }

        private void HandleSetup()
        {
        }

        private void CleanUpSetup(BattleStates next)
        {
        }

        #endregion

        #region Unit Selection

        private void StartUnitSelection(BattleStates previous)
        {
            unitSelector.PlayerHovered += OnPlayerHovered;
            unitSelector.SelectOne(SelectionFlags.Alive | SelectionFlags.Ally | SelectionFlags.Actionable, SelectUnit);
        }

        /// Intended as a callback function for unit selector. if called when state is not unit selection an exception is thrown
        private void SelectUnit(int team, int unit)
        {
            if (currentState != BattleStates.UnitSelection)
            {
                throw new Exception("Tried calling SelectUnit outside of UnitSelection state");
            }

            if (combatManager.TrySelectUnit(0, team, unit,
                    SelectionFlags.Alive | SelectionFlags.Ally | SelectionFlags.Actionable,
                    out CombatUnit selectedUnit))
            {
                DisplayUnit(selectedUnit);
                selectedPlayer = unit;
                TriggerState(BattleStates.ActionSelection);
            }
            else
            {
                Debug.Log("Unit selection failed likely due to a state mismatch between combat manager and view");
            }
        }

        private void HandleUnitSelection()
        {
        }

        private void CleanUpUnitSelection(BattleStates next)
        {
            unitSelector.PlayerHovered -= OnPlayerHovered;
        }

        #endregion

        #region Target Selection

        private void StartTargetSelection(BattleStates previous)
        {
        }

        private void HandleTargetSelection()
        {
        }

        private void CleanUpTargetSelection(BattleStates next)
        {
        }

        #endregion

        #region ActionSelection

        private int selectedPlayer = -1;

        private void DisplayUnit(CombatUnit selectedUnit)
        {
            Debug.Log($"Displaying unit {selectedUnit}");
            UpdateAbilityCallback(selectedUnit);
            if (selectedUnit.TryGetModule(out HealthModule healthModule))
            {
                //TODO prevent edge cases where this number can go out of date
                playerTotalHp.text = $"{healthModule.CurrentHealth()}/{healthModule.GetMaxHealth()}";
            }

            if (selectedUnit.TryGetModule(out AffinityModule affinityModule))
            {
                switch (affinityModule.GetWeaknessAffinity())
                {
                    case AffinityType.Blue:
                        playerWeaknessIcon.style.backgroundImage = new StyleBackground(battleSprites.waterPlayerWeakness);
                        break;
                        case AffinityType.Red:
                        playerWeaknessIcon.style.backgroundImage =  new StyleBackground(battleSprites.firePlayerWeakness);
                        break;
                        case AffinityType.Green:
                        playerWeaknessIcon.style.backgroundImage = new StyleBackground(battleSprites.physicalPlayerWeakness);
                        break;
                        case AffinityType.Yellow:
                        playerWeaknessIcon.style.backgroundImage =  new StyleBackground(battleSprites.lightningPlayerWeakness);
                        break;
                        case AffinityType.None:
                        break;
                }
            }

            playerCharacterName.text = selectedUnit.GetName();
            if (selectedUnit.TryGetModule(out abilityCache))
            {
                foreach (var ab in abilityCache.GetAbilities())
                {
                    Debug.Log(ab);
                }
                var abilities = abilityCache.GetAbilities();
                attackButton1.text = abilities[0].GetAbilityData().Name;
                attackButton2.text = abilities[1].GetAbilityData().Name;
                attackButton3.text = abilities[2].GetAbilityData().Name;
                attackButton4.text = abilities[3].GetAbilityData().Name;
            }
        }

        private void UpdateAbilityCallback(CombatUnit selectedUnit)
        {
            selectedUnit.TryGetModule(out abilityCache);
            var abilities = abilityCache.GetAbilities();
            // TODO support more than 4 attacks
            for (int i = 0; i < 4; i++)
            {
                int j = i; // prevents captured variable shenanigans 
                attackCallbacks[i] = (mde) => AttackClicked(mde, abilities[j], j);
            }
        }

        private void StartActionSelection(BattleStates previous)
        {
            backToUnits.RegisterCallback<MouseDownEvent>(TriggerUnitSelection);

            attackButton1.RegisterCallback<MouseDownEvent>(attackCallbacks[0]);
            attackButton2.RegisterCallback<MouseDownEvent>(attackCallbacks[1]);
            attackButton3.RegisterCallback<MouseDownEvent>(attackCallbacks[2]);
            attackButton4.RegisterCallback<MouseDownEvent>(attackCallbacks[3]);

            if (selectedPlayer == -1)
            {
                throw new Exception("Something went wrong during unit selection");
            }
        }

        // cache to unregister callbacks
        private readonly EventCallback<MouseDownEvent>[] attackCallbacks = new EventCallback<MouseDownEvent>[4];

        private void AttackClicked(MouseDownEvent e, IAbility ability, int index)
        {
            ActionData actionData = new ActionData();
            actionData.Action = ability;
            //TODO: look at stub combat view for reference target selection will be triggered here which will then trigger affinity selection

            //  actionData.
            // CombatUnit selected_unit = model.GetTeam(0).GetUnit(selectedPlayer); 
            // combatManager.PerformAction(ability.GetAbilityData());
        }

        private void TriggerUnitSelection(MouseDownEvent evt)
        {
            TriggerState(BattleStates.UnitSelection);
        }

        private void HandleActionSelection()
        {
        }

        private void CleanUpActionSelection(BattleStates next)
        {
            selectedPlayer = -1;
            backToUnits.UnregisterCallback<MouseDownEvent>(TriggerUnitSelection);
            attackButton1.UnregisterCallback<MouseDownEvent>(attackCallbacks[0]);
            attackButton2.UnregisterCallback<MouseDownEvent>(attackCallbacks[1]);
            attackButton3.UnregisterCallback<MouseDownEvent>(attackCallbacks[2]);
            attackButton4.UnregisterCallback<MouseDownEvent>(attackCallbacks[3]);
        }

        #endregion

        #region EnemyTurn

        private void StartEnemyTurn(BattleStates previous)
        {
        }

        private void HandleEnemyTurn()
        {
        }

        private void CleanUpEnemyTurn(BattleStates next)
        {
        }

        #endregion
    }
}
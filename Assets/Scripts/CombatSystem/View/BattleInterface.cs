using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private Button confirmButton;
        private Label playerCharacterName;
        private Label playerTotalHp;
        private Button backToUnits;
        private Label actionDescription;
        private ProgressBar enemyHealthbar;

        private IUnitSelector unitSelector;
        [SerializeField]
        private AffinityTargeter affinityTargeter;

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
            DisplayUnit(model.GetTeam(0).GetUnit(index));
        }

        private void OnEnable()
        {
            attackButton1 = ui.Q<Button>("Action1");
            attackButton2 = ui.Q<Button>("Action2");
            attackButton3 = ui.Q<Button>("Action3");
            attackButton4 = ui.Q<Button>("Action4");
            playerCharacterName = ui.Q<Label>("PlayerName");
            playerWeaknessIcon = ui.Q<VisualElement>("PlayerWeaknessIcon");
            playerTotalHp = ui.Q<Label>("PlayerTotalHp");
            backToUnits = ui.Q<Button>("BackToUnits");
            actionDescription = ui.Q<Label>("ActionDescription");
            confirmButton = ui.Q<Button>("Confirm");
            enemyHealthbar = ui.Q<ProgressBar>("EnemyHealthbar");

            attackButton1.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 0));

            attackButton2.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 1));
            attackButton3.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 2));

            attackButton4.RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, 3));
            confirmButton.RegisterCallback<ClickEvent>(ConfirmTargets);
            backToUnits.RegisterCallback<ClickEvent>(OnBackToUnits);
            unitSelector.EnemyHovered += OnEnemyHovered;
            DisplayUnit(model.GetTeam(0).GetUnit(0));
        }

        private int subscribedToEnemy = -1;

        private void UpdateEnemyHealth(int max, int current)
        {
            enemyHealthbar.lowValue = 0;
            enemyHealthbar.highValue = max;
            enemyHealthbar.value = current;
        }
        private void OnEnemyHovered(int index, IUnit unit)
        {
            if (subscribedToEnemy == index)
            {
                return;
            }

            if (subscribedToEnemy != -1)
            {
                if (model.GetTeam(1).GetUnit(subscribedToEnemy).TryGetModule<HealthModule>(out var healthbar))
                {
                    healthbar.OnHealthChanged -= UpdateEnemyHealth;
                }

                if (model.GetTeam(1).GetUnit(subscribedToEnemy).TryGetModule<AffinityBarModule>(out var affinityBar))
                {
                    affinityBar.OnAffinityBarChanged -= affinityTargeter.OnAffinityBarChanged;
                }
            }
            if (model.GetTeam(1).GetUnit(index).TryGetModule<AffinityBarModule>(out var affinityModule))
            {
                affinityTargeter.SetAffinityBar(affinityModule.GetAffinities());
                affinityModule.OnAffinityBarChanged += affinityTargeter.OnAffinityBarChanged;
            }

            if (model.GetTeam(1).GetUnit(index).TryGetModule<HealthModule>(out var healthModule))
            {
                healthModule.OnHealthChanged += UpdateEnemyHealth;
                UpdateEnemyHealth(healthModule.GetMaxHealth(), healthModule.CurrentHealth());
            }

            subscribedToEnemy = index;
        }

        private void OnBackToUnits(ClickEvent e)
        {
            if (currentState.HasFlag(BattleStates.ActionSelection | BattleStates.TargetSelection))
                Debug.Log(Enum.GetName(typeof(BattleStates), currentState));
            if (currentState == BattleStates.ActionSelection)
            {
                TriggerState(BattleStates.UnitSelection);
            }

            switch (currentState)
            {
                case BattleStates.ActionSelection:
                case BattleStates.TargetSelection:
                    TriggerState(BattleStates.UnitSelection);
                    break;
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
            Debug.Log($"Phase change: {phase_turn_number}");

            yield break;
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

        [Flags]
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
                case BattleStates.AffinityTargeting:
                    StartAffinityTargeting(previous);
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
                case BattleStates.AffinityTargeting:
                    CleanUpAffinitySelection();
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
                case BattleStates.EnemyTurn:
                    HandleEnemyTurn();
                    break;
                case BattleStates.UnitSelection:
                case BattleStates.TargetSelection:
                case BattleStates.ActionSelection:
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
            unitSelector.ClearSelection();
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
                Debug.Log("un" + selectedPlayer);
                TriggerState(BattleStates.ActionSelection);
            }
            else
            {
                Debug.Log("Unit selection failed likely due to a state mismatch between combat manager and view");
            }
        }

        private void CleanUpUnitSelection(BattleStates next)
        {
            unitSelector.PlayerHovered -= OnPlayerHovered;
        }

        #endregion

        #region Target Selection

        private ActionData actionData;
        private List<(int team, int unit)> selectedTargets = new();

        private void StartTargetSelection(BattleStates previous)
        {
            attackButton1.style.display = DisplayStyle.None;
            attackButton2.style.display = DisplayStyle.None;
            attackButton3.style.display = DisplayStyle.None;
            attackButton4.style.display = DisplayStyle.None;
            confirmButton.style.display = DisplayStyle.Flex;
            selectedTargets.Clear();
            var abilityData = actionData.Action.GetAbilityData();
            actionData.UserTeamUnitIndex.team_index = 0;
            actionData.UserTeamUnitIndex.unit_index = selectedPlayer;
            foreach (var target in abilityData.RequiredTargets)
            {
                SelectionFlags additionalFlag = target.Key == 0 ? SelectionFlags.Ally : SelectionFlags.Enemy;
                if (target.Value is { min: -1, max: -1 })
                {
                    Debug.Log("min");
                    selectedTargets.AddRange(unitSelector.SelectAll(abilityData.TargetCriteria | additionalFlag));
                }

                for (int i = 0; i < target.Value.min; i++)
                {
                    unitSelector.SelectOne(abilityData.TargetCriteria,
                        (team, unit) => { selectedTargets.Add((team, unit)); });
                }
            }
        }

        private void ConfirmTargets(ClickEvent e)
        {
            if (currentState != BattleStates.TargetSelection) return;
            if (CanPrepAbility(actionData.Action.GetAbilityData(), selectedTargets))
            {
                TriggerState(BattleStates.AffinityTargeting);
            }
        }

        private bool CanPrepAbility(AbilityData data, List<(int team, int unit)> targets)
        {
            foreach (var entry in data.RequiredTargets)
            {
                var (min, max) = entry.Value;
                if (min == -1 && max == -1) continue;
                int count = selectedTargets.Select(pair => pair.team == entry.Key).Count();
                if (count < min || count > max) return false;
            }

            return true;
        }

        private void CleanUpTargetSelection(BattleStates next)
        {
            actionData.TargetIndices = selectedTargets.ToArray();
            unitSelector.ClearRequests();
            selectedTargets.Clear();
            
            attackButton1.style.display = DisplayStyle.Flex;
            attackButton2.style.display = DisplayStyle.Flex;
            attackButton3.style.display = DisplayStyle.Flex;
            attackButton4.style.display = DisplayStyle.Flex;
            confirmButton.style.display = DisplayStyle.None;
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
                        playerWeaknessIcon.style.backgroundImage =
                            new StyleBackground(battleSprites.waterPlayerWeakness);
                        break;
                    case AffinityType.Red:
                        playerWeaknessIcon.style.backgroundImage =
                            new StyleBackground(battleSprites.firePlayerWeakness);
                        break;
                    case AffinityType.Green:
                        playerWeaknessIcon.style.backgroundImage =
                            new StyleBackground(battleSprites.physicalPlayerWeakness);
                        break;
                    case AffinityType.Yellow:
                        playerWeaknessIcon.style.backgroundImage =
                            new StyleBackground(battleSprites.lightningPlayerWeakness);
                        break;
                    case AffinityType.None:
                        break;
                }
            }

            playerCharacterName.text = selectedUnit.GetName();
            if (selectedUnit.TryGetModule(out abilityCache))
            {
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
            backToUnits.RegisterCallback<ClickEvent>(TriggerUnitSelection);

            attackButton1.RegisterCallback<ClickEvent>(attackCallbacks[0]);
            attackButton2.RegisterCallback<ClickEvent>(attackCallbacks[1]);
            attackButton3.RegisterCallback<ClickEvent>(attackCallbacks[2]);
            attackButton4.RegisterCallback<ClickEvent>(attackCallbacks[3]);

            if (selectedPlayer == -1)
            {
                throw new Exception("Something went wrong during unit selection");
            }
        }

        // cache to unregister callbacks
        private readonly EventCallback<ClickEvent>[] attackCallbacks = new EventCallback<ClickEvent>[4];

        private void AttackClicked(ClickEvent e, IAbility ability, int index)
        {
            Debug.Log("Attacked");
            TriggerState(BattleStates.TargetSelection);
            actionData = new ActionData();
            actionData.Action = ability;
            actionData.UserTeamUnitIndex.team_index = 0;
            actionData.UserTeamUnitIndex.unit_index = selectedPlayer;
        }

        private void TriggerUnitSelection(ClickEvent evt)
        {
            TriggerState(BattleStates.UnitSelection);
        }

        private void CleanUpActionSelection(BattleStates next)
        {
            backToUnits.UnregisterCallback<ClickEvent>(TriggerUnitSelection);
            attackButton1.UnregisterCallback<ClickEvent>(attackCallbacks[0]);
            attackButton2.UnregisterCallback<ClickEvent>(attackCallbacks[1]);
            attackButton3.UnregisterCallback<ClickEvent>(attackCallbacks[2]);
            attackButton4.UnregisterCallback<ClickEvent>(attackCallbacks[3]);
        }

        #endregion

        #region AffinityTargeting

        private void StartAffinityTargeting(BattleStates previous)
        {
            if (actionData.Action.GetAbilityData().RequiredMetadata.Count == 0)
            {
                combatManager.PerformAction(actionData);
            }
            else
            {
                Debug.Log("Unsupported attack");
            }
        }

        private void CleanUpAffinitySelection()
        {
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
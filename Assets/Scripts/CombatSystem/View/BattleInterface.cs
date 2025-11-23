using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace CombatSystem.View
{
    public class BattleInterface : MonoBehaviour, ICombatView
    {
        [SerializeField] private CombatDataSO runtimeCombatData;
        [SerializeField] private AttackBannerUI attackBannerReference;
        [SerializeField] private HealthChangeDisplayManager healthChangeDisplayManager;

        [Space]

        // Start is called befomre the first frame update
        public ViewSprites battleSprites;
        private VisualElement ui;
        private VisualElement commandMenu;
        private VisualElement playerWeaknessIcon;
        private CombatManager combatManager;
        private readonly Button[] actions = new Button[4];
        private readonly Button[] options = new Button[4];

        private Button confirmButton;
        private Button fleeButton;
        private Label playerCharacterName;
        private Label playerTotalHp;
        private Button backToUnits;
        private Label actionDescription;
        private ProgressBar enemyHealthbar;
        private VisualElement playerWeaponIcon;
        private VisualElement portrait;
        private VisualElement enemyPortrait;
        private VisualElement enemyInfo;

        StatusUpdater enemyStatusUpdater;

        private bool isPlayerPhase = true; // assume you start combat on your turn

        private IUnitSelector unitSelector;
        [SerializeField] private AffinityTargeter affinityTargeter;

        private CombatUnit GetPlayerUnit(int player_index)
        {
            if (!combatManager.TrySelectUnit(0, 0, player_index, SelectionFlags.Ally, out var unit))
            {
                throw new Exception("Unable to access unit.");
            }

            return unit;
        }

        private CombatUnit GetEnemyUnit(int enemy_index)
        {
            if (!combatManager.TrySelectUnit(0, 1, enemy_index, SelectionFlags.Enemy, out var unit))
            {
                throw new Exception("Unable to access unit.");
            }

            return unit;
        }

        private void Awake()
        {
            ui = GetComponent<UIDocument>().rootVisualElement;
            enemyStatusUpdater = GetComponent<StatusUpdater>();
            combatManager = GetComponent<CombatManager>();
            unitSelector = GetComponent<UnitSelector>();
            InitializeCombat();

        }
        public void InitializeCombat(/*probably take some values here at some point*/)
        {
            if (runtimeCombatData == null || !runtimeCombatData.HasData())
            {
                throw new Exception("Combat data is null or has invalid combat data.");
            }

            var player_unit_sos = runtimeCombatData.PlayerUnits;
            var encounter_so = runtimeCombatData.Encounter;
            for (int i = 0; i < 4; i++)
            {
                unitSelector.Players[i].SetUnit(null);
                unitSelector.Enemies[i].SetUnit(null);
            }


            combatManager.InitCombat(player_unit_sos, encounter_so);
            BeginUnitSelection();
        }

        private void OnSelectablePlayerHovered(int index, IUnit unit)
        {
            commandMenu.RemoveFromClassList("off-screen");
            selectedPlayer = index;
            DisplayUnit(GetPlayerUnit(index));
        }

        private void OnEnable()
        {
            for (int i = 0; i < 4; i++)
            {
                int capturedi = i;
                actions[i] = ui.Q<Button>($"Action{i + 1}");
                actions[i].RegisterCallback<ClickEvent>((ce) =>
                {
                    if (attackCallbacks[capturedi] != null)
                    {
                        attackCallbacks[capturedi](ce);
                    }
                });
                actions[i].RegisterCallback<MouseOverEvent>((moe) => UpdateAttackDescription(moe, capturedi));

                options[i] = ui.Q<Button>($"Option{i + 1}");
                options[i].RegisterCallback<ClickEvent>((ce) =>
                {
                    if (optionCallbacks[capturedi] != null)
                    {
                        optionCallbacks[capturedi](ce);
                    }
                });
            }

            playerCharacterName = ui.Q<Label>("PlayerName");
            playerWeaknessIcon = ui.Q<VisualElement>("PlayerWeaknessIcon");
            playerWeaponIcon = ui.Q<VisualElement>("PlayerWeaponIcon");
            playerTotalHp = ui.Q<Label>("PlayerTotalHp");
            backToUnits = ui.Q<Button>("BackToUnits");
            actionDescription = ui.Q<Label>("ActionDescription");
            confirmButton = ui.Q<Button>("Confirm");
            enemyHealthbar = ui.Q<ProgressBar>("EnemyHealthbar");
            commandMenu = ui.Q<VisualElement>("CommandMenu");
            portrait = ui.Q<VisualElement>("Portrait");
            enemyPortrait = ui.Q<VisualElement>("EnemyPortrait");
            enemyInfo = ui.Q<VisualElement>("EnemyInfo");
            confirmButton.RegisterCallback<ClickEvent>((ce) =>
            {
                if (confirmCallback != null)
                {
                    confirmCallback(ce);
                }
            });
            fleeButton = ui.Q<Button>("Flee");
            fleeButton.RegisterCallback<ClickEvent>((ce) =>
            {
                AttemptFleeCombat();
            });
            backToUnits.RegisterCallback<ClickEvent>(OnBackToUnits);
            unitSelector.SelectableEnemyHovered += OnSelectableEnemyHovered;
            unitSelector.EnemyHovered += OnEnemyHovered;
            // DisplayUnit(model.GetTeam(0).GetUnit(0));
            // DisplayUnit(GetPlayerUnit(0));

        }

        private int subscribedToEnemy = -1;
        private int subscribedToHoverEnemy = -1;

        private void UpdateEnemyHealth(int max, int current, int _)
        {
            enemyHealthbar.lowValue = 0;
            enemyHealthbar.highValue = max;
            enemyHealthbar.value = current;
        }
        private void OnEnemyHovered(int index, IUnit unit)
        {
            if (currentState == BattleStates.TargetSelection || currentState == BattleStates.AffinityTargeting)
            {
                return;
            }
            if (subscribedToHoverEnemy == index)
            {
                return;
            }
            enemyInfo.RemoveFromClassList("off-screen-top");

            OnSelectableEnemyHovered(index, unit);
        }
        private void OnSelectableEnemyHovered(int index, IUnit unit)
        {
            if (subscribedToEnemy == index)
            {
                return;
            }

            enemyInfo.RemoveFromClassList("off-screen-top");
            if (subscribedToEnemy != -1)
            {
                var sub_enemy_unit = GetEnemyUnit(subscribedToEnemy);

                if (sub_enemy_unit.TryGetModule<HealthModule>(out var healthbar))
                {
                    healthbar.OnHealthChanged -= UpdateEnemyHealth;
                }

                if (sub_enemy_unit.TryGetModule<AffinityBarModule>(out var affinityBar))
                {
                    affinityBar.OnAffinityBarChanged -= affinityTargeter.OnAffinityBarChanged;
                }
            }

            var enemy_unit = GetEnemyUnit(index);

            if (enemy_unit.TryGetModule<AffinityBarModule>(out var affinityModule))
            {
                affinityTargeter.SetAffinityBar(affinityModule.GetAffinities());
                affinityModule.OnAffinityBarChanged += affinityTargeter.OnAffinityBarChanged;
            }

            if (enemy_unit.TryGetModule<HealthModule>(out var healthModule))
            {
                healthModule.OnHealthChanged += UpdateEnemyHealth;
                UpdateEnemyHealth(healthModule.GetMaxHealth(), healthModule.CurrentHealth(), 0);
            }
            if (enemy_unit.TryGetModule<ReferenceModule>(out var refModule))
            {
                enemyPortrait.style.backgroundImage = new StyleBackground(refModule.CombatUnit.portrait);
            }

            if (enemy_unit.TryGetModule<StatusModule>(out var statusModule))
            {
                enemyStatusUpdater.Display(statusModule);
            }
            else
            {
                enemyStatusUpdater.Display(null);
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
            // Debug.Log(index + "  " + abilityCache.GetAbilities().Count);
            actionDescription.text = abilityCache.GetAbilities()[index].GetAbilityData().Description;
        }

        private AbilityModule abilityCache;

        public void BeginUnitSelection()
        {
            TriggerState(BattleStates.UnitSelection);
        }

        public IEnumerator NextPhase(int to_phase_number)
        {
            isPlayerPhase = to_phase_number == 0;

            yield return new WaitForSecondsRealtime(1f);

            attackBannerReference.ShowBanner(isPlayerPhase ? "Player Phase!" : "Enemy Phase...", isPlayerPhase ? Color.green : Color.red);

            yield return new WaitForSecondsRealtime(2f);
        }

        public void UpdateView(CombatUnit new_unit, int team_id, int unit_index)
        {
            if (new_unit.TryGetModule(out HealthModule healthbar))
            {
                if (team_id == 0)
                {
                    healthbar.OnHealthChanged += unitSelector.Players[unit_index].UpdateHp;
                    if (new_unit.TryGetModule<ReferenceModule>(out var module))
                    {
                        // Debug.Log(module, module.CombatUnit);
                        unitSelector.Players[unit_index].SetUnit(module.CombatUnit);
                    }
                    else
                    {
                        Debug.Log("could not display unit");
                    }
                }
                else if (team_id == 1)
                {
                    healthbar.OnHealthChanged += unitSelector.Enemies[unit_index].UpdateHp;
                    if (new_unit.TryGetModule<ReferenceModule>(out var module))
                    {

                        Debug.Log(module);
                        unitSelector.Enemies[unit_index].SetUnit(module.CombatUnit);
                    }
                    else
                    {
                        Debug.Log("could not display unit");
                    }
                }

                healthbar.OnHealthChanged += (int _, int _, int diff) => healthChangeDisplayManager.Popup(unit_index, team_id, diff);
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
            if (previous == currentState)
            {
                return;
            }

            switch (currentState)
            {
                case BattleStates.Setup:
                    StartSetup(previous);
                    break;
                case BattleStates.UnitSelection:
                    StartUnitSelection(previous);
                    ShowFleeButton();
                    break;
                case BattleStates.TargetSelection:
                    StartTargetSelection(previous);
                    break;
                case BattleStates.AffinityTargeting:
                    StartAffinityTargeting(previous);
                    break;
                case BattleStates.ActionSelection:
                    StartActionSelection(previous);
                    ShowFleeButton();
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
                    HideFleeButton();
                    break;
                case BattleStates.AffinityTargeting:
                    CleanUpAffinitySelection();
                    break;
                case BattleStates.TargetSelection:
                    CleanUpTargetSelection(next);
                    break;
                case BattleStates.ActionSelection:
                    CleanUpActionSelection(next);
                    HideFleeButton();
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
                case BattleStates.TargetSelection:
                    HandleTargetSelection();
                    break;
                case BattleStates.UnitSelection:
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
            unitSelector.SelectablePlayerHovered += OnSelectablePlayerHovered;
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

        private void CleanUpUnitSelection(BattleStates next)
        {
            unitSelector.SelectablePlayerHovered -= OnSelectablePlayerHovered;
        }

        #endregion

        #region Target Selection

        private ActionData actionData;
        private readonly List<(int team, int unit)> selectedTargets = new();

        private void StartTargetSelection(BattleStates previous)
        {
            HideActionButtons();
            // ShowConfirmButton();
            confirmCallback = (ce) =>
            {
                ConfirmTargets();
                confirmCallback = null;
                HideConfirmButton();
            };
            selectedTargets.Clear();
            var abilityData = actionData.Action.GetAbilityData();
            actionData.UserTeamUnitIndex.team_index = 0;
            actionData.UserTeamUnitIndex.unit_index = selectedPlayer;
            foreach (var target in abilityData.RequiredTargets)
            {
                SelectionFlags additionalFlag = target.Key == 0 ? SelectionFlags.Ally : SelectionFlags.Enemy;
                if (target.Value is { min: -1, max: -1 })
                {
                    selectedTargets.AddRange(unitSelector.SelectAll(abilityData.TargetCriteria | additionalFlag));
                }

                for (int i = 0; i < target.Value.min; i++)
                {
                    unitSelector.SelectOne(abilityData.TargetCriteria,
                        (team, unit) => { selectedTargets.Add((team, unit)); });
                }
            }
        }

        private void HandleTargetSelection()
        {
            if (CanPrepAbility(actionData.Action.GetAbilityData(), selectedTargets, out bool canTakeMore))
            {
                if (!canTakeMore)
                {
                    // ShowConfirmButton();
                    // somewhat feels weird
                    HideConfirmButton();
                    TriggerState(BattleStates.AffinityTargeting);
                }
                else
                {
                    ShowConfirmButton();
                }
            }
            else
            {
                HideConfirmButton();
            }
        }

        private void ConfirmTargets()
        {
            if (currentState != BattleStates.TargetSelection) return;
            if (CanPrepAbility(actionData.Action.GetAbilityData(), selectedTargets, out _))
            {
                TriggerState(BattleStates.AffinityTargeting);
            }
        }

        private bool CanPrepAbility(AbilityData data, List<(int team, int unit)> targets, out bool canTakeMore)
        {
            canTakeMore = false;
            foreach (var entry in data.RequiredTargets)
            {
                var (min, max) = entry.Value;
                if (min == -1 && max == -1) continue;
                int count = selectedTargets.Select(pair => pair.team == entry.Key).Count();
                if (count < min || count > max) return false;
                if (count < max) canTakeMore = true;
            }

            return true;
        }

        private void CleanUpTargetSelection(BattleStates next)
        {
            actionData.TargetIndices = selectedTargets.ToArray();
            unitSelector.ClearRequests();
            selectedTargets.Clear();
            confirmCallback = null;
            ShowActionButtons();
            HideConfirmButton();
        }

        #endregion

        #region ActionSelection

        private int selectedPlayer = -1;

        private void DisplayUnit(CombatUnit selectedUnit)
        {
            // Debug.Log($"Displaying unit {selectedUnit}");
            ShowActionButtons();
            UpdateAbilityCallback(selectedUnit);
            actionDescription.text = "";
            if (selectedUnit.TryGetModule(out HealthModule healthModule))
            {
                //TODO prevent edge cases where this number can go out of date
                playerTotalHp.text = $"{healthModule.CurrentHealth()}/{healthModule.GetMaxHealth()}";
            }

            if (selectedUnit.TryGetModule(out AffinityModule affinityModule))
            {
                void SetIcon(VisualElement ui, AffinityType affinity)
                {
                    switch (affinity)
                    {
                        case AffinityType.Water:
                            ui.style.backgroundImage =
                                new StyleBackground(battleSprites.waterPlayerWeakness);
                            break;
                        case AffinityType.Fire:
                            ui.style.backgroundImage =
                                new StyleBackground(battleSprites.firePlayerWeakness);
                            break;
                        case AffinityType.Physical:
                            ui.style.backgroundImage =
                                new StyleBackground(battleSprites.physicalPlayerWeakness);
                            break;
                        case AffinityType.Lightning:
                            ui.style.backgroundImage =
                                new StyleBackground(battleSprites.lightningPlayerWeakness);
                            break;
                        case AffinityType.None:
                            break;
                    }
                }

                SetIcon(playerWeaknessIcon, affinityModule.GetWeaknessAffinity());
                SetIcon(playerWeaponIcon, affinityModule.GetWeaponAffinity());
                if (selectedUnit.TryGetModule<ReferenceModule>(out var module))
                {
                    portrait.style.backgroundImage = new StyleBackground(module.CombatUnit.portrait);
                }
                //TODO: listend to change callbacks
            }


            playerCharacterName.text = selectedUnit.GetName();
            if (selectedUnit.TryGetModule(out abilityCache))
            {
                var abilities = abilityCache.GetAbilities();
                for (int i = 0; i < 4; i++)
                {
                    actions[i].text = abilities[i].GetAbilityData().Name;
                }
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
                switch (currentState)
                {
                    case BattleStates.UnitSelection:
                        attackCallbacks[i] = (mde) =>
                        {
                            unitSelector.ManualSelect(0, selectedPlayer);
                            AttackClicked(mde, abilities[j], j);
                        };
                        break;
                    case BattleStates.ActionSelection:
                        attackCallbacks[i] = (mde) => AttackClicked(mde, abilities[j], j);
                        break;
                }
            }
        }

        private void StartActionSelection(BattleStates previous)
        {
            backToUnits.RegisterCallback<ClickEvent>(TriggerUnitSelection);

            if (selectedPlayer == -1)
            {
                throw new Exception("Something went wrong during unit selection");
            }
        }

        /// <summary>
        ///  Set attackCallbacks[i] to null to remove callback
        /// </summary>
        private readonly EventCallback<ClickEvent>[] attackCallbacks = new EventCallback<ClickEvent>[4];

        /// <summary>
        /// Set optionCallbacks[i] to null to remove callback
        /// </summary>
        private readonly EventCallback<ClickEvent>[] optionCallbacks = new EventCallback<ClickEvent>[4];

        [CanBeNull] private EventCallback<ClickEvent> confirmCallback = null;

        private void AttackClicked(ClickEvent e, IAbility ability, int index)
        {
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
            for (int i = 0; i < 4; i++)
            {
                attackCallbacks[i] = null;
            }
        }

        #endregion

        #region AffinityTargeting

        private int metadata_index;

        private void StartAffinityTargeting(BattleStates previous)
        {
            metadata_index = -1;
            HideActionButtons();
            actionData.ActionMetadata = new Dictionary<string, string>();
            NextMetadata();
        }

        private void NextMetadata()
        {
            metadata_index++;
            HideOptionButtons();
            if (metadata_index >= actionData.Action.GetAbilityData().RequiredMetadata.Count)
            {
                Debug.Log("Performing Action");
                combatManager.PerformAction(actionData);
                return;
            }

            for (int i = 0; i < selectedTargets.Count; i++)
            {
                Debug.Log(selectedTargets[i]);
            }

            var meta = actionData.Action.GetAbilityData().RequiredMetadata[metadata_index];
            switch (meta)
            {
                case MetadataConstants.AFF_INDEX_TARGET_INDEX:
                    affinityTargeter.SelectOne((int index) =>
                    {
                        switch (actionData.TargetIndices.Length)
                        {
                            case 1:
                                actionData.AddToMetadata(meta,
                                    AbilityUtils.MakeAffinityIndexTargetIndexString(index,
                                        (actionData.TargetIndices[0])));
                                NextMetadata();
                                break;
                            default:
                                TriggerState(BattleStates.UnitSelection);
                                throw new Exception(
                                    $"Do not know how to handle AFF_INDEX_TARGET_INDEX for {selectedTargets.Count} players returning to unit selection");
                        }
                    }, IAffinityTargeter.All);
                    break;

                case MetadataConstants.OPTIONAL_AITI:
                    ShowConfirmButton();
                    confirmCallback = (ce) =>
                    {
                        HideConfirmButton();
                        affinityTargeter.CancelRequests();
                        confirmCallback = null;
                        HideConfirmButton();
                        NextMetadata();
                    };
                    IAffinityTargeter.SelectedOne request_callback = (int index) =>
                    {
                        switch (actionData.TargetIndices.Length)
                        {
                            case 0:
                                NextMetadata();
                                break;
                            case 1:
                                actionData.AddToMetadata(meta,
                                    AbilityUtils.MakeAffinityIndexTargetIndexString(index,
                                        (actionData.TargetIndices[0])));
                                NextMetadata();
                                break;
                            default:
                                TriggerState(BattleStates.UnitSelection);
                                throw new Exception(
                                    $"Do not know how to handle OPTIONAL_AITI for {selectedTargets.Count} players returning to unit selection");
                        }
                    };
                    affinityTargeter.SelectOne(request_callback, IAffinityTargeter.All);
                    break;
                case MetadataConstants.WEAPON_ELEMENT:
                    ShowOptionButtons(4);
                    int index = 0;
                    foreach (var affinity in Enum.GetValues(typeof(AffinityType)))
                    {
                        switch (affinity)
                        {
                            case AffinityType.Fire:
                            case AffinityType.Water:
                            case AffinityType.Lightning:
                            case AffinityType.Physical:
                                options[index].text = Enum.GetName(typeof(AffinityType), affinity);
                                optionCallbacks[index] = (ce) =>
                                {
                                    actionData.AddToMetadata(meta, Enum.GetName(typeof(AffinityType), affinity));
                                    NextMetadata();
                                };
                                index++;
                                break;
                        }
                    }

                    break;
                case MetadataConstants.WEAPON_OR_WEAKNESS:
                    ShowOptionButtons(2);
                    options[0].text = "Weapon";
                    optionCallbacks[0] = (ce) =>
                    {
                        actionData.AddToMetadata(meta, MetadataConstants.WEAPON);
                        NextMetadata();
                    };
                    options[1].text = "Weakness";
                    optionCallbacks[1] = (ce) =>
                    {
                        actionData.AddToMetadata(meta, MetadataConstants.WEAKNESS);
                        NextMetadata();
                    };
                    break;
                case MetadataConstants.WEAKNESS:
                case MetadataConstants.WEAPON:
                case MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX:
                    affinityTargeter.SelectPair((int index) =>
                    {
                        switch (actionData.TargetIndices.Length)
                        {
                            case 1:
                                actionData.AddToMetadata(meta,
                                    AbilityUtils.MakeAffinityIndexTargetIndexString(index,
                                        (actionData.TargetIndices[0])));
                                NextMetadata();
                                break;
                            default:
                                TriggerState(BattleStates.UnitSelection);
                                throw new Exception(
                                    $"Do not know how to handle PAIR_AFF_INDEX_TARGET_INDEX for {selectedTargets.Count} players returning to unit selection");
                        }
                    }, IAffinityTargeter.AllWithoutLast);
                    break;
            }
        }

        private void HandleAffinityTargeting()
        {
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

        private void HideActionButtons()
        {
            foreach (var action in actions)
            {
                action.style.display = DisplayStyle.None;
            }
        }

        private void ShowActionButtons()
        {
            foreach (var action in actions)
            {
                action.style.display = DisplayStyle.Flex;
            }
        }

        private void HideOptionButtons()
        {
            foreach (var option in options)
            {
                option.style.display = DisplayStyle.None;
            }
        }

        private void ShowOptionButtons(int num)
        {
            for (int i = 0; i < num && i < options.Length; i++)
            {
                options[i].style.display = DisplayStyle.Flex;
            }
        }

        private void HideConfirmButton()
        {
            confirmButton.style.display = DisplayStyle.None;
        }

        private void ShowConfirmButton()
        {
            confirmButton.style.display = DisplayStyle.Flex;
        }

        private void HideFleeButton()
        {
            fleeButton.style.display = DisplayStyle.None;
        }

        private void ShowFleeButton()
        {
            fleeButton.style.display = DisplayStyle.Flex;
        }

        private void AttemptFleeCombat()
        {
            HideFleeButton();

            // you can only flee on your turn
            if (!isPlayerPhase) return;

            var flee_data = new ActionData
            {
                Action = new System_FleeAbility(),
                UserTeamUnitIndex = (0, 0), // always assume player team is fleeing
                TargetIndices = null, // sources targets from user team
                ActionMetadata = null // no metadata
            };

            combatManager.PerformAction(flee_data);
        }
    }
}
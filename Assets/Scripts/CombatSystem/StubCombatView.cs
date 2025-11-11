using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StubCombatView : MonoBehaviour, ICombatView
{
    [SerializeField] private CombatDataSO m_runtimeCombatData;

    [Space]

    [SerializeField] private CombatTestingScript m_unitViewPrefab;
    [SerializeField] private BrainSO m_simpleEnemyCPUBrain;
    [SerializeField] private Transform m_playerRow;
    [SerializeField] private Transform m_enemyRow;

    [Space(10)] [SerializeField] private CombatManager m_manager;
    [SerializeField] private InputField m_dataField;

    private string m_data;
    private bool m_hasData;

    private int m_selectedUserIndexWithinTeam = -1; // TODO, stateful but bad. Getting index from Model needs a rework.

    private void Awake()
    {
        m_dataField.onSubmit.AddListener(UpdateData);

        m_manager.InitCombat(m_runtimeCombatData.PlayerUnits, m_runtimeCombatData.Encounter);
        BeginUnitSelection();
    }

    public void UpdateData(string data)
    {
        m_data = data;
        m_hasData = data != string.Empty;
    }

    public void UpdateView(CombatUnit new_unit, int team_id, int unit_index)
    {
        var instance = GameObject.Instantiate(m_unitViewPrefab);

        var parent = team_id == 0 ? m_playerRow : m_enemyRow;
        instance.transform.SetParent(parent);

        instance.MakeNew(new_unit, team_id, unit_index);
    }

    public void BeginUnitSelection()
    {
        StartCoroutine(IE_SelectUnitProcess());
    }

    private IEnumerator IE_SelectUnitProcess()
    {
        while (true)
        {
            Debug.Log("Input unit index.");

            yield return new WaitUntil(() => m_hasData);

            // mark data as consumed
            m_hasData = false;

            // test to see if input was good.
            var flag_criteria = SelectionFlags.Ally | SelectionFlags.Actionable | SelectionFlags.Alive;
            if (int.TryParse(m_data, out int result) && result >= 0 && result < 4
                && m_manager.TrySelectUnit(0, 0, result, flag_criteria, out var selected))
            {
                // we're on to the next phase.
                m_selectedUserIndexWithinTeam = result;

                ProcessUnit(selected);
                yield break;
            }

            // if not, repeat
            Debug.Log("Invalid index.");
        }

        // DESIGN NOTE: Handling "back" inputs between signal states? E.g. going back after selecting a unit.
    }

    public IEnumerator NextPhase(int phase_turn_number)
    {
        Debug.Log($"Next Phase stub. Turn count is {phase_turn_number}.");

        yield return null;
    }

    private void ProcessUnit(CombatUnit selected_unit)
    {
        var builder = new StringBuilder("Unit Processed:\n");
        if (selected_unit.TryGetModule<AbilityModule>(out var module))
        {
            var abilities = module.GetAbilities();

            int index = 0;
            foreach (var ability in abilities)
            {
                builder.Append($"{index++}: {ability.GetAbilityData().Name}, ");
            }

            builder.Remove(builder.Length - 2, 2);

            Debug.Log(builder.ToString());
            StartCoroutine(IE_TakePlayerTurn(abilities));
        }
        else
        {
            Debug.Log("No abilites.");
        }
    }

    private IEnumerator IE_TakePlayerTurn(IReadOnlyList<IAbility> abilities)
    {
        int chosen_move = -1; // -1 is invalid, -2 is item usage
        while (chosen_move == -1)
        {
            Debug.Log("Select Ability by Index or type \"item\" to use an item.");

            yield return new WaitUntil(() => m_hasData);

            m_hasData = false;

            // handle item case
            if (m_data.ToLower() == "item")
            {
                chosen_move = -2;
            }
            // if selecting a move failed, invalidate it.
            else if (!(int.TryParse(m_data, out chosen_move) && chosen_move >= 0 && chosen_move < abilities.Count))
            {
                chosen_move = -1;

                Debug.Log("Invalid input.");
            }
        }

        // process if using inventory access
        IAbility ability;
        if (chosen_move != -2) ability = abilities[chosen_move];
        else
        {
            // print all items
            var inventory_contents = InventorySingleton.Instance.ViewItems();
            int max_index = inventory_contents.Count;

            var builder = new StringBuilder();
            int index = 0;
            foreach (var item in inventory_contents)
                builder.Append(index++).Append(": ").Append(item.GetAbilityData().Name).Append(", ");
            builder.Remove(builder.Length - 2, 2);

            Debug.Log(builder.ToString());

            int chosen_item_index = -1;
            while (chosen_item_index == -1)
            {
                Debug.Log("Select Item by index.");

                yield return new WaitUntil(() => m_hasData);

                m_hasData = false;

                // if selecting a move failed, invalidate it.
                if (!(int.TryParse(m_data, out chosen_item_index) && chosen_item_index >= 0 &&
                      chosen_item_index < max_index))
                {
                    chosen_item_index = -1;

                    Debug.Log("Invalid input.");
                }
            }

            ability = new System_UseItemAbility(InventorySingleton.Instance.ConsumeItemAtIndex(chosen_item_index));
        }

        // ACTION DATA SETUP
        var action_data = new ActionData();
        action_data.Action = ability;
        action_data.UserTeamUnitIndex = (0, m_selectedUserIndexWithinTeam); // TODO, see comment on class field.

        // TARGET INDICES
        var targets = new List<(int team, int unit)>();
        while (true)
        {
            bool is_ready = CanPrepAbility(ability.GetAbilityData(), targets, out bool can_take_more);

            if (is_ready && !can_take_more)
            {
                Debug.Log($"Press 'y' to confirm {ability.GetAbilityData().Name} on {string.Join(" & ", targets)}.");
            }
            else if (is_ready && can_take_more)
            {
                Debug.Log($"Ability is prepped, but you may continue selecting units.");
                Debug.Log(
                    "Select Target(s) by Inputting \"Team_Index, Unit_Index\", or 'y' to confirm current selection.");
            }
            else
            {
                Debug.Log(
                    "Select Target(s) by Inputting \"Team_Index, Unit_Index\", or 'y' to confirm current selection.");
            }

            yield return new WaitUntil(() => m_hasData);

            m_hasData = false;

            if (m_data.ToLower() == "y" && is_ready)
            {
                Debug.Log("Continuing...");
                break;
            }
            else if (m_data.ToLower() == "y" && !is_ready)
            {
                Debug.LogError("Valid targets have not been selected.");
                continue;
            }

            string[] split = m_data.Split(", ");

            if (split.Length != 2)
            {
                Debug.Log("Invalid format.");
                continue;
            }

            // determine if a target is valid by if it is in the pool of units we can target (determined by
            // AbilityData.SelectionFlags)
            (int team, int unit) pairing = default;
            var flags = ability.GetAbilityData().TargetCriteria;

            if (!(int.TryParse(split[0], out pairing.team) && int.TryParse(split[1], out pairing.unit)))
            {
                Debug.Log("Invalid format.");
                continue;
            }

            if (!m_manager.TrySelectUnit(0, pairing.team, pairing.unit, flags, out var _))
            {
                Debug.Log("Invalid Unit.");
                continue;
            }

            targets.Add(pairing);
        }

        // checking sweep-target collection
        var data = ability.GetAbilityData();
        foreach (var entry in data.RequiredTargets)
        {
            // if we have a sweep-target (-1, -1), fill it before moving on.
            // NOTE: This does not stop duplicate targeting entries
            var (min, max) = entry.Value;
            if (min == max && min == -1)
            {
                FillTargets(entry.Key, data.TargetCriteria, targets);
            }
        }

        (int, int)[] target_indices = new (int, int)[targets.Count];
        for (int i = 0; i < targets.Count; i++) target_indices[i] = targets[i];

        action_data.TargetIndices = target_indices;

        // METADATA
        var metadata = ability.GetAbilityData().RequiredMetadata;
        action_data.ActionMetadata = new Dictionary<string, string>();

        for (int i = 0; i < metadata.Count; i++)
        {
            Debug.Log("Handling metadata data for key entry: " + metadata[i]);

            bool break_loop;
            switch (metadata[i])
            {
                case MetadataConstants.WEAPON_ELEMENT:
                    break_loop = false;
                    while (!break_loop)
                    {
                        Debug.Log("Select Affinity for Weapon Element.");
                        Debug.Log("Fire, Lightning, Water, Physical.");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;
                        if (Enum.TryParse(m_data, true, out AffinityType value))
                        {
                            action_data.AddToMetadata(metadata[i], Enum.GetName(typeof(AffinityType), value));
                            break_loop = true;
                        }
                        else
                        {
                            Debug.Log("Invalid input.");
                            continue;
                        }
                    }

                    break;

                case MetadataConstants.WEAPON_OR_WEAKNESS:
                    break_loop = false;
                    while (!break_loop)
                    {
                        Debug.Log("Select Weapon Element or Weakness Element to modify.");
                        Debug.Log("weapon, weakness.");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;

                        switch (m_data.ToLower())
                        {
                            case "weapon":
                            case "weakness":
                                action_data.AddToMetadata(metadata[i], m_data.ToLower());
                                break_loop = true;
                                break;

                            default:
                                Debug.Log("Invalid Input.");
                                continue;
                        }
                    }

                    break;

                case MetadataConstants.OPTIONAL_AITI:
                case MetadataConstants.AFF_INDEX_TARGET_INDEX:
                    bool is_optional = metadata[i] == MetadataConstants.OPTIONAL_AITI;

                    break_loop = false;
                    while (!break_loop)
                    {
                        if (!is_optional) Debug.Log("Input target indices and element index.");
                        else Debug.Log("Input target indices and element index, or \"n\" to finalize.");
                        Debug.Log(
                            "e.g. 1, 0, 0 = target team index, target unit index, 0-indexed element index in bar");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;

                        if (is_optional && m_data.ToLower() == "n")
                        {
                            // not needed, but semantically similar
                            // break_loop = true;
                            break;
                        }

                        string[] string_data = m_data.Split(", ");

                        try
                        {
                            if (string_data.Length != 3)
                                throw new Exception("Invalid number of data entries: " + string_data.Length);

                            int team_index = int.Parse(string_data[0]);
                            int unit_index = int.Parse(string_data[1]);
                            int aff_index = int.Parse(string_data[2]);

                            // if trying to target a unit not in the selection scope, ensure valid targeting
                            if (!action_data.TargetIndices.Contains((team_index, unit_index)))
                                throw new Exception("Target not in selected targets.");

                            // if trying to target OoB unit, ensure valid indices
                            if (!m_manager.TrySelectUnit(0, team_index, unit_index,
                                    SelectionFlags.Enemy | SelectionFlags.Ally, out var unit))
                                throw new Exception("Unable to select unit.");

                            // if OoB or missing a bar module, ensure valid selection
                            if (!(unit.TryGetModule<AffinityBarModule>(out var module)
                                  && aff_index >= module.GetFirstNonNoneIndex()
                                  && aff_index < module.BarLength()))
                                throw new Exception("Affinity index OoB or no bar module on target.");

                            // if the specific target is already added to the metadata key, ensure uniqueness
                            string aiti_string =
                                AbilityUtils.MakeAffinityIndexTargetIndexString(aff_index, (team_index, unit_index));
                            if ((action_data.ActionMetadata.TryGetValue(MetadataConstants.AFF_INDEX_TARGET_INDEX,
                                     out string value)
                                 && value == aiti_string)
                                || (action_data.ActionMetadata.TryGetValue(MetadataConstants.OPTIONAL_AITI,
                                        out string o_value)
                                    && o_value == aiti_string))
                                throw new Exception(
                                    "String already exists in AITI metadata key. Choose a unique target.");


                            // all conditions pass? good to go.
                            action_data.AddToMetadata(
                                is_optional
                                    ? MetadataConstants.OPTIONAL_AITI
                                    : MetadataConstants.AFF_INDEX_TARGET_INDEX,
                                aiti_string);

                            break_loop = true;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Invalid input: " + e.Message);
                            continue;
                        }
                    }

                    break;

                case MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX:
                    break_loop = false;
                    while (!break_loop)
                    {
                        Debug.Log("Input target indices and element-pair index.");
                        Debug.Log(
                            "e.g. 1, 0, 0 = target team index, target unit index, 0-indexed left-element index in bar");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;

                        string[] string_data = m_data.Split(", ");

                        try
                        {
                            if (string_data.Length != 3)
                                throw new Exception("Invalid number of data entries: " + string_data.Length);

                            int team_index = int.Parse(string_data[0]);
                            int unit_index = int.Parse(string_data[1]);
                            int aff_index = int.Parse(string_data[2]);

                            // if trying to target a unit not in the selection scope, ensure valid targeting
                            if (!action_data.TargetIndices.Contains((team_index, unit_index)))
                                throw new Exception("Target not in selected targets.");

                            // if trying to target OoB unit, ensure valid indices
                            if (!m_manager.TrySelectUnit(0, team_index, unit_index,
                                    SelectionFlags.Enemy | SelectionFlags.Ally, out var unit))
                                throw new Exception("Unable to select unit.");

                            // if OoB or missing a bar module, ensure valid selection
                            if (!(unit.TryGetModule<AffinityBarModule>(out var module)
                                  && aff_index >= module.GetFirstNonNoneIndex()
                                  && aff_index + 1 < module.BarLength()))
                                throw new Exception("Affinity index OoB or no bar module on target.");

                            // if the specific target is already added to the metadata key, ensure uniqueness
                            if (!AssertPairNoOverlap(aff_index, action_data.ActionMetadata))
                            {
                                throw new Exception("Index creates overlapping pair element zone.");
                            }


                            // all conditions pass? good to go.
                            action_data.AddToMetadata(
                                MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX,
                                AbilityUtils.MakeAffinityIndexTargetIndexString(aff_index, (team_index, unit_index)));

                            break_loop = true;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Invalid input: " + e.Message);
                            continue;
                        }
                    }

                    break;

                // removed Metadata examples at bottom of script

                default:
                    Debug.Log($"Metadata {metadata[i]} is not implemented.");
                    break;
            }
        }

        Debug.Log("Activating ability...");
        m_manager.PerformAction(action_data);
    }

    private bool AssertPairNoOverlap(int new_element_index, Dictionary<string, string> current_metadata)
    {
        if (current_metadata.TryGetValue(MetadataConstants.PAIR_AFF_INDEX_TARGET_INDEX, out string value))
        {
            (int _, int _, int index) = AbilityUtils.ParseAffinityIndexTargetIndexString(value);

            return new_element_index != index && new_element_index - 1 != index && new_element_index + 1 != index;
        }

        return true;
    }

    private void FillTargets(int team_number, SelectionFlags flags, IList<(int, int)> targets)
    {
        int unit_index = 0;
        while (true)
        {
            try
            {
                // get all valid units on the team until we go OoB
                if (m_manager.TrySelectUnit(0, team_number, unit_index++, flags, out var unit))
                {
                    targets.Add(unit.GetIndices());
                }
            }
            catch (IndexOutOfRangeException)
            {
                break;
            }
        }
    }

    // this is fine since player-perspective is 0 = allies and 1 = enemies
    // enemies might need to "flip" this logic
    private bool CanPrepAbility(AbilityData data, List<(int team, int unit)> targets, out bool can_take_more)
    {
        can_take_more = false;
        foreach (var entry in data.RequiredTargets)
        {
            int team_id = entry.Key;
            var (min, max) = entry.Value;

            // if "target all viable," exit early because we'll be filling it automatically
            if (min == -1 && max == -1) return true;

            int count = targets.Select(pair => pair.team == team_id).Count();

            if (count < min || count > max) return false;
            if (count < max) can_take_more = true;
        }

        return true;
    }
}

/*
 * case "COLOR_TYPE":
                    string affinity = string.Empty;
                    while (affinity == string.Empty)
                    {
                        Debug.Log("Select Affinity to Change by Name.");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;

                        switch (m_data.ToLower())
                        {
                            case "red":
                            case "blue":
                            case "yellow":
                            case "green":
                                action_data.ActionMetadata.Add(metadata[i], m_data.ToLower());
                                affinity = m_data.ToLower();
                                break;

                            default:
                                Debug.Log("Invalid Input.");
                                continue;
                        }
                    }

                    break;

                case "NUMBER_OF_LEADING":
                    int num = -1;
                    while (num == -1)
                    {
                        Debug.Log("Input Leading Number Count [1-3].");

                        yield return new WaitUntil(() => m_hasData);

                        m_hasData = false;

                        if (!(int.TryParse(m_data, out num) && num > 0 && num <= 3))
                        {
                            Debug.Log("Invalid input.");
                            num = -1;
                            continue;
                        }

                        action_data.ActionMetadata.Add(metadata[i], num.ToString());
                    }

                    break;
*/
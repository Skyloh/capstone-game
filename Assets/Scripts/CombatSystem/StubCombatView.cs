using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StubCombatView : MonoBehaviour, ICombatView
{
    [SerializeField] private CombatManager m_manager;
    [SerializeField] private InputField m_dataField;

    private string m_data;
    private bool m_hasData;

    private int m_selectedUserIndexWithinTeam = -1; // TODO, stateful but bad. Getting index from Model needs a rework.

    private void Awake()
    {
        m_dataField.onSubmit.AddListener(UpdateData);

        m_manager.InitCombat();
        BeginUnitSelection();
    }

    public void UpdateData(string data) 
    {
        m_data = data;
        m_hasData = data != string.Empty;
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
                && m_manager.TrySelectUnit(0, result, flag_criteria, out var selected))
            {
                // we're on to the next phase.
                m_selectedUserIndexWithinTeam = result;

                ProcessUnit(selected); // NOTE: remove this from the interface?
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

    public void ProcessUnit(CombatUnit selected_unit)
    {
        var builder = new StringBuilder("Unit Processed:\n");
        if (selected_unit.TryGetModule<AbilityModule>(out var module))
        {
            var abilities = module.GetAbilities();

            for (int i = 0; i < abilities.Count; ++i)
            {
                builder.AppendLine($"{i}: {abilities[i].GetAbilityData().Name}");
            }

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
        int chosen_move = -1;
        while (chosen_move == -1)
        {
            Debug.Log("Select Ability by Index.");

            yield return new WaitUntil(() => m_hasData);

            m_hasData = false;

            // if selecting a move failed, invalidate it.
            if (!(int.TryParse(m_data, out chosen_move) && chosen_move >= 0 && chosen_move < abilities.Count))
            {
                chosen_move = -1;

                Debug.Log("Invalid input.");
            }
        }

        var ability = abilities[chosen_move];

        // ACTION DATA SETUP
        var action_data = new ActionData();
        action_data.Action = ability;
        action_data.UserTeamUnitIndex = (0, m_selectedUserIndexWithinTeam); // TODO, see comment on class field.

        // TARGET INDICES
        var targets = new List<(int team, int unit)>();
        while (!ability.CanPrepAbility(targets))
        {
            Debug.Log("Select Target(s) by Inputting \"Team_Index, Unit_Index\".");

            yield return new WaitUntil(() => m_hasData);

            m_hasData = false;

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

            if (!m_manager.TrySelectUnit(pairing.team, pairing.unit, flags, out var _))
            {
                Debug.Log("Invalid Unit.");
                continue;
            }

            targets.Add(pairing);
        }

        (int, int)[] target_indices = new (int, int)[targets.Count];
        for (int i = 0; i < targets.Count; i++) target_indices[i] = targets[i];

        action_data.TargetIndices = target_indices;

        // METADATA
        var metadata = ability.GetAbilityData().RequiredMetadata;
        action_data.ActionMetadata = new Dictionary<string, string>();

        for (int i = 0; i < metadata.Count; i++) 
        {
            switch (metadata[i])
            {
                case "COLOR_TYPE":
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

                default:
                    Debug.Log($"Metadata {metadata[i]} is not implemented.");
                    break;
            }
        }

        m_manager.PerformAction(action_data);
    }


}

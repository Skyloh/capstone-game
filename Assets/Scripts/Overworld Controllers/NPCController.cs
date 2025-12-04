using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TNRD;

public class NPCController : MonoBehaviour
{
    public Vector3Int currentPos;
    [SerializeField]
    private Tilemap collisionTileMap;

    public string npcName = "Test NPC";

    [SerializeField]
    private TextAsset dialogueFile;

    [SerializeField]
    private string dialogueID;

    [SerializeField]
    private bool playMultipleTimes = false;

    [Header("Postinteraction Stuff")]
    [SerializeField] private ParticleSystem m_system;
    [SerializeField] private SerializableInterface<IAbility> m_rewardedAbility;
    [SerializeField] private EncounterSO m_initiatedCombat;

    // Start is called before the first frame update
    void Start()
    {
        Vector3Int cell = collisionTileMap.WorldToCell(transform.position);
        transform.position = collisionTileMap.GetCellCenterWorld(cell);

        currentPos = cell;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = Vector2.one;
            col.offset = Vector2.zero;
        }

        if (!playMultipleTimes && DialogueManager.HasDialogueBeenPlayed(dialogueID))
        {
            m_system.Stop();
        }
    }

    // If the player interacts with this NPC, start dialogue
    public void Interact()
    {
        if (!playMultipleTimes && DialogueManager.HasDialogueBeenPlayed(dialogueID))
        {
            return; 
        }

        //Debug.Log("I am interacted with");

        DialogueManager.OnDialogueComplete += PostDialogue; // exit dialogue wipes this action, so no need to desub

        DialogueManager.GetInstance().EnterDialogueMode(dialogueFile, dialogueID);
    }

    private void PostDialogue()
    {
        if (m_rewardedAbility.Value != null)
        {
            InventorySingleton.Instance.AddItem(AbilityFactory.MakeAbility(m_rewardedAbility.Value.GetType().Name));
        }

        if (m_initiatedCombat != null)
        {
            var combat_kickoff = FindFirstObjectByType<CombatZoneManager>();
            if (combat_kickoff == null)
            {
                Debug.LogError("CombatZoneManager not found in scene!");
            }

            combat_kickoff.StartCombat(m_initiatedCombat);
        }

        if (!playMultipleTimes)
        {
            m_system.Stop();
        }
    }

}

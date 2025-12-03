using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    // private static PlayerSpawnManager instance;

    public static string nextSpawnPointID;

    private static Vector3 combatReturnPosition;
    private static string combatReturnScene;
    private static bool returningFromCombat = false;

    [SerializeField]
    private SpawnPoint[] spawnPoints;

    // because of how we separate scene loading and scene activating, this has a chance 
    // to get subscribed before scene activation.
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene scene, Scene to_scene)
    {
        // if the scene is a transition scene, then we don't need to perform the player spawn functionality
        if (to_scene.name.ToLower().Contains("transition"))
        {
            Debug.Log("Scene change triggered on transition. Ignoring...");
        }

        SpawnPlayer();
    }
    

    private void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("No player found");
            return;
        }

        // Check if returning from combat
        if (returningFromCombat)
        {
            player.transform.position = combatReturnPosition;
            Debug.Log($"Spawning player at combat return position: {combatReturnPosition}");
            returningFromCombat = false;
            return;
        }

        // Otherwise use spawn points as normal
        if (string.IsNullOrEmpty(nextSpawnPointID))
        {
            Debug.Log("No spawn point ID set, using player's current position");
            return;
        }

        foreach (SpawnPoint sp in spawnPoints)
        {
            if (sp.spawnPointID == nextSpawnPointID)
            {
                player.transform.position = sp.transform.position;

                Debug.Log($"Spawning player at spawn point: {nextSpawnPointID}");
                nextSpawnPointID = null;
                return;
            }
        }

        Debug.LogWarning($"Spawn point '{nextSpawnPointID}' not found");
        nextSpawnPointID = null;
    }

    // Call this when entering combat to save the return position
    public static void SetCombatReturnPoint(Vector3 position, string sceneName)
    {
        combatReturnPosition = position;
        combatReturnScene = sceneName;
        returningFromCombat = false; // Not yet returning, just saving
        Debug.Log($"Combat return point set: {position} in scene {sceneName}");
    }

    // Call this when leaving combat to trigger the return spawn
    public static void ReturnFromCombat()
    {
        returningFromCombat = true;
        Debug.Log("Marked as returning from combat");
    }

    // Get the scene to return to
    public static string GetCombatReturnScene()
    {
        return combatReturnScene;
    }
}
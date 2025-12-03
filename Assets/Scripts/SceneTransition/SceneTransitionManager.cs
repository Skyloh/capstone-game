using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    private bool m_isInTransition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        m_isInTransition = false;
    }

    /// <summary>
    /// Change to the given scene name, using the transition mode given. Does not transition if one is in progress.
    /// </summary>
    /// <param name="to_scene"></param>
    /// <param name="type"></param>
    public static void Transition(string to_scene, TransitionType type)
    {
        if (Instance.m_isInTransition) return;

        Instance.StartCoroutine(Instance.IE_Transition(to_scene, Instance.MatchTransitionType(type)));
    }

    private IEnumerator IE_Transition(string to_scene_name, string transition_scene)
    {
        m_isInTransition = true;

        string from_scene_name = SceneManager.GetActiveScene().name;

        // no longer freezes time; instead just attempts to deactivate a player if they exist
        // Time.timeScale = 0f; // STOP
        if (PlayerController.StaticSetPlayerActionability(false))
        {
            Debug.Log("No player instance found to deactivate. Progressing with scene load...");
        }

        // smoothly load in the blank loading scene and wait till that finishes to begin transitioning
        var l_task = SceneManager.LoadSceneAsync(transition_scene, LoadSceneMode.Additive);
        yield return l_task;

        // not explicitly needed, but takes control from main scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(transition_scene));

        // find the animation in the blank scene and start it
        var animation = GameObject.FindFirstObjectByType<AMonoSceneTransition>();
        animation.Begin();

        // begin loading in background, not allowing completion
        var task = SceneManager.LoadSceneAsync(to_scene_name, LoadSceneMode.Additive);
        task.allowSceneActivation = false;

        // wait for the animation's first half (the beginning) to elapse
        yield return new WaitForSecondsRealtime(animation.GetBeginDuration());

        // allow completion so that progression can go beyond 90%
        task.allowSceneActivation = true;

        // wait till it finishes up
        yield return new WaitUntil(() => task.progress >= 0.90f);

        yield return new WaitUntil(() => task.isDone);

        // set the main scene to be the newly loaded one
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(to_scene_name));

        // clean up the old scene we can't see anymore
        var from_scn_cleanup = SceneManager.UnloadSceneAsync(from_scene_name);
        yield return from_scn_cleanup;

        // tell animation to finish up now that scene is ready
        animation.Finish();

        // wait for the animation to elapse
        yield return new WaitForSecondsRealtime(animation.GetFinishDuration());

        // clean up transition scene
        var blank_scn_cleanup = SceneManager.UnloadSceneAsync(transition_scene);
        yield return blank_scn_cleanup;

        // unfreeze time and start
        Time.timeScale = 1f; // START

        m_isInTransition = false;
    }

    private string MatchTransitionType(TransitionType transition_type)
    {
        return transition_type switch
        {
            TransitionType.Wipe => "WipeBattleTransitionScene",
            TransitionType.Fade => "FadeTransitionScene",
            _ => throw new System.ArgumentException("Unsupported type: " + transition_type),
        };
    }
}

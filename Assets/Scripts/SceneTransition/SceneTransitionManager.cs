using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public static void Transition(string to_scene, string transition_scene = "BlankScene")
    {
        Instance.StartCoroutine(Instance.IE_Transition(to_scene, transition_scene));
    }

    private IEnumerator IE_Transition(string to_scene_name, string transition_scene)
    {
        string from_scene_name = SceneManager.GetActiveScene().name;

        Time.timeScale = 0f; // STOP

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
    }
}

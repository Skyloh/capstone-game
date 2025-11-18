using UnityEngine;

public abstract class AMonoSceneTransition : MonoBehaviour, ISceneTransition
{
    public abstract void Begin();
    public abstract void Finish();
    public abstract float GetBeginDuration();
    public abstract float GetFinishDuration();
}

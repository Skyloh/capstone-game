using UnityEngine;

public abstract class AMonoSceneTransition : MonoBehaviour, ISceneTransition
{
    [SerializeField] private float m_multSpeed = 1f;

    /// <summary>
    /// Returns the speed scaling multiplier affecting this transition.
    /// A multiplier of 2 means the transition animations play twice as fast.
    /// </summary>
    /// <returns></returns>
    protected float GetMultiplier() => m_multSpeed;

    /// <summary>
    /// Starts the beginning transition animation that covers the screen.
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// Starts the ending transition animation that uncovers the screen.
    /// </summary>
    public abstract void Finish();

    /// <summary>
    /// Returns the full duration in seconds of the beginning transition effect, scaled by
    /// speed multiplier.
    /// </summary>
    /// <returns></returns>
    public float GetBeginDuration() => GetRawBeginDuration() / m_multSpeed;

    /// <summary>
    /// Returns the full duration in seconds of the finishing transition effect, scaled by
    /// speed multiplier.
    /// </summary>
    /// <returns></returns>
    public float GetFinishDuration() => GetRawFinishDuration() / m_multSpeed;

    /// <summary>
    /// Returns the raw unscaled duration of the beginning animation, without having 
    /// the speed multiplier applied to it.
    /// </summary>
    /// <returns></returns>
    protected abstract float GetRawBeginDuration();

    /// <summary>
    /// Returns the raw unscaled duration of the finishing animation, without having 
    /// the speed multiplier applied to it.
    /// </summary>
    /// <returns></returns>
    protected abstract float GetRawFinishDuration();
}

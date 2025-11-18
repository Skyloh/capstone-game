/// <summary>
/// Small interface to define scripts that handle scene transition animations. Defines a function to start
/// the transition, and a function to get the duration of the animation.
/// </summary>
public interface ISceneTransition
{
    public void Begin();
    public void Finish();
    public float GetBeginDuration();
    public float GetFinishDuration();
}

using UnityEngine;

// simple component to play a BGM from the AudioManager on Start
public class PlayBGMOnStartBehavior : MonoBehaviour
{
    [SerializeField] private string m_song;
    [SerializeField] private bool m_bookmarkInterrupted;

    private void Start()
    {
        AudioManager.PlayBGM(m_song, m_bookmarkInterrupted);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource m_bgmSource1;
    [SerializeField] private AudioSource m_bgmSource2;

    [Space]

    [SerializeField] private AudioSource m_sfxSource;
    [SerializeField] private AudioDatabaseSO m_database;

    [Header("Clips")]

    [SerializeField] private SerializableKVPair<string, AudioClip>[] m_bgms;

    [Header("Fade Curves (Normalized)")]

    [SerializeField, Tooltip("Time must be between 0,1")] private AnimationCurve m_fadeOutCurve;
    [SerializeField, Tooltip("Time must be between 0,1")] private AnimationCurve m_fadeInCurve;
    [SerializeField] private float m_fadeTime;

    // -1 is no player
    private int m_activeAudioSourceId = -1;

    private IDictionary<AudioClip, float> m_timeStamps;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        m_timeStamps = new Dictionary<AudioClip, float>();
        // No DDL needed bc it gos on the Persistent Manager object
    }

    public static void PlaySFX(string name)
    {
        Instance.m_sfxSource.PlayOneShot(Instance.m_database.GetItem(name));
    }

    public static void PlayBGM(string clip, bool bookmark_timestamp = false)
    {
        Instance.InternalPlayBGM(clip, bookmark_timestamp);
    }

    private void InternalPlayBGM(string clip, bool bookmark_timestamp)
    {
        StopAllCoroutines();
        StartCoroutine(IE_TransitionAudio(clip, bookmark_timestamp));
    }

    private IEnumerator IE_TransitionAudio(string clip_to_play, bool bookmark_timestamp)
    {
        var (fading_out, fading_in) = ArrangeSources();

        var clip = StringToClip(clip_to_play);

        // if we're fading into a clip we're already playing, stop.
        if (clip == fading_out.clip) yield break;

        // assume volume starts at 0 for fade-in
        fading_in.clip = clip;
        fading_in.volume = 0f;

        // start playing as we fade in
        fading_in.Play();

        // check for a bookmarked timestamp to forward to
        if (m_timeStamps.TryGetValue(clip, out float time))
        {
            fading_in.time = time;
        }

        // assume volume starts at 1 for fade-out
        fading_out.volume = 1f;

        float start_time = Time.time;
        while (Time.time - start_time < m_fadeTime)
        {
            float n_t = (Time.time - start_time) / m_fadeTime;

            fading_out.volume = m_fadeOutCurve.Evaluate(n_t);
            fading_in.volume = m_fadeInCurve.Evaluate(n_t);

            yield return new WaitForEndOfFrame();
        }
        
        // set final values
        fading_out.volume = 0f;
        fading_in.volume = 1f;

        // if we need to, bookmark where we left
        if (bookmark_timestamp) m_timeStamps[fading_out.clip] = fading_out.time;

        // stop the fade-out system
        fading_out.Stop();

        // update the new playing source
        m_activeAudioSourceId = fading_in.GetInstanceID();
    }

    private (AudioSource playing, AudioSource waiting) ArrangeSources()
    {
        bool nothing_playing = m_activeAudioSourceId == -1;

        if (nothing_playing || m_activeAudioSourceId == m_bgmSource1.GetInstanceID())
        {
            return (m_bgmSource1, m_bgmSource2);
        }
        else // equivalent to: m_activeAudioSourceId == m_bgmSource2.GetInstanceID()
        {
            return (m_bgmSource2, m_bgmSource1);
        }
    }

    private AudioClip StringToClip(string s)
    {
        foreach (var pair in m_bgms)
        {
            if (pair.key == s) return pair.value;
        }

        throw new System.ArgumentException($"No clip found for key \"{s}\"");
    }
}

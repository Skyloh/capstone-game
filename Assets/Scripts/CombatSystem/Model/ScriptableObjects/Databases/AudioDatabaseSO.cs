using UnityEngine;

/// <summary>
/// A simple data SO for storing mapping of audio sfx names to audio clips.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/AudioDB", fileName = "AudioDatabaseObject", order = 0)]
public class AudioDatabaseSO : AItemDatabaseSO<AudioClip> { }

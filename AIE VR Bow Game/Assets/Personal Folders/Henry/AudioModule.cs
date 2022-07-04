using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioModule : MonoBehaviour
{
    #region SoundClass
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range (0f, 1f)] 
        public float volume;
    }
    #endregion

    public Sound[] sounds;
    public Dictionary<string, Sound> soundLookup; 
    public AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        soundLookup = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            soundLookup.Add(s.name, s);
        }
    }

    public void PlaySFX (string name)
    {
        Sound s = soundLookup[name];

        source.PlayOneShot(s.clip, s.volume);
    }
}

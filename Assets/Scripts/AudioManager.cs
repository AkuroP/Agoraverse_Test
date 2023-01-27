using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixerGroup soundEffectMixer;
    private float soundEffectVolume;

    public AudioMixerGroup ostMixer;
    private float ostMixerVolume;

    [System.Serializable]
    public class KeyValue
    {
        public string audioName;
        public AudioClip audio;
    }
    [SerializeField]private List<KeyValue> audioList = new List<KeyValue>();
    public Dictionary<string, AudioClip> allAudio = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (instance != null)Destroy(this.gameObject);
        instance = this;

        DontDestroyOnLoad(this.gameObject);

        foreach(var audio in audioList)
        {
            allAudio[audio.audioName] = audio.audio;
        }

        //ostMixerVolume = ostMixer.audioMixer.
        
    }

    public AudioSource PlayClipAt(AudioClip clip, Vector3 pos, AudioMixerGroup whatMixer, bool isSFX, bool islooping)
    {
        //Create GameObject
        GameObject tempGO = new GameObject("TempAudio");
        //pos of GO
        tempGO.transform.position = pos;
        //Add an audiosource
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        //Get the audio mixer
        audioSource.outputAudioMixerGroup = whatMixer;
        audioSource.loop = islooping;
        if(isSFX)audioSource.PlayOneShot(audioSource.clip);
        else audioSource.Play();
        //Destroy at the lenght of the clip
        if(!audioSource.loop)Destroy(tempGO, clip.length);
        return audioSource;
    }

    public void PlaySFX(string audioName)
    {
        PlayClipAt(this.allAudio[audioName], this.transform.position, soundEffectMixer, true, false);
    }


    public void SetOSTVolume(float volume)
    {
        ostMixer.audioMixer.SetFloat(ostMixer.name, volume);
    }

    public void SetSFXVolume(float volume)
    {
        soundEffectMixer.audioMixer.SetFloat("SFX", volume);
    }
}

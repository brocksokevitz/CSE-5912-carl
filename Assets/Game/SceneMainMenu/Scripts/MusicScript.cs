using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;


public class MusicScript : MonoBehaviour {

    public AudioMixerSnapshot menuShot;
    public AudioMixerSnapshot buildShot;
    public AudioMixerSnapshot combatShot;
    public AudioMixerGroup menuMix;
    public AudioMixerGroup buildMix;
    public AudioMixerGroup combatMix;

    public AudioClip[] menuSongs;
    public AudioClip[] buildSongs;
    public AudioClip[] combatSongs;
    public AudioSource source;
    public int bpm = 128;
    public enum SongStates { Menu, Build, Combat};
    public SongStates currentState = SongStates.Menu;
    private float transitionOut;
    private float quarterNote;
    int lastIndex = -1;
    // Use this for initialization
    void Start () {
        quarterNote = 60 / bpm;
        transitionOut = quarterNote * 16;
        source = gameObject.GetComponent<AudioSource>();
        PlayRandomTrack();
	}
	
    public void SwitchState(SongStates inNewState)
    {
        if (currentState != inNewState)
        {
            Debug.Log("Musicced Up");
            currentState = inNewState;
            if (currentState == SongStates.Menu)
            {
                menuShot.TransitionTo(transitionOut);
            }
            else if (currentState == SongStates.Build)
            {
                buildShot.TransitionTo(transitionOut);
            }
            else if (currentState == SongStates.Combat)
            {
                combatShot.TransitionTo(transitionOut);
            }
            lastIndex = -1;
            PlayRandomTrack();
        }
        
    }

    public void PlayRandomTrack()
    {
        CancelInvoke();
        source.Stop();
        if (currentState == SongStates.Menu)
        {
            int songChoice = Random.Range(0, menuSongs.Length);
            while (songChoice == lastIndex)
            {
                songChoice = Random.Range(0, menuSongs.Length);
            }
            source.clip = menuSongs[songChoice];
            source.outputAudioMixerGroup = menuMix;
            lastIndex = songChoice;
        }
        else if (currentState == SongStates.Build)
        {
            int songChoice = Random.Range(0, buildSongs.Length);
            while (songChoice == lastIndex)
            {
                songChoice = Random.Range(0, buildSongs.Length);
            }
            source.clip = buildSongs[songChoice];
            source.outputAudioMixerGroup = buildMix;
            lastIndex = songChoice;
        }
        else
        {
            int songChoice = Random.Range(0, combatSongs.Length);
            while (songChoice == lastIndex)
            {
                songChoice = Random.Range(0, combatSongs.Length);
            }
            source.clip = combatSongs[songChoice];
            source.outputAudioMixerGroup = combatMix;
            lastIndex = songChoice;
        }
        source.Play();
        Invoke("PlayRandomTrack", source.clip.length);
    }
}

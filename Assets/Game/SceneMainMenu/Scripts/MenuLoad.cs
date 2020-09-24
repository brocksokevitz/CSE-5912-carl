using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MenuLoad : MonoBehaviour {

    bool exists;
    public static string filepath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string pong = System.IO.Path.Combine(filepath, "BattlementBlast/");
    string saves = System.IO.Path.Combine(filepath, "BattlementBlast/Saves");
    string screenshots = System.IO.Path.Combine(filepath, "BattlementBlast/Screenshots");
    string videos = System.IO.Path.Combine(filepath, "BattlementBlast/Videos");

    // Use this for initialization
    void Start()
    {
        // Create the folders that are needed for screen capturing
        exists = System.IO.Directory.Exists(pong);
        if (!exists)
            System.IO.Directory.CreateDirectory(pong);

        exists = System.IO.Directory.Exists(saves);
        if (!exists)
            System.IO.Directory.CreateDirectory(saves);

        exists = System.IO.Directory.Exists(screenshots);
        if (!exists)
            System.IO.Directory.CreateDirectory(screenshots);

        exists = System.IO.Directory.Exists(videos);
        if (!exists)
            System.IO.Directory.CreateDirectory(videos);

        AudioSource musicAudioSource = GameObject.Find("MusicAudioSource").GetComponent<AudioSource>();
        AudioSource sfxAudioSource = GameObject.Find("SfxAudioSource").GetComponent<AudioSource>();
        // Load the settings into the GameSettings class
        GameSettings.AntiAliasing = GetCurrentAntiAliasingSetting();
        GameSettings.Quality = QualitySettings.masterTextureLimit;
        GameSettings.Fullscreen = Screen.fullScreen;
        // Only pull the audio source volume if it hasn't been done before due to audio sources getting
        // destroyed on scene load. If it's already been set in GameSettings then set the actual audio
        // source volume to be the same as what is in the GameSettings property
        if (GameSettings.MusicVolume == -1)
            GameSettings.MusicVolume = musicAudioSource.volume;
        else
            musicAudioSource.volume = GameSettings.MusicVolume;
        if (GameSettings.SfxVolume == -1)
            GameSettings.SfxVolume = sfxAudioSource.volume;
        else
            sfxAudioSource.volume = GameSettings.SfxVolume;

        Resolution[] resolutions = Screen.resolutions;
        // Set the current resolution...this isn't exactly the most efficient way of doing this
        // but at least it works.
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].Equals(Screen.currentResolution))
            {
                GameSettings.ResolutionIndex = i;
                break;
            }
        }
        //Debug.Log("Unlock the cursor...just to be sure");
        //// Unlock the cursor
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private int GetCurrentAntiAliasingSetting()
    {
        int output = 0;

        for (int i = 0; i < 3; i++)
        {
            if (QualitySettings.antiAliasing == (int)Mathf.Pow(2f, i))
            {
                output = i;
                break;
            }
        }

        return output;
    }
}

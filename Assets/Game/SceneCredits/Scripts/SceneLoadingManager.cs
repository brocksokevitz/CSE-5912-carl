using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoBehaviour {

    public int SecondsToShowCreditsScene = 5;

    private DateTime _loadTime;
    private Boolean _isLoadingMenu = false;

	// Use this for initialization
	void Start () {
        _loadTime = DateTime.Now;
    }
	
	// Update is called once per frame
	void Update () {
		if (DateTime.Now.Subtract(_loadTime).TotalSeconds > SecondsToShowCreditsScene && !_isLoadingMenu)
        {
            _isLoadingMenu = true;
            SceneManager.LoadScene("MenuScene");
        }
	}
}

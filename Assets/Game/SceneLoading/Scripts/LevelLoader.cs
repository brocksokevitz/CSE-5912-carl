using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour {

    public Slider slider;

	// Use this for initialization
	public void Start() {
        StartCoroutine(LoadAsynchronously());
    }

    IEnumerator LoadAsynchronously() {
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameScene");

        while (!operation.isDone) {
            float progress = Mathf.Clamp01(operation.progress/.9f);
            slider.value = progress;
            //if(progress == 1)
            //    SaveController.saveController.Load();
            yield return null;
        }
    }

}

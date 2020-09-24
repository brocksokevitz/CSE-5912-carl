using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    GameObject pausePanel;
    public delegate void Exit();
    public static event Exit ExitGame;
    
    public static bool IsPaused { get; set; }

    public static bool IsGameReady { get; set; }

    void OnEnable()
    {
        KeyboardInput.Paused += OpenPauseMenu;
    }

    void Start()
    {
        pausePanel = transform.GetChild(0).gameObject;
        IsPaused = true;
        IsGameReady = false;
    }

    public void MainMenuQuery()
    {
        GameObject.Find("_UI").transform.GetChild(2).gameObject.SetActive(true);
        GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(false);
    }

    public void CancelQuery()
    {
        GameObject.Find("_UI").transform.GetChild(2).gameObject.SetActive(false);
        GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(true);
    }

    public void MainMenu()
    {
        //Add main menu load
        ExitGame();
        KeyboardInput.Paused -= OpenPauseMenu;
        SceneManager.LoadScene("MenuScene");
    }

    public void ResumeGame()
    {
        OpenPauseMenu();
    }

    public void Save_OnClick()
    {
        SaveController.saveController.Save();
    }

    void OpenPauseMenu()
    {
        if (IsGameReady)
        {
            if (pausePanel == null)
            {
                pausePanel = transform.GetChild(0).gameObject;
            }
            if (!IsPaused)
            {
                pausePanel.SetActive(true);
            }
            else
            {
                pausePanel.SetActive(false);
            }
            IsPaused = !IsPaused;
        }
    }
}

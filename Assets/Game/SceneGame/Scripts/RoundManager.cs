using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject UIRoundDetailsPanel;
    [SerializeField]
    private Text txtRoundManager;
    [SerializeField]
    private GameObject goBarriers;

    [SerializeField]
    private int buildRoundSeconds = 120;

    [SyncVar(hook = "buildRoundSecondsLeftChanged")]
    public int buildRoundSecondsLeft;

    private DateTime lastTick;
    public GameObject RedSpawnCore;
    public GameObject BlueSpawnCore;
    public EmperorController emperor;
    public bool endedOnce;
    public GameObject lookText;
    GameObject audioObject;
    MusicScript music;
    NotificationManager nm;
    public GameObject[] players;
    public string[] playerNames;
    // Use this for initialization
    void Start () {
        endedOnce = false;
        lastTick = DateTime.Now;
        buildRoundSecondsLeft = buildRoundSeconds;
        UIRoundDetailsPanel = GameObject.Find("UIRoundDetails");
        txtRoundManager = UIRoundDetailsPanel.GetComponentInChildren<Text>();
        goBarriers = GameObject.Find("BuildRoundBarriers");
        audioObject = GameObject.Find("MusicAudioSource");
        music = audioObject.GetComponent<MusicScript>();
        music.SwitchState(MusicScript.SongStates.Build);
        RpcUpdateForRoundBegin();
        StartCoroutine(Delayer());
    }

    public IEnumerator Delayer()
    {
        float remainingTime = 1f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }

        emperor = GameObject.FindGameObjectWithTag("Emperor").GetComponent<EmperorController>();
        nm = GameObject.Find("UINotifications").GetComponent<NotificationManager>();
        

    }

    // Update is called once per frame
    void Update () {
        if (!isServer)
            return;

		if (DateTime.Now.Subtract(lastTick).TotalSeconds > 1)
        {
            lastTick = DateTime.Now;
            buildRoundSecondsLeft--;
            GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
            bool redLives = false;
            bool blueLives = false;
            int redCounter = 0;
            int blueCounter = 0;
            foreach (GameObject tempPlayer in tempPlayers)
            {
                if (!redLives && tempPlayer.GetComponent<PlayerTeam>().team == 0 && tempPlayer.GetComponent<Target>().locked == false)
                {
                    redLives = true;
                }
                else if (!blueLives && tempPlayer.GetComponent<PlayerTeam>().team == 1 && tempPlayer.GetComponent<Target>().locked == false)
                {
                    blueLives = true;
                }
                if (tempPlayer.GetComponent<PlayerTeam>().team == 0)
                {
                    redCounter++;
                }
                else if (tempPlayer.GetComponent<PlayerTeam>().team == 1)
                {
                    blueCounter++;
                }
            }
            if (!redLives && redCounter > 0)
            {
                CmdRoundManagerActive("Blue Wins!!!");
                CmdRestartRound();
                music.SwitchState(MusicScript.SongStates.Build);
            }
            if (!blueLives && blueCounter > 0)
            {
                CmdRoundManagerActive("Red Wins!!!");
                CmdRestartRound();
                RpcChangeSongState(MusicScript.SongStates.Build);
            }
            if (players != null)
            {
                if (players.Length != tempPlayers.Length)
                {
                    int oldCounter = 0;
                    foreach (GameObject tempPriorPlayer in players)
                    {
                        if (tempPriorPlayer == null)
                        {
                            RpcSendMessage("Player " + playerNames[oldCounter] + " disconnected.");
                        }
                        oldCounter++;
                    }
                    
                }
            }
            players = tempPlayers;
            playerNames = new string[tempPlayers.Length];
            int playerCounter = 0;
            foreach (GameObject tempPlayer in players)
            {
                playerNames[playerCounter] = tempPlayer.GetComponent<PlayerLobbyInfo>().playerName;
                playerCounter++;

            }
        }
    }

    [Command]
    public void CmdRoundManagerActive(string message)
    {
        RpcRoundManagerActive(message);
    }

    [ClientRpc]
    public void RpcRoundManagerActive(string message)
    {
        txtRoundManager.text = message;
        UIRoundDetailsPanel.SetActive(true);
        //txtRoundManager.gameObject.SetActive(true);
        endedOnce = true;
    }

    [ClientRpc]
    public void RpcSendMessage(string message)
    {
        nm.NewNotification(message, true);
    }

    [ClientRpc]
    public void RpcChangeSongState(MusicScript.SongStates state)
    {
        music.SwitchState(state);
    }

    [Command]
    public void CmdRestartRound()
    {
        GameObject currentRedCore = GameObject.FindGameObjectWithTag("RedSpawnCore");
        if (currentRedCore != null)
        {
            Destroy(currentRedCore);
        }
        GameObject currentBlueCore = GameObject.FindGameObjectWithTag("BlueSpawnCore");
        if (currentBlueCore != null)
        {
            Destroy(currentBlueCore);
        }
        CmdSpawnCores();
        GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject tempPlayer in tempPlayers)
        {
            Target tempTarget = tempPlayer.GetComponent<Target>();
            tempTarget.LockPlayer(false);
            tempTarget.CmdLockPlayer(false);
            tempTarget.CmdUpdateSpawnObject(tempPlayer.GetComponent<PlayerTeam>().team);
            tempTarget.Respawn();
        }
        GameObject[] tempBuildings = GameObject.FindGameObjectsWithTag("Building");
        for (int counter = tempBuildings.Length - 1; counter >= 0; counter--)
        {
            Destroy(tempBuildings[counter]);
        }
        GameObject[] tempCapPoints = GameObject.FindGameObjectsWithTag("CapturePoint");
        foreach (GameObject tempCapPoint in tempCapPoints)
        {
            CapturePoint tempCap = tempCapPoint.GetComponent<CapturePoint>();
            tempCap.ResetPoint();
        }
        GameObject base1 = GameObject.Find("Base1Center");
        base1.GetComponent<ResourceBank>().ResetBank();
        base1.GetComponent<BaseBuildings>().CmdResetBuildings();
        GameObject base2 = GameObject.Find("Base2Center");
        base2.GetComponent<ResourceBank>().ResetBank();
        base2.GetComponent<BaseBuildings>().CmdResetBuildings();
        emperor.ResetEmperor();
        RpcRefreshRoundStuff();
        RpcUpdateForRoundBegin();
    }

    [ClientRpc]
    public void RpcUpdateForRoundBegin()
    {
        Text tempText = lookText.GetComponent<Text>();
        tempText.text = "Destroy the opposing Spawn Core!" + Environment.NewLine + "Press 'B' to build." +
            Environment.NewLine + "Press 'V' to buy weapons." + Environment.NewLine + "Press 'T' to talk.";
        tempText.enabled = true;
        StartCoroutine(TextDelayer());
    }

    public IEnumerator TextDelayer()
    {
        float remainingTime = 5f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }

        lookText.GetComponent<Text>().enabled = false;

    }

    [ClientRpc]
    public void RpcRefreshRoundStuff()
    {
        buildRoundSecondsLeft = buildRoundSeconds;
        goBarriers.SetActive(true);
    }

    [Command]
    public void CmdSpawnCores()
    {

        GameObject redInstance = Instantiate(RedSpawnCore, RedSpawnCore.transform.position, RedSpawnCore.transform.rotation);
        NetworkServer.Spawn(redInstance);

        GameObject blueInstance = Instantiate(BlueSpawnCore, BlueSpawnCore.transform.position, BlueSpawnCore.transform.rotation);
        NetworkServer.Spawn(blueInstance);
    }

    private void EndedOnceChanged(bool newVal)
    {
        endedOnce = newVal;
    }

    private void buildRoundSecondsLeftChanged(int newVal)
    {
        buildRoundSecondsLeft = newVal;
        if (newVal > 0)
        {
            if (!endedOnce || endedOnce && newVal < buildRoundSeconds - 3)
            {
                // The countdown is still happening. Show the current time left.
                int minutes = (int)((double)newVal / 60);
                int seconds = buildRoundSecondsLeft - (minutes * 60);
                txtRoundManager.text = "Build Round" + Environment.NewLine + "<size=45>" + String.Format("{0:00}", minutes) + ":" + String.Format("{0:00}", seconds) + "</size>";
            }
            
        }
        else if (newVal == 0)
        {
            // The countdown is now at 0. Show the countdown has ended.
            txtRoundManager.text = "Build Round Ended!";
            // Disable the round barriers
            goBarriers.SetActive(false);
            music.SwitchState(MusicScript.SongStates.Combat);
            //Destroy(goBarriers);
        }
        else
        {
            // The countdown has ended and we are showing that to the user.
            // Do something here to disable/change what is showing to the user.
            // For example, newVal == -3 means do this 3 seconds after the countdown has ended.
            if (newVal == -3)
            {
                txtRoundManager.text = "PvP now enabled!";
            }
            else if (newVal == -5)
            {
                // Take the message off the screen.
                UIRoundDetailsPanel.SetActive(false);
                //txtRoundManager.gameObject.SetActive(false);
            }
        }
        
    }
}

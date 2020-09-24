using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public class HostConsole : NetworkBehaviour
{
    bool open;
    bool toClose;
    public GameObject panel;
    private Image UIPanelImg;
    public InputField input;

    public string[] tokens;

    public ResourceBank bank1;
    public ResourceBank bank2;

    public GunController playerGun;
    public Target playerTarget;
    public GameObject player;
    public NotificationManager nm;
    public string playerName;
    public PlayerTeam team;

    void Start()
    {
        if (isLocalPlayer)
        {
            UIPanelImg = GameObject.Find("UINotificationCenter").GetComponent<Image>();
            panel = GameObject.Find("UINotificationConsole");
            input = panel.GetComponentInChildren<InputField>();
            panel.SetActive(open);
            SetNotificationCenterBackVisible(open);
            playerName = gameObject.GetComponent<PlayerLobbyInfo>().playerName;
            team = gameObject.GetComponent<PlayerTeam>();
        }

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
        nm = GameObject.Find("UINotifications").GetComponent<NotificationManager>();
        

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (bank1 == null)
            {
                GameObject b1c = GameObject.Find("Base1Center");
                if (b1c != null)
                    bank1 = b1c.GetComponent<ResourceBank>();
            }
            if (bank2 == null)
            {
                GameObject b2c = GameObject.Find("Base2Center");
                if (b2c != null)
                    bank2 = b2c.GetComponent<ResourceBank>();
            }
            if (player == null)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject tempPlayer in players)
                {
                    PlayerController tempController = tempPlayer.GetComponent<PlayerController>();
                    if (tempController != null)
                    {
                        player = tempPlayer;
                        playerTarget = player.GetComponent<Target>();
                        playerGun = player.GetComponent<GunController>();
                    }
                }
            }
            //Open close console
            if (toClose || !open && Input.GetKeyDown(KeyCode.T))
            {
                toClose = false;
                open = !open;
                panel.SetActive(open);
                SetNotificationCenterBackVisible(open);
                if (open)
                {
                    player.GetComponent<PlayerController>().locked = true;
                    player.GetComponent<BuildScript>().locked = true;
                    player.GetComponent<EManager>().locked = true;
                    player.GetComponent<BuyManager>().locked = true;
                    player.GetComponent<GunController>().locked = true;
                    input.ActivateInputField();
                }
                else
                {
                    player.GetComponent<PlayerController>().locked = false;
                    player.GetComponent<BuildScript>().locked = false;
                    player.GetComponent<EManager>().locked = false;
                    player.GetComponent<BuyManager>().locked = false;
                    player.GetComponent<GunController>().locked = false;
                    input.text = "";
                }
            }
            if (open)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    toClose = true;
                    String msgToParse = input.text.Trim();
                    PlayerLobbyInfo lobbyInfo = player.GetComponent<PlayerLobbyInfo>();
                    Color playerTeamColor = lobbyInfo.playerColor;
                    String playerName = lobbyInfo.playerName;

                    if (msgToParse.Length > 0 && msgToParse.StartsWith("/"))
                    {
                        msgToParse = msgToParse.Remove(0, 1);
                        tokens = msgToParse.Split(' ');
                        if (tokens.Length >= 3 && tokens[0] == "add")
                        {
                            if (tokens.Length == 4 && tokens[1] == "wood")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to wood...", false);
                                    CmdAddToResources("Wood", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to wood...", false);
                                    CmdAddToResources("Wood", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens.Length == 4 && tokens[1] == "stone")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to stone...", false);
                                    CmdAddToResources("Stone", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to stone...", false);
                                    CmdAddToResources("Stone", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens.Length == 4 && tokens[1] == "metal")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to metal...", false);
                                    CmdAddToResources("Metal", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()) + " to metal...", false);
                                    CmdAddToResources("Metal", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens[1] == "ammo")
                            {
                                if (!IsInt(tokens[2]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Amount must be a number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                nm.NewNotification("Adding " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[2]).ToString()) + " to reserve ammo...", false);
                                playerGun.currentAmmoInReserve += int.Parse(tokens[2]);
                                toClose = true;
                            }
                            else
                            {
                                nm.NewNotification(NotificationManager.GetRedString("Unknown command"), false);
                                input.text = "";
                                input.ActivateInputField();
                            }
                        }
                        else if (tokens.Length >= 3 && tokens[0] == "set")
                        {
                            if (tokens.Length == 4 && tokens[1] == "wood")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Setting wood to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Wood", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Setting wood to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Wood", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens.Length == 4 && tokens[1] == "stone")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Setting stone to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Stone", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Setting stone to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Stone", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens.Length == 4 && tokens[1] == "metal")
                            {
                                if (!IsInt(tokens[2]) || !IsInt(tokens[3]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Team or Amount not numbers"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                if (tokens[2].Equals("1"))
                                {
                                    nm.NewNotification("Setting metal to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Metal", Convert.ToInt32(tokens[3]), 0);
                                }
                                else if (tokens[2].Equals("2"))
                                {
                                    nm.NewNotification("Setting metal to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[3]).ToString()), false);
                                    CmdSetResources("Metal", Convert.ToInt32(tokens[3]), 1);
                                }
                                else
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Invalid team number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                input.text = "";
                                input.ActivateInputField();
                                return;
                            }
                            else if (tokens[1] == "ammo")
                            {
                                if (!IsInt(tokens[2]))
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Amount must be a number"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                nm.NewNotification("Setting reserve ammo to " + NotificationManager.GetColoredString(Color.green, Convert.ToInt32(tokens[2]).ToString()), false);
                                playerGun.currentAmmoInReserve = int.Parse(tokens[2]);
                                input.text = "";
                                input.ActivateInputField();
                            }
                            else if (tokens[1] == "health")
                            {
                                if (!IsInt(tokens[2]) || int.Parse(tokens[2]) > 100 || int.Parse(tokens[2]) < 0)
                                {
                                    nm.NewNotification(NotificationManager.GetRedString("Health must be between 0 and 100"), false);
                                    input.text = "";
                                    input.ActivateInputField();
                                    return;
                                }
                                int newHealth = int.Parse(tokens[2]);
                                if (newHealth > playerTarget.startingHealth)
                                    newHealth = (int)playerTarget.startingHealth;
                                nm.NewNotification("Setting health to " + NotificationManager.GetColoredString(Color.green, newHealth.ToString()), false);
                                playerTarget.currentHealth = (float)newHealth;
                                input.text = "";
                                input.ActivateInputField();
                            }
                            else
                            {
                                nm.NewNotification(NotificationManager.GetRedString("Unknown command"), false);
                                input.text = "";
                                input.ActivateInputField();
                            }
                        }
                        else if (tokens.Length == 1 && tokens[0] == "kill")
                        {
                            if (playerTeamColor == Color.blue)
                                nm.NewNotification(NotificationManager.GetBlueString(playerName) + " committed suicide", false);
                            else
                                nm.NewNotification(NotificationManager.GetRedString(playerName) + " committed suicide", false);
                            playerTarget.CmdDie();
                            input.text = "";
                            input.ActivateInputField();
                        }
                        else
                        {
                            nm.NewNotification(NotificationManager.GetRedString("Unknown command"), false);
                            input.text = "";
                            input.ActivateInputField();
                        }
                    }
                    else if (msgToParse.Length > 0)
                    {
                        CmdSendMessage(NotificationManager.GetColoredString(team.team == 0 ? Color.red : Color.blue, 
                            playerName) + ": " + msgToParse);
                        input.text = "";
                        input.ActivateInputField();
                    }
                    else
                    {
                        input.text = "";
                        input.ActivateInputField();
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSendMessage(string message)
    {
        RpcSendMessage(message);
    }

    [ClientRpc]
    public void RpcSendMessage(string message)
    {
        nm.NewNotification(message, true);
    }

    [Command]
    void CmdAddToResources(string Type, int amount, int team)
    {
        GameObject serverBaseObject = GameObject.Find("Base" + (team + 1) + "Center");
        serverBaseObject.GetComponent<ResourceBank>().Add(Type, amount);
    }

    [Command]
    void CmdSetResources(string Type, int amount, int team)
    {
        GameObject serverBaseObject = GameObject.Find("Base" + (team + 1) + "Center");
        serverBaseObject.GetComponent<ResourceBank>().Set(Type, amount);
    }

    private void SetNotificationCenterBackVisible(Boolean visible)
    {
        if (isLocalPlayer)
        {
            if (visible)
            {
                UIPanelImg.color = new Color(1f, 1f, 1f, 0.32f);
            }
            else
            {
                UIPanelImg.color = new Color(1f, 1f, 1f, 0);
            }
        }
    }

    private Boolean IsInt(string strToTest)
    {
        Boolean output = false;
        try
        {
            int i = int.Parse(strToTest);
            output = true;
        }
        catch (Exception ex)
        {
            output = false;
        }
        return output;
    }
}

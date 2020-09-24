using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EmperorController : NetworkBehaviour
{
    [SyncVar(hook = "RedFavorChanged")]
    public int redFavor;
    [SyncVar(hook = "BlueFavorChanged")]
    public int blueFavor;
    [SyncVar(hook = "EntertainmentChanged")]
    public int entertainment;
    [SyncVar(hook = "GiftTimeoutChanged")]
    public float giftTimeout;
    [SyncVar(hook = "GiftSeverityChanged")]
    public int giftSeverity;
    public enum Mood { Captivated, Entertained, Interested, Unimpressed, Bored, Displeased };
    public Mood mood;
    public GameObject RoundManager;
    public RoundManager RoundScript;
    //public GameObject emperorText;
    public GameObject UIEmperorPanel;
    public Text UIEmperorText;
    public float popupDelay;
    private float boredomTimer;
    public float boredomCooldown;
    public float giftTimer;
    public float timeoutMultiplier;
    public int moodChangeCost;
    public float playerTimeModifier = 1f;

    public GameObject RedBase;
    public GameObject BlueBase;
	// Use this for initialization
	void Start () {
        ResetEmperor();
        RoundManager = GameObject.Find("Round Manager");
        RoundScript = RoundManager.GetComponent<RoundManager>();
        UIEmperorPanel = GameObject.Find("UIEmperorInfo");
        UIEmperorText = GameObject.Find("EmperorText").GetComponent<Text>();
        UIEmperorPanel.SetActive(false);
        StartCoroutine(Delayer());
    }

    public void ResetEmperor()
    {
        redFavor = 20;
        blueFavor = 20;
        giftTimer = 0;
        entertainment = 5;
        boredomTimer = 0f;
    }

    public IEnumerator Delayer()
    {
        float remainingTime = 1f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }
        GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
        playerTimeModifier = 1f + (12 - tempPlayers.Length) * .25f;
        //emperorText = GameObject.Find("EmperorText");
        RedBase = GameObject.Find("Base1Center");
        BlueBase = GameObject.Find("Base2Center");
    }

    // Update is called once per frame
    void Update()
    {
        if (RoundScript != null)
        {
            if (RoundScript.buildRoundSecondsLeft <= 0)
            {

                boredomTimer += Time.deltaTime;
                if (boredomTimer >= boredomCooldown * playerTimeModifier)
                {
                    entertainment -= 1;
                    boredomTimer = 0;
                }
                giftTimer += Time.deltaTime;
                if (giftTimer >= giftTimeout * playerTimeModifier)
                {
                    CmdGenerateGift();
                    giftTimer = 0;
                }
            }
        }
    }

    [Command]
    public void CmdGenerateGift()
    {
        int rndChance = Random.Range(0, 2);
        if (mood == Mood.Captivated || mood == Mood.Entertained || mood == Mood.Interested)
        {
            if (rndChance == 0)
            {
                ResourceBank resources;
                int team = -1;
                if (redFavor > blueFavor)
                {
                    resources = RedBase.GetComponent<ResourceBank>();
                    redFavor /= 2;
                    team = 0;
                }
                else if (blueFavor > redFavor)
                {
                    resources = BlueBase.GetComponent<ResourceBank>();
                    blueFavor /= 2;
                    team = 1;
                }
                else
                {
                    team = Random.Range(0, 2);

                    resources = team == 0 ? RedBase.GetComponent<ResourceBank>() : BlueBase.GetComponent<ResourceBank>();
                    if (team == 0)
                    {
                        redFavor /= 2;
                    }
                    else
                    {
                        blueFavor /= 2;
                    }
                }
                if (resources != null)
                {
                    resources.Add("Wood", giftSeverity * 300);
                    resources.Add("Stone", giftSeverity * 300);
                    resources.Add("Metal", giftSeverity * 150);
                }

                if (team > -1)
                {
                    RpcUpdateMessage("The Emperor has granted " + (team == 0 ? "Red" : "Blue") + " resources!");
                }
            }
            else if (rndChance == 1)
            {
                GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
                int team = -1;
                if (redFavor > blueFavor)
                {
                    team = 0;
                    redFavor /= 2;
                }
                else if (blueFavor > redFavor)
                {
                    team = 1;
                    blueFavor /= 2;
                }
                else
                {
                    team = Random.Range(0, 2);
                    if (team == 0)
                    {
                        redFavor /= 2;
                    }
                    else
                    {
                        blueFavor /= 2;
                    }

                }
                if (team > -1)
                {
                    foreach (GameObject tempPlayer in tempPlayers)
                    {
                        PlayerTeam pt = tempPlayer.GetComponent<PlayerTeam>();
                        if (pt.team == team)
                        {
                            Target target = tempPlayer.GetComponent<Target>();
                            target.currentHealth = target.startingHealth;
                            if (giftSeverity >= 2)
                            {
                                GunController gc = tempPlayer.GetComponent<GunController>();
                                gc.RpcResetAmmo(false);
                            }
                        }
                    }
                    if (giftSeverity >= 3)
                    {
                        GameObject[] tempBuildings = GameObject.FindGameObjectsWithTag("Building");
                        foreach (GameObject tempBuilding in tempBuildings)
                        {
                            BuildIdentifier bid = tempBuilding.GetComponent<BuildIdentifier>();
                            if (bid.team == team)
                            {
                                Target target = tempBuilding.GetComponent<Target>();
                                if (target != null)
                                {
                                    target.RepairBuilding(target.startingHealth);
                                }
                            }
                        }
                    }
                    RpcUpdateMessage("The Emperor has granted " + (team == 0 ? "Red" : "Blue") + " healing!");
                }
            }
        }
        else
        {
            if (rndChance == 0)
            {
                ResourceBank resources;
                int team = -1;
                if (redFavor < blueFavor)
                {
                    resources = RedBase.GetComponent<ResourceBank>();
                    blueFavor /= 2;
                    team = 0;
                }
                else if (blueFavor < redFavor)
                {
                    resources = BlueBase.GetComponent<ResourceBank>();
                    redFavor /= 2;
                    team = 1;
                }
                else
                {
                    team =  Random.Range(0, 2);

                    resources = team == 0 ? RedBase.GetComponent<ResourceBank>() : BlueBase.GetComponent<ResourceBank>();
                    if (team == 0)
                    {
                        blueFavor /= 2;
                    }
                    else
                    {
                        redFavor /= 2;
                    }
                }
                if (resources != null)
                {
                    resources.Add("Wood", giftSeverity * -300);
                    resources.Add("Stone", giftSeverity * -300);
                    resources.Add("Metal", giftSeverity * -150);
                }

                if (team > -1)
                {
                    RpcUpdateMessage("The Emperor has taken " + (team == 0 ? "Red's" : "Blue's") + " resources!");
                }
            }
            else
            {
                GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
                int team = -1;
                if (redFavor < blueFavor)
                {
                    team = 0;
                    blueFavor /= 2;
                }
                else if (blueFavor < redFavor)
                {
                    team = 1;
                    redFavor /= 2;
                }
                else
                {
                    team = Random.Range(0, 2);
                    if (team == 0)
                    {
                        blueFavor /= 2;
                    }
                    else
                    {
                        redFavor /= 2;
                    }
                }
                if (team > -1)
                {
                    foreach (GameObject tempPlayer in tempPlayers)
                    {
                        PlayerTeam pt = tempPlayer.GetComponent<PlayerTeam>();
                        if (pt.team == team)
                        {
                            Target target = tempPlayer.GetComponent<Target>();
                            target.SetHealth(target.currentHealth * (4 - giftSeverity) / 4f);
                        }
                    }
                    if (giftSeverity >= 1)
                    {
                        GameObject[] tempBuildings = GameObject.FindGameObjectsWithTag("Building");
                        foreach (GameObject tempBuilding in tempBuildings)
                        {
                            BuildIdentifier bid = tempBuilding.GetComponent<BuildIdentifier>();
                            if (bid.team == team)
                            {
                                Target target = tempBuilding.GetComponent<Target>();
                                if (target != null)
                                {
                                    target.SetHealth(target.currentHealth / 2f);
                                }
                            }
                        }
                    }
                    RpcUpdateMessage("The Emperor has harmed " + (team == 0 ? "Red" : "Blue") + "!");
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUpdateMessage(string inText)
    {
        UIEmperorPanel.SetActive(true);
        //emperorOutput.enabled = true;
        UIEmperorText.text = inText;
        StartCoroutine(MessageChangeDelay());
    }

    private void RedFavorChanged(int newVal)
    {
        redFavor = newVal;
    }

    private void BlueFavorChanged(int newVal)
    {
        blueFavor = newVal;
    }


    private void GiftSeverityChanged(int newVal)
    {
        giftSeverity = newVal;
    }

    private void GiftTimeoutChanged(float newVal)
    {
        giftTimeout = newVal;
    }

    private void EntertainmentChanged(int newVal)
    {
        entertainment = newVal;
        Mood previousMood = mood;
        if (entertainment >= moodChangeCost * 2)
        {
            mood = Mood.Captivated;
            giftSeverity = 3;
            giftTimeout = 5.5f * timeoutMultiplier;
        }
        else if (entertainment >= moodChangeCost)
        {
            mood = Mood.Entertained;
            giftSeverity = 2;
            giftTimeout = 5f * timeoutMultiplier;
        }
        else if (entertainment >= 0)
        {
            mood = Mood.Interested;
            giftSeverity = 1;
            giftTimeout = 4.5f * timeoutMultiplier;
        }
        else if (entertainment >= -moodChangeCost)
        {
            mood = Mood.Unimpressed;
            giftSeverity = 1;
            giftTimeout = 4f * timeoutMultiplier;
        }
        else if (entertainment >= -moodChangeCost * 2)
        {
            mood = Mood.Bored;
            giftSeverity = 2;
            giftTimeout = 3.5f * timeoutMultiplier;
        }
        else
        {
            mood = Mood.Displeased;
            giftSeverity = 3;
            giftTimeout = 3 * timeoutMultiplier;
        }
        if (UIEmperorText != null && UIEmperorPanel != null)
        {
            if (previousMood != mood)
            {
                UIEmperorPanel.SetActive(true);
                //emperorOutput.enabled = true;
                UIEmperorText.text = "The Emperor is " + mood + "!";
                StartCoroutine(MessageChangeDelay());
            }
        }
    }


    public IEnumerator MessageChangeDelay()
    {
        float remainingTime = popupDelay;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }
        UIEmperorPanel.SetActive(false);
        //emperorOutput.enabled = false;
    }

    [Command]
    public void CmdAddRedFavor(int Amount)
    {
        redFavor += Amount;
    }

    [Command]
    public void CmdAddBlueFavor(int Amount)
    {
        blueFavor += Amount;
    }
    
    [Command]
    public void CmdAddEntertainment(int Amount)
    {
        entertainment += Amount;
    }
}

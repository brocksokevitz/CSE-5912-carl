using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class CapturePoint : NetworkBehaviour
{

    public float radius;
    public float captureTime;
    public int resourcePerSecond;

    [SyncVar]
    public float ownership;
    [SyncVar]
    public float resourceTimer;
    [SyncVar]
    public string currentOwner;

    public GameObject uiObject;
	public GameObject zone; 
    public int localTeam;

    public float Ownership
    {
        get
        {
            return ownership;
        }
        set
        {
            if (value > captureTime * 2)
            {
                ownership = captureTime * 2;
            }
            else if (value < -captureTime * 2)
            {
                ownership = -captureTime * 2;
            }
            else
            {
                if (value < -captureTime)
                {
                    currentOwner = "Team Two";
                    uiObject.GetComponent<Image>().color = Color.blue;
                    uiObject.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0:F0}", Mathf.Abs(value) - captureTime);
					zone.GetComponent<Renderer>().material.SetColor("_EmissionColor",new Color(0, 0, 1f, 0.8f));
				}
                else if (value > captureTime)
                {
                    currentOwner = "Team One";
                    uiObject.GetComponent<Image>().color = Color.red;
                    uiObject.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0:F0}", Mathf.Abs(value) - captureTime);
zone.GetComponent<Renderer>().material.SetColor("_EmissionColor",new Color(1f, 0, 0, 0.8f));
				}
                else
                {
                    currentOwner = "No One";
                    uiObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 100f / 256);
                    uiObject.transform.GetChild(1).GetComponent<Text>().text = string.Format("{0:F0}", localTeam == 0 ? value : -value);
					zone.GetComponent<Renderer>().material.SetColor("_EmissionColor",new Color(1f, 1f, 1f, 0.8f));
			  }
                ownership = value;
            }
        }
    }
    // Use this for initialization
    void Start()
    {
        ResetPoint();
    }

    public void ResetPoint()
    {
        resourceTimer = 0;
        Ownership = 0f;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        GameObject[] tempPlayers = GameObject.FindGameObjectsWithTag("Player");
        float timeAdded = 0f;
        foreach (GameObject tempPlayer in tempPlayers)
        {
            float magnitude = (transform.position - tempPlayer.transform.position).magnitude;
            if (magnitude < radius)
            {
                PlayerTeam tempTeam = tempPlayer.GetComponent<PlayerTeam>();
                if (tempTeam.team == 0)
                {
                    timeAdded += Time.deltaTime;
                }
                else
                {
                    timeAdded -= Time.deltaTime;
                }
            }

        }
        Ownership += timeAdded;
        if (currentOwner != "No One")
        {
            if (resourceTimer >= 1f)
            {
                resourceTimer = 0;
                int teamNumber = currentOwner == "Team One" ? 1 : 2;
                GameObject baseObject = GameObject.Find("Base" + teamNumber + "Center");
                baseObject.GetComponent<ResourceBank>().Add("Stone", resourcePerSecond);
                baseObject.GetComponent<ResourceBank>().Add("Wood", resourcePerSecond);
                baseObject.GetComponent<ResourceBank>().Add("Metal", resourcePerSecond);
            }
            else
            {
                resourceTimer += Time.deltaTime;
            }
        }

    }
}

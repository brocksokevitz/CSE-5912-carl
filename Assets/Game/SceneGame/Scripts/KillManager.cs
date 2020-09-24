using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KillManager : NetworkBehaviour {
    public GameObject uiKillObject;
    public Sprite pistol;
    public Sprite assault;
    public Sprite shotgun;
    public Sprite sniper;
    public Sprite rockets;
    public Sprite minigun;
    // Use this for initialization
    void Start () {
		
	}
	
    public void AddKill(string killerName, int killerTeam, string victimName, int victimTeam, int gunChoice)
    {
        GameObject instance = Instantiate(uiKillObject);
        instance.transform.SetParent(transform, false);
        Text killerText = instance.transform.GetChild(0).GetComponent<Text>();
        killerText.text = killerName;
        killerText.color = killerTeam == 0 ? Color.red : Color.blue;

        Image weaponImage = instance.transform.GetChild(1).GetComponent<Image>();
        weaponImage.sprite = gunChoice == 0 ? pistol : (gunChoice == 1 ? assault : (gunChoice == 2 ? shotgun : (gunChoice == 3 ? sniper :
            (gunChoice == 4 ? rockets : minigun))));

        Text victimText = instance.transform.GetChild(2).GetComponent<Text>();
        victimText.text = victimName;
        victimText.color = victimTeam == 0 ? Color.red : Color.blue;
        StartCoroutine(Delayer(3f));
    }

    public IEnumerator Delayer(float delay)
    {
        float remainingTime = delay;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }
        int childCount = gameObject.transform.childCount;
        if (childCount > 0)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        }
        
        if (childCount > 1)
        {
            StartCoroutine(Delayer(1.5f));
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}

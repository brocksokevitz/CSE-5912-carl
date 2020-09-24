using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EManager : NetworkBehaviour {
    public float eTime;
    public int eTarget;
    public float repairTime;
    public float hpPerRepair;
    public Transform cameraTrans;
    public PlayerTeam team;
    public GameObject lookTextBox;
    public int repairCost;
    public Text lookText;
    public bool locked;
    public bool isLooking;
    // Use this for initialization
    void Start () {
        locked = false;
        eTime = 0;
        eTarget = -1;
        team = gameObject.GetComponent<PlayerTeam>();
        if (isLocalPlayer)
        {
            StartCoroutine(TextDelayer());
        }
        
    }

    public IEnumerator TextDelayer()
    {
        float remainingTime = 1f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }

        lookTextBox = GameObject.Find("LookText");
        lookText = lookTextBox.GetComponent<Text>();

    }

    void FixedUpdate () {
        if (isLocalPlayer && !locked)
        {
            Ray previewRay = new Ray(cameraTrans.position, cameraTrans.forward);
            RaycastHit previewHit;
            if (Physics.Raycast(previewRay, out previewHit, 3f))
            {
                    Transform hitTrans = previewHit.transform;
                    GameObject hitGameObject = hitTrans.parent != null ? hitTrans.parent.gameObject : hitTrans.gameObject;
                Target hitTarget = hitGameObject.GetComponent<Target>();
                bool CanAfford = true;
                if (hitTarget != null)
                {
                    if (hitTarget.currentHealth < hitTarget.startingHealth && hitTarget.bid != null)
                    {
                        ResourceBank tempBank = team.baseObject.GetComponent<ResourceBank>();
                        if (tempBank.wood >= repairCost && tempBank.stone >= repairCost)
                        {
                            lookText.text = "Hold 'E' to repair (" + string.Format("{0:P0}", hitTarget.currentHealth / hitTarget.startingHealth) + ")";
                            
                        }
                        else
                        {
                            lookText.text = "Can't afford to repair!";
                            CanAfford = false;
                        }
                        lookText.enabled = true;
                        isLooking = true;
                    }
                    
                }
                if (Input.GetKey(KeyCode.E) && CanAfford)
                {
                    BuildIdentifier bid = hitGameObject.GetComponent<BuildIdentifier>();

                    if (bid != null && bid.team == team.team)
                    {
                        if (eTarget != bid.id)
                        {
                            eTime = 0;
                            eTarget = bid.id;
                        }
                        else
                        {
                            eTime += Time.deltaTime;
                            if (eTime > repairTime)
                            {
                                ResourceBank tempBank = team.baseObject.GetComponent<ResourceBank>();
                                tempBank.Add("Wood", -repairCost);
                                tempBank.Add("Stone", -repairCost);
                                eTime = 0;
                                CmdRepair(hitGameObject.GetComponent<NetworkIdentity>().netId);
                            }
                        }
                    }
                    else
                    {
                        eTime = 0;
                        eTarget = -1;
                    }
                }
                else
                {
                    eTime = 0;
                    eTarget = -1;
                }
            }
            else
            {
                eTime = 0;
                eTarget = -1;
                if (lookText != null && isLooking)
                {
                    lookText.enabled = false;
                    isLooking = false;
                }
                
            }
        }
        
	}

    [Command]
    public void CmdRepair(NetworkInstanceId nid)
    {
        GameObject hitGameObject = ClientScene.FindLocalObject(nid);
        Target target = hitGameObject.GetComponent<Target>();
        target.RepairBuilding(hpPerRepair);
    }
}

using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Target : NetworkBehaviour {

    public float startingHealth = 100f;
    [SyncVar(hook = "OnCurrentHealthChange")]
    public float currentHealth;

    public float startingFatigue = 100f;
    [SyncVar(hook = "OnCurrentFatigueChange")]
    public float currentFatigue;

    public bool isVulnerable = true;
    public Vector3 startingPos;
    public int respawnTime = 5;
    public float timer;
    [SyncVar]
    private bool isDead;
    public bool _isDead
    {
        get { return isDead; }
        protected set { isDead = value; }
    }
    MeshRenderer renderer;
	MeshFilter tempMesh; 
	MeshFilter mesh;
    public AudioSource walking;
    //Renderer rend;
    CapsuleCollider col;
    Rigidbody rb;
    public PlayerTeam team;
    public BuildIdentifier bid;
    public GameObject emperor;
    public GunController gunScript;
    public GameObject SpawnObject;
    public GameObject lookTextBox;
    public Text lookText;
    public bool locked;
    IEnumerator damageRoutine;

    public delegate void OnHealthChanged(float prevValue, float newValue);
    public OnHealthChanged onHealthChanged;

    public delegate void OnFatigueChanged(float prevValue, float newValue);
    public OnFatigueChanged onFatigueChanged;

    void Start()
    {
        currentHealth = startingHealth;
        currentFatigue = startingFatigue;

        isDead = false;
        //rend = transform.GetComponent<Renderer>();
        col = transform.GetComponent<CapsuleCollider>();
        rb = transform.GetComponent<Rigidbody>();
		mesh = transform.GetComponent<MeshFilter>();
		tempMesh = mesh;
        team = GetComponent<PlayerTeam>();
        bid = GetComponent<BuildIdentifier>();
        gunScript = GetComponent<GunController>();
        renderer = GetComponent<MeshRenderer>();
        
        if (bid == null)
        {
            StartCoroutine(Delayer());
        }
        else
        {
            emperor = GameObject.FindGameObjectWithTag("Emperor");
        }
        
        //StartCoroutine(EmperorDelayer());
        //healthbar.sizeDelta = new Vector2(health * 2, healthbar.sizeDelta.y);
        }

    public IEnumerator Delayer()
    {
        float remainingTime = 2f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }
        emperor = GameObject.FindGameObjectWithTag("Emperor");
        if (team != null)
        {
            if (team.team == 0)
            {
                SpawnObject = GameObject.FindGameObjectWithTag("RedSpawnCore");
            }
            else
            {
                SpawnObject = GameObject.FindGameObjectWithTag("BlueSpawnCore");
            }
        }
        lookTextBox = GameObject.Find("LookText");
        lookText = lookTextBox.GetComponent<Text>();
    }

    [Command]
    public void CmdPlayWalkingSound()
    {
        walking.Play();
        RpcPlayWalkingSound();
    }

    [ClientRpc]
    public void RpcPlayWalkingSound()
    {
        walking.Play();

    }

    [Command]
    public void CmdStopWalkingSound()
    {
        walking.Stop();
        RpcStopWalkingSound();
    }

    [ClientRpc]
    public void RpcStopWalkingSound()
    {
        walking.Stop();

    }

    [Command]
    public void CmdUpdateSpawnObject(int team)
    {
        GameObject tempObject = GameObject.FindGameObjectWithTag(team == 0 ? "RedSpawnCore" : "BlueSpawnCore");
        SpawnObject = tempObject;
        RpcUpdateSpawnObject(team);
    }

    [ClientRpc]
    public void RpcUpdateSpawnObject(int team)
    {
        GameObject tempObject = GameObject.FindGameObjectWithTag(team == 0 ? "RedSpawnCore" : "BlueSpawnCore");
        SpawnObject = tempObject;
    }

    private void Update()
    {
        if (team != null)
        {
            if (isServer)
            {
                if (isDead)
                {
                    timer += Time.deltaTime;
                }
                if (timer >= respawnTime)
                {

                    if (SpawnObject != null)
                    {
                        Respawn();
                    }
                    else
                    {
                        CmdLockPlayer(true);
                    }
                    timer = 0;
                }
            }
        }
        
    }

    [Command]
    public void CmdLockPlayer(bool locked)
    {
        //LockPlayer(locked);
        RpcLockPlayer(locked);
    }

    [ClientRpc]
    public void RpcLockPlayer(bool locked)
    {
        LockPlayer(locked);
    }

    public void LockPlayer(bool isLocked)
    {
        locked = isLocked;
        GunController gc = gameObject.GetComponent<GunController>();
        if (gc != null)
        {
            gc.locked = locked;
        }
        EManager em = gameObject.GetComponent<EManager>();
        if (em != null)
        {
            em.locked = locked;
        }
        BuildScript bs = gameObject.GetComponent<BuildScript>();
        if (bs != null)
        {
            bs.locked = locked;
        }
        BuyManager bm = gameObject.GetComponent<BuyManager>();
        if (bm != null)
        {
            bm.locked = locked;
        }
        PlayerController pc = gameObject.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.locked = locked;
        }
    }

    public void RepairBuilding(float amount)
    {
        if (bid != null)
        {
            currentHealth += amount;
            if (currentHealth > startingHealth)
            {
                currentHealth = startingHealth;
            }
            UpdateLifeColor();
        }
    }

    public void SetHealth(float amount)
    {
        currentHealth = amount;
        if (currentHealth > startingHealth)
        {
            currentHealth = startingHealth;
        }
        if (currentHealth < 1)
        {
            currentHealth = 1;
        }
        if (bid != null)
        {
            UpdateLifeColor();
        }
    }

    public void UpdateLifeColor()
    {
        bool hasChildren = gameObject.transform.childCount > 0 && gameObject.GetComponentInChildren<ParticleSystem>() == null;
        NetworkIdentity nid = gameObject.GetComponent<NetworkIdentity>();

        if (currentHealth > startingHealth * 3 / 4)
        {
            CmdChangeObjectColor(nid.netId, 1f, 1f, 1f, hasChildren);
        }
        else if (currentHealth > startingHealth * 2 / 4)
        {
            CmdChangeObjectColor(nid.netId, 1f, .8f, .8f, hasChildren);
        }
        else if (currentHealth > startingHealth / 4)
        {
            CmdChangeObjectColor(nid.netId, 1f, .6f, .6f, hasChildren);
        }
        else if (currentHealth > 0)
        {
            CmdChangeObjectColor(nid.netId, 1f, .4f, .4f, hasChildren);
        }
    }

    public bool TakeDamage(float damage) {
        if (isVulnerable && !isDead)
        {
            float priorHealth = currentHealth;
            currentHealth -= damage;
            if (team != null)
            {
                if (priorHealth > startingHealth * 3 / 4 && currentHealth <= startingHealth * 3 / 4)
                {
                    emperor.GetComponent<EmperorController>().CmdAddEntertainment(1);
                }
                else if (priorHealth > startingHealth * 2 / 4 && currentHealth <= startingHealth * 2 / 4)
                {
                    emperor.GetComponent<EmperorController>().CmdAddEntertainment(1);
                }
                else if (priorHealth > startingHealth / 4 && currentHealth <= startingHealth / 4)
                {
                    emperor.GetComponent<EmperorController>().CmdAddEntertainment(1);
                }
            }
            else
            {
                UpdateLifeColor();
                if (bid == null)
                {
                    if (gameObject.tag == "RedSpawnCore")
                    {
                        CmdSendSpawnDamageAlert(0);
                    }
                    else if (gameObject.tag == "BlueSpawnCore")
                    {
                        CmdSendSpawnDamageAlert(1);
                    }
                }
            }
            if (currentHealth <= 0)
            {
                Die();
                return true;
            }
            Debug.Log("Hit: " + currentHealth);
        }
        else
        {
            Debug.Log("This object is invulnerable");
        }
        return false;
    }

    [Command]
    public void CmdSendSpawnDamageAlert(int team)
    {
        RpcSendSpawnDamageAlert(team);
    }

    [ClientRpc]
    public void RpcSendSpawnDamageAlert(int team)
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }
        lookText.text = (team == 0 ? "Red" : "Blue") + " team's Spawn Core is under attack!";
        lookText.enabled = true;
        damageRoutine = TextDelayer();
        StartCoroutine(damageRoutine);
    }

    public IEnumerator TextDelayer()
    {
        float remainingTime = 3f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }

        lookText.enabled = false;

    }

    [Command]
    public void CmdChangeObjectColor(NetworkInstanceId nid, float r, float g, float b, bool hasChildren)
    {
        RpcChangeObjectColor(nid, r, g, b, hasChildren);
    }

    [ClientRpc]
    public void RpcChangeObjectColor(NetworkInstanceId nid, float r, float g, float b, bool hasChildren)
    {

        GameObject mainGameObj = ClientScene.FindLocalObject(nid);
        if (hasChildren)
        {
            for (int counter = 0; counter < mainGameObj.transform.childCount; counter++)
            {
                Transform childTrans = mainGameObj.transform.GetChild(counter);
                childTrans.GetComponent<Renderer>().material.color = new Color(r, g, b);
            }
        }
        else
        {
            mainGameObj.GetComponent<Renderer>().material.color = new Color(r, g, b);
        }
    }

    private void OnCurrentHealthChange(float newHealth)
    {
        if (newHealth > startingHealth)
        {
            newHealth = startingHealth;
        }
        if (newHealth < 0)
        {
            newHealth = 0;
        }
        currentHealth = newHealth;
        if (onHealthChanged != null)
            onHealthChanged(currentHealth, newHealth);
    }

    private void OnCurrentFatigueChange(float newFatigue)
    {
        if (newFatigue > startingFatigue)
        {
            newFatigue = startingFatigue;
        }
        if (newFatigue < 0)
        {
            newFatigue = 0;
        }
        currentFatigue = newFatigue;
        if (onFatigueChanged != null)
            onFatigueChanged(currentFatigue, newFatigue);
    }

    [Command]
    public void CmdChangeFatigue(float amount)
    {
        RpcChangeFatigue(amount);
    }

    [ClientRpc]
    public void RpcChangeFatigue(float amount)
    {
        currentFatigue += amount;
    }

    [Command]
    public void CmdDie()
    {
        RpcDie();
    }

    [ClientRpc]
    public void RpcDie()
    {
        Die();
    }

    public void Die()
    {
        
        
		tempMesh = mesh;
        isDead = true;
        if (team != null)
        {
            col.enabled = false;
            rb.useGravity = false;
            RpcChangeDeadStatus(false);

        }
        else if (bid != null)
        {
            Destroy(gameObject);
            GameObject tempBase;
            if (bid.team == 0)
            {
                tempBase = GameObject.Find("Base1Center");
            }
            else
            {
                tempBase = GameObject.Find("Base2Center");
            }
            BaseBuildings tempBuilding = tempBase.GetComponent<BaseBuildings>();
            tempBuilding.CmdDestroyMountPoint(bid.parentMountPoint, bid.parentMountBool, bid.id, bid.team);
        }
        else
        {
            Destroy(gameObject);
        }
        if (isServer)
        {
            if (team != null)
            {
                emperor.GetComponent<EmperorController>().CmdAddEntertainment(5);
                if (team.team == 0)
                {
                    emperor.GetComponent<EmperorController>().CmdAddBlueFavor(5);
                }
                else
                {
                    emperor.GetComponent<EmperorController>().CmdAddRedFavor(5);
                }
            }
            else if (bid != null)
            {
                emperor.GetComponent<EmperorController>().CmdAddEntertainment(3);
                if (bid.team == 0)
                {
                    emperor.GetComponent<EmperorController>().CmdAddBlueFavor(3);
                }
                else
                {
                    emperor.GetComponent<EmperorController>().CmdAddRedFavor(3);
                }
            }
            else
            {
                emperor.GetComponent<EmperorController>().CmdAddEntertainment(30);
            }
        }
        
        //rend.enabled = false;
    }

    [Command]
    public void CmdChangeUseGravity(bool value)
    {
        RpcChangeDeadStatus(value);
    }

    [ClientRpc]
    public void RpcChangeDeadStatus(bool value)
    {
        col.enabled = value;
        rb.useGravity = value;
    }
    public void Respawn()
    {
		mesh = tempMesh;
        isDead = false;
        
        //rend.enabled = true;
        col.enabled = true;
        currentHealth = startingHealth;
        currentFatigue = startingFatigue;
        isDead = false;
        rb.useGravity = true;
        RpcChangeDeadStatus(true);
        if (isServer)
        {
            team.RpcChangeLocation(LobbyManager.s_Singleton.GetSpawnLocation(team.team));
            gunScript.CmdSwitch(0);
            gunScript.RpcResetAmmo(true);
        }
        if (isLocalPlayer)
        {
            gameObject.GetComponent<PlayerController>().isSniping = false;
        }
    }
    
}

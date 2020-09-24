using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour {

    public Text txtLoadedAmmo;
    public Text txtReserveAmmo;
    public Text txtPlayerHealth;
    public Text txtPlayerFatigue;

    public GunController gunController;
    public Target playerTarget;
    public RectTransform healthbar;
    public RectTransform fatiguebar;

    // Use this for initialization
    void Start () {
        // if this player is not the local player...
        if (!isLocalPlayer)
        {
            // then remove this script. By removing this script all the rest of the code will not run.
            Destroy(this);
            return;
        }
        gunController = GetComponent<GunController>();
        txtLoadedAmmo = GameObject.Find("UIWILoadedAmmo").GetComponent<Text>();
        txtReserveAmmo = GameObject.Find("UIWIReserveAmmo").GetComponent<Text>();
        playerTarget = GetComponent<Target>();
        playerTarget.onHealthChanged += HealthChanged;
        playerTarget.onFatigueChanged += FatigueChanged;
        txtPlayerHealth = GameObject.Find("UIPHBText").GetComponent<Text>();
        txtPlayerFatigue = GameObject.Find("UIPFBText").GetComponent<Text>();
        healthbar = GameObject.Find("UIPHBValue").GetComponent<RectTransform>();
        fatiguebar = GameObject.Find("UIPFBValue").GetComponent<RectTransform>();
        txtPlayerHealth.text = string.Format("{0:N0}", playerTarget.currentHealth) + " | " + string.Format("{0:N0}", playerTarget.startingHealth);
        txtPlayerFatigue.text = string.Format("{0:N0}", playerTarget.currentFatigue) + " | " + string.Format("{0:N0}", playerTarget.startingFatigue);
    }

    private void HealthChanged(float prevVal, float newVal)
    {
        Debug.Log("UIManager Health Changed");
        if (healthbar != null)
        {
            Debug.Log("Healthbar UI Changed. Was: " + string.Format("{0:N2}", prevVal) + "; Now: " + string.Format("{0:N2}", prevVal));
            healthbar.sizeDelta = new Vector2(newVal * 5, healthbar.sizeDelta.y);
            txtPlayerHealth.text = string.Format("{0:N0}", playerTarget.currentHealth) + " | " + string.Format("{0:N0}", playerTarget.startingHealth);
        }
    }

    private void FatigueChanged(float prevVal, float newVal)
    {
        Debug.Log("UIManager Fatigue Changed");
        if (fatiguebar != null)
        {
            Debug.Log("Fatiguebar UI Changed. Was: " + string.Format("{0:N2}", prevVal) + "; Now: " + string.Format("{0:N2}", prevVal));
            fatiguebar.sizeDelta = new Vector2(newVal * 5, fatiguebar.sizeDelta.y);
            txtPlayerFatigue.text = string.Format("{0:N0}", playerTarget.currentFatigue) + " | " + string.Format("{0:N0}", playerTarget.startingFatigue);
        }
    }

    // Update is called once per frame
    void Update () {
        // Only update the text if there is a reason to update it.
        if (gunController.currentAmmoInMag.ToString() != txtLoadedAmmo.text)
        {
            txtLoadedAmmo.text = gunController.currentAmmoInMag.ToString();
        }
        // Only update the text if there is a reason to update it.
        if (gunController.currentAmmoInReserve.ToString() != txtReserveAmmo.text)
        {
            txtReserveAmmo.text = gunController.currentAmmoInReserve.ToString();
        }
        // Only update the text if there is a reason to update it.
        //if (playerTarget.currentHealth.ToString() != txtPlayerHealth.text.Substring(txtPlayerHealth.text.IndexOf(":") + 2, txtPlayerHealth.text.IndexOf("/") - (txtPlayerHealth.text.IndexOf(":") + 2)))
        //{
        //    txtPlayerHealth.text = txtPlayerHealth.text.Substring(0, txtPlayerHealth.text.IndexOf(":")) + ": " + playerTarget.currentHealth.ToString() + "/" + playerTarget.startingHealth.ToString();
        //}
    }
}

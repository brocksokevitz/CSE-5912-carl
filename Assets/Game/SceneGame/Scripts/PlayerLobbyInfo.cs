using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerLobbyInfo : NetworkBehaviour {

    [SyncVar]
    public Color playerColor;
    [SyncVar]
    public string playerName;

    //public Text playerNameIndicator;

    MeshRenderer[] rends;
    SkinnedMeshRenderer[] skinRends;

	// Use this for initialization
	void Start ()
    {
        // 
        rends = GetComponentsInChildren<MeshRenderer>();
        /*for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material.color = playerColor;
        }*/
        skinRends = GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < skinRends.Length; i++)
        {
            skinRends[i].material.color = playerColor;
        }
        //playerNameIndicator.text = playerName;
        Debug.Log("Player Color: " + playerColor.ToString());
        Debug.Log("Player Name: " + playerName);
	}
	
	public void HidePlayer()
    {
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material.color = Color.clear;
        }
        for (int i = 0; i < skinRends.Length; i++)
        {
            skinRends[i].material.color = Color.clear;
        }
    }
}

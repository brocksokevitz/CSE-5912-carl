using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;
    Camera mainCamera;

	// Use this for initialization
	void Start () {

        if (!isLocalPlayer) {
            for (int i=0; i<componentsToDisable.Length; i++) {
                componentsToDisable[i].enabled = false;
            }
        }
        else
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(false);
            }
        }

	}
	
	void OnDisable () {

        if (mainCamera != null) {
            mainCamera.gameObject.SetActive(true);
        }

	}
}

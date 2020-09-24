using UnityEngine;

public class Billboard : MonoBehaviour {

    public static Camera CameraToFocusOn { get; set; }

    // Update is called once per frame
    void Update () {
        // CameraToFocusOn should be set by the PlayerController script since it has access to the isLocalPlayer property
        if (CameraToFocusOn != null)
        {
            transform.LookAt(CameraToFocusOn.transform);
            transform.Rotate(new Vector3(0, 180, 0), Space.Self);
        }
	}
}

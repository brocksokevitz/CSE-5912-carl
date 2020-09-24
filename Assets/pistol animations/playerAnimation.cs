using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class playerAnimation : NetworkBehaviour {

    [SerializeField]
    private float lookSensitivity = 5f;

    private float inputH;
    private float inputV;
    float xRotation;
    float aimOffset;
    float aim;
    bool isDead;
    bool sprint;
    public Transform head;
    public Animator anim;
    public Target target;
    Camera mainCamera;

    // Use this for initialization
    void Start () {
        if (!isLocalPlayer)
            return;
        anim = GetComponent<Animator>();
        aimOffset = 0.04f;
        //mainCamera = Camera.main;
        //mainCamera.transform.position = head.position;
        isDead = false;
        target = GetComponent<Target>();
    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
            return;
        xRotation -= Input.GetAxis("Mouse Y") * lookSensitivity;
        if (xRotation > 80f)
        {
            xRotation = 80f;
        }
        else if (xRotation < -80f)
        {
            xRotation = -80f;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKey(KeyCode.LeftShift)) && target.currentFatigue != 0.00)
        {
            sprint = true;
        }
        else
        {
            sprint = false;
        }

        //Debug.Log(xRotation);
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");
        isDead = gameObject.GetComponent<Target>()._isDead;

        //mainCamera.transform.rotation = Quaternion.Euler(xRotation, 0, 0);
        //mainCamera.transform.position = head.position;

        anim.SetFloat("inputH", inputH);
        anim.SetFloat("inputV", inputV);
        anim.SetFloat("xRotation", -xRotation);
        anim.SetBool("isDead", isDead);
        anim.SetBool("sprinting", sprint);

    }
    
    
}

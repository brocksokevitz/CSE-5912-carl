using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newAnimator : MonoBehaviour
{

    [SerializeField]
    private float lookSensitivity = 5f;

    private float inputH;
    private float inputV;
    float xRotation;
    float aimOffset;
    float aim;
    public Transform head;
    public Animator anim;
    Camera mainCamera;

    // Use this for initialization
    void Start()
    {
        //if (!isLocalPlayer)
        //    return;
        anim = GetComponent<Animator>();
        aimOffset = 0.04f;
        mainCamera = Camera.main;
        mainCamera.transform.position = head.position;    
            
            }

    // Update is called once per frame
    void Update()
    {
        //if (!isLocalPlayer)
        //    return;
        xRotation -= Input.GetAxis("Mouse Y") * lookSensitivity;
        if (xRotation > 80f)
        {
            xRotation = 80f;
        }
        else if (xRotation < -80f)
        {
            xRotation = -80f;
        }

        if (Input.GetKeyDown("1"))
        {
            anim.Play("HM_Aim_Revolver_Walk");
        }

        Debug.Log(xRotation);
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");

        //mainCamera.transform.rotation = Quaternion.Euler(xRotation, 0, 0);
        //mainCamera.transform.position = head.position;

        anim.SetFloat("inputH", inputH);
        anim.SetFloat("inputV", inputV);
        anim.SetFloat("xRotation", -xRotation);

    }


}

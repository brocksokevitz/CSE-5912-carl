using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Variables")]
    [SerializeField]
    private float jumpSensitivity = 1500f;
    [SerializeField]
    private float lookSensitivity = 5f;

    [Header("First Person Camera Position")]
    [SerializeField]
    public bool locked;
    float fpCameraY = 0.45f;                 // The height off of the ground that the camera should be
    [SerializeField]
    float fpCameraX = 0f;                    // The height off of the ground that the camera should be
    [SerializeField]
    float fpCameraZ = 0.2f;                  // The height off of the ground that the camera should be

    [Header("Third Person Camera Position")]
    [SerializeField]
    float tpCameraDistance = 6f;            // Distance from the player that the camera should be
    [SerializeField]
    float tpCameraY = 7f;                   // The height off of the ground that the camera should be
    [SerializeField]
    bool isFirstPerson = true;

    [Header("UI")]
    //[SerializeField]
    //GameObject HUDLayout;

    Vector3 tpCameraOffset;
    Vector3 fpCameraOffset;


    //[SerializeField]
    //private Image crosshair; 

    float xRotation;
    float yRotation;
    float xRotationV;
    float yRotationV;
    float lookSmoothDamp = 0.1f;
    public float speed;
    float walkingSpeed = 6f;
    float sprintSpeed = 10f;
    Vector3 flatTransform;
    public GameObject mainCamera;
    public bool isSniping;
    public GunController gc;
    public Camera camera;
    public GameObject[] snipeObjects;
    public Target target;
    public float jumpCooldown;

    private Rigidbody rb;
    //private GameObject clientHUD;

    // Use this for initialization
    void Start()
    {
        locked = false;
        jumpCooldown = 0f;
        // if this player is not the local player...
        if (!isLocalPlayer)
        {
            mainCamera.GetComponent<AudioListener>().enabled = false;
            // then remove this script. By removing this script all the rest of the code will not run.
            Destroy(this);
            return;
        }
        //clientHUD = Instantiate(HUDLayout);
        //clientHUD.name = HUDLayout.name;
        //clientHUD.transform.SetParent(GameObject.Find("_UI").transform);
        //clientHUD.transform.localScale = Vector3.one;
        //clientHUD.transform.localPosition = new Vector3(0, 0, 0);
        //clientHUD.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        //clientHUD.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        Billboard.CameraToFocusOn = GetComponentInChildren<Camera>();
        //crosshair.enabled = true;
        rb = GetComponent<Rigidbody>();
        gc = GetComponent<GunController>();
        tpCameraOffset = new Vector3(0f, tpCameraY, -tpCameraDistance);
        fpCameraOffset = new Vector3(fpCameraX, fpCameraY, fpCameraZ);
        camera = mainCamera.GetComponent<Camera>();
        target = GetComponent<Target>();
        //mainCamera = transform.GetChild(0);
        //MoveCamera();
        //GameObject.FindObjectOfType<AudioListener>().transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool playWalk = false;
        if (!locked)
        {
            xRotation -= Input.GetAxis("Mouse Y") * lookSensitivity;
            if (xRotation > 70)
            {
                xRotation = 70;
            }
            else if (xRotation < -80)
            {
                xRotation = -80;
            }

            if (Input.GetKey(KeyCode.LeftShift) && target.currentFatigue > 0)
            {
                speed = sprintSpeed;
                if (target != null)
                {
                    target.CmdChangeFatigue(-Time.deltaTime * 25);
                }
            }
            else
            {
                speed = walkingSpeed;
                if (target != null && !Input.GetKey(KeyCode.LeftShift) && target.currentFatigue < 100)
                {
                    target.CmdChangeFatigue(Time.deltaTime * 25);
                }
            }

            yRotation += Input.GetAxis("Mouse X") * lookSensitivity;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);


            //Vector3 direction = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * speed, 0, Input.GetAxis("Vertical") * Time.deltaTime * speed);
            //transform.Translate(direction); //Old movement  <--- + ^^^

            Vector3 direction = (transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * speed) + (transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * speed);
            rb.MovePosition(rb.position + direction); 
            //Debug.Log(rb.rotation);
            //Debug.Log(direction);

            playWalk = direction.magnitude > 0;
            
            if (jumpCooldown > 0f)
            {
                jumpCooldown += Time.deltaTime;
                if (jumpCooldown >= 1f)
                {
                    jumpCooldown = 0f;
                }
            }
            Ray jumpRay = new Ray(transform.position, -Vector3.up);
            RaycastHit jumpRayhit;

            Vector3 jumpForce = Vector3.zero;
            if (Physics.Raycast(jumpRay, out jumpRayhit, 0.5f))
            {
                if (Input.GetButtonDown("Jump") && jumpCooldown == 0f)
                {
                    jumpForce = Vector3.up * jumpSensitivity;
                    rb.AddForce(jumpForce);
                    jumpCooldown += Time.deltaTime;
                    //rb.AddForce(jumpForce, ForceMode.Acceleration);
                }
            }
            else
            {
                if (rb != null)
                {
                    rb.AddForce(Physics.gravity, ForceMode.Acceleration);
                }
            }
            if (gc != null && gc.sniper && Input.GetButtonUp("Fire2"))
            {
                isSniping = !isSniping;
            }
            if (gc != null && !gc.sniper && isSniping)
            {
                isSniping = !isSniping;
            }
        }
        else
        {
            if (target != null && target.currentFatigue < 100)
            {
                target.CmdChangeFatigue(Time.deltaTime * 25);
            }
            rb.angularVelocity = new Vector3(0f, 0f, 0f);
        }
        if (playWalk)
        {
            if (!target.walking.isPlaying)
            {
                target.CmdPlayWalkingSound();
            }
        }
        else
        {
            if (target.walking.isPlaying)
            {
                target.CmdStopWalkingSound();
            }
        }
    }

    public void LateUpdate()
    {
        if (!locked)
        {
            // Update the camera's position/rotation
            MoveCamera();
        }
    }

    void MoveCamera()
    {
        //mainCamera.position = transform.position;
        //mainCamera.rotation = transform.rotation;
        if (isFirstPerson)
        {
            //fpCameraOffset = new Vector3(fpCameraX, fpCameraY, fpCameraZ);
            //mainCamera.Translate(fpCameraOffset);
            if (camera != null)
            {
                if (isSniping && camera.fieldOfView == 70)
                {
                    foreach (GameObject tempSnipe in snipeObjects)
                    {

                        tempSnipe.SetActive(false);
                    }
                    camera.fieldOfView = 20;
                }
                else if (!isSniping && camera.fieldOfView == 20)
                {
                    foreach (GameObject tempSnipe in snipeObjects)
                    {
                        tempSnipe.SetActive(true);
                    }
                    camera.fieldOfView = 70;
                }
            }
            
            //mainCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
        else
        {
            tpCameraOffset = new Vector3(0f, tpCameraY, -tpCameraDistance);
            //mainCamera.transform.Translate(tpCameraOffset);
            //mainCamera.transform.LookAt(transform);
        }
    }

    void OnDisable()
    {
        //death state
    }

    void OnStopServer()
    {
        //Destroy(clientHUD);
    }
}

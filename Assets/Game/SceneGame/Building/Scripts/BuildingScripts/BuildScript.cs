using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BuildScript : NetworkBehaviour
{
    //TODO: Re-add preview material
    //Fix placement rotations to be type independent
    //Clean up code (Getcomponents) and overall structure
    //Snapped and move away and click causes mount point to be removed
    public List<Material> materials = new List<Material>();
    
    public bool buildMode;
    public Material invalidMaterial;
    public Material validMaterial;
    public List<GameObject> objects = new List<GameObject>();
    public Transform baseParent;
    public BaseBuildings baseBuildings;
    public Transform camera;
    public GameObject floorPrefab;

    GameObject previewObject;
    BuildPoints previewBuildPoints;
    MeshRenderer meshRend;
    int currentObject;
    Material originalMaterial;


    public GameObject BuildMenu;
    public GameObject BuyMenu;
    public GunController gun;
    public PlayerTeam team;
    public BuyManager buyManager;
    int snapMountIndex = -1;
    int snapBoolIndex = -1;
    int previousObject = 0;
    public bool locked;

    void Start()
    {
        locked = false;
        currentObject = 0;
        if (isLocalPlayer)
        {
            BuildMenu = GameObject.Find("BuildMenu");
            BuyMenu = GameObject.Find("BuyMenu");
        }
        buyManager = GetComponent<BuyManager>();
        StartCoroutine(LoadDelayer());
    }

    public IEnumerator LoadDelayer()
    {
        float remainingTime = 1.5f;

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;

        }
        if (isLocalPlayer)
        {
            team = GetComponent<PlayerTeam>();
            gun = GetComponent<GunController>();
            buildMode = false;
            baseParent = team.baseObject.transform;
            baseBuildings = team.baseObject.GetComponent<BaseBuildings>();
        }
        
    }

    void Update()
    {
        if (!locked)
        {


            if (Input.GetKeyDown(KeyCode.B))
            {
                buildMode = !buildMode;
            }

            if (isLocalPlayer && buildMode)
            {
                gun.enabled = !buildMode;
                buyManager.buyMode = false;
                BuyMenu.SetActive(false);
                BuildMenu.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SetpreviewObjectObject(0);
                    previousObject = 0;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SetpreviewObjectObject(1);
                    previousObject = 1;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SetpreviewObjectObject(2);
                    previousObject = 2;
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    SetpreviewObjectObject(3);
                    previousObject = 3;
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    SetpreviewObjectObject(4);
                    previousObject = 4;
                }
                if (previewObject != null)
                {
                    PositionPreview();

                    if (Input.GetMouseButtonDown(0))
                    {
                        PlaceObject();
                        SetpreviewObjectObject(previousObject);

                    }
                }
            }
            else
            {
                if (BuildMenu != null)
                {
                    BuildMenu.SetActive(buildMode);
                }
                if (gun != null)
                {
                    gun.enabled = !buildMode;
                }
                if (previewObject != null)
                {
                    Destroy(previewObject);
                }
            }
        }
    }
    void PositionPreview()
    {
        Ray previewRay = new Ray(camera.position, camera.forward);
        RaycastHit previewHit;
        SetMaterial(invalidMaterial);
        previewObject.transform.eulerAngles = new Vector3(0, camera.eulerAngles.y - 90, 0);
        previewBuildPoints.valid = false;
        if (Physics.Raycast(previewRay, out previewHit, 5f))
        {

            previewObject.transform.position = previewHit.point;
            bool colliding = false;
            if (currentObject == 0)
            {
                Matrix4x4 tempMatrix = Matrix4x4.identity;
                tempMatrix[0, 0] = Mathf.Cos(-transform.localEulerAngles.y * Mathf.Deg2Rad);
                tempMatrix[0, 2] = -Mathf.Sin(-transform.localEulerAngles.y * Mathf.Deg2Rad);
                tempMatrix[2, 0] = Mathf.Sin(-transform.localEulerAngles.y * Mathf.Deg2Rad);
                tempMatrix[2, 2] = Mathf.Cos(-transform.localEulerAngles.y * Mathf.Deg2Rad);
                Vector3 localCorner1 = new Vector3(floorPrefab.transform.localScale.x / 2, 0, floorPrefab.transform.localScale.z / 2);
                Vector3 localCorner2 = new Vector3(floorPrefab.transform.localScale.x / 2, 0, -floorPrefab.transform.localScale.z / 2);
                Vector3 localCorner3 = new Vector3(-floorPrefab.transform.localScale.x / 2, 0, -floorPrefab.transform.localScale.z / 2);
                Vector3 localCorner4 = new Vector3(-floorPrefab.transform.localScale.x / 2, 0, floorPrefab.transform.localScale.z / 2);
                localCorner1 = tempMatrix * localCorner1;
                localCorner2 = tempMatrix * localCorner2;
                localCorner3 = tempMatrix * localCorner3;
                localCorner4 = tempMatrix * localCorner4;
                Vector3 corner1 = new Vector3(previewHit.point.x + localCorner1.x, 2, previewHit.point.z + localCorner1.z);
                Vector3 corner2 = new Vector3(previewHit.point.x + localCorner2.x, 2, previewHit.point.z + localCorner2.z);
                Vector3 corner3 = new Vector3(previewHit.point.x + localCorner3.x, 2, previewHit.point.z + localCorner3.z);
                Vector3 corner4 = new Vector3(previewHit.point.x + localCorner4.x, 2, previewHit.point.z + localCorner4.z);
                Debug.DrawRay(corner1, Vector3.up, Color.green);
                Debug.DrawRay(corner2, Vector3.up, Color.green);
                Debug.DrawRay(corner3, Vector3.up, Color.green);
                Debug.DrawRay(corner4, Vector3.up, Color.green);
                colliding = DetermineCollision(corner1, corner2) ||
                    DetermineCollision(corner2, corner3) ||
                    DetermineCollision(corner3, corner4) ||
                    DetermineCollision(corner4, corner1);

            }
            if (!colliding)
            {
                if (currentObject == 0 && previewHit.collider.gameObject.tag == "Ground")
                {
                    SetMaterial(validMaterial);
                    previewBuildPoints.valid = true;
                }
            }
            SnapPreview();
        
        }
        else
        {
            //Snap and check
            previewObject.transform.position = camera.position + previewBuildPoints.offset + previewRay.direction * 5f;
            SnapPreview();
        }



    }

    bool DetermineCollision(Vector3 originPoint, Vector3 targetPoint)
    {
        GameObject[] tempObjs = GameObject.FindGameObjectsWithTag("Building");
        List<GameObject> collidableObjects = new List<GameObject>();
        foreach (GameObject tempObj in tempObjs)
        {
            if ((tempObj.name == "Floor(Clone)" || tempObj.name == "Ceiling(Clone)") && tempObj.transform.parent == null)
            {
                collidableObjects.Add(tempObj);
            }
        }
        Vector2 origin2D = new Vector2(originPoint.x, originPoint.z);
        Vector2 target2D = new Vector2(targetPoint.x, targetPoint.z);

        foreach (GameObject tempObj in collidableObjects)
        {
            Matrix4x4 tempMatrix = Matrix4x4.identity;
            tempMatrix[0, 0] = Mathf.Cos(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
            tempMatrix[0, 2] = -Mathf.Sin(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
            tempMatrix[2, 0] = Mathf.Sin(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
            tempMatrix[2, 2] = Mathf.Cos(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
            Vector3 localCorner1 = new Vector3(tempObj.transform.localScale.x / 2, 0, tempObj.transform.localScale.z / 2);
            Vector3 localCorner2 = new Vector3(tempObj.transform.localScale.x / 2, 0, -tempObj.transform.localScale.z / 2);
            Vector3 localCorner3 = new Vector3(-tempObj.transform.localScale.x / 2, 0, -tempObj.transform.localScale.z / 2);
            Vector3 localCorner4 = new Vector3(-tempObj.transform.localScale.x / 2, 0, tempObj.transform.localScale.z / 2);
            localCorner1 = tempMatrix * localCorner1;
            localCorner2 = tempMatrix * localCorner2;
            localCorner3 = tempMatrix * localCorner3;
            localCorner4 = tempMatrix * localCorner4;
            Vector3 corner1 = new Vector3(tempObj.transform.position.x + localCorner1.x, 2, tempObj.transform.position.z + localCorner1.z);
            Vector3 corner2 = new Vector3(tempObj.transform.position.x + localCorner2.x, 2, tempObj.transform.position.z + localCorner2.z);
            Vector3 corner3 = new Vector3(tempObj.transform.position.x + localCorner3.x, 2, tempObj.transform.position.z + localCorner3.z);
            Vector3 corner4 = new Vector3(tempObj.transform.position.x + localCorner4.x, 2, tempObj.transform.position.z + localCorner4.z);
            Debug.DrawRay(corner1, Vector3.up, Color.yellow);
            Debug.DrawRay(corner2, Vector3.up, Color.yellow);
            Debug.DrawRay(corner3, Vector3.up, Color.yellow);
            Debug.DrawRay(corner4, Vector3.up, Color.yellow);
            Vector2 corner2D1 = new Vector2(corner1.x, corner1.z);
            Vector2 corner2D2 = new Vector2(corner2.x, corner2.z);
            Vector2 corner2D3 = new Vector2(corner3.x, corner3.z);
            Vector2 corner2D4 = new Vector2(corner4.x, corner4.z);
            if (ChecksCollide(origin2D, target2D, corner2D1, corner2D2) ||
                ChecksCollide(origin2D, target2D, corner2D2, corner2D3) ||
                ChecksCollide(origin2D, target2D, corner2D3, corner2D4) ||
                ChecksCollide(origin2D, target2D, corner2D4, corner2D1))
            {
                return true;
            }
        }
        return false;
    }

    bool ChecksCollide(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        if (((p4.x - p3.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p4.y - p3.y)) == 0)
        {
            return false;
        }
        float ta = ((p3.y - p4.y) * (p1.x - p3.x) + (p4.x - p3.x) * (p1.y - p3.y)) /
            ((p4.x - p3.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p4.y - p3.y));
        float tb = ((p1.y - p2.y) * (p1.x - p3.x) + (p2.x - p1.x) * (p1.y - p3.y)) /
            ((p4.x - p3.x) * (p1.y - p2.y) - (p1.x - p2.x) * (p4.y - p3.y));
        
        if (ta >= 0 && ta <= 1f && tb >= 0 && tb <= 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void SnapPreview()
    {
        bool snapped = false;
        int mpCounter = 0;
        foreach (MountPoint mp in baseBuildings.mountPoints)
        {
            GameObject[] tempObjs = GameObject.FindGameObjectsWithTag("Building");
            GameObject tempObj = null;
            foreach (GameObject currentObj in tempObjs)
            {
                BuildIdentifier tempBuild = currentObj.GetComponent<BuildIdentifier>();
                if (tempBuild.id == mp.objectID && tempBuild.team == mp.team)
                {
                    tempObj = currentObj;
                    break;
                }
            }
            for (int j = 0; j < mp.pointType.Length; j++)
            {

                if (mp.pointType[j] == previewBuildPoints.type)
                {
                    Vector3 v3 = mp.points[j];
                    if (!(baseBuildings.pointUsed[mpCounter].boolList[j]))
                    {
                        Vector3 newVec3 = tempObj.transform.TransformPoint(new Vector3(v3.x / (tempObj.transform.localScale.x), v3.y / (tempObj.transform.localScale.y), v3.z / (tempObj.transform.localScale.z)));
                        //Matrix4x4 m = Matrix4x4.TRS(mp.parentPosition, mp.parentRotation, mp.parentScale);
                        //Vector3 newVec3 = m.MultiplyPoint3x4(new Vector3(v3.x / mp.parentScale.x, v3.y / mp.parentScale.y, v3.z / mp.parentScale.z));
                        Debug.DrawRay(newVec3, Vector3.up, Color.red);

                        bool colliding = false;
                        if (previewBuildPoints.type == BuildPoints.MountType.Floor1 || previewBuildPoints.type == BuildPoints.MountType.Ceiling1)
                        {
                            Vector3 radius = newVec3 - tempObj.transform.position;
                            radius.x = radius.x / 2f;
                            radius.z = radius.z / 2f;
                            Vector3 perp1 = new Vector3(radius.z * .95f, 2, -radius.x * .95f);
                            Vector3 perp2 = -perp1;
                            Vector3 corner1 = new Vector3(tempObj.transform.position.x + radius.x * 1.05f + perp1.x, 2, 
                                tempObj.transform.position.z + radius.z * 1.05f + perp1.z);
                            Vector3 corner2 = new Vector3(tempObj.transform.position.x + radius.x * 1.05f + perp2.x, 2, 
                                tempObj.transform.position.z + radius.z * 1.05f + perp2.z);
                            Vector3 corner3 = new Vector3(corner2.x + radius.x * 2 * .95f, 2, corner2.z + radius.z * 2 * .95f);
                            Vector3 corner4 = new Vector3(corner1.x + radius.x * 2 * .95f, 2, corner1.z + radius.z * 2 * .95f);
                            Debug.DrawRay(corner1, Vector3.up, Color.cyan);
                            Debug.DrawRay(corner2, Vector3.up, Color.cyan);
                            Debug.DrawRay(corner3, Vector3.up, Color.cyan);
                            Debug.DrawRay(corner4, Vector3.up, Color.cyan);
                            colliding = DetermineCollision(corner1, corner2) ||
                                DetermineCollision(corner2, corner3) ||
                                DetermineCollision(corner3, corner4) ||
                                DetermineCollision(corner4, corner1);
                        }
                        if (!colliding)
                        {
                            if (Vector3.Distance(previewObject.transform.position, newVec3) < 2)
                            {
                                bool near = false;
                                foreach (Vector3 nearV3 in baseBuildings.placedObjects)
                                {
                                    if (Vector3.Distance(nearV3, newVec3) < 0.1f)
                                    {
                                        near = true;
                                    }
                                }
                                if (!near)
                                {
                                    snapMountIndex = mpCounter;
                                    snapBoolIndex = j;
                                    snapped = true;
                                    previewObject.transform.position = newVec3 + previewBuildPoints.offset;
                                    previewObject.transform.LookAt(tempObj.transform);
                                    previewObject.transform.localEulerAngles = new Vector3(0, previewObject.transform.localEulerAngles.y - 90, 0);
                                    if (previewBuildPoints.type == mp.parentMountType)
                                    {
                                        previewObject.transform.localEulerAngles = tempObj.transform.localEulerAngles;
                                    }
                                    if (previewBuildPoints.type == BuildPoints.MountType.Stair1)
                                    {
                                        Matrix4x4 tempMatrix = Matrix4x4.identity;
                                        tempMatrix[0, 0] = Mathf.Cos(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
                                        tempMatrix[0, 2] = -Mathf.Sin(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
                                        tempMatrix[2, 0] = Mathf.Sin(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
                                        tempMatrix[2, 2] = Mathf.Cos(-tempObj.transform.localEulerAngles.y * Mathf.Deg2Rad);
                                        Vector3 localCorner1 = new Vector3(tempObj.transform.localScale.x / 2, 0, tempObj.transform.localScale.z / 2);
                                        Vector3 localCorner2 = new Vector3(tempObj.transform.localScale.x / 2, 0, -tempObj.transform.localScale.z / 2);
                                        Vector3 localCorner3 = new Vector3(-tempObj.transform.localScale.x / 2, 0, -tempObj.transform.localScale.z / 2);
                                        Vector3 localCorner4 = new Vector3(-tempObj.transform.localScale.x / 2, 0, tempObj.transform.localScale.z / 2);
                                        localCorner1 = tempMatrix * localCorner1;
                                        localCorner2 = tempMatrix * localCorner2;
                                        localCorner3 = tempMatrix * localCorner3;
                                        localCorner4 = tempMatrix * localCorner4;
                                        Vector3 worldCorner1 = tempObj.transform.position + localCorner1;
                                        Vector3 worldCorner2 = tempObj.transform.position + localCorner2;
                                        Vector3 worldCorner3 = tempObj.transform.position + localCorner3;
                                        Vector3 worldCorner4 = tempObj.transform.position + localCorner4;
                                        float distCorner1 = (worldCorner1 - gameObject.transform.position).magnitude;
                                        float distCorner2 = (worldCorner2 - gameObject.transform.position).magnitude;
                                        float distCorner3 = (worldCorner3 - gameObject.transform.position).magnitude;
                                        float distCorner4 = (worldCorner4 - gameObject.transform.position).magnitude;
                                        if (distCorner1 < distCorner3 && distCorner1 < distCorner4 && distCorner2 < distCorner3 && distCorner2 < distCorner4)
                                        {
                                            previewObject.transform.localEulerAngles = new Vector3(0, tempObj.transform.localEulerAngles.y + 90, 0);
                                        }
                                        else if (distCorner2 < distCorner4 && distCorner2 < distCorner1 && distCorner3 < distCorner4 && distCorner3 < distCorner1)
                                        {
                                            previewObject.transform.localEulerAngles = new Vector3(0, tempObj.transform.localEulerAngles.y + 180, 0);
                                        }
                                        else if (distCorner3 < distCorner1 && distCorner3 < distCorner2 && distCorner4 < distCorner1 && distCorner4 < distCorner2)
                                        {
                                            previewObject.transform.localEulerAngles = new Vector3(0, tempObj.transform.localEulerAngles.y + 270, 0);
                                        }
                                        else
                                        {
                                            previewObject.transform.localEulerAngles = new Vector3(0, tempObj.transform.localEulerAngles.y, 0);
                                        }

                                    }
                                    if (previewBuildPoints.type == BuildPoints.MountType.Wall
                                        && mp.parentMountType == BuildPoints.MountType.Door1)
                                    {
                                        previewObject.transform.localEulerAngles = tempObj.transform.localEulerAngles;
                                    }
                                    if (previewBuildPoints.type == BuildPoints.MountType.Door1
                                       && mp.parentMountType == BuildPoints.MountType.Wall)
                                    {
                                        previewObject.transform.localEulerAngles = tempObj.transform.localEulerAngles;
                                    }
                                    SetMaterial(validMaterial);
                                    previewBuildPoints.valid = true;
                                    
                                }
                            }
                        }
                    }
                }
            }
            mpCounter++;
        }
        if (!snapped && snapMountIndex > -1)
        {
            snapMountIndex = -1;
            snapBoolIndex = -1;
        }
    }

    void SetpreviewObjectObject(int id)
    {
        BuildIdentifier bid = objects[id].GetComponent<BuildIdentifier>();
        ResourceBank rba = team.baseObject.GetComponent<ResourceBank>();
        if (bid.woodCost <= rba.wood && bid.stoneCost <= rba.stone && bid.metalCost <= rba.metal)
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
            }
            currentObject = id;
            previewObject = Instantiate(objects[currentObject], baseParent);

            if (previewObject.transform.childCount > 0)
            {
                for (int counter = 0; counter < previewObject.transform.childCount; counter++)
                {
                    previewObject.transform.GetChild(counter).gameObject.GetComponent<BoxCollider>().enabled = false;
                }
            }
            else
            {
                previewObject.GetComponent<BoxCollider>().enabled = false;
            }
            meshRend = previewObject.GetComponent<MeshRenderer>();
            SaveMaterials();
            SetMaterial(invalidMaterial);
            previewObject.layer = 2;
            previewBuildPoints = previewObject.GetComponent<BuildPoints>();
            previewObject.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
    
    void PlaceObject()
    {
        if (previewBuildPoints.valid)
        {
            //previewObject.name = (previewBuildPoints.type.ToString() + " Placed");
            BuildPoints bp = previewBuildPoints;
            int mountIndex = baseBuildings.mountPoints.Count;
            bp.mounting.SetTeam(team.team);
            bp.mounting.SetObjectID(mountIndex);
            CmdAddMountPoint(bp.mounting, team.team);
            SyncStructBool tempBools;
            tempBools.boolList = new bool[0];
            foreach (Vector3 v3 in bp.mounting.points)
            {
                bool used = false;
                tempBools.Add(used);
            }
            CmdAddPointUsed(tempBools, team.team);
            ResetMaterials();
            CmdAddPlacedObject(previewObject.transform.position - previewBuildPoints.offset, team.team);
            previewObject.layer = 0;
            Vector3 poPosition = previewObject.transform.position;
            Quaternion poRotation = previewObject.transform.rotation;
            Destroy(previewObject);
            CmdSpawnBuildingPart(objects[currentObject].ToString(), currentObject, poPosition,
                poRotation, mountIndex, team.team, snapMountIndex, snapBoolIndex);
            BuildIdentifier bid = objects[currentObject].GetComponent<BuildIdentifier>();
            CmdChangeResources(-bid.woodCost, -bid.stoneCost, -bid.metalCost, team.team);
            previewObject = null;
            meshRend = null;
        }
    }

    [Command]
    public void CmdChangeResources(int woodCost, int stoneCost, int metalCost, int team)
    {
        ResourceBank rba;
        if (team == 0)
        {
            rba = GameObject.Find("Base1Center").GetComponent<ResourceBank>();
        }
        else
        {
            rba = GameObject.Find("Base2Center").GetComponent<ResourceBank>();
        }
        rba.Add("Wood", woodCost);
        rba.Add("Stone", stoneCost);
        rba.Add("Metal", metalCost);
    }

    void SaveMaterials()
    {
        materials.Clear();
        if (meshRend != null)
        materials.Add(meshRend.material);
        for (int i = 0; i < previewObject.transform.childCount; i++)
        {
            MeshRenderer rend = previewObject.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (rend != null)
                materials.Add(rend.material);
        }

    }
    void ResetMaterials()
    {
        int index = 0;
        if (meshRend != null)
        {
            meshRend.material = materials[index];
        }
        else
        {
            index--;
        }
        for (int i = 0; i < previewObject.transform.childCount; i++)
        {
            MeshRenderer rend = previewObject.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.material = materials[index + 1];
                index++;
            }
        }
        materials.Clear();
    }
    void SetMaterial(Material mat)
    {
        if(meshRend != null)
        meshRend.material = mat;
        for (int i = 0; i < previewObject.transform.childCount; i++)
        {
            MeshRenderer rend = previewObject.transform.GetChild(i).GetComponent<MeshRenderer>();
            if (rend != null)
                rend.material = mat;
        }
    }

    [Command]
    void CmdAddPointUsed(SyncStructBool point, int team)
    {
        GameObject baseObj;
        if (team == 0)
        {
            baseObj = GameObject.Find("Base1Center");
        }
        else
        {
            baseObj = GameObject.Find("Base2Center");
        }
        baseObj.GetComponent<BaseBuildings>().AddPointUsed(point);
    }

    [Command]
    void CmdAddMountPoint(MountPoint mp, int team)
    {
        GameObject baseObj;
        if (team == 0)
        {
            baseObj = GameObject.Find("Base1Center");
        }
        else
        {
            baseObj = GameObject.Find("Base2Center");
        }
        baseObj.GetComponent<BaseBuildings>().AddMountPoint(mp);
    }

    [Command]
    void CmdAddPlacedObject(Vector3 vec, int team)
    {
        GameObject baseObj;
        if (team == 0)
        {
            baseObj = GameObject.Find("Base1Center");
        }
        else
        {
            baseObj = GameObject.Find("Base2Center");
        }
        baseObj.GetComponent<BaseBuildings>().AddPlacedObject(vec);
    }

    [Command]
    void CmdSpawnBuildingPart(string objectString, int objectID, Vector3 position, Quaternion rotation, int objectIndex, int inTeam, 
        int parentMountPoint, int parentMountBool)
    {
        Debug.Log("Placing " + objectString + "With Object ID:" + currentObject + " at X:" + position.x + "; Y:" + position.y + "; Z:" + position.z);
        GameObject tempObj = objects[objectID];
        BuildIdentifier tempID = tempObj.GetComponent<BuildIdentifier>();
        if (tempObj.transform.childCount > 0)
        {
            for (int counter = 0; counter < tempObj.transform.childCount; counter++)
            {
                tempObj.transform.GetChild(counter).gameObject.GetComponent<BoxCollider>().enabled = true;
            }
        }
        else
        {
            tempObj.GetComponent<BoxCollider>().enabled = true;
        }
        tempID.team = inTeam;
        tempID.id = objectIndex;
        tempID.parentMountPoint = parentMountPoint;
        tempID.parentMountBool = parentMountBool;
        GameObject instance = Instantiate(tempObj, position, rotation);
        NetworkServer.Spawn(instance);
    }
    
    
}

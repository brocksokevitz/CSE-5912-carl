using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BaseBuildings : NetworkBehaviour
{
    public SyncStructList pointUsed = new SyncStructList();
    public MountPointList mountPoints = new MountPointList();
    public SyncStructVector placedObjects = new SyncStructVector();
    // Use this for initialization
    void Start () {

    }

    [Command]
    public void CmdResetBuildings()
    {
        pointUsed.Clear();
        mountPoints.Clear();
        placedObjects.Clear();
    }

    public void AddPointUsed(SyncStructBool point)
    {
        pointUsed.Add(point);
        //CmdAddPointUsed(point);
    }

    [Command]
    public void CmdDestroyMountPoint(int parentMountPoint, int parentMountBool, int childMountPoint, int team)
    {
        if (parentMountPoint > -1)
        {
            pointUsed[parentMountPoint].boolList[parentMountBool] = false;
        }
        pointUsed.RemoveAt(childMountPoint);
        mountPoints.RemoveAt(childMountPoint);
        placedObjects.RemoveAt(childMountPoint);
        for (int counter = childMountPoint; counter < mountPoints.Count; counter++)
        {
            MountPoint tempPoint = mountPoints[counter];
            tempPoint.objectID--;
            mountPoints[counter] = tempPoint;
        }
        BuildIdentifier[] buildIdentifiers = Component.FindObjectsOfType<BuildIdentifier>();
        foreach (BuildIdentifier tempBuildIdentifiers in buildIdentifiers)
        {
            if (tempBuildIdentifiers.team == team)
            {
                if (tempBuildIdentifiers.parentMountPoint == childMountPoint)
                {
                    tempBuildIdentifiers.parentMountPoint = -1;
                    tempBuildIdentifiers.parentMountBool = -1;
                }
            }
            
        }
        foreach (BuildIdentifier tempBuildIdentifiers in buildIdentifiers)
        {
            if (tempBuildIdentifiers.team == team)
            {
                Debug.Log("CMP: " + childMountPoint + " TBI id: " + tempBuildIdentifiers.id + " TBI mp: " + tempBuildIdentifiers.parentMountPoint);
                tempBuildIdentifiers.DecrementMPIfHigher(childMountPoint);
                tempBuildIdentifiers.DecrementIDIfHigher(childMountPoint);
                Debug.Log("After CMP: " + childMountPoint + " TBI id: " + tempBuildIdentifiers.id + " TBI mp: " + tempBuildIdentifiers.parentMountPoint);
            }
        }
    }

    [Command]
    public void CmdAddPointUsed(SyncStructBool point)
    {
        //pointUsed.Add(point);
        RpcAddPointUsed(point);
    }

    [ClientRpc]
    public void RpcAddPointUsed(SyncStructBool point)
    {
        pointUsed.Add(point);
    }

    public void AddMountPoint(MountPoint mountPoint)
    {
        mountPoints.Add(mountPoint);
        //CmdAddMountPoint(mountPoint);
    }

    [Command]
    public void CmdAddMountPoint(MountPoint mountPoint)
    {
        //mountPoints.Add(mountPoint);
        RpcAddMountPoint(mountPoint);
    }

    [ClientRpc]
    public void RpcAddMountPoint(MountPoint mountPoint)
    {
        mountPoints.Add(mountPoint);
    }

    public void AddPlacedObject(Vector3 placedObject)
    {
        placedObjects.Add(placedObject);
        //CmdAddPlacedObject(placedObject);
    }

    [Command]
    public void CmdAddPlacedObject(Vector3 placedObject)
    {
        //placedObjects.Add(placedObject);
        RpcAddPlacedObject(placedObject);
    }

    [ClientRpc]
    public void RpcAddPlacedObject(Vector3 placedObject)
    {
        placedObjects.Add(placedObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

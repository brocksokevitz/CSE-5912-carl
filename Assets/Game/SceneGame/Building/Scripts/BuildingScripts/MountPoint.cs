using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct MountPoint
{
    public int team;
    public int objectID;
    /*public Vector3 parentPosition;
    public Vector3 parentLocalPosition;
    public Vector3 parentScale;
    public Quaternion parentRotation;
    public Vector3 parentLocalEuler;*/
    public BuildPoints.MountType parentMountType;
    public Vector3[] points;
    public BuildPoints.MountType[] pointType;

    
    public void DecrementObjectID()
    {
        objectID--;
    }

    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }

    public void SetObjectID(int newObjectID)
    {
        objectID = newObjectID;
    }

    public void AddPoint(Vector3 value)
    {
        Vector3[] tempArray = points;
        points = new Vector3[tempArray.Length + 1];
        for (int counter = 0; counter < tempArray.Length; counter++)
        {
            points[counter] = tempArray[counter];
        }
        points[tempArray.Length] = value;
    }

    public void AddMountType(BuildPoints.MountType value)
    {
        BuildPoints.MountType[] tempArray = pointType;
        pointType = new BuildPoints.MountType[tempArray.Length + 1];
        for (int counter = 0; counter < tempArray.Length; counter++)
        {
            pointType[counter] = tempArray[counter];
        }
        pointType[tempArray.Length] = value;
    }
}

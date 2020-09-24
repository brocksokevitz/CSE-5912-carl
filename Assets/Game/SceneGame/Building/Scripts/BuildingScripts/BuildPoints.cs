using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BuildPoints : NetworkBehaviour
{
    public enum MountType { Floor1, Wall, Ceiling1, Door1, Stair1, Destroyed};
    public MountType type;
    string[] fileStrings;
    public MountPoint mounting;
    public Vector3 offset;
    public bool valid;

    void Start()
    {
        TextAsset textFile = (TextAsset)Resources.Load("MountingPoints");
        fileStrings = textFile.text.Split('[');
        mounting = new MountPoint();
        mounting.points = new Vector3[0];
        mounting.pointType = new BuildPoints.MountType[0];
        AddPoints(type, transform);
    }

    void Update()
    {

    }

    void AddPoints(MountType inType, Transform trans)
    {
        string typeData = "";
        foreach (string str in fileStrings)
        {
            if (str.Length > 0)
            {
                string str2 = str.Substring(0, str.IndexOf(']'));
                if (str.Substring(0, str.IndexOf(']')) == inType.ToString())
                {
                    typeData = str;
                }
            }
        }
        string[] filePoints = typeData.Split(':');
        for (int i = 1; i < filePoints.Length; i++)
        {
            string[] xyz = filePoints[i].Split(',');
            float x;
            float.TryParse(xyz[0],out x);
            float y;
            float.TryParse(xyz[1], out y);
            float z;
            float.TryParse(xyz[2], out z);
            int type;
            int.TryParse(xyz[3], out type);
            mounting.parentMountType = inType;
            mounting.AddPoint(new Vector3(x, y, z));
            mounting.AddMountType((MountType)type);
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public struct SyncStructBool
{
    public bool[] boolList;

    public void Add(bool value)
    {
        bool[] tempArray = boolList;
        boolList = new bool[tempArray.Length + 1];
        for (int counter = 0; counter < tempArray.Length; counter++)
        {
            boolList[counter] = tempArray[counter];
        }
        boolList[tempArray.Length] = value;
    }
}
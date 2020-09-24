using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour {

    public static float WaterLevel;

	void Start () {
        WaterLevel = 0;	
	}
	void Update () {
        WaterLevel = transform.position.y;
    }
}

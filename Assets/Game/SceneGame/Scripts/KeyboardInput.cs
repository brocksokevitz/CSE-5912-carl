using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInput : MonoBehaviour {

    public delegate void KeyboardUp();
    public static event KeyboardUp KeyUp;
    
    public delegate void KeyboardDown();
    public static event KeyboardDown KeyDown;

    public delegate void KeyboardEscape();
    public static event KeyboardEscape Paused;

    bool Started;
	
	void Update () {
        if (Input.GetKey("up") || Input.GetKey("right"))
        {
            KeyUp();
        }
        if (Input.GetKey("down") || Input.GetKey("left"))
        {
            KeyDown();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Paused();
        }
    }
}

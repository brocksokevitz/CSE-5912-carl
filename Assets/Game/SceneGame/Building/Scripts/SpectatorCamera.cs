using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour {
    float moveSpeed = 5;
	
	void Update () {
        transform.position += transform.forward*Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.position += transform.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= transform.up * moveSpeed * Time.deltaTime;
        }
    }
}

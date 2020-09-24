using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour {

    public float speed;
    public float respawnTime;
    public int amount;
    float respawnTimer;
	// Use this for initialization
	void Start () {
        respawnTimer = respawnTime;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(new Vector3(0f, speed * Time.deltaTime, 0f));
        if (respawnTimer < respawnTime)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<BoxCollider>().enabled = true;
            }
        }
	}

    public void StartRespawnTimer()
    {
        respawnTimer = 0f;
    }
}

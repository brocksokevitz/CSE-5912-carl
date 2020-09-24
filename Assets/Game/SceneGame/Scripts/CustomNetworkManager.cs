using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

    [Header("Scene Camera Properties")]
    [SerializeField] Transform sceneCamera;
    [SerializeField] float cameraRotationRadius = 100f;
    [SerializeField] float cameraRotationSpeed = 10f;
    [SerializeField] bool canRotate;

    float rotation;

    List<Vector3> redSpawns;
    int redSpawnCounter = 0;
    List<Vector3> blueSpawns;
    int blueSpawnCounter = 0;
    short playerCounter = 0;

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        Debug.Log("Starting Client");
        canRotate = false;
        // Turn on the gameplay's HUD
        GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(true);
        // Hide and lock the cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = (false);
        ClientScene.AddPlayer(client.connection, playerCounter);
        playerCounter++;
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("Starting Host");
        canRotate = false;
        // Turn on the gameplay's HUD
        GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(true);
        // Hide and lock the cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = (false);
        GameObject[] tempRedSpawns = GameObject.FindGameObjectsWithTag("RedSpawn");
        GameObject[] tempBlueSpawns = GameObject.FindGameObjectsWithTag("BlueSpawn");
        redSpawns = new List<Vector3>();
        blueSpawns = new List<Vector3>();
        foreach (GameObject tempSpawn in tempRedSpawns)
        {
            redSpawns.Add(new Vector3(tempSpawn.transform.position.x, tempSpawn.transform.position.y, tempSpawn.transform.position.z ));
        }
        foreach (GameObject tempSpawn in tempBlueSpawns)
        {
            blueSpawns.Add(new Vector3(tempSpawn.transform.position.x, tempSpawn.transform.position.y, tempSpawn.transform.position.z));
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        canRotate = true;
        // Turn off the gameplay's HUD
        GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(false);
        // Show and unlock the cursor from the game window
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = (true);
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        canRotate = true;
        // Turn off the gameplay's HUD
       GameObject.Find("_UI").transform.GetChild(0).gameObject.SetActive(false);
        // Show and unlock the cursor from the game window
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = (true);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Client Connect");
        ClientScene.AddPlayer(client.connection, playerCounter);
        playerCounter++;
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("Player Adding");
        int team = playerControllerId % 2;
        if (team == 0)
        {
            var player = (GameObject)GameObject.Instantiate(playerPrefab, redSpawns[redSpawnCounter], Quaternion.identity);
            redSpawnCounter++;
            if (redSpawnCounter == redSpawns.Count)
            {
                redSpawnCounter = 0;
            }
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
        else
        {
            var player = (GameObject)GameObject.Instantiate(playerPrefab, blueSpawns[blueSpawnCounter], Quaternion.identity);
            blueSpawnCounter++;
            if (blueSpawnCounter == blueSpawns.Count)
            {
                blueSpawnCounter = 0;
            }
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
        
    }

    void Update()
    {
        if (!canRotate)
            return;

        rotation += cameraRotationSpeed * Time.deltaTime;
        if (rotation >= 360f)
            rotation -= 360f;

        sceneCamera.position = Vector3.zero;
        sceneCamera.rotation = Quaternion.Euler(0f, rotation, 0f);
        sceneCamera.Translate(0f, cameraRotationRadius*0.75f, -cameraRotationRadius);
        sceneCamera.LookAt(Vector3.zero);
    }
}

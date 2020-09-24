using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceBank : NetworkBehaviour
{
    [SyncVar(hook = "SetStone")]
    public int stone;
    [SyncVar(hook = "SetWood")]
    public int wood;
    [SyncVar(hook = "SetMetal")]
    public int metal;

    // Utilize a resource cap even though the resources will probably never get this high...
    private int maxResources = 9999;
    // Use this for initialization
    void Start()
    {
    }

    public void ResetBank()
    {
        stone = 500;
        wood = 500;
        metal = 300;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetStone(int Stone)
    {
        stone = Stone;
        if (stone > maxResources)
            stone = maxResources;
    }

    public void SetWood(int Wood)
    {
        wood = Wood;
        if (wood > maxResources)
            wood = maxResources;
    }

    public void SetMetal(int Metal)
    {
        metal = Metal;
        if (metal > maxResources)
            metal = maxResources;
    }

    public void AddStone(int Amount)
    {
        stone += Amount;
        if (stone > maxResources)
            stone = maxResources;
        if (stone < 0)
        {
            stone = 0;
        }
    }

    public void AddWood(int Amount)
    {
        wood += Amount;
        if (wood > maxResources)
            wood = maxResources;
        if (wood < 0)
        {
            wood = 0;
        }
    }

    public void AddMetal(int Amount)
    {
        metal += Amount;
        if (metal > maxResources)
            metal = maxResources;
        if (metal < 0)
        {
            metal = 0;
        }
    }

    public void Add(string Type, int Amount)
    {
        if (Type == "Stone")
        {
            AddStone(Amount);
        }
        else if (Type == "Metal")
        {
            AddMetal(Amount);
        }
        else
        {
            AddWood(Amount);
        }
    }

    public void Set(string Type, int Amount)
    {
        if (Type == "Stone")
        {
            SetStone(Amount);
        }
        else if (Type == "Metal")
        {
            SetMetal(Amount);
        }
        else
        {
            SetWood(Amount);
        }
    }

}

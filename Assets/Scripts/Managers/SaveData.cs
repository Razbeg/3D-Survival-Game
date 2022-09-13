using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public SVec3 playerPos;
    public SVec3 playerRot;
    public SVec3 playerLook;

    public float health;
    public float hunger;
    public float thirst;
    public float sleep;

    public SInventorySlot[] inventory;

    public SDroppedItem[] droppedItems;

    public SBuilding[] buildings;

    public SResource[] resources;

    public SNPC[] npcs;

    public float timeOfDay;
}

[System.Serializable]
public struct SVec3
{
    public float x;
    public float y;
    public float z;

    public SVec3 (Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 GetVector3 ()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public struct SInventorySlot
{
    public bool occupied;
    public string itemId;
    public int quantity;
    public bool equipped;
}

[System.Serializable]
public struct SDroppedItem
{
    public string itemId;
    public SVec3 position;
    public SVec3 rotation;
}

[System.Serializable]
public struct SBuilding
{
    public string buildingId;
    public SVec3 position;
    public SVec3 rotation;
    public string customProperties;
}

[System.Serializable]
public struct SResource
{
    public int index;
    public bool destroyed;
    public int capacity;
}

[System.Serializable]
public struct SNPC
{
    public string prefabId;
    public SVec3 position;
    public SVec3 rotation;
    public int aiState;
    public bool hasAgentDestination;
    public SVec3 agentDestination;
}
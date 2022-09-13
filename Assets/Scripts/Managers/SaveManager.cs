using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    private void Start ()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame ()
    {
        yield return new WaitForEndOfFrame();

        if(PlayerPrefs.HasKey("Save"))
            Load();
    }

    private void Update ()
    {
        if(Keyboard.current.nKey.wasPressedThisFrame)
            Save();

        if(Keyboard.current.mKey.wasPressedThisFrame)
            Load();
    }

    private void Save()
    {
        SaveData data = new SaveData();

        data.playerPos = new SVec3(PlayerController.instance.transform.position);
        data.playerRot = new SVec3(PlayerController.instance.transform.eulerAngles);
        data.playerLook = new SVec3(PlayerController.instance.cameraContainer.localEulerAngles);

        data.health = PlayerNeeds.instance.health.curValue;
        data.hunger = PlayerNeeds.instance.hunger.curValue;
        data.thirst = PlayerNeeds.instance.thirst.curValue;
        data.sleep = PlayerNeeds.instance.sleep.curValue;

        data.inventory = new SInventorySlot[Inventory.instance.slots.Length];

        for(int x = 0; x < Inventory.instance.slots.Length; x++)
        {
            data.inventory[x] = new SInventorySlot();
            data.inventory[x].occupied = Inventory.instance.slots[x].item != null;

            if(!data.inventory[x].occupied)
                continue;

            data.inventory[x].itemId = Inventory.instance.slots[x].item.id;
            data.inventory[x].quantity = Inventory.instance.slots[x].quantity;
            data.inventory[x].equipped = Inventory.instance.uiSlots[x].equipped;
        }

        ItemObject[] droppedItems = FindObjectsOfType<ItemObject>();
        data.droppedItems = new SDroppedItem[droppedItems.Length];

        for(int x = 0; x < droppedItems.Length; x++)
        {
            data.droppedItems[x] = new SDroppedItem();
            data.droppedItems[x].itemId = droppedItems[x].item.id;
            data.droppedItems[x].position = new SVec3(droppedItems[x].transform.position);
            data.droppedItems[x].rotation = new SVec3(droppedItems[x].transform.eulerAngles);
        }

        Building[] buildingObjects = FindObjectsOfType<Building>();
        data.buildings = new SBuilding[buildingObjects.Length];

        for(int x = 0; x < buildingObjects.Length; x++)
        {
            data.buildings[x] = new SBuilding();
            data.buildings[x].buildingId = buildingObjects[x].data.id;
            data.buildings[x].position = new SVec3(buildingObjects[x].transform.position);
            data.buildings[x].rotation = new SVec3(buildingObjects[x].transform.eulerAngles);
            data.buildings[x].customProperties = buildingObjects[x].GetCustomProperties();
        }

        data.resources = new SResource[ObjectManager.instance.resources.Length];

        for(int x = 0; x < ObjectManager.instance.resources.Length; x++)
        {
            data.resources[x] = new SResource();
            data.resources[x].index = x;
            data.resources[x].destroyed = ObjectManager.instance.resources[x] == null;

            if(!data.resources[x].destroyed)
                data.resources[x].capacity = ObjectManager.instance.resources[x].capacity;
        }

        NPC[] npcs = FindObjectsOfType<NPC>();
        data.npcs = new SNPC[npcs.Length];

        for(int x = 0; x < npcs.Length; x++)
        {
            data.npcs[x] = new SNPC();
            data.npcs[x].prefabId = npcs[x].data.id;
            data.npcs[x].position = new SVec3(npcs[x].transform.position);
            data.npcs[x].rotation = new SVec3(npcs[x].transform.eulerAngles);
            data.npcs[x].aiState = (int)npcs[x].aiState;
            data.npcs[x].hasAgentDestination = !npcs[x].agent.isStopped;
            data.npcs[x].agentDestination = new SVec3(npcs[x].agent.destination);
        }

        data.timeOfDay = DayNightCycle.instance.time;

        string rawData = JsonUtility.ToJson(data);

        PlayerPrefs.SetString("Save", rawData);
    }

    private void Load()
    {
        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("Save"));

        PlayerController.instance.transform.position = data.playerPos.GetVector3();
        PlayerController.instance.transform.eulerAngles = data.playerRot.GetVector3();
        PlayerController.instance.cameraContainer.localEulerAngles = data.playerLook.GetVector3();

        PlayerNeeds.instance.health.curValue = data.health;
        PlayerNeeds.instance.hunger.curValue = data.hunger;
        PlayerNeeds.instance.thirst.curValue = data.thirst;
        PlayerNeeds.instance.sleep.curValue = data.sleep;

        int equippedItem = 999;

        for(int x = 0; x < data.inventory.Length; x++)
        {
            if(!data.inventory[x].occupied)
                continue;

            Inventory.instance.slots[x].item = ObjectManager.instance.GetItemByID(data.inventory[x].itemId);
            Inventory.instance.slots[x].quantity = data.inventory[x].quantity;

            if(data.inventory[x].equipped)
            {
                equippedItem = x;
            }
        }

        if(equippedItem != 999)
        {
            Inventory.instance.SelectItem(equippedItem);
            Inventory.instance.OnEquipButton();
        }

        ItemObject[] droppedItems = FindObjectsOfType<ItemObject>();

        for(int x = 0; x < droppedItems.Length; x++)
            Destroy(droppedItems[x].gameObject);

        for(int x = 0; x < data.droppedItems.Length; x++)
        {
            GameObject prefab = ObjectManager.instance.GetItemByID(data.droppedItems[x].itemId).dropPrefab;
            Instantiate(prefab, data.droppedItems[x].position.GetVector3(), Quaternion.Euler(data.droppedItems[x].rotation.GetVector3()));
        }

        for(int x = 0; x < data.buildings.Length; x++)
        {
            GameObject prefab = ObjectManager.instance.GetBuildingByID(data.buildings[x].buildingId).spawnPrefab;
            GameObject building = Instantiate(prefab, data.buildings[x].position.GetVector3(), Quaternion.Euler(data.buildings[x].rotation.GetVector3()));
            building.GetComponent<Building>().ReceiveCustomProperties(data.buildings[x].customProperties);
        }

        for(int x = 0; x < ObjectManager.instance.resources.Length; x++)
        {
            if(data.resources[x].destroyed)
            {
                if (ObjectManager.instance.resources[x] != null)
                    Destroy(ObjectManager.instance.resources[x].gameObject);
                continue;
            }

            ObjectManager.instance.resources[x].capacity = data.resources[x].capacity;
        }

        NPC[] npcs = FindObjectsOfType<NPC>();

        for(int x = 0; x < npcs.Length; x++)
            Destroy(npcs[x].gameObject);

        for(int x = 0; x < data.npcs.Length; x++)
        {
            GameObject prefab = ObjectManager.instance.GetNPCByID(data.npcs[x].prefabId).spawnPrefab;
            GameObject npcObj = Instantiate(prefab, data.npcs[x].position.GetVector3(), Quaternion.Euler(data.npcs[x].rotation.GetVector3()));
            NPC npc = npcObj.GetComponent<NPC>();

            npc.aiState = (AIState)data.npcs[x].aiState;
            npc.agent.isStopped = !data.npcs[x].hasAgentDestination;

            if(!npc.agent.isStopped)
                npc.agent.SetDestination(data.npcs[x].agentDestination.GetVector3());
        }

        DayNightCycle.instance.time = data.timeOfDay;
    }
}
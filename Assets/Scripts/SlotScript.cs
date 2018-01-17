using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotScript : MonoBehaviour, IDropHandler {

    Inventory inv;
    public int slotNumber;
    public string itemName;

	// Use this for initialization
	void Start () {
        inv = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<Inventory>();
    }
	
	// Update is called once per frame
	void Update () {
        itemName = inv.items[slotNumber].Name;
	}

    public void OnDrop(PointerEventData eventData)
    {
        ItemData droppedItem = eventData.pointerDrag.GetComponent<ItemData>();
        if (inv.items[slotNumber].ID == -1)
        {
            inv.items[droppedItem.curSlot] = inv.database.GetItemByID(-1);
            inv.items[slotNumber] = droppedItem.item;
            droppedItem.curSlot = slotNumber;
        }
        else if (droppedItem.curSlot != slotNumber)
        {
            Transform item = this.transform.GetChild(0);
            item.GetComponent<ItemData>().curSlot = droppedItem.curSlot;
            item.transform.SetParent(inv.slots[droppedItem.curSlot].transform);
            item.transform.position = inv.slots[droppedItem.curSlot].transform.position;

            inv.items[droppedItem.curSlot] = item.GetComponent<ItemData>().item;
            inv.items[slotNumber] = droppedItem.item;

            droppedItem.curSlot = slotNumber;
            droppedItem.transform.SetParent(this.transform);
            droppedItem.transform.position = this.transform.position;
        }
    }
}

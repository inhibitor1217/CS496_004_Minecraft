using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    public ItemDatabase database;

    int maxAmount = 64;
    int slotAmount = 9;
    int storageAmount = 36;

    public GameObject slot;
    public GameObject invItem;
    public GameObject hotbarPanel;
    public GameObject inventoryPanel;

    public List<GameObject> slots = new List<GameObject>();
    public List<Item> items = new List<Item>();

    public Font textFont;
    public Material tileset;

    // Use this for initialization
    void Start () {

        database = gameObject.GetComponent<ItemDatabase>();

        for (int i = 0; i < slotAmount; i++)
        {
            items.Add(database.GetItemByID(-1));
            slots.Add(Instantiate(slot));
            slots[i].GetComponent<SlotScript>().slotNumber = i;
            slots[i].transform.SetParent(hotbarPanel.transform);
            slots[i].GetComponent<RectTransform>().transform.localScale = Vector3.one;
        }

        for (int i = slotAmount; i < storageAmount; i++)
        {
            items.Add(database.GetItemByID(-1));
            slots.Add(Instantiate(slot));
            slots[i].GetComponent<SlotScript>().slotNumber = i;
            slots[i].transform.SetParent(inventoryPanel.transform);
            slots[i].GetComponent<RectTransform>().transform.localScale = Vector3.one;
        }

    }

    public void AddItem(int id)
    {
        Item itemToAdd = database.GetItemByID(id);
        if (itemToAdd.Stackable && CheckInventory(itemToAdd)){

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == id)
                {
                    if (slots[i].transform.GetChild(0).GetComponent<ItemData>().amount < maxAmount)
                    {
                        ItemData data = slots[i].transform.GetChild(0).GetComponent<ItemData>();
                        data.amount++;
                        
                        Text amountText = data.transform.GetChild(0).GetComponent<Text>();
                        amountText.text = data.amount.ToString();
                        amountText.color = Color.white;
                        amountText.fontSize = 20;
                        amountText.font = textFont;

                        RectTransform pos = data.transform.GetChild(0).GetComponent<RectTransform>();
                        pos.anchorMin = Vector2.right;
                        pos.anchorMax = Vector2.right;
                        pos.anchoredPosition = new Vector2(15, 15);

                    }
                }
            }

        }  
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == 0)
                {
                    items[i] = itemToAdd;
                    GameObject itemObj = Instantiate(invItem);
                    itemObj.GetComponent<ItemData>().item = itemToAdd;
                    itemObj.GetComponent<ItemData>().curSlot = i;
                    itemObj.transform.SetParent(slots[i].transform);
                    itemObj.name = itemToAdd.Name;
                    itemObj.transform.localScale = Vector3.one;
                    itemObj.GetComponent<Image>().sprite = itemToAdd.Sprite;
                    break;
                }
            }
        }
    }

    bool CheckInventory(Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ID == item.ID)
            {
                if (slots[i].transform.GetChild(0).GetComponent<ItemData>().amount < maxAmount)
                {
                    return true;
                }
            }
        }
        return false;
    }

}

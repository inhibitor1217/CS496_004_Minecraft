using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    Inventory inv;

    public Item item;
    public int amount;
    public int curSlot;

    // Use this for initialization
    void Start () {
        inv = GameObject.FindGameObjectWithTag("InventoryManager").GetComponent<Inventory>();
        this.transform.position = inv.slots[curSlot].transform.position;
    }
	
	// Update is called once per frame
	void Update () {
		
	} 

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(item.ID != -1)
        {
            this.transform.SetParent(this.transform.parent.parent.parent);
            gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
            this.transform.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(item.ID != -1)
        {
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(inv.slots[curSlot].transform);
        this.transform.position = inv.slots[curSlot].transform.position;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}

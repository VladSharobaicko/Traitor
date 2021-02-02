﻿using ScriptableItems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UserInterface.Inventory
{
    public class ItemSlotUI : MonoBehaviour, IDropHandler
    {
        [SerializeField] protected Image background;
        [SerializeField] protected Image icon;
        [SerializeField] protected TextMeshProUGUI itemQuantityText;

        
        // public ItemContainer ItemContainer;
        public ItemContainer ItemContainer { get; set; }
        public ItemSlot ItemSlot { get; set; }
        public ItemContainerUI ItemContainerUI { get; set; }

        public void OnDrop(PointerEventData eventData)
        {
            var itemDragHandler = eventData.pointerDrag.GetComponent<ItemDragHandler>();
            if (itemDragHandler is null)
                return;
            var target = itemDragHandler.ItemSlotUi;
            ItemContainer.Combine(target.ItemSlot, ItemSlot);

            // TODO the only way to shift items between chests
            if (ItemContainer != target.ItemContainer)
            {
                target.ItemContainer.onItemsUpdated.Invoke();
            }
        }

        public void UpdateSlotUI()
        {
            if (ItemSlot.IsEmpty)
            {
                EnableSlotUI(false);
                return;
            }
            EnableSlotUI(true);
            icon.sprite = ItemSlot.Item.Icon;
            itemQuantityText.text = ItemSlot.Quantity.ToString();
        }

        private void EnableSlotUI(bool enable)
        {
            icon.enabled = enable;
            itemQuantityText.enabled = enable;
        }
    }
}
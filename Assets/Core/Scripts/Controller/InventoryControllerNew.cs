using Game.Model.Inventory.Struct;
using Game.Model.Player;
using Inventory.Model;
using Inventory.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Inventory
{
    public class InventoryControllerNew : MonoBehaviour
    {
        [SerializeField]
        private InventoryManager inventoryManager; //TAG : PanelInventory

        [SerializeField]
        private UIInventoryPage inventoryUI; // Core Store inventory //TAG : PanelInventory

        [SerializeField]
        private InventorySO inventoryData;

        [SerializeField]
        private PlayerStatusUI playerStatusUI; //TAG: PanelInventoryStatus
        //public List<InventoryItemUI> initialItems = new List<InventoryItemUI>();

        [SerializeField]
        private AudioClip dropClip;

        [SerializeField]
        private AudioSource audioSource;
        [SerializeField]
        private PlayerEntity playerEntity;
        //[SerializeField]
        // private InventoryControllerOld controllerBridging;

        private void Awake()
        {
            if (playerEntity == null)
            {
                playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
            }
            //inventoryManager = FinderTagHelper.FindTagged("PanelInventory").GetComponent<InventoryManager>();
            //inventoryUI = FinderTagHelper.FindTagged("PanelInventory").GetComponent<UIInventoryPage>();
            //playerStatusUI = FinderTagHelper.FindTagged("PanelInventoryStatus").GetComponent<PlayerStatusUI>();
        }
        private void Start()
        {
            //Example independent init
            //EncryptionManager.Init();
            //SaveManager.Init();


            PrepareUI();
            PrepareInventoryData();
        }

        //Start HERE!
        private void PrepareInventoryData()
        {
            //inventoryData.Initialize(); // Set Empty Slot without loaded Items
            inventoryManager.LoadInventoryData(); //Handle Replace from Empty
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;

            UpdateInventoryUI(inventoryData.ReInitializeByUpdate());
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItemUI> inventoryState) //On heap check
        {
            Resources.Load<InventorySO>("InventoryData/PlayerInventory"); //prepare refresh db asset
            inventoryUI.ResetAllItems(); // reset UI
            foreach (var item in inventoryState) // loop change display UI
            {
                if (item.Value.IsEmpty)
                    continue;
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }


        private void PrepareUI()
        {

            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            if (inventoryData == null)
            {
                Debug.LogError("HandleItemActionRequest: inventoryData is null");
                return;
            }

            if (inventoryUI == null)
            {
                Debug.LogError("HandleItemActionRequest: inventoryUI is null");
                return;
            }

            var invItem = inventoryData.GetItemAt(itemIndex);
            if (invItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            IItemAction itemAction = invItem.item as IItemAction;
            if (itemAction != null)
            {
                // Show popup panel
                inventoryUI.ShowItemAction(itemIndex);

                // Safely keep the action panel within camera bounds
                if (inventoryUI.ActionPanel != null)
                    AttachKeepInCameraView(inventoryUI.ActionPanel.gameObject);
                else
                    Debug.LogWarning("HandleItemActionRequest: inventoryUI.ActionPanel is null");

                // Add main item action
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
            }

            IDestroyableItem destroyableItem = invItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                // Add a drop option
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, invItem.quantity));
            }
        }


        private void AttachKeepInCameraView(GameObject panelObj)
        {
            if (panelObj == null) return;

            var keepInView = panelObj.GetComponent<KeepInCameraView>();
            if (keepInView == null)
                keepInView = panelObj.AddComponent<KeepInCameraView>();

            keepInView.useTween = true;          // Smooth motion
            keepInView.tweenDuration = 0.12f;    // Slightly fast for UI
            keepInView.includeOffscreenCheck = true;
        }
        private void DropItem(int itemIndex, int quantity)
        {

            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropClip);
        }

        public void PerformAction(int itemIndex)
        {

            var invItem = inventoryData.GetItemAt(itemIndex);
            if (invItem.IsEmpty)
                return;

            IItemAction itemAction = invItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, invItem.itemState);
                audioSource.PlayOneShot(itemAction.actionSFX);
                if (itemAction.ActionName != "Consume")
                {
                    playerStatusUI.Refresh(playerEntity, invItem.item.ItemImage);
                }

                //if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                inventoryUI.ResetSelection();
            }
        }

        private void HandleDragging(int itemIndex)
        {

            var invItem = inventoryData.GetItemAt(itemIndex);
            if (invItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(invItem.item.ItemImage, invItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {

            var invItem = inventoryData.GetItemAt(itemIndex);
            if (invItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = invItem.item;
            string description = PrepareDescription(invItem);
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
                item.name, description);
        }

        private string PrepareDescription(InventoryItemUI invItem)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(invItem.item.Description);
            sb.AppendLine();
            for (int i = 0; i < invItem.itemState.Count; i++)
            {
                sb.Append($"{invItem.itemState[i].itemParameter.ParameterName} " +
                    $": {invItem.itemState[i].value} / " +
                    $"{invItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void Update()
        {
            if (!inventoryUI.gameObject.activeInHierarchy || !inventoryManager.gameObject.activeInHierarchy) return;

            // --- TOUCH (Input System) ---
            var touchScreen = Touchscreen.current;
            if (touchScreen != null && touchScreen.primaryTouch.press.wasPressedThisFrame)
            {
                Vector2 touchPos = touchScreen.primaryTouch.position.ReadValue();

                // fingerId untuk Input System = always 0 untuk primaryTouch
                if (!EventSystem.current.IsPointerOverGameObject(0))
                {
                    HandleClickOutsideTouch(touchPos);
                }
                return;
            }

            // --- MOUSE (Input System) ---
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Vector2 mousePos = mouse.position.ReadValue();
                    HandleClickOutsideTouch(mousePos);
                }
            }
        }

        private void OnDestroy()
        {
            if (inventoryData != null)
                inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
        }

        private void HandleClickOutsideTouch(Vector2 screenPosition)
        {
            // Only check if the ActionPanel is visible
            if (inventoryUI.ActionPanel != null && inventoryUI.ActionPanel.isActiveAndEnabled)
            {
                RectTransform panelRect = inventoryUI.ActionPanel.GetComponent<RectTransform>();

                // ✅ Check if the click/tap is OUTSIDE the ActionPanel area
                bool isOutsidePanel = !RectTransformUtility.RectangleContainsScreenPoint(
                    panelRect,
                    screenPosition,
                    inventoryUI.GetComponentInParent<Canvas>().worldCamera
                );

                if (isOutsidePanel)
                {
                    inventoryUI.DeselectAllItems();
                }
            }
        }


    }
}
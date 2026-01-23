using UnityEngine;
using System.Collections.Generic;
using RobbieWagnerGames.UI;
using UnityEngine.UI;

namespace RobbieWagnerGames.RoguelikeAsteroids
{
    public class ShopMenu : Menu
    {
        [SerializeField] private List<ShopItem> availableItems;
        [SerializeField] private ShopButton shopButtonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private Button leaveShopButton;
        [SerializeField] private ShopLevelController levelController;
        
        private Dictionary<string, ShopItemData> itemData = new Dictionary<string, ShopItemData>();
        private List<ShopButton> shopButtons = new List<ShopButton>();

        protected override void Awake()
        {
            base.Awake();
            if (levelController != null)
                levelController.OnLevelStarted += DisplayShopMenu;
            
            if (leaveShopButton != null)
                leaveShopButton.onClick.AddListener(LeaveShop);
        }

        private void DisplayShopMenu()
        {
            activeMenu = this;
        }

        public override void Open()
        {
            LoadOrCreateItemData();
            PopulateShop();
            SetupNavigation();
            base.Open();
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            
            RefreshSelectableElements();
            
            if (shopButtons.Count == 0 && leaveShopButton != null)
                firstSelected = leaveShopButton;
        }
        
        protected override void OnClosed()
        {
            base.OnClosed();
        }
        
        protected override void HandleCancel()
        {
            LeaveShop();
        }

        private void LoadOrCreateItemData()
        {
            if (RunManager.Instance == null || RunManager.Instance.CurrentRun == null)
                return;
                
            var currentRun = RunManager.Instance.CurrentRun;
            
            foreach (ShopItem item in availableItems)
            {
                if (!itemData.ContainsKey(item.name))
                {
                    ShopItemData existingData = currentRun.shopPurchases
                        .Find(d => d.itemName == item.name);
                    
                    if (existingData != null)
                        itemData[item.name] = existingData;
                    else
                    {
                        var newData = new ShopItemData { itemName = item.name };
                        itemData[item.name] = newData;
                        
                        if (!currentRun.shopPurchases.Contains(newData))
                            currentRun.shopPurchases.Add(newData);
                    }
                }
            }
        }
        
        private void PopulateShop()
        {
            Debug.Log("pop shop");
            foreach (Transform child in buttonContainer)
                Destroy(child.gameObject);
            
            shopButtons.Clear();
            
            foreach (ShopItem item in availableItems)
            {
                Debug.Log("item");
                if (itemData.TryGetValue(item.name, out ShopItemData data))
                {
                    ShopButton button = Instantiate(shopButtonPrefab, buttonContainer);
                    button.Initialize(item, data, () => OnItemPurchased(item, data));
                    shopButtons.Add(button);
                }
            }
            
            SetupButtonNavigation();
            
            RefreshSelectableElements();
        }
        
        private void SetupButtonNavigation()
        {
            if (shopButtons.Count == 0) return;
            
            Button[] buttonComponents = new Button[shopButtons.Count];
            for (int i = 0; i < shopButtons.Count; i++)
                buttonComponents[i] = shopButtons[i].GetComponent<Button>();
            int columns = gridLayout != null ? gridLayout.constraintCount : 3;
            int rows = Mathf.CeilToInt((float)buttonComponents.Length / columns);
            
            for (int i = 0; i < buttonComponents.Length; i++)
            {
                if (buttonComponents[i] == null) continue;
                
                Navigation nav = buttonComponents[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                
                int row = i / columns;
                int col = i % columns;
                
                if (row > 0)
                {
                    int upIndex = (row - 1) * columns + col;
                    if (upIndex < buttonComponents.Length)
                        nav.selectOnUp = buttonComponents[upIndex];
                }
                
                int downRow = row + 1;
                int downIndex = downRow * columns + col;
                if (downRow < rows && downIndex < buttonComponents.Length)
                    nav.selectOnDown = buttonComponents[downIndex];
                
                if (col > 0)
                {
                    int leftIndex = row * columns + (col - 1);
                    if (leftIndex < buttonComponents.Length)
                        nav.selectOnLeft = buttonComponents[leftIndex];
                }
                
                if (col < columns - 1 && i + 1 < buttonComponents.Length)
                    nav.selectOnRight = buttonComponents[i + 1];
                
                buttonComponents[i].navigation = nav;
            }
            
            if (leaveShopButton != null)
            {
                int bottomRowStart = Mathf.Max(0, rows - 1) * columns;
                for (int i = bottomRowStart; i < Mathf.Min(buttonComponents.Length, bottomRowStart + columns); i++)
                {
                    if (buttonComponents[i] != null)
                    {
                        Navigation nav = buttonComponents[i].navigation;
                        
                        if (i >= (rows - 1) * columns)
                            nav.selectOnDown = leaveShopButton;
                        
                        buttonComponents[i].navigation = nav;
                    }
                }
                
                Navigation leaveNav = leaveShopButton.navigation;
                leaveNav.mode = Navigation.Mode.Explicit;
                
                if (buttonComponents.Length > 0)
                {
                    leaveNav.selectOnUp = buttonComponents[Mathf.Min(columns - 1, buttonComponents.Length - 1)];
                    leaveNav.selectOnLeft = buttonComponents[Mathf.Min(columns - 1, buttonComponents.Length - 1)];
                    leaveNav.selectOnRight = buttonComponents[Mathf.Min(columns - 1, buttonComponents.Length - 1)];
                }
                
                leaveShopButton.navigation = leaveNav;
            }
            
            if (firstSelected == null && buttonComponents.Length > 0)
                firstSelected = buttonComponents[0];
        }
        
        public override void RefreshSelectableElements()
        {
            base.RefreshSelectableElements();
            
            selectableElements.Clear();
            
            foreach (ShopButton shopButton in shopButtons)
            {
                Button button = shopButton?.GetComponent<Button>();
                if (button != null && button.interactable && button.gameObject.activeInHierarchy)
                    selectableElements.Add(button);
            }
            
            if (leaveShopButton != null && leaveShopButton.interactable && leaveShopButton.gameObject.activeInHierarchy)
                selectableElements.Add(leaveShopButton);
            selectableElements.Sort((a, b) =>
            {
                Vector3 aPos = a.transform.position;
                Vector3 bPos = b.transform.position;
                int yCompare = bPos.y.CompareTo(aPos.y);
                return yCompare != 0 ? yCompare : aPos.x.CompareTo(bPos.x);
            });
        }
        
        private void OnItemPurchased(ShopItem item, ShopItemData data)
        {
            if (data.TryPurchase(item))
            {
                foreach (ShopButton button in shopButtons)
                {
                    Button btn = button.GetComponent<Button>();
                    if (btn != null && btn.interactable)
                        button.GetComponent<Selectable>().OnSelect(null);
                }
                PopulateShop();
                if (isUsingController)
                    RestoreOrSetDefaultSelection();
            }
        }
        
        private void LeaveShop()
        {
            Close();
            
            levelController.CompleteLevel();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (levelController != null)
                levelController.OnLevelStarted -= DisplayShopMenu;
            
            if (leaveShopButton != null)
                leaveShopButton.onClick.RemoveListener(LeaveShop);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (levelController != null)
                levelController.OnLevelStarted -= DisplayShopMenu;
            
            if (leaveShopButton != null)
                leaveShopButton.onClick.RemoveListener(LeaveShop);
        }
        
        protected override void NavigateVertical(int direction)
        {
            if (shopButtons.Count == 0) return;
            
            if (gridLayout == null || gridLayout.constraintCount <= 1)
            {
                base.NavigateVertical(direction);
                return;
            }
            
            int columns = gridLayout.constraintCount;
            int currentIndex = currentSelectedIndex;
            
            if (currentIndex < 0 || currentIndex >= selectableElements.Count)
                return;
            
            int newIndex = currentIndex;
            
            if (leaveShopButton != null && selectableElements[currentIndex] == leaveShopButton)
            {
                int lastRowStart = Mathf.Max(0, shopButtons.Count - columns);
                newIndex = Mathf.Min(lastRowStart, shopButtons.Count - 1);
            }
            else
            {
                int gridIndex = currentIndex;
                if (gridIndex < shopButtons.Count)
                {
                    int row = gridIndex / columns;
                    int col = gridIndex % columns;
                    
                    if (direction < 0)
                    {
                        if (row > 0)
                            newIndex = (row - 1) * columns + col;
                    }
                    else
                    {
                        if (row < Mathf.CeilToInt((float)shopButtons.Count / columns) - 1)
                            newIndex = Mathf.Min((row + 1) * columns + col, shopButtons.Count - 1);
                        else if (leaveShopButton != null)
                            newIndex = selectableElements.IndexOf(leaveShopButton);
                    }
                }
            }
            
            if (newIndex >= 0 && newIndex < selectableElements.Count)
            {
                Selectable newSelection = selectableElements[newIndex];
                if (newSelection.interactable && newSelection.gameObject.activeInHierarchy)
                {
                    currentSelectedIndex = newIndex;
                    ForceSelectElement(newSelection);
                }
            }
        }
    }
}
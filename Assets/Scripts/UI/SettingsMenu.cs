using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using RobbieWagnerGames.Managers;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace RobbieWagnerGames.UI
{
    public class SettingsMenu : Menu
    {
        [Header("Tab Management")]
        [SerializeField] private GameObject graphicsTab;
        [SerializeField] private GameObject audioTab;
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Toggle fullscreenToggle;
        
        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;
        [SerializeField] private Slider hazardVolumeSlider;
        [SerializeField] private Slider playerVolumeSlider;
        [SerializeField] private AudioMixer audioMixer;
        
        private Tab currentTab = Tab.GRAPHICS;
        private List<Selectable> currentTabSelectables = new List<Selectable>();
        
        private enum Tab
        {
            GRAPHICS,
            AUDIO
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            graphicsTabButton.onClick.AddListener(() => SwitchTab(Tab.GRAPHICS));
            audioTabButton.onClick.AddListener(() => SwitchTab(Tab.AUDIO));
            
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            
            SetupAudioSliders();
            SwitchTab(Tab.GRAPHICS);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
			InputManager.Instance.Controls.UI.NavigateTabs.performed += OnNavigateTabsPerformed;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            InputManager.Instance.Controls.UI.NavigateTabs.performed -= OnNavigateTabsPerformed;
        }
        
        private void SetupAudioSliders()
        {
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);
            float hazardVolume = PlayerPrefs.GetFloat("HazardVolume", 1f);
            float playerVolume = PlayerPrefs.GetFloat("PlayerVolume", 1f);
            
            masterVolumeSlider.value = masterVolume;
            musicVolumeSlider.value = musicVolume;
            uiVolumeSlider.value = uiVolume;
            hazardVolumeSlider.value = hazardVolume;
            playerVolumeSlider.value = playerVolume;
            
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
            hazardVolumeSlider.onValueChanged.AddListener(OnHazardVolumeChanged);
            playerVolumeSlider.onValueChanged.AddListener(OnPlayerVolumeChanged);
            
            UpdateAudioMixer();
        }
        
        private void SwitchTab(Tab tab)
        {
            if (currentTab == tab) return;
            
            currentTab = tab;
            
            graphicsTab.SetActive(tab == Tab.GRAPHICS);
            audioTab.SetActive(tab == Tab.AUDIO);
            
            UpdateTabButtonVisuals();
            RefreshSelectableElements();
            
            if (tab == Tab.GRAPHICS)
                firstSelected = fullscreenToggle;
            else
                firstSelected = masterVolumeSlider;
            
            SetupNavigation();
        }
        
        private void UpdateTabButtonVisuals()
        {
            ColorBlock graphicsColors = graphicsTabButton.colors;
            ColorBlock audioColors = audioTabButton.colors;
            
            if (currentTab == Tab.GRAPHICS)
            {
                graphicsColors.normalColor = graphicsColors.selectedColor;
                audioColors.normalColor = audioColors.disabledColor;
            }
            else
            {
                graphicsColors.normalColor = graphicsColors.disabledColor;
                audioColors.normalColor = audioColors.selectedColor;
            }
            
            graphicsTabButton.colors = graphicsColors;
            audioTabButton.colors = audioColors;
        }
        
        public override void RefreshSelectableElements()
        {
            base.RefreshSelectableElements();
            
            currentTabSelectables.Clear();
            
            currentTabSelectables.Add(graphicsTabButton);
            currentTabSelectables.Add(audioTabButton);
            
            if (currentTab == Tab.GRAPHICS)
            {
                foreach (Selectable selectable in selectableElements)
                {
                    if (selectable.transform.IsChildOf(graphicsTab.transform) && 
                        selectable != graphicsTabButton && 
                        selectable != audioTabButton)
					{
                        currentTabSelectables.Add(selectable);
					}
				}
            }
            else
            {
                foreach (Selectable selectable in selectableElements)
                {
                    if (selectable.transform.IsChildOf(audioTab.transform) && 
                        selectable != graphicsTabButton && 
                        selectable != audioTabButton)
                    {
                        currentTabSelectables.Add(selectable);
                    }
                }
            }
            
            currentTabSelectables.Sort((a, b) =>
            {
                Vector3 aPos = a.transform.position;
                Vector3 bPos = b.transform.position;
                return bPos.y.CompareTo(aPos.y);
            });
        }
        
        protected override void HandleNavigationInput(Vector2 input)
        {
            if (currentTabSelectables.Count == 0) return;
            
            bool inputHandled = false;
            
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                if (input.y > 0.5f)
                {
                    NavigateVertical(-1);
                    inputHandled = true;
                }
                else if (input.y < -0.5f)
                {
                    NavigateVertical(1);
                    inputHandled = true;
                }
            }
            else
            {
                if (input.x < -0.5f || input.x > 0.5f)
                {
                    NavigateHorizontal(input.x > 0 ? 1 : -1);
                    inputHandled = true;
                }
            }
            
            if (inputHandled)
            {
                canNavigateWithController = false;
                Invoke(nameof(ResetControllerNavigation), controllerNavigationDelay);
            }
        }
        
        protected override void NavigateVertical(int direction)
        {
            if (currentTabSelectables.Count == 0) return;
            
            int newIndex = currentSelectedIndex;
            int attempts = 0;
            
            do
            {
                newIndex += direction;
                
                if (wrapNavigation)
                {
                    if (newIndex < 0) 
						newIndex = currentTabSelectables.Count - 1;
                    else if (newIndex >= currentTabSelectables.Count) 
						newIndex = 0;
                }
                else
                    newIndex = Mathf.Clamp(newIndex, 0, currentTabSelectables.Count - 1);
                
                attempts++;
                
                if (attempts > currentTabSelectables.Count)
                {
                    Debug.LogWarning("Failed to find valid element to navigate to", this);
                    return;
                }
                
            } while (!currentTabSelectables[newIndex].interactable || 
                     !currentTabSelectables[newIndex].gameObject.activeInHierarchy);
            
            currentSelectedIndex = newIndex;
            ForceSelectElement(currentTabSelectables[currentSelectedIndex]);
        }
        
        protected override void NavigateHorizontal(int direction)
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= currentTabSelectables.Count) return;
            
            Selectable currentElement = currentTabSelectables[currentSelectedIndex];
            
            if (currentElement is Slider slider)
            {
                float step = (slider.maxValue - slider.minValue) / 20f;
                slider.value += direction * step;
                slider.onValueChanged?.Invoke(slider.value);
            }
            else if (currentElement is Toggle toggle)
            {
                toggle.isOn = !toggle.isOn;
                toggle.onValueChanged?.Invoke(toggle.isOn);
            }
            else if (currentElement is Button button && (button == graphicsTabButton || button == audioTabButton))
                return;
            else
                base.NavigateHorizontal(direction);
        }
        
        private void OnNavigateTabsPerformed(InputAction.CallbackContext context)
        {
            if (!IsOpen) return;
            
            float value = context.ReadValue<float>();
            
            if (value > 0.5f)
                SwitchTab(currentTab == Tab.GRAPHICS ? Tab.AUDIO : Tab.GRAPHICS);
            else if (value < -0.5f)
                SwitchTab(currentTab == Tab.GRAPHICS ? Tab.AUDIO : Tab.GRAPHICS);
        }
        
        protected override void SetupNavigation()
        {
            RefreshSelectableElements();
            
            isUsingController = true;
            lastMouseActivityTime = Time.time - mouseInactivityTimeout;
            
            Selectable elementToSelect = firstSelected;
            
            if (elementToSelect == null || !elementToSelect.interactable || !elementToSelect.gameObject.activeInHierarchy)
            {
                foreach (Selectable element in currentTabSelectables)
                {
                    if (element.interactable && element.gameObject.activeInHierarchy)
                    {
                        elementToSelect = element;
                        break;
                    }
                }
            }
            
            if (elementToSelect != null)
            {
                ForceSelectElement(elementToSelect);
                currentSelectedIndex = currentTabSelectables.IndexOf(elementToSelect);
            }
        }
        
        protected override void HandleCancel()
        {
            Close();
        }
        
        public override void Open()
        {
            base.Open();
            SwitchTab(Tab.GRAPHICS);
        }
        
        public override void Close()
        {
            base.Close();
            SaveSettings();
        }
        
        private void OnFullscreenToggled(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MasterVolume", value);
            UpdateAudioMixer();
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            UpdateAudioMixer();
        }
        
        private void OnUIVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("UIVolume", value);
            UpdateAudioMixer();
        }
        
        private void OnHazardVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("HazardVolume", value);
            UpdateAudioMixer();
        }
        
        private void OnPlayerVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("PlayerVolume", value);
            UpdateAudioMixer();
        }
        
        private void UpdateAudioMixer()
        {
            if (audioMixer == null) return;
            
            float masterVolume = Mathf.Lerp(-80f, 0f, masterVolumeSlider.value);
            float musicVolume = Mathf.Lerp(-80f, 0f, musicVolumeSlider.value);
            float uiVolume = Mathf.Lerp(-80f, 0f, uiVolumeSlider.value);
            float hazardVolume = Mathf.Lerp(-80f, 0f, hazardVolumeSlider.value);
            float playerVolume = Mathf.Lerp(-80f, 0f, playerVolumeSlider.value);
            
            audioMixer.SetFloat("MasterVolume", masterVolume);
            audioMixer.SetFloat("MusicVolume", musicVolume);
            audioMixer.SetFloat("UIVolume", uiVolume);
            audioMixer.SetFloat("HazardVolume", hazardVolume);
            audioMixer.SetFloat("PlayerVolume", playerVolume);
        }
        
        private void SaveSettings()
        {
            PlayerPrefs.Save();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InputManager.Instance.Controls.UI.NavigateTabs.performed -= OnNavigateTabsPerformed;

            graphicsTabButton.onClick.RemoveAllListeners();
            audioTabButton.onClick.RemoveAllListeners();
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (uiVolumeSlider != null)
                uiVolumeSlider.onValueChanged.RemoveAllListeners();
            if (hazardVolumeSlider != null)
                hazardVolumeSlider.onValueChanged.RemoveAllListeners();
            if (playerVolumeSlider != null)
                playerVolumeSlider.onValueChanged.RemoveAllListeners();
        }
    }
}
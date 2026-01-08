using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using RobbieWagnerGames.Managers;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;

namespace RobbieWagnerGames.UI
{
    public class SettingsMenu : Menu
    {
        [Header("General")]
        public Button backButton;

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

        protected override void Update()
        {
            if (!IsOpen) return;
            
            HandleInputModeDetection();
            HandleControllerNavigation();
            HandleMouseNavigation();
            CheckForLostSelection();
        }

        protected override void ForceControllerMode()
        {
            if (!EventSystemManager.Instance.HasSelection())
                RestoreOrSetDefaultSelection();
        }

        protected override void CheckForLostSelection()
        {
            if (!IsOpen || !maintainSelectionAlways || !isUsingController) return;
            
            if (!EventSystemManager.Instance.HasSelection())
                RestoreOrSetDefaultSelection();
        }

        protected override void HandleControllerNavigation()
        {
            if (!isUsingController || !canNavigateWithController) return;
            
            InputAction navigateAction = InputManager.Instance.GetAction(ActionMapName.UI, "Navigate");
            if (navigateAction != null)
            {
                Vector2 input = navigateAction.ReadValue<Vector2>();
                
                if (input != Vector2.zero && (lastControllerInput == Vector2.zero || Time.time - lastControllerNavigationTime > controllerRepeatRate))
                {
                    HandleNavigationInput(input);
                    lastControllerNavigationTime = Time.time;
                }
                else if (input == Vector2.zero && lastControllerInput != Vector2.zero)
                    canNavigateWithController = true;
                
                lastControllerInput = input;
            }
        }

        protected override void HandleNavigationInput(Vector2 input)
        {
            bool inputHandled = false;
            
            if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
            {
                if (input.y > 0.5f)
                    inputHandled = true;
                else if (input.y < -0.5f) 
                    inputHandled = true;
            }
            else
            {
                if (input.x < -0.5f)
                {
                    HandleHorizontalNavigation(-1);
                    inputHandled = true;
                }
                else if (input.x > 0.5f)
                {
                    HandleHorizontalNavigation(1);
                    inputHandled = true;
                }
            }
            
            if (inputHandled)
            {
                canNavigateWithController = false;
                Invoke(nameof(ResetControllerNavigation), controllerNavigationDelay);
            }
        }

        private void HandleHorizontalNavigation(int direction)
        {
            GameObject selected = EventSystemManager.Instance.CurrentSelected;
            if (selected == null) return;
            
            Selectable currentElement = selected.GetComponent<Selectable>();
            if (currentElement == null) return;
            
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
            
            EventSystemManager.Instance.SetSelected(null);
            
            currentTab = tab;
            
            graphicsTab.SetActive(tab == Tab.GRAPHICS);
            audioTab.SetActive(tab == Tab.AUDIO);
            
            UpdateTabButtonVisuals();
            RefreshSelectableElements();
            
            if (tab == Tab.GRAPHICS)
            {
                Navigation backButtonNav = backButton.navigation;
                backButtonNav.mode = Navigation.Mode.Explicit;
                backButtonNav.selectOnUp = fullscreenToggle;
                backButtonNav.selectOnDown = fullscreenToggle;
                backButton.navigation = backButtonNav;

                firstSelected = fullscreenToggle;
            }
            else if (tab == Tab.AUDIO)
            {
                Navigation backButtonNav = backButton.navigation;
                backButtonNav.mode = Navigation.Mode.Explicit;
                backButtonNav.selectOnUp = playerVolumeSlider;
                backButtonNav.selectOnDown = masterVolumeSlider;
                backButton.navigation = backButtonNav;

                firstSelected = masterVolumeSlider;
            }

            StartCoroutine(SwitchTabDelayed());
        }

        private IEnumerator SwitchTabDelayed()
        {
            yield return new WaitForSecondsRealtime(.1f);
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

        private void OnNavigateTabsPerformed(InputAction.CallbackContext context)
        {
            if (!IsOpen) return;
            
            float value = context.ReadValue<float>();
            
            if (value > 0.5f || value < -0.5f)
            {
                Tab newTab = currentTab == Tab.GRAPHICS ? Tab.AUDIO : Tab.GRAPHICS;
                SwitchTab(newTab);
            }
        }

        public override void Open()
        {
            base.Open();
            SwitchTab(Tab.AUDIO);
        }
        
        public override void Close()
        {
            base.Close();
            SaveSettings();
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

        protected override void OnElementSelected(Selectable element)
        {
            base.OnElementSelected(element);
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
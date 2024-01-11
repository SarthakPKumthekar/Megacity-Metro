using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    /// <summary>
    /// Allows game settings to navigate through game setting views.
    /// Shows and hides view according to the game settings menu.
    /// </summary>
    public class UIGameSettings : MonoBehaviour
    {
        [SerializeField] private UISettingsTab[] m_TabViews;
        [SerializeField] private InputAction BackAction;
        private int m_CurrentView;

        private Button m_GraphicsButton;
        private Button m_AudioButton;
        private Button m_CloseButton;
        private Button m_ApplyButton;
        private Button m_ControlsButton;

        private VisualElement m_GameSettings;
        private VisualElement m_GameSettingsPanel;
        private VisualElement m_TriggerSettings;
        private List<VisualElement> m_ViewSettingsList = new();
        private List<Button> m_TabButtons = new();

        private const string SelectedStyle = "menu-button-active";
        [SerializeField] private StyleSheet m_MobileStyleSheet;

        public bool IsVisible => m_GameSettings.style.display == DisplayStyle.Flex;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
#if UNITY_ANDROID || UNITY_IPHONE
            root.styleSheets.Clear();
            root.styleSheets.Add(m_MobileStyleSheet);
#endif
            
            m_GameSettings = root.Q<VisualElement>("game-settings");
            m_GameSettingsPanel = root.Q<VisualElement>("settings-menu-panel");

            foreach (var view in m_TabViews)
            {
                view.GameSettingsView = root.Q<VisualElement>(view.TabName);
                m_ViewSettingsList.Add(view.GameSettingsView);
            }

            m_GraphicsButton = root.Q<Button>("graphics-button");
            m_AudioButton = root.Q<Button>("audio-button");
            m_ControlsButton = root.Q<Button>("controls-button");
            m_CloseButton = root.Q<Button>("close-button");
            m_ApplyButton = root.Q<Button>("apply-button");

            m_TabButtons.Add(m_GraphicsButton);
            m_TabButtons.Add(m_AudioButton);
            m_TabButtons.Add(m_ControlsButton);

            m_GraphicsButton.clicked += () =>
            {
                m_CurrentView = 0;
                ShowSettings();
            };

            m_AudioButton.clicked += () =>
            {
                m_CurrentView = 1;
                ShowSettings();
            };

            m_ControlsButton.clicked += () =>
            {
                m_CurrentView = 2;
                ShowSettings();
            };

            m_CloseButton.clicked += () =>
            {
                Reset();
                Hide();
            };

            m_ApplyButton.clicked += () =>
            {
                Apply();
                Hide();
            };

            BackAction.performed += _ =>
            {
                if (IsVisible)
                {
                    Reset();
                    Hide();
                }
            };

#if UNITY_ANDROID || UNITY_IPHONE
            var hideInMobileElements = root.Query(className: "hide-in-mobile").ToList();
            foreach (var mobileElement in hideInMobileElements)
            {
                mobileElement.style.display = DisplayStyle.None;
            }
#endif
        }

        private void OnDisable()
        {
            BackAction.Disable();
        }

        public void Show()
        {
            AnimateIn();
            ShowSettings();
        }

        private void AnimateIn()
        {
            m_GameSettings.style.opacity = 0;
            m_GameSettings.style.display = DisplayStyle.Flex;
            m_GameSettingsPanel.experimental.animation
                .Start(new StyleValues {top = -100}, new StyleValues {top = 0}, 500).Ease(Easing.OutCubic);
            m_GameSettings.experimental.animation.Start(new StyleValues {opacity = 1}, 250)
                .OnCompleted(() => BackAction.Enable());
        }

        private void AnimateOut()
        {
            m_GameSettingsPanel.experimental.animation
                .Start(new StyleValues {top = 0}, new StyleValues {top = -100}, 500).Ease(Easing.OutCubic);
            m_GameSettings.experimental.animation.Start(new StyleValues {opacity = 0}, 250)
                .OnCompleted(() =>
                {
                    m_GameSettings.style.display = DisplayStyle.None;
                    BackAction.Disable();
                });
        }

        public void Hide()
        {
            if (m_TriggerSettings != null)
            {
                m_TriggerSettings.style.display = DisplayStyle.Flex;
                m_TriggerSettings = null;
            }

            AnimateOut();
            m_CurrentView = 0;
            
            if(MainMenu.Instance != null)
                MainMenu.Instance.CurrentState = MenuState.MainMenu;
        }

        private void ShowSettings()
        {
            m_ViewSettingsList.ForEach(element => element.style.display = DisplayStyle.None);
            m_ViewSettingsList[m_CurrentView].style.display = DisplayStyle.Flex;
            foreach (var tab in m_TabViews)
            {
                tab.Hide();
            }

            m_TabViews[m_CurrentView].Show();
            m_TabButtons.ForEach(b => b.RemoveFromClassList(SelectedStyle));
            m_TabButtons[m_CurrentView].AddToClassList(SelectedStyle);
        }

        private void Reset()
        {
            m_TabViews[m_CurrentView].Reset();
        }

        private void Apply()
        {
            m_TabViews[m_CurrentView].Apply();
        }
    }
}
using Unity.MegacityMetro.Gameplay;
using Unity.MegacityMetro.UGS;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Unity.MegacityMetro.UI
{
    public class MatchMakingUI : MonoBehaviour
    {
        [SerializeField] private MultiplayerServerSettings m_ServerSettings;

        // UI Elements
        private TextField m_MultiplayerTextField;
        private DropdownField m_MultiplayerServerDropdownMenu;
        private Label m_ConnectionStatusLabel;
        private VisualElement m_MatchmakingLoadingBar;
        private VisualElement m_MatchmakingSpinner;

        public static MatchMakingUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Init()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            m_ConnectionStatusLabel = root.Q<Label>("connection-label");
            m_MatchmakingSpinner = root.Q<VisualElement>("matchmaking-spinner");
            m_MatchmakingLoadingBar = root.Q<VisualElement>("matchmaking-loading-bar");
            m_MultiplayerTextField = root.Q<TextField>("multiplayer-server-textfield");
            m_MultiplayerServerDropdownMenu = root.Q<DropdownField>("multiplayer-server-location");
            m_MultiplayerServerDropdownMenu.choices = m_ServerSettings.GetUIChoices();
            m_MultiplayerServerDropdownMenu.RegisterValueChangedCallback(OnServerDropDownChanged);

            m_MultiplayerTextField.RegisterValueChangedCallback(newStringEvent =>
            {
                UpdatePortAndIP(newStringEvent.newValue);
            });
        }


        private void OnServerDropDownChanged(ChangeEvent<string> value)
        {
            m_MultiplayerTextField.value = m_ServerSettings.GetIPByName(value.newValue);
            UpdatePortAndIP(m_MultiplayerTextField.value);
        }

        private void UpdatePortAndIP(string newStringEvent)
        {
            var ipSplit = newStringEvent.Split(":");
            if (ipSplit.Length < 2)
                return;

            var ip = ipSplit[0];
            var portString = ipSplit[1];
            if (!ushort.TryParse(portString, out var portShort))
                return;
            MatchMakingConnector.Instance.SetIPAndPort(ip, portShort);
        }

        public void UpdateConnectionStatus(string status)
        {
            m_ConnectionStatusLabel.style.display = DisplayStyle.Flex;
            m_ConnectionStatusLabel.text = status;
        }

        public void SetUIConnectionStatusEnable(bool matchmaking)
        {
            m_MatchmakingLoadingBar.style.display = matchmaking ? DisplayStyle.Flex : DisplayStyle.None;
            m_ConnectionStatusLabel.style.display = matchmaking ? DisplayStyle.Flex : DisplayStyle.None;
            
            if(matchmaking)
                AnimateLoadingBar();
        }

        public void SetConnectionMode(bool isMatchMaking)
        {
            m_MultiplayerTextField.style.display = isMatchMaking ? DisplayStyle.None : DisplayStyle.Flex;
            m_MultiplayerServerDropdownMenu.style.display = isMatchMaking ? DisplayStyle.None : DisplayStyle.Flex;
            m_MultiplayerServerDropdownMenu.value = m_MultiplayerServerDropdownMenu.choices[0] ?? "LOCAL";
            UpdatePortAndIP(m_ServerSettings.GetIPByName(m_MultiplayerServerDropdownMenu.value));
        }

        private void AnimateLoadingBar()
        {
            if (m_MatchmakingLoadingBar.style.display == DisplayStyle.Flex)
            {
                var currentRotation = m_MatchmakingSpinner.style.rotate;
                var angle = currentRotation.value.angle.value;
                var q = Quaternion.Euler(0f, 0f, angle + 180f);
                m_MatchmakingSpinner.experimental.animation.Rotation(q, 500).OnCompleted(AnimateLoadingBar)
                    .easingCurve = Easing.Linear;
            }
        }
    }
}
#if DEBUG
using BalanceTweaksPlugin.Patches;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BalanceTweaksPlugin.DebugTools
{
    internal class StressTimerDebugUI : MonoBehaviour
    {
        private static bool uiCreated;
        private Canvas canvas;
        private Text stressText;
        private Image fillBar;
        private bool visible = true;

        private void Awake()
        {
            if (uiCreated)
            {
                Destroy(this);
                return;
            }
            uiCreated = true;
            CreateUI();
        }

        private void CreateUI()
        {
            var root = new GameObject("StressTimerDebugUI");
            root.transform.SetParent(transform, false);

            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.6f);
            var panelRect = panelImage.rectTransform;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(8f, -8f);
            panelRect.sizeDelta = new Vector2(340f, 150f);

            var text = new GameObject("StressText");
            text.transform.SetParent(panel.transform, false);
            stressText = text.AddComponent<Text>();
            stressText.font = LoadBuiltinFont();
            stressText.fontSize = 16;
            stressText.color = Color.white;
            stressText.alignment = TextAnchor.UpperLeft;
            stressText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var textRect = stressText.rectTransform;
            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = new Vector2(0f, 1f);
            textRect.pivot = new Vector2(0f, 1f);
            textRect.anchoredPosition = new Vector2(10f, -6f);
            textRect.sizeDelta = new Vector2(320f, 110f);

            var barBg = new GameObject("BarBackground");
            barBg.transform.SetParent(panel.transform, false);
            var barBgImage = barBg.AddComponent<Image>();
            barBgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            var barBgRect = barBgImage.rectTransform;
            barBgRect.anchorMin = new Vector2(0f, 0f);
            barBgRect.anchorMax = new Vector2(0f, 0f);
            barBgRect.pivot = new Vector2(0f, 0f);
            barBgRect.anchoredPosition = new Vector2(10f, 8f);
            barBgRect.sizeDelta = new Vector2(320f, 16f);

            var barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBg.transform, false);
            fillBar = barFill.AddComponent<Image>();
            fillBar.color = new Color(0.9f, 0.3f, 0.1f, 1f);
            fillBar.type = Image.Type.Filled;
            fillBar.fillMethod = Image.FillMethod.Horizontal;
            fillBar.fillOrigin = 0;
            fillBar.fillAmount = 0f;
            var fillRect = fillBar.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private static Font LoadBuiltinFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
            {
                visible = !visible;
            }
            canvas.enabled = visible;
            if (!visible)
            {
                return;
            }

            float stress = StressMechanismPatch.stressTimer;
            float locationRate = StressMechanismPatch.currentLocationRate;
            bool inShip = StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase;
            bool modEnabled = BalanceTweaksPlugin.EnableStressMechanism.Value;
            int playersConnected = StartOfRound.Instance != null ? StartOfRound.Instance.connectedPlayersAmount : -1;
            int playersLiving = StartOfRound.Instance != null ? StartOfRound.Instance.livingPlayers : -1;

            stressText.text = string.Format(
                "Stress      {0,4:0.00} / 1.00\n" +
                "LocRate     {1,6:0.00000} /s\n" +
                "InShipPhase {2}    ModEnabled {3}\n" +
                "Players     conn {4,2}  alive {5,2}",
                stress, locationRate,
                inShip ? "Y" : "N",
                modEnabled ? "Y" : "N",
                playersConnected, playersLiving);

            fillBar.fillAmount = Mathf.Clamp01(stress);
        }
    }
}
#endif

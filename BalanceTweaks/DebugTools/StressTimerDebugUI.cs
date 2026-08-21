#if DEBUG
using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
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

            PlayerControllerB localPlayer = GameNetworkManager.Instance != null
                ? GameNetworkManager.Instance.localPlayerController
                : null;

            float stress = WalkingStaminaPatch.stressTimer;
            float threshold = WalkingStaminaPatch.stressChargeThreshold;
            float insanity = localPlayer != null ? localPlayer.insanityLevel : 0f;
            float maxInsanity = localPlayer != null ? localPlayer.maxInsanityLevel : 0f;
            bool inShip = StartOfRound.Instance != null && StartOfRound.Instance.inShipPhase;
            bool modEnabled = BalanceTweaksPlugin.EnableWalkDrainsStamina.Value;
            bool charging = insanity > threshold;

            float ratePerSecond = charging ? Mathf.InverseLerp(threshold, maxInsanity, insanity) / WalkingStaminaPatch.SecondsToFullStress : 0f;

            stressText.text = string.Format(
                "Stress      {0,4:0.00} / 1.00\n" +
                "Insanity    {1,4:0.0} / {2:0.0}   (charge > {3:0.0})\n" +
                "Rate        {4,4:0.000} /s\n" +
                "InShipPhase {5}    ModEnabled {6}",
                stress, insanity, maxInsanity, threshold, ratePerSecond,
                inShip ? "Y" : "N",
                modEnabled ? "Y" : "N");

            fillBar.fillAmount = Mathf.Clamp01(stress);
        }
    }
}
#endif

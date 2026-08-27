using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BalanceTweaksPlugin.Effects
{
    internal class StressVignetteEffect : MonoBehaviour
    {
        private const float VignetteIntensity = 0.75f;
        private const float VignetteSmoothness = 0.45f;
        private const float CenterExposure = -1f;
        private const float DesaturateThreshold = 0.85f;
        private const float DesaturateSaturation = -100f;

        private const float TriggerThreshold = 0.5f;
        private const float MinChancePerSecond = 0.01f;
        private const float MaxChancePerSecond = 0.2f;

        private const float EpisodeDuration = 10f;
        private const float CooldownAtThreshold = 60f;
        private const float CooldownAtMaxStress = 30f;
        private const float FadeInPortion = 0.2f;
        private const float FadeOutPortion = 0.3f;

        private Volume stressVolume;
        private Vignette vignette;
        private ColorAdjustments colorAdjustments;

        private AudioManager audio;
        private bool desaturateSoundTriggered;

        private bool episodeActive;
        private float episodeTimer;
        private float episodeDuration;
        private float cooldownRemaining;

        private void Awake()
        {
            CreateVolume();
            audio = GetComponent<AudioManager>();
        }

        private void CreateVolume()
        {
            var volumeGo = new GameObject("StressVignetteVolume");
            volumeGo.transform.SetParent(transform, false);

            stressVolume = volumeGo.AddComponent<Volume>();
            stressVolume.isGlobal = true;
            stressVolume.priority = 100f;
            stressVolume.weight = 0f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            stressVolume.profile = profile;

            vignette = profile.Add<Vignette>(true);
            vignette.mode.Override(VignetteMode.Procedural);
            vignette.color.Override(Color.black);
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0f);
            vignette.smoothness.Override(VignetteSmoothness);
            vignette.roundness.Override(1f);
            vignette.opacity.Override(1f);

            colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.postExposure.Override(0f);
            colorAdjustments.saturation.Override(0f);
        }

        private void Update()
        {
            if (episodeActive)
            {
                if (InGameContext())
                {
                    UpdateEpisode(StressMechanismPatch.stressTimer);
                }
                else
                {
                    EndEpisode(StressMechanismPatch.stressTimer);
                }
                return;
            }

            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
                return;
            }

            if (!InGameContext())
                return;

            float stress = StressMechanismPatch.stressTimer;
            if (stress > TriggerThreshold)
            {
                TryStartEpisode(stress);
            }
        }

        private bool InGameContext()
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                return false;

            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase)
                return false;

            PlayerControllerB local = GameNetworkManager.Instance != null
                ? GameNetworkManager.Instance.localPlayerController
                : null;

            return local != null && !local.isPlayerDead && local.isInsideFactory;
        }

        private void TryStartEpisode(float stress)
        {
            float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
            float chance = Mathf.Lerp(MinChancePerSecond, MaxChancePerSecond, intensity) * Time.deltaTime;

            if (Random.value < chance)
            {
                episodeActive = true;
                episodeTimer = 0f;
                episodeDuration = EpisodeDuration;
                desaturateSoundTriggered = false;
                audio.PlayBlackoutSound();
            }
        }

        private void UpdateEpisode(float stress)
        {
            episodeTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(episodeTimer / episodeDuration);

            if (stress > DesaturateThreshold && !desaturateSoundTriggered)
            {
                desaturateSoundTriggered = true;
                audio.PlayDesaturateSound();
            }

            float alpha;
            if (progress < FadeInPortion)
            {
                alpha = Mathf.SmoothStep(0f, 1f, progress / FadeInPortion);
            }
            else if (progress > 1f - FadeOutPortion)
            {
                float fadeOutProgress = (progress - (1f - FadeOutPortion)) / FadeOutPortion;
                alpha = 1f - Mathf.SmoothStep(0f, 1f, fadeOutProgress);
            }
            else
            {
                alpha = 1f;
            }
            /*
            if (stress <= TriggerThreshold && progress > FadeInPortion)
            {
                ApplyDepth(alpha, stress);
                EndEpisode(stress);
                return;
            }
            */

            ApplyDepth(alpha, stress);

            if (progress >= 1f)
            {
                EndEpisode(stress);
            }
        }

        private void ApplyDepth(float alpha, float stress)
        {
            vignette.intensity.Override(VignetteIntensity * alpha);
            colorAdjustments.postExposure.Override(CenterExposure * alpha);
            colorAdjustments.saturation.Override(stress > DesaturateThreshold ? DesaturateSaturation * alpha : 0f);
            stressVolume.weight = 1f;
        }

        private void EndEpisode(float stress)
        {
            if (!episodeActive)
                return;

            episodeActive = false;
            vignette.intensity.Override(0f);
            colorAdjustments.postExposure.Override(0f);
            colorAdjustments.saturation.Override(0f);
            stressVolume.weight = 0f;
            desaturateSoundTriggered = false;

            float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
            cooldownRemaining = Mathf.Lerp(CooldownAtThreshold, CooldownAtMaxStress, intensity);
        }
    }
}

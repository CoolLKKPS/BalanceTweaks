using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BalanceTweaksPlugin.Effects
{
    internal class StressVignetteEffect : MonoBehaviour
    {
        private const float VignetteIntensity = 0.85f;
        private const float VignetteSmoothness = 0.6f;
        private const float CenterExposure = -1f;

        private const float TriggerThreshold = 0.4f;

        private const float MinChancePerSecond = 0.01f;
        private const float MaxChancePerSecond = 0.2f;

        private const float CooldownAtThreshold = 60f;
        private const float CooldownAtMaxStress = 30f;

        private const float BlackoutDuration = 12f;

        private AudioManager audio;
        private Volume stressVolume;
        private Vignette vignette;
        private ColorAdjustments colorAdjustments;
        private bool blackoutActive;
        private float blackoutTimer;
        private float blackoutDuration;
        private float cooldownRemaining;

        private void Awake()
        {
            CreateVolume();
            audio = GetComponent<AudioManager>();
        }

        private void Update()
        {
            if (blackoutActive)
            {
                if (InGameContext())
                {
                    UpdateBlackout(StressMechanismPatch.stressTimer);
                }
                else
                {
                    EndBlackout(StressMechanismPatch.stressTimer);
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
                TryStartBlackout(stress);
            }
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
            colorAdjustments.saturation.overrideState = false;
        }

        private bool InGameContext()
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                return false;

            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase)
                return false;

            PlayerControllerB local = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;

            return local != null && !local.isPlayerDead;
        }

        private void TryStartBlackout(float stress)
        {
            float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
            float chance = Mathf.Lerp(MinChancePerSecond, MaxChancePerSecond, intensity) * Time.deltaTime;

            if (Random.value < chance)
            {
                blackoutActive = true;
                blackoutTimer = 0f;
                blackoutDuration = BlackoutDuration;
                audio.PlayBlackoutSound();
            }
        }

        private void UpdateBlackout(float stress)
        {
            blackoutTimer += Time.deltaTime;

            ApplyDepth(1f, stress);

            if (blackoutTimer >= blackoutDuration)
            {
                EndBlackout(stress);
            }
        }

        private void ApplyDepth(float alpha, float stress)
        {
            vignette.intensity.Override(VignetteIntensity * alpha);
            colorAdjustments.postExposure.Override(CenterExposure * alpha);
            stressVolume.weight = 1f;
        }

        private void EndBlackout(float stress)
        {
            if (!blackoutActive)
                return;

            blackoutActive = false;
            vignette.intensity.Override(0f);
            colorAdjustments.postExposure.Override(0f);
            stressVolume.weight = 0f;
            audio.StopBlackoutSound();

            float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
            cooldownRemaining = Mathf.Lerp(CooldownAtThreshold, CooldownAtMaxStress, intensity);
        }
    }
}

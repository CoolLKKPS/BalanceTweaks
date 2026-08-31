using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BalanceTweaksPlugin.Effects
{
    internal class DesaturateEffect : MonoBehaviour
    {
        private const float DesaturateSaturation = -100f;

        private const float TriggerThreshold = 0.6f;

        private const float MinChancePerSecond = 0.01f;
        private const float MaxChancePerSecond = 0.2f;

        private const float CooldownAtThreshold = 60f;
        private const float CooldownAtMaxStress = 30f;

        private const float DesaturateDuration = 12f;

        private AudioManager audio;
        private Volume desaturateVolume;
        private ColorAdjustments colorAdjustments;
        private bool hallucinationActive;
        private float hallucinationTimer;
        private float cooldownRemaining;

        private void Awake()
        {
            CreateVolume();
            audio = GetComponent<AudioManager>();
        }

        private void Update()
        {
            if (hallucinationActive)
            {
                if (InGameContext())
                {
                    UpdateActiveHallucination();
                }
                else
                {
                    EndHallucination(StressMechanismPatch.stressTimer);
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
                float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
                float chance = Mathf.Lerp(MinChancePerSecond, MaxChancePerSecond, intensity) * Time.deltaTime;

                if (Random.value < chance)
                {
                    StartHallucination();
                }
            }
        }

        private void CreateVolume()
        {
            var volumeGo = new GameObject("StressDesaturateVolume");
            volumeGo.transform.SetParent(transform, false);

            desaturateVolume = volumeGo.AddComponent<Volume>();
            desaturateVolume.isGlobal = true;
            desaturateVolume.priority = 100f;
            desaturateVolume.weight = 0f;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            desaturateVolume.profile = profile;

            colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.saturation.Override(0f);
            colorAdjustments.postExposure.overrideState = false;
        }

        private bool InGameContext()
        {
            if (!BalanceTweaksPlugin.EnableStressMechanism.Value)
                return false;

            if (StartOfRound.Instance == null || StartOfRound.Instance.inShipPhase)
                return false;

            PlayerControllerB local = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;

            return local != null && !local.isPlayerDead && local.isInsideFactory;
        }

        private void StartHallucination()
        {
            hallucinationActive = true;
            hallucinationTimer = 0f;
            audio.PlayDesaturateSound();
        }

        private void UpdateActiveHallucination()
        {
            hallucinationTimer += Time.deltaTime;

            colorAdjustments.saturation.Override(DesaturateSaturation);
            desaturateVolume.weight = 1f;

            if (hallucinationTimer >= DesaturateDuration)
            {
                EndHallucination(StressMechanismPatch.stressTimer);
            }
        }

        private void EndHallucination(float stress)
        {
            if (!hallucinationActive)
                return;

            hallucinationActive = false;
            colorAdjustments.saturation.Override(0f);
            desaturateVolume.weight = 0f;

            float intensity = Mathf.InverseLerp(TriggerThreshold, 1f, stress);
            cooldownRemaining = Mathf.Lerp(CooldownAtThreshold, CooldownAtMaxStress, intensity);
        }
    }
}

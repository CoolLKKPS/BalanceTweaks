using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
using UnityEngine;

namespace BalanceTweaksPlugin.Effects
{
    internal class TinnitusEffect : MonoBehaviour
    {
        private const float DeafenDecibels = -80f;
        private const float HallucinationMuteDelay = 5f;

        private const float TinnitusThreshold = 0.2f;
        private const float HallucinationThreshold = 0.65f;

        private const float MinChancePerSecond = 0.01f;
        private const float MaxChancePerSecond = 0.25f;

        private const float CooldownAtThreshold = 70f;
        private const float CooldownAtMaxStress = 35f;

        private const float TinnitusDuration = 5f;
        private const float HallucinationDuration = 10f;

        private AudioManager audio;
        private bool tinnitusActive;
        private float soundRemaining;
        private float muteDelayRemaining;
        private float cooldownRemaining;

        private void Awake()
        {
            audio = GetComponent<AudioManager>();
        }

        private void Update()
        {
            if (tinnitusActive)
            {
                if (InGameContext())
                {
                    UpdateActiveTinnitus();
                }
                else
                {
                    EndTinnitus(StressMechanismPatch.stressTimer);
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
            if (stress > TinnitusThreshold)
            {
                float intensity = Mathf.InverseLerp(TinnitusThreshold, 1f, stress);
                float chance = Mathf.Lerp(MinChancePerSecond, MaxChancePerSecond, intensity) * Time.deltaTime;

                if (Random.value < chance)
                {
                    StartTinnitus(stress);
                }
            }
        }

        private void OnDisable()
        {
            EndTinnitus(StressMechanismPatch.stressTimer);
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

        private void StartTinnitus(float stress)
        {
            tinnitusActive = true;

            bool hallucination = stress >= HallucinationThreshold && IsInsideFactory() && Random.value < 0.5f;

            if (hallucination)
            {
                soundRemaining = HallucinationDuration;
                muteDelayRemaining = HallucinationMuteDelay;
                audio.PlayHallucinationSound();
            }
            else
            {
                soundRemaining = TinnitusDuration;
                muteDelayRemaining = 0f;
                audio.PlayTinnitusSound();
            }
        }

        private void UpdateActiveTinnitus()
        {
            soundRemaining -= Time.deltaTime;

            if (muteDelayRemaining > 0f)
            {
                muteDelayRemaining -= Time.deltaTime;
            }
            else
            {
                ApplyMute();
            }

            if (soundRemaining <= 0f)
            {
                EndTinnitus(StressMechanismPatch.stressTimer);
            }
        }

        private void ApplyMute()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetDiageticMasterVolume(DeafenDecibels);
            }

            audio.SetDesaturateSoundMuted(true);

            if (StartOfRound.Instance == null || GameNetworkManager.Instance == null)
                return;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player == GameNetworkManager.Instance.localPlayerController)
                    continue;

                if (player.currentVoiceChatIngameSettings != null && player.currentVoiceChatIngameSettings.voiceAudio != null)
                {
                    player.currentVoiceChatIngameSettings.voiceAudio.mute = true;
                }
            }
        }

        private static bool IsInsideFactory()
        {
            return GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.isInsideFactory;
        }

        private void EndTinnitus(float stress)
        {
            if (!tinnitusActive)
                return;

            tinnitusActive = false;
            soundRemaining = 0f;
            muteDelayRemaining = 0f;

            audio.StopTinnitusSound();
            audio.StopHallucinationSound();
            audio.SetDesaturateSoundMuted(false);

            float intensity = Mathf.InverseLerp(TinnitusThreshold, 1f, stress);
            cooldownRemaining = Mathf.Lerp(CooldownAtThreshold, CooldownAtMaxStress, intensity);

            if (StartOfRound.Instance == null || GameNetworkManager.Instance == null)
                return;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player == GameNetworkManager.Instance.localPlayerController)
                    continue;

                if (player.currentVoiceChatIngameSettings != null && player.currentVoiceChatIngameSettings.voiceAudio != null)
                {
                    player.currentVoiceChatIngameSettings.voiceAudio.mute = false;
                }
            }
        }
    }
}

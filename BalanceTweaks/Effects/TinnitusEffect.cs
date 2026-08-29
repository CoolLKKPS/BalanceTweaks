using BalanceTweaksPlugin.Patches;
using GameNetcodeStuff;
using UnityEngine;

namespace BalanceTweaksPlugin.Effects
{
    internal class TinnitusEffect : MonoBehaviour
    {
        private const float TinnitusThreshold = 0.2f;
        private const float RampStartStress = 0.2f;
        private const float RampEndStress = 0.84f;
        private const float HallucinationThreshold = 0.85f;

        private const float CooldownAtThreshold = 60f;
        private const float CooldownAtMaxStress = 30f;

        private const float TinnitusMuteDuration = 5f;
        private const float HallucinationMuteDuration = 10f;

        private const float DeafenDecibels = -80f;

        private AudioManager audio;
        private bool tinnitusActive;
        private float tinnitusRemaining;
        private float cooldownRemaining;

        private void Awake()
        {
            audio = GetComponent<AudioManager>();
        }

        private void Update()
        {
            if (!InGameContext())
            {
                EndTinnitus();
                return;
            }

            if (tinnitusActive)
            {
                tinnitusRemaining -= Time.deltaTime;
                ApplyMute();
                if (tinnitusRemaining <= 0f)
                {
                    EndTinnitus();
                }
                return;
            }

            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
                return;
            }

            float stress = StressMechanismPatch.stressTimer;
            if (stress > TinnitusThreshold)
            {
                StartTinnitus(stress);
            }
        }

        private void StartTinnitus(float stress)
        {
            tinnitusActive = true;

            bool hallucination = stress >= HallucinationThreshold && Random.value < 0.5f;

            if (hallucination)
            {
                tinnitusRemaining = HallucinationMuteDuration;
                audio.PlayHallucinationSound();
            }
            else
            {
                tinnitusRemaining = TinnitusMuteDuration;
                audio.PlayTinnitusSound();
            }

            float intensity = Mathf.InverseLerp(RampStartStress, RampEndStress, stress);
            cooldownRemaining = Mathf.Lerp(CooldownAtThreshold, CooldownAtMaxStress, intensity);
        }

        private void ApplyMute()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetDiageticMasterVolume(DeafenDecibels);
            }

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

        private void EndTinnitus()
        {
            if (!tinnitusActive)
                return;

            tinnitusActive = false;
            tinnitusRemaining = 0f;

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

        private void OnDisable()
        {
            EndTinnitus();
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
    }
}

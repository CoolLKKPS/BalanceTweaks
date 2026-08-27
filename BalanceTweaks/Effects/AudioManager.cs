using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace BalanceTweaksPlugin.Effects
{
    internal class AudioManager : MonoBehaviour
    {
        private const string BlackoutSoundName = "blackout";
        private const string DesaturateSoundName = "desaturate";
        private const float SoundVolume = 1f;
        private static readonly string[] AudioExtensions = { ".wav", ".ogg", ".mp3" };

        private AudioSource audioSource;
        private AudioClip blackoutClip;
        private AudioClip desaturateClip;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            StartCoroutine(LoadClip(BlackoutSoundName, clip => blackoutClip = clip));
            StartCoroutine(LoadClip(DesaturateSoundName, clip => desaturateClip = clip));
        }

        public void PlayBlackoutSound()
        {
            if (blackoutClip == null)
                return;

            audioSource.volume = SoundVolume;
            audioSource.PlayOneShot(blackoutClip);
        }

        public void PlayDesaturateSound()
        {
            if (desaturateClip == null)
                return;

            audioSource.volume = SoundVolume;
            audioSource.PlayOneShot(desaturateClip);
        }

        private IEnumerator LoadClip(string baseName, System.Action<AudioClip> onLoaded)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                yield break;

            string assetsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");

            string path = null;
            AudioType audioType = AudioType.WAV;
            foreach (string ext in AudioExtensions)
            {
                string candidate = Path.Combine(assetsDir, baseName + ext);
                if (File.Exists(candidate))
                {
                    path = candidate;
                    audioType = GetAudioType(ext);
                    break;
                }
            }

            if (path == null)
            {
                BalanceTweaksPlugin.logger.LogWarning($"Stress sound '{baseName}' not found in '{assetsDir}' (tried {string.Join(", ", AudioExtensions)})");
                yield break;
            }

            using var request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType);
            request.SendWebRequest();

            while (!request.isDone)
                yield return null;

            if (request.result != UnityWebRequest.Result.Success)
            {
                BalanceTweaksPlugin.logger.LogWarning($"Failed to load stress sound '{path}': {request.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip == null)
            {
                BalanceTweaksPlugin.logger.LogWarning($"Stress sound '{path}' loaded but produced no clip.");
            }
            else
            {
                onLoaded?.Invoke(clip);
            }
        }

        private static AudioType GetAudioType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".ogg" => AudioType.OGGVORBIS,
                ".mp3" => AudioType.MPEG,
                _ => AudioType.WAV,
            };
        }
    }
}

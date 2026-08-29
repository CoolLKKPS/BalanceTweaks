using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace BalanceTweaksPlugin.Effects
{
    internal class AudioManager : MonoBehaviour
    {
        private const string BlackoutFolder = "blackout";
        private const string TinnitusFolder = "tinnitus";
        private const string BlackoutSoundName = "blackout";
        private const string DesaturateSoundName = "desaturate";
        private const string TinnitusSoundName = "tinnitus";
        private const string HallucinationPrefix = "hallucination";
        private const float SoundVolume = 1f;
        private static readonly string[] AudioExtensions = { ".wav", ".ogg", ".mp3" };

        private AudioSource audioSource;
        private AudioClip blackoutClip;
        private AudioClip desaturateClip;
        private AudioClip tinnitusClip;
        private readonly List<AudioClip> hallucinationClips = new List<AudioClip>();

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            StartCoroutine(LoadClip(BlackoutSoundName, BlackoutFolder, clip => blackoutClip = clip));
            StartCoroutine(LoadClip(DesaturateSoundName, BlackoutFolder, clip => desaturateClip = clip));
            StartCoroutine(LoadClip(TinnitusSoundName, TinnitusFolder, clip => tinnitusClip = clip));
            StartCoroutine(LoadHallucinationClips());
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

        public void PlayTinnitusSound()
        {
            if (tinnitusClip == null)
                return;

            audioSource.volume = SoundVolume;
            audioSource.PlayOneShot(tinnitusClip);
        }

        public void PlayHallucinationSound()
        {
            if (hallucinationClips.Count == 0)
                return;

            audioSource.volume = SoundVolume;
            audioSource.PlayOneShot(hallucinationClips[Random.Range(0, hallucinationClips.Count)]);
        }

        private IEnumerator LoadClip(string baseName, string subDir, System.Action<AudioClip> onLoaded)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                yield break;

            string assetsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets", subDir);

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

            yield return LoadClipAtPath(path, audioType, onLoaded);
        }

        private IEnumerator LoadHallucinationClips()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets", TinnitusFolder);

            if (!Directory.Exists(dir))
            {
                BalanceTweaksPlugin.logger.LogWarning($"Hallucination sound folder not found: {dir}");
                yield break;
            }

            foreach (string file in Directory.GetFiles(dir))
            {
                string baseName = Path.GetFileNameWithoutExtension(file);
                if (!baseName.StartsWith(HallucinationPrefix, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (System.Array.IndexOf(AudioExtensions, ext) < 0)
                    continue;

                yield return LoadClipAtPath(file, GetAudioType(ext), clip => hallucinationClips.Add(clip));
            }

            if (hallucinationClips.Count == 0)
            {
                BalanceTweaksPlugin.logger.LogWarning($"No hallucination sounds found in '{dir}' (expected files starting with '{HallucinationPrefix}')");
            }
        }

        private IEnumerator LoadClipAtPath(string path, AudioType audioType, System.Action<AudioClip> onLoaded)
        {
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

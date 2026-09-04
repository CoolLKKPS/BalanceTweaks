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
        private const string BasicFolder = "Basic";
        private const string HallucinationFolder = "Hallucination";
        private const string HallucinationPrefix = "Hallucination";
        private const string BlackoutSoundName = "Blackout";
        private const string DesaturateSoundName = "Desaturate";
        private const string TinnitusSoundName = "Tinnitus";
        private const float SoundVolume = 1f;
        private static readonly string[] AudioExtensions = { ".wav", ".ogg", ".mp3" };

        private AudioSource blackoutSource;
        private AudioSource desaturateSource;
        private AudioSource tinnitusSource;
        private AudioSource hallucinationSource;
        private float desaturateTargetVolume = SoundVolume;
        private AudioClip blackoutClip;
        private AudioClip desaturateClip;
        private AudioClip tinnitusClip;
        private readonly List<AudioClip> hallucinationClips = new List<AudioClip>();

        private void Awake()
        {
            blackoutSource = CreateSource();
            desaturateSource = CreateSource();
            tinnitusSource = CreateSource();
            hallucinationSource = CreateSource();

            StartCoroutine(LoadClip(BlackoutSoundName, BasicFolder, clip => blackoutClip = clip));
            StartCoroutine(LoadClip(DesaturateSoundName, BasicFolder, clip => desaturateClip = clip));
            StartCoroutine(LoadClip(TinnitusSoundName, BasicFolder, clip => tinnitusClip = clip));
            StartCoroutine(LoadHallucinationClips());
        }

        private void Update()
        {
            if (desaturateSource == null)
                return;

            desaturateSource.volume = Mathf.Lerp(desaturateSource.volume, desaturateTargetVolume, 5f * Time.deltaTime);
        }

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = SoundVolume;
            return source;
        }

        private void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null)
                return;

            source.PlayOneShot(clip);
        }

        public void PlayBlackoutSound()
        {
            PlayOneShot(blackoutSource, blackoutClip);
        }

        public void PlayDesaturateSound()
        {
            PlayOneShot(desaturateSource, desaturateClip);
        }

        public void PlayTinnitusSound()
        {
            PlayOneShot(tinnitusSource, tinnitusClip);
        }

        public void PlayHallucinationSound()
        {
            if (hallucinationClips.Count == 0)
                return;

            PlayOneShot(hallucinationSource, hallucinationClips[Random.Range(0, hallucinationClips.Count)]);
        }

        private void StopOneShot(AudioSource source)
        {
            if (source == null)
                return;

            source.Stop();
        }

        public void StopBlackoutSound()
        {
            StopOneShot(blackoutSource);
        }

        public void StopDesaturateSound()
        {
            StopOneShot(desaturateSource);
        }

        public void StopTinnitusSound()
        {
            StopOneShot(tinnitusSource);
        }

        public void StopHallucinationSound()
        {
            StopOneShot(hallucinationSource);
        }

        public void SetDesaturateSoundMuted(bool muted)
        {
            desaturateTargetVolume = muted ? 0f : SoundVolume;
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
            string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets", HallucinationFolder);

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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Clip Library")]
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private string[] _clipKeys;

    // Built from _clips/_clipKeys in Awake for O(1) lookup instead of
    // scanning the array on every Play() call.
    private Dictionary<string, AudioClip> _clipDict;

    public event Action OnNarrationComplete;

    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        _clipDict = new Dictionary<string, AudioClip>();

        if (_clips == null || _clipKeys == null) return;

        if (_clips.Length != _clipKeys.Length)
            Debug.LogWarning($"[NarrationManager] _clips ({_clips.Length}) and _clipKeys ({_clipKeys.Length}) are different lengths — some entries will be skipped.");

        int count = Mathf.Min(_clips.Length, _clipKeys.Length);
        for (int i = 0; i < count; i++)
        {
            if (string.IsNullOrEmpty(_clipKeys[i]) || _clips[i] == null) continue;

            if (_clipDict.ContainsKey(_clipKeys[i]))
                Debug.LogWarning($"[NarrationManager] Duplicate key '{_clipKeys[i]}' at index {i} — skipped.");
            else
                _clipDict.Add(_clipKeys[i], _clips[i]);
        }
    }

    public void Play(string key)
    {
        if (_audioSource == null)
        {
            Debug.LogError("[NarrationManager] No AudioSource assigned — cannot play narration.");
            OnNarrationComplete?.Invoke();
            return;
        }

        if (!_clipDict.TryGetValue(key, out AudioClip clip))
        {
            Debug.LogWarning($"[NarrationManager] Clip '{key}' not found.");
            OnNarrationComplete?.Invoke();
            return;
        }

        // Stop any in-progress narration before starting a new one —
        // prevents two WaitForEnd coroutines firing OnNarrationComplete twice.
        StopAllCoroutines();
        _audioSource.Stop();

        _audioSource.clip = clip;
        _audioSource.Play();

        // WaitForSecondsRealtime is not affected by Time.timeScale,
        // so pausing the game won't desynchronise the callback.
        StartCoroutine(WaitForEnd(clip.length));
    }

    private IEnumerator WaitForEnd(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        OnNarrationComplete?.Invoke();
    }
}

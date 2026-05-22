using System;
using System.Collections;
using UnityEngine;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance { get; private set; }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private string[] _clipKeys;

    public event Action OnNarrationComplete;

    public bool IsPlaying => _audioSource.isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(string key)
    {
        AudioClip clip = FindClip(key);
        if (clip == null)
        {
            OnNarrationComplete?.Invoke();
            return;
        }
        _audioSource.clip = clip;
        _audioSource.Play();
        StartCoroutine(WaitForEnd(clip.length));
    }

    private IEnumerator WaitForEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnNarrationComplete?.Invoke();
    }

    private AudioClip FindClip(string key)
    {
        for (int i = 0; i < _clipKeys.Length; i++)
        {
            if (_clipKeys[i] == key)
                return _clips[i];
        }
        Debug.LogWarning($"NarrationManager: clip '{key}' not found.");
        return null;
    }
}

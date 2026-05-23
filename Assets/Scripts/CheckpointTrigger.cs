using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private string _narrationKey;
    [SerializeField] private bool _saveRespawnPosition = true;
    [SerializeField] private Vector2 _respawnOffset = new Vector2(0f, 0.5f);

    private bool _hasTriggered = false;
    private PlayerManagerScript _player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        _hasTriggered = true;

        if (_saveRespawnPosition && GameManager.Instance != null)
            GameManager.Instance.SaveCheckpoint((Vector2)transform.position + _respawnOffset);

        // Fixed: was GetComponent<PlayerMovement>() ? wrong script, always returned null
        _player = other.GetComponent<PlayerManagerScript>();
        if (_player != null)
            _player.DisableInput();

        // string.IsNullOrEmpty handles both null and "" safely
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(_narrationKey))
        {
            NarrationManager.Instance.OnNarrationComplete += OnNarrationDone;
            NarrationManager.Instance.Play(_narrationKey);
        }
        else
        {
            if (_player != null) _player.EnableInput();
            _player = null;
        }
    }

    private void OnNarrationDone()
    {
        // Null-check Instance — it could theoretically be destroyed before
        // the coroutine finishes, which would crash without this guard.
        if (NarrationManager.Instance != null)
            NarrationManager.Instance.OnNarrationComplete -= OnNarrationDone;

        if (_player != null) _player.EnableInput();
        _player = null;
    }
}

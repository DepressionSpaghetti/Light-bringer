using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private string _narrationKey;
    [SerializeField] private bool _saveRespawnPosition = true;
    [SerializeField] private Vector2 _respawnOffset = new Vector2(0f, 0.5f);

    private bool _hasTriggered = false;
    private PlayerMovement _player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        _hasTriggered = true;

        if (_saveRespawnPosition && GameManager.Instance != null)
            GameManager.Instance.SaveCheckpoint((Vector2)transform.position + _respawnOffset);

        _player = other.GetComponent<PlayerMovement>();
        if (_player != null)
            _player.DisableInput();

        if (NarrationManager.Instance != null && _narrationKey != "")
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
        NarrationManager.Instance.OnNarrationComplete -= OnNarrationDone;
        if (_player != null) _player.EnableInput();
        _player = null;
    }
}

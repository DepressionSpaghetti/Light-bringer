using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State (read-only in Inspector)")]
    public int CurrentScene { get; private set; }
    public string LastCheckpointScene { get; private set; }
    public Vector3 LastCheckpointPos { get; private set; }

    // Set when the player dies in a different scene — the player is repositioned
    // inside OnSceneLoaded once the target scene has finished loading.
    private bool _respawnPending = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Default checkpoint = the scene the game starts in
        LastCheckpointScene = SceneManager.GetActiveScene().name;
        LastCheckpointPos = Vector3.zero;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CurrentScene = scene.buildIndex;

        // If the player died in a different scene, reposition them now that
        // the correct scene has finished loading.
        if (_respawnPending)
        {
            _respawnPending = false;
            RepositionPlayer();
        }
    }

    public void SaveCheckpoint(Vector3 position)
    {
        LastCheckpointPos = position;
        LastCheckpointScene = SceneManager.GetActiveScene().name;

#if UNITY_EDITOR
        Debug.Log($"[GameManager] Checkpoint saved at {position} in '{LastCheckpointScene}'");
#endif
    }

    public void PlayerDied()
    {
        if (SceneManager.GetActiveScene().name == LastCheckpointScene)
        {
            // Same scene — teleport and re-enable directly
            RepositionPlayer();
        }
        else
        {
            // Different scene — flag the pending respawn, then load
            _respawnPending = true;
            SceneManager.LoadScene(LastCheckpointScene);
        }
    }

    // Shared respawn logic used by both same-scene and cross-scene paths.
    private void RepositionPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[GameManager] RepositionPlayer: no GameObject tagged 'Player' found.");
            return;
        }

        player.transform.position = LastCheckpointPos;
        player.GetComponent<PlayerManagerScript>()?.EnableInput();
    }
}

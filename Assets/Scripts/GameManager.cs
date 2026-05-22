using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int CurrentScene { get; private set; }
    public Vector3 LastCheckpointPos { get; private set; }
    public string LastCheckpointScene { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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
    }

    public void SaveCheckpoint(Vector3 position)
    {
        LastCheckpointPos = position;
        LastCheckpointScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Checkpoint saved: {position} in {LastCheckpointScene}");
    }

    public void PlayerDied()
    {
        if (SceneManager.GetActiveScene().name == LastCheckpointScene)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = LastCheckpointPos;
                player.GetComponent<PlayerMovement>()?.EnableInput();
            }
        }
        else
        {
            SceneManager.LoadScene(LastCheckpointScene);
        }
    }
}

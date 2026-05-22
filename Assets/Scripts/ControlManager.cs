using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    public static ControlManager Instance { get; private set; }

    //InputActions declarations
    InputAction moveAction;
    InputAction jumpAction;
    InputAction runAction;
    InputAction shootAction;
    public Vector2 MoveValue { get; private set; }

    private bool _lastRunState;

    //Events
    public event Action<Vector2> Move;
    public event Action Jump;
    public event Action<bool> Run;
    public event Action Shoot;

    void Awake()
    {
        //Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Find input actions — log errors if any name is misspelled
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        runAction = InputSystem.actions.FindAction("Run");
        shootAction = InputSystem.actions.FindAction("Shoot");

        if (moveAction == null) Debug.LogError("ControlManager: 'Move' action not found in Input System!");
        if (jumpAction == null) Debug.LogError("ControlManager: 'Jump' action not found in Input System!");
        if (runAction == null) Debug.LogError("ControlManager: 'Run' action not found in Input System!");
        if (shootAction == null) Debug.LogError("ControlManager: 'Shoot' action not found in Input System!");
    }

    void Update()
    {
        // Move — broadcast every frame so PlayerManagerScript always has current input
        // (triggered only fires on the first pressed frame, missing held movement)
        MoveValue = moveAction.ReadValue<Vector2>();
        Move?.Invoke(MoveValue);

        // Jump — one-shot on press, correct
        if (jumpAction.WasPressedThisFrame())
            Jump?.Invoke();

        // Run — only fire on state change to avoid spamming the event every frame
        bool running = runAction.ReadValue<float>() > 0.5f;
        if (running != _lastRunState)
        {
            _lastRunState = running;
            Run?.Invoke(running);
        }

        // Shoot — one-shot on press, correct
        if (shootAction.WasPressedThisFrame())
            Shoot?.Invoke();
    }
}

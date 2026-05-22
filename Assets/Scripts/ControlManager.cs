using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class ControlManager : MonoBehaviour
{
    public static ControlManager Instance { get; private set; }

    //InputActions declarations
    InputAction moveAction;
    InputAction jumpAction;
    InputAction runAction;
    InputAction shootAction;
    Vector2 MoveValue { get; set; }

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

        //Find input actions from the Input System
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        runAction = InputSystem.actions.FindAction("Run");
        shootAction = InputSystem.actions.FindAction("Shoot");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Invoke move action
        MoveValue = moveAction.ReadValue<Vector2>();
        if (moveAction.triggered || moveAction.WasPerformedThisFrame())
            Move?.Invoke(MoveValue);
        else if (moveAction.WasReleasedThisFrame())
            Move?.Invoke(Vector2.zero);

        //Invoke jump action
        if (jumpAction.triggered)
        {
            Debug.Log($"jump trigger: {jumpAction.activeControl}");
            Jump?.Invoke();
        }

        //Invoke run action
        switch(runAction.ReadValue<float>())
        {
            case 1:
                Run?.Invoke(true);
                break;
            case 0:
                Run?.Invoke(false);
                break;
        }

        //Invoke shoot action
        if(shootAction.triggered)
        {
            Debug.Log($"Shoot trigger: {shootAction.activeControl}");
            Shoot?.Invoke();
        }
    }
}

using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private GameplayControls controls;

    public float Move { get; private set; }

    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnDashPressed;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new GameplayControls();

        controls.Player.XMovement.performed += ctx => Move = ctx.ReadValue<float>();
        controls.Player.XMovement.canceled += _ => Move = 0f;

        controls.Player.Jump.started += _ => OnJumpPressed?.Invoke();
        controls.Player.Jump.canceled += _ => OnJumpReleased?.Invoke();

        controls.Player.Dash.started += _ => OnDashPressed?.Invoke();
    }

    private void OnEnable() => controls?.Enable();
    private void OnDisable() => controls?.Disable();
}

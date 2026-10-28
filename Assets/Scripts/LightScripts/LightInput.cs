using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightInput : MonoBehaviour
{
    public static LightInput Instance { get; private set; }
    public InputActionReference leftClickAction;
    public InputActionReference scrollAction;

    public Action OnLeftClickActionPerformed;
    public Action<float> OnScrollActionPerformed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void OnEnable()
    {
        if (leftClickAction != null)
        {
            leftClickAction.action.Enable();
            leftClickAction.action.performed += HandleLeftClick;
        }

        if (scrollAction != null)
        {
            scrollAction.action.Enable();
            scrollAction.action.performed += HandleScroll;
        }
    }

    void OnDisable()
    {
        if (leftClickAction != null)
        {
            leftClickAction.action.performed -= HandleLeftClick;
            leftClickAction.action.Disable();
        }

        if (scrollAction != null)
        {
            scrollAction.action.performed -= HandleScroll;
            scrollAction.action.Disable();
        }
    }

    public void HandleLeftClick(InputAction.CallbackContext ctx)
    {
        OnLeftClickActionPerformed?.Invoke();
    }

    public void HandleScroll(InputAction.CallbackContext ctx)
    {
        float scrollY = ctx.ReadValue<Vector2>().y;
        OnScrollActionPerformed?.Invoke(scrollY);
    }
}
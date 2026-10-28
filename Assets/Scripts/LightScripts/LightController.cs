using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class LightController : MonoBehaviour
{
    protected Light2D lightSource;
    [SerializeField] protected bool isSelected = false;
    public bool IsSelected { get; set; }

    protected virtual void Start()
    {
        if (LightInput.Instance != null)
        {
            LightInput.Instance.OnScrollActionPerformed += ProcessInput;
        }

        lightSource = GetComponentInChildren<Light2D>();

        if (lightSource == null)
        {
            Debug.LogWarning("No light source on this game object");
        }
    }

    protected virtual void OnDestroy()
    {
        if (LightInput.Instance != null)
        {
            LightInput.Instance.OnScrollActionPerformed -= ProcessInput;
        }
    }

    private void ProcessInput(float direction)
    {
        if (lightSource != null)
        {
            MoveLight(direction);
        }
    }

    protected abstract void MoveLight(float direction);
}
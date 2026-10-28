using UnityEngine;

public class SpotLightController : LightController
{
    [SerializeField] private float rotationSpeed = 10f;

    protected override void MoveLight(float direction)
    {
        if (!isSelected) return;
        transform.Rotate(0, 0, direction * rotationSpeed);
    }
}
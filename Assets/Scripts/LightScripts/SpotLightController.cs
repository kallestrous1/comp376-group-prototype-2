using UnityEngine;

public class SpotLightController : LightController
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float minRotation = -45f;
    [SerializeField] private float maxRotation = 45f;

    private float currentRotation;

    protected override void Start()
    {
        base.Start();

        currentRotation = transform.localEulerAngles.z;

        if (currentRotation > 180f)
        {
            currentRotation -= 360f;
        }
    }

    protected override void MoveLight(float direction)
    {
        if (!isSelected) return;

        currentRotation += direction * rotationSpeed;

        currentRotation = Mathf.Clamp(currentRotation, minRotation, maxRotation);

        transform.localEulerAngles = new Vector3(0, 0, currentRotation);
    }
}
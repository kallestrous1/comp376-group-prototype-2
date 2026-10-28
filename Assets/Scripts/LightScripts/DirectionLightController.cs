using UnityEngine;

public enum DirectionType
{
    Horizontal,
    Vertical
}

public class DirectionLightController : LightController
{
    [SerializeField] private float translationSpeed = 0.5f;
    [SerializeField] private DirectionType directionType = DirectionType.Horizontal;
    [SerializeField] private float minPosition = -5f;
    [SerializeField] private float maxPosition = 5f;

    protected override void MoveLight(float direction)
    {
        if (!isSelected) return;

        Vector3 newPosition = transform.position;

        if (directionType == DirectionType.Horizontal)
        {
            newPosition.y += direction * translationSpeed;
            newPosition.y = Mathf.Clamp(newPosition.y, minPosition, maxPosition);
        }
        else
        {
            newPosition.x += direction * translationSpeed;
            newPosition.x = Mathf.Clamp(newPosition.x, minPosition, maxPosition);
        }

        transform.position = newPosition;
    }
}
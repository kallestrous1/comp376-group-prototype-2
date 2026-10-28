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

    protected override void MoveLight(float direction)
    {
        if (!isSelected) return;
        if (directionType == DirectionType.Horizontal)
        {
            transform.Translate(direction * translationSpeed, 0, 0, Space.World);
        }
        else
        {
            transform.Translate(0, direction * translationSpeed, 0, Space.World);
        }
    }
}
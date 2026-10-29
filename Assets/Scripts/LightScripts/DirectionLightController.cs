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
        // Wide level lights stay fixed. Only the circular spotlight rotates.
    }
}
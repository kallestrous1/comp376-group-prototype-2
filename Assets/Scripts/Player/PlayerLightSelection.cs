using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to the player prefab. Holds 0 or 1 selected light and selects it from the mouse
/// using the scene's Light Input click. Scroll control follows LightController.IsSelected.
/// Light prefabs do not need a click collider.
/// </summary>
[DisallowMultipleComponent]
public class PlayerLightSelection : MonoBehaviour
{
    [Header("Line of sight")]
    [Tooltip("World layers containing walls/platforms. Other lights are handled separately; triggers are ignored.")]
    [SerializeField] private LayerMask blockingLayers = ~0;
    [Tooltip("0 means no distance limit.")]
    [Min(0f)] [SerializeField] private float maximumDistance = 0f;
    [Min(0.05f)] [SerializeField] private float minimumLightTargetRadius = 0.4f;

    [Header("Feedback")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Color hoverColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.32f, 0.95f, 0.5f, 1f);
    [SerializeField] private Color rayColor = new Color(0.65f, 0.85f, 1f, 0.45f);
    [Min(0.005f)] [SerializeField] private float rayWidth = 0.025f;
    [SerializeField] private int raySortingOrder = 100;

    private LightInput subscribedInput;
    private LightController hoveredLight;
    private bool beamActive;
    private LineRenderer aimRay;
    private Material rayMaterial;
    private readonly Dictionary<SpriteRenderer, Color> originalTints = new Dictionary<SpriteRenderer, Color>();
    private bool warnedAboutInput;

    public LightController SelectedLight { get; private set; }

    private void Awake()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        CreateAimRay();
        if (aimRay != null) aimRay.enabled = false;
    }

    private void Start()
    {
        SetSelectedLight(null);
    }

    private void OnEnable() => TrySubscribeToInput();

    private void Update()
    {
        TrySubscribeToInput();
        if (gameCamera == null) gameCamera = Camera.main;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!overUi)
            {
                beamActive = !beamActive;
                if (!beamActive)
                {
                    ClearSelection();
                    SetHovered(null);
                    if (aimRay != null) aimRay.enabled = false;
                }
            }
        }

        if (!beamActive)
        {
            if (aimRay != null) aimRay.enabled = false;
            return;
        }

        EvaluateAim();
    }

    private void TrySubscribeToInput()
    {
        LightInput target = LightInput.Instance;
        if (target == subscribedInput) return;
        if (subscribedInput != null)
            subscribedInput.OnLeftClickActionPerformed -= HandleClick;
        subscribedInput = target;
        if (subscribedInput != null)
        {
            subscribedInput.OnLeftClickActionPerformed += HandleClick;
            warnedAboutInput = false;
        }
        else if (!warnedAboutInput)
        {
            Debug.LogWarning("PlayerLightSelection: this scene has no Light Input object, so clicks cannot select lights.", this);
            warnedAboutInput = true;
        }
    }

    private void HandleClick()
    {
        if (!beamActive) return;
        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!pointerOverUi)
            EvaluateAim();

        if (!pointerOverUi && hoveredLight != null && hoveredLight.CanBeSelected)
        {
            if (hoveredLight == SelectedLight)
                ClearSelection();
            else
                SelectLight(hoveredLight);
            return;
        }

        ClearSelection();
    }

    public void SelectLight(LightController next)
    {
        LightController previous = SelectedLight;
        SetSelectedLight(next);
        RefreshTint(previous);
        RefreshTint(SelectedLight);
        RefreshTint(hoveredLight);
    }

    public void SetSelectedLight(LightController next)
    {
        if (next != null && !next.CanBeSelected) return;

        foreach (LightController light in FindObjectsByType<LightController>(FindObjectsSortMode.None))
            light.IsSelected = (light == next);

        SelectedLight = next;
    }

    public void ClearSelection() => SelectLight(null);

    private void EvaluateAim()
    {
        if (gameCamera == null || Mouse.current == null)
        {
            SetHovered(null);
            if (aimRay != null) aimRay.enabled = false;
            return;
        }

        Vector2 origin = transform.position;
        Ray screenRay = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, transform.position);
        if (!plane.Raycast(screenRay, out float distanceAlongRay))
        {
            SetHovered(null);
            if (aimRay != null) aimRay.enabled = false;
            return;
        }

        Vector2 pointer = screenRay.GetPoint(distanceAlongRay);
        Vector2 vector = pointer - origin;
        float totalDistance = vector.magnitude;
        if (maximumDistance > 0f) totalDistance = Mathf.Min(totalDistance, maximumDistance);

        Vector2 rayEnd = origin;
        LightController firstLightHit = null;
        LightController hoveredCandidate = null;

        if (totalDistance > 0.0001f)
        {
            Vector2 dir = vector.normalized;
            float firstHitDistance = totalDistance;

            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, dir, totalDistance, blockingLayers))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (hit.collider.transform.IsChildOf(transform.root)) continue;
                if (hit.collider.GetComponentInParent<LightController>() != null) continue;
                firstHitDistance = hit.distance;
                break;
            }

            float closestLightDistance = firstHitDistance;
            float closestHoveredDistance = float.PositiveInfinity;
            foreach (LightController light in FindObjectsByType<LightController>(FindObjectsSortMode.None))
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                SpriteRenderer sprite = GetFixtureSprite(light);
                Vector2 center = sprite != null ? (Vector2)sprite.bounds.center : (Vector2)light.transform.position;
                float radius = GetFixtureRadius(sprite);

                bool underPointer = (pointer - center).sqrMagnitude <= radius * radius;
                if (underPointer)
                {
                    float toLight = (center - origin).magnitude;
                    if (toLight < closestHoveredDistance)
                    {
                        hoveredCandidate = light;
                        closestHoveredDistance = toLight;
                    }
                }

                Vector2 offset = center - origin;
                float along = Vector2.Dot(offset, dir);
                if (along < 0f || along > totalDistance + radius) continue;
                float perpendicular2 = offset.sqrMagnitude - along * along;
                if (perpendicular2 > radius * radius) continue;
                float entry = Mathf.Max(0f, along - Mathf.Sqrt(Mathf.Max(0f, radius * radius - perpendicular2)));
                if (entry < closestLightDistance)
                {
                    closestLightDistance = entry;
                    firstLightHit = light;
                }
            }

            float stopDistance = firstLightHit != null ? closestLightDistance : firstHitDistance;
            rayEnd = origin + dir * stopDistance;

            if (hoveredCandidate != firstLightHit || firstLightHit == null || !firstLightHit.CanBeSelected)
                hoveredCandidate = null;
        }

        SetHovered(hoveredCandidate);
        if (aimRay != null)
        {
            aimRay.enabled = true;
            aimRay.SetPosition(0, new Vector3(origin.x, origin.y, transform.position.z));
            aimRay.SetPosition(1, new Vector3(rayEnd.x, rayEnd.y, transform.position.z));
        }
    }

    private float GetFixtureRadius(SpriteRenderer sprite)
    {
        if (sprite == null) return minimumLightTargetRadius;
        Vector3 extent = sprite.bounds.extents;
        return Mathf.Max(minimumLightTargetRadius, Mathf.Max(extent.x, extent.y));
    }

    private static SpriteRenderer GetFixtureSprite(LightController light)
    {
        SpriteRenderer ownSprite = light.GetComponent<SpriteRenderer>();
        return ownSprite != null ? ownSprite : light.GetComponentInChildren<SpriteRenderer>();
    }

    private void SetHovered(LightController next)
    {
        if (hoveredLight == next) return;
        LightController previous = hoveredLight;
        hoveredLight = next;
        RefreshTint(previous);
        RefreshTint(hoveredLight);
    }

    private void RefreshTint(LightController light)
    {
        if (light == null) return;
        SpriteRenderer sprite = GetFixtureSprite(light);
        if (sprite == null) return;
        if (!originalTints.TryGetValue(sprite, out Color original))
        {
            original = sprite.color;
            originalTints.Add(sprite, original);
        }
        Color tint = light == SelectedLight ? selectedColor : (light == hoveredLight ? hoverColor : original);
        tint.a = original.a;
        sprite.color = tint;
    }

    private void CreateAimRay()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            Debug.LogWarning("PlayerLightSelection: could not find a shader for the LOS line; light selection will still work.", this);
            return;
        }
        GameObject visual = new GameObject("Player-to-Mouse LOS Ray");
        visual.transform.SetParent(transform, false);
        aimRay = visual.AddComponent<LineRenderer>();
        rayMaterial = new Material(shader);
        aimRay.material = rayMaterial;
        aimRay.positionCount = 2;
        aimRay.useWorldSpace = true;
        aimRay.widthMultiplier = rayWidth;
        aimRay.startColor = rayColor;
        aimRay.endColor = rayColor;
        aimRay.sortingOrder = raySortingOrder;
        aimRay.numCapVertices = 4;
    }

    private void OnDisable()
    {
        if (subscribedInput != null)
            subscribedInput.OnLeftClickActionPerformed -= HandleClick;
        subscribedInput = null;
        if (aimRay != null) aimRay.enabled = false;
        foreach (var entry in originalTints)
            if (entry.Key != null) entry.Key.color = entry.Value;
        originalTints.Clear();
        SetSelectedLight(null);
        hoveredLight = null;
    }

    private void OnDestroy()
    {
        if (rayMaterial != null) Destroy(rayMaterial);
    }
}

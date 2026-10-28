using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// GDD: player-to-mouse LOS ray and exclusive light selection.
/// Uses Diana's LightInput click event and LightController.IsSelected property.
/// No click colliders are required on her existing three light prefabs.
/// Add to the scene's existing "Light Input" GameObject.
/// </summary>
[DisallowMultipleComponent]
public class KDLightSelector2D : MonoBehaviour
{
    [Header("References (player is auto-found by Player tag)")]
    [SerializeField] private Transform playerOrigin;
    [SerializeField] private Camera gameCamera;

    [Header("Line of sight")]
    [Tooltip("World layers containing walls/platforms. Other lights are handled separately; triggers are ignored.")]
    [SerializeField] private LayerMask blockingLayers = ~0;
    [Tooltip("0 means no distance limit.")]
    [Min(0f)] [SerializeField] private float maximumDistance = 0f;
    [Min(0.05f)] [SerializeField] private float minimumLightTargetRadius = 0.4f;

    [Header("Feedback")]
    [SerializeField] private Color hoverColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.32f, 0.95f, 0.5f, 1f);
    [SerializeField] private Color rayColor = new Color(0.65f, 0.85f, 1f, 0.45f);
    [Min(0.005f)] [SerializeField] private float rayWidth = 0.025f;
    [SerializeField] private int raySortingOrder = 100;

    private LightInput subscribedInput;
    private LightController selectedLight;
    private LightController hoveredLight;
    private LineRenderer aimRay;
    private Material rayMaterial;
    private readonly Dictionary<SpriteRenderer, Color> originalTints = new Dictionary<SpriteRenderer, Color>();
    private bool warnedAboutPlayer;

    public LightController SelectedLight => selectedLight;

    private void Awake()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        FindPlayer();
        CreateAimRay();
    }

    private void OnEnable() => TrySubscribeToInput();

    private void Update()
    {
        TrySubscribeToInput();
        if (playerOrigin == null) FindPlayer();
        if (gameCamera == null) gameCamera = Camera.main;
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
            subscribedInput.OnLeftClickActionPerformed += HandleClick;
    }

    private void FindPlayer()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            playerOrigin = taggedPlayer.transform;
            warnedAboutPlayer = false;
        }
        else if (!warnedAboutPlayer)
        {
            Debug.LogWarning("KDLightSelector2D: no GameObject tagged Player. Assign Player Origin in the Inspector.", this);
            warnedAboutPlayer = true;
        }
    }

    private void HandleClick()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        EvaluateAim(); // Read pointer at click time, not a stale previous-frame hover.
        if (hoveredLight != null && hoveredLight.CanBeSelected)
            SelectLight(hoveredLight);
    }

    public void SelectLight(LightController next)
    {
        if (next != null && !next.CanBeSelected) return;
        // Enforce only one selection even if another light was selected in the prefab.
        foreach (LightController light in FindObjectsByType<LightController>(FindObjectsSortMode.None))
            light.IsSelected = (light == next);

        LightController previous = selectedLight;
        selectedLight = next;
        RefreshTint(previous);
        RefreshTint(selectedLight);
        RefreshTint(hoveredLight);
    }

    public void ClearSelection() => SelectLight(null);

    private void EvaluateAim()
    {
        if (playerOrigin == null || gameCamera == null || Mouse.current == null)
        {
            SetHovered(null);
            if (aimRay != null) aimRay.enabled = false;
            return;
        }

        Vector2 origin = playerOrigin.position;
        Ray screenRay = gameCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, playerOrigin.position);
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

            // The nearest real, non-trigger wall stops the line. Ignore the player's collider.
            foreach (RaycastHit2D hit in Physics2D.RaycastAll(origin, dir, totalDistance, blockingLayers))
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (hit.collider.transform.IsChildOf(playerOrigin.root)) continue;
                // Light fixtures with colliders must not be mistaken for walls.
                if (hit.collider.GetComponentInParent<LightController>() != null) continue;
                firstHitDistance = hit.distance;
                break;
            }

            // Existing light prefabs have sprites but NO Collider2D. Intersect fixture
            // bounds with the ray to detect and stop at light targets nonetheless.
            float closestLightDistance = firstHitDistance;
            float closestHoveredDistance = float.PositiveInfinity;
            foreach (LightController light in FindObjectsByType<LightController>(FindObjectsSortMode.None))
            {
                if (light == null || !light.isActiveAndEnabled) continue;
                SpriteRenderer sprite = GetFixtureSprite(light);
                Vector2 center = sprite != null ? (Vector2)sprite.bounds.center : (Vector2)light.transform.position;
                float radius = GetFixtureRadius(sprite);

                // A click must be physically over the light fixture (or near its center).
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

            // Hover/select only if the target is the FIRST light reached by the ray;
            // an intervening wall or another fixture prevents selection.
            if (hoveredCandidate != firstLightHit || firstLightHit == null || !firstLightHit.CanBeSelected)
                hoveredCandidate = null;
        }

        SetHovered(hoveredCandidate);
        if (aimRay != null)
        {
            aimRay.enabled = true;
            aimRay.SetPosition(0, new Vector3(origin.x, origin.y, playerOrigin.position.z));
            aimRay.SetPosition(1, new Vector3(rayEnd.x, rayEnd.y, playerOrigin.position.z));
        }
    }

    private float GetFixtureRadius(SpriteRenderer sprite)
    {
        if (sprite == null) return minimumLightTargetRadius;
        Vector3 extent = sprite.bounds.extents;
        // A forgiving target radius around the light fixture itself (not its beam).
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
        Color tint = light == selectedLight ? selectedColor : (light == hoveredLight ? hoverColor : original);
        tint.a = original.a;
        sprite.color = tint;
    }

    private void CreateAimRay()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            Debug.LogWarning("KDLightSelector2D: could not find a shader for the LOS line; light selection will still work.", this);
            return;
        }
        GameObject visual = new GameObject("KD Player-to-Mouse LOS Ray");
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
        if (selectedLight != null) selectedLight.IsSelected = false;
        selectedLight = null;
        hoveredLight = null;
    }

    private void OnDestroy()
    {
        if (rayMaterial != null) Destroy(rayMaterial);
    }
}

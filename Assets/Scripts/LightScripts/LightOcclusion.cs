using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Solid colliders stop 2D light. Platforms already had a shadow caster, but the
/// shadow mesh was empty, so the beam drew straight through them.
/// </summary>
public class LightOcclusion : MonoBehaviour
{
    static readonly FieldInfo ForceRebuild = typeof(ShadowCaster2D).GetField(
        "m_ForceShadowMeshRebuild",
        BindingFlags.Instance | BindingFlags.NonPublic);

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(ApplySoon());
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplySoon());
    }

    IEnumerator ApplySoon()
    {
        yield return null;
        Apply();
    }

    void Apply()
    {
        foreach (Collider2D collider in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (!collider.enabled || collider.isTrigger) continue;
            if (collider.GetComponent<Light2D>() != null) continue;

            Rigidbody2D body = collider.attachedRigidbody;
            if (body != null && body.bodyType == RigidbodyType2D.Dynamic) continue;

            ShadowCaster2D caster = collider.GetComponent<ShadowCaster2D>();
            if (caster == null)
                caster = collider.gameObject.AddComponent<ShadowCaster2D>();

            caster.castsShadows = true;
            caster.selfShadows = false;
            ForceRebuild?.SetValue(caster, true);
        }
    }
}

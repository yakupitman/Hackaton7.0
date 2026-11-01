using UnityEngine;
using UnityEngine.Rendering;

public class CorridorDarkness : MonoBehaviour
{
    [SerializeField] private Shader darknessShader;
    [SerializeField, Min(1), Tooltip("Layers are built once in Awake: re-enter Play mode after changing layer settings.")] private int layerCount = 8;
    [SerializeField, Min(0f)] private float depth = 10f;
    [SerializeField] private Vector2 size = new Vector2(3f, 3f);
    [SerializeField] private AnimationCurve alphaByDepth = AnimationCurve.Linear(0f, 0.15f, 1f, 0.6f);
    [SerializeField, Tooltip("RGB of the darkness; alpha is ignored, each layer's density comes from Alpha By Depth.")] private Color darknessColor = Color.black;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private Material darknessMaterial;

    private void Awake()
    {
        if (darknessShader == null) darknessShader = Shader.Find("Custom/CorridorDarkness");
        if (darknessShader == null)
        {
            Debug.LogWarning($"CorridorDarkness on '{name}': shader 'Custom/CorridorDarkness' not found. Assign it in the Inspector or add it to Always Included Shaders.", this);
            return;
        }
        darknessMaterial = new Material(darknessShader);
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        for (int i = 0; i < layerCount; i++)
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"DarknessLayer{i}";
            quad.layer = gameObject.layer;
            // Remove the collider right away so it never blocks raycasts or physics
            DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, LayerZ(i));
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(size.x, size.y, 1f);
            MeshRenderer quadRenderer = quad.GetComponent<MeshRenderer>();
            quadRenderer.sharedMaterial = darknessMaterial;
            quadRenderer.shadowCastingMode = ShadowCastingMode.Off;
            quadRenderer.receiveShadows = false;
            quadRenderer.lightProbeUsage = LightProbeUsage.Off;
            quadRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            // Each layer keeps the darkness colour but gets its own density from near to far
            Color layerColor = darknessColor;
            layerColor.a = Mathf.Clamp01(alphaByDepth.Evaluate(LayerFraction(i)));
            block.SetColor(ColorId, layerColor);
            quadRenderer.SetPropertyBlock(block);
        }
    }

    private void OnDestroy()
    {
        if (darknessMaterial != null) Destroy(darknessMaterial);
    }

    private float LayerZ(int index)
    {
        // Centre each layer in its slice so none sits exactly on the door or the far wall
        return depth * (index + 0.5f) / layerCount;
    }

    private float LayerFraction(int index)
    {
        return layerCount > 1 ? (float)index / (layerCount - 1) : 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.6f, 0.2f, 1f, 1f);
        Gizmos.DrawWireCube(new Vector3(0f, 0f, depth * 0.5f), new Vector3(size.x, size.y, depth));
        Gizmos.color = new Color(0.6f, 0.2f, 1f, 0.35f);
        for (int i = 0; i < layerCount; i++)
        {
            Gizmos.DrawWireCube(new Vector3(0f, 0f, LayerZ(i)), new Vector3(size.x, size.y, 0f));
        }
    }
}

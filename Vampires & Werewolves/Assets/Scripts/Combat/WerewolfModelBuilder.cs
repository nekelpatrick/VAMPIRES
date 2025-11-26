using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class WerewolfModelBuilder : MonoBehaviour
{
    [SerializeField] Color furColor = new Color(0.25f, 0.22f, 0.3f);
    [SerializeField] Color highlightColor = new Color(0.4f, 0.35f, 0.45f);
    [SerializeField] Color eyeColor = new Color(1f, 0.3f, 0.1f);
    [SerializeField] Color clawColor = new Color(0.9f, 0.85f, 0.75f);

    void Awake()
    {
        Rebuild();
    }

    void OnEnable()
    {
        Rebuild();
    }

    void OnValidate()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        ClearChildren();
        BuildBody();
        BuildHead();
        BuildArms();
        BuildLegs();
        BuildTail();
        ApplyLODGroup();
    }

    void BuildBody()
    {
        CreatePart("Body", PrimitiveType.Capsule, new Vector3(0f, 1.2f, 0f), new Vector3(0.7f, 0.9f, 0.5f), furColor);
        CreatePart("Chest", PrimitiveType.Sphere, new Vector3(0f, 1.4f, 0.1f), new Vector3(0.75f, 0.6f, 0.5f), highlightColor);
    }

    void BuildHead()
    {
        GameObject head = CreatePart("Head", PrimitiveType.Sphere, new Vector3(0f, 2.1f, 0.15f), new Vector3(0.45f, 0.4f, 0.4f), furColor);
        GameObject snout = CreatePart("Snout", PrimitiveType.Capsule, new Vector3(0f, 2f, 0.35f), new Vector3(0.2f, 0.2f, 0.25f), furColor);
        snout.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        CreateEye("LeftEye", new Vector3(-0.12f, 2.15f, 0.28f));
        CreateEye("RightEye", new Vector3(0.12f, 2.15f, 0.28f));

        GameObject leftEar = CreatePart("LeftEar", PrimitiveType.Cube, new Vector3(-0.15f, 2.35f, 0.05f), new Vector3(0.08f, 0.2f, 0.05f), furColor);
        leftEar.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
        GameObject rightEar = CreatePart("RightEar", PrimitiveType.Cube, new Vector3(0.15f, 2.35f, 0.05f), new Vector3(0.08f, 0.2f, 0.05f), furColor);
        rightEar.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
    }

    void BuildArms()
    {
        BuildArm("LeftArm", -0.4f);
        BuildArm("RightArm", 0.4f);
    }

    void BuildArm(string name, float xOffset)
    {
        CreatePart(name + "_Shoulder", PrimitiveType.Sphere, new Vector3(xOffset * 0.9f, 1.5f, 0f), new Vector3(0.25f, 0.2f, 0.2f), furColor);
        GameObject upper = CreatePart(name + "_Upper", PrimitiveType.Capsule, new Vector3(xOffset, 1.2f, 0.1f), new Vector3(0.15f, 0.35f, 0.15f), furColor);
        upper.transform.localRotation = Quaternion.Euler(20f, 0f, xOffset > 0f ? -20f : 20f);

        GameObject forearm = CreatePart(name + "_Forearm", PrimitiveType.Capsule, new Vector3(xOffset * 1.2f, 0.85f, 0.25f), new Vector3(0.12f, 0.3f, 0.12f), furColor);
        forearm.transform.localRotation = Quaternion.Euler(45f, 0f, xOffset > 0f ? -10f : 10f);

        GameObject hand = CreatePart(name + "_Hand", PrimitiveType.Sphere, new Vector3(xOffset * 1.3f, 0.55f, 0.35f), new Vector3(0.18f, 0.12f, 0.15f), furColor);

        for (int i = 0; i < 4; i++)
        {
            float angle = (i - 1.5f) * 15f;
            Vector3 clawPos = hand.transform.localPosition + new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.08f, -0.08f, 0.1f + Mathf.Cos(angle * Mathf.Deg2Rad) * 0.02f);
            GameObject claw = CreatePart(name + "_Claw" + i, PrimitiveType.Capsule, clawPos, new Vector3(0.025f, 0.12f, 0.025f), clawColor);
            claw.transform.localRotation = Quaternion.Euler(70f, angle, 0f);
        }
    }

    void BuildLegs()
    {
        BuildLeg("LeftLeg", -0.15f);
        BuildLeg("RightLeg", 0.15f);
    }

    void BuildLeg(string name, float xOffset)
    {
        CreatePart(name + "_Thigh", PrimitiveType.Capsule, new Vector3(xOffset, 0.7f, -0.05f), new Vector3(0.18f, 0.35f, 0.18f), furColor);
        CreatePart(name + "_Shin", PrimitiveType.Capsule, new Vector3(xOffset, 0.25f, 0.05f), new Vector3(0.12f, 0.3f, 0.12f), furColor);
        CreatePart(name + "_Foot", PrimitiveType.Sphere, new Vector3(xOffset, 0.05f, 0.1f), new Vector3(0.15f, 0.08f, 0.2f), furColor);
    }

    void BuildTail()
    {
        GameObject tail = CreatePart("Tail", PrimitiveType.Capsule, new Vector3(0f, 0.9f, -0.35f), new Vector3(0.12f, 0.4f, 0.12f), furColor);
        tail.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
    }

    void CreateEye(string name, Vector3 position)
    {
        GameObject eye = CreatePart(name, PrimitiveType.Sphere, position, new Vector3(0.1f, 0.08f, 0.05f), eyeColor);
        GameObject glow = CreatePart(name + "_Glow", PrimitiveType.Sphere, position, Vector3.one * 0.15f, new Color(eyeColor.r, eyeColor.g, eyeColor.b, 0.3f));
        glow.transform.SetParent(eye.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.5f;
    }

    GameObject CreatePart(string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        Transform t = part.transform;
        t.SetParent(transform);
        t.localPosition = position;
        t.localRotation = Quaternion.identity;
        t.localScale = scale;

        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = CreateMaterial(color, primitive == PrimitiveType.Sphere && name.EndsWith("_Glow"));

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        DestroyImmediate(collider);
                    }
                };
#endif
            }
        }

        return part;
    }

    Material CreateMaterial(Color color, bool unlit)
    {
        string shaderName = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#endif
            }
        }
    }

    void ApplyLODGroup()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        LODGroup lodGroup = GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = gameObject.AddComponent<LODGroup>();
        }

        LOD[] lods = new LOD[1];
        lods[0] = new LOD(0.3f, renderers);
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }
}


using UnityEngine;
using System.Collections.Generic;

public class AtmosphereSystem : MonoBehaviour
{
    public static AtmosphereSystem Instance { get; private set; }

    [Header("Background Layers")]
    [SerializeField] private int fogLayers = 3;
    [SerializeField] private float fogSpeed = 0.5f;

    [Header("Ambient Particles")]
    [SerializeField] private int emberCount = 20;
    [SerializeField] private int dustCount = 30;

    [Header("Colors")]
    [SerializeField] private Color skyColorTop = new Color(0.08f, 0.02f, 0.02f);
    [SerializeField] private Color skyColorBottom = new Color(0.15f, 0.05f, 0.08f);
    [SerializeField] private Color moonColor = new Color(0.9f, 0.3f, 0.2f);
    [SerializeField] private Color fogColor = new Color(0.1f, 0.05f, 0.08f, 0.3f);

    private List<Transform> fogPlanes = new List<Transform>();
    private List<AmbientParticle> embers = new List<AmbientParticle>();
    private List<AmbientParticle> dust = new List<AmbientParticle>();

    struct AmbientParticle
    {
        public Transform transform;
        public Vector3 velocity;
        public float lifetime;
        public float maxLifetime;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateBackground();
        CreateMoon();
        CreateFogLayers();
        CreateAmbientParticles();
        CreateGroundFog();
    }

    void CreateBackground()
    {
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.SetParent(transform);
        bg.transform.position = new Vector3(0, 3, 15);
        bg.transform.localScale = new Vector3(50, 20, 1);

        Renderer r = bg.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

        Texture2D gradientTex = new Texture2D(1, 64);
        for (int y = 0; y < 64; y++)
        {
            float t = y / 63f;
            gradientTex.SetPixel(0, y, Color.Lerp(skyColorBottom, skyColorTop, t));
        }
        gradientTex.Apply();
        mat.mainTexture = gradientTex;
        r.material = mat;

        Object.Destroy(bg.GetComponent<Collider>());

        CreateMountainSilhouettes();
        CreateTreeSilhouettes();
    }

    void CreateMountainSilhouettes()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject mountain = new GameObject("Mountain_" + i);
            mountain.transform.SetParent(transform);

            float x = (i - 2) * 8f + Random.Range(-2f, 2f);
            float height = Random.Range(4f, 8f);
            mountain.transform.position = new Vector3(x, height * 0.3f, 12);

            GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            peak.transform.SetParent(mountain.transform);
            peak.transform.localPosition = Vector3.zero;
            peak.transform.localScale = new Vector3(Random.Range(3f, 6f), height, 1);
            peak.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));

            Renderer r = peak.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.03f, 0.01f, 0.02f);
            r.material = mat;

            Object.Destroy(peak.GetComponent<Collider>());
        }
    }

    void CreateTreeSilhouettes()
    {
        for (int i = 0; i < 12; i++)
        {
            GameObject tree = new GameObject("Tree_" + i);
            tree.transform.SetParent(transform);

            float x = (i - 6) * 3f + Random.Range(-1f, 1f);
            tree.transform.position = new Vector3(x, 0, 8 + Random.Range(0, 3f));

            float height = Random.Range(2f, 4f);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trunk.transform.SetParent(tree.transform);
            trunk.transform.localPosition = new Vector3(0, height * 0.3f, 0);
            trunk.transform.localScale = new Vector3(0.15f, height * 0.6f, 0.1f);

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.transform.SetParent(tree.transform);
            canopy.transform.localPosition = new Vector3(0, height * 0.7f, 0);
            canopy.transform.localScale = new Vector3(Random.Range(0.8f, 1.5f), height * 0.5f, 0.1f);
            canopy.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.02f, 0.01f, 0.015f);

            trunk.GetComponent<Renderer>().material = mat;
            canopy.GetComponent<Renderer>().material = mat;

            Object.Destroy(trunk.GetComponent<Collider>());
            Object.Destroy(canopy.GetComponent<Collider>());
        }
    }

    void CreateMoon()
    {
        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = "BloodMoon";
        moon.transform.SetParent(transform);
        moon.transform.position = new Vector3(8, 8, 10);
        moon.transform.localScale = Vector3.one * 2.5f;

        Renderer r = moon.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = moonColor;
        r.material = mat;

        Object.Destroy(moon.GetComponent<Collider>());

        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = "MoonGlow";
        glow.transform.SetParent(moon.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.4f;

        Renderer gr = glow.GetComponent<Renderer>();
        Material glowMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        glowMat.color = new Color(moonColor.r, moonColor.g, moonColor.b, 0.2f);
        gr.material = glowMat;

        Object.Destroy(glow.GetComponent<Collider>());
    }

    void CreateFogLayers()
    {
        for (int i = 0; i < fogLayers; i++)
        {
            GameObject fog = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fog.name = "FogLayer_" + i;
            fog.transform.SetParent(transform);
            fog.transform.position = new Vector3(0, 0.5f + i * 0.3f, 5 - i);
            fog.transform.localScale = new Vector3(40, 3, 1);

            Renderer r = fog.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            Color c = fogColor;
            c.a = 0.15f - i * 0.03f;
            mat.color = c;
            r.material = mat;

            Object.Destroy(fog.GetComponent<Collider>());
            fogPlanes.Add(fog.transform);
        }
    }

    void CreateGroundFog()
    {
        GameObject groundFog = GameObject.CreatePrimitive(PrimitiveType.Quad);
        groundFog.name = "GroundFog";
        groundFog.transform.SetParent(transform);
        groundFog.transform.position = new Vector3(0, -0.3f, 0);
        groundFog.transform.rotation = Quaternion.Euler(90, 0, 0);
        groundFog.transform.localScale = new Vector3(30, 10, 1);

        Renderer r = groundFog.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(0.05f, 0.02f, 0.03f, 0.4f);
        r.material = mat;

        Object.Destroy(groundFog.GetComponent<Collider>());
    }

    void CreateAmbientParticles()
    {
        for (int i = 0; i < emberCount; i++)
        {
            CreateEmber();
        }

        for (int i = 0; i < dustCount; i++)
        {
            CreateDust();
        }
    }

    void CreateEmber()
    {
        GameObject ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ember.name = "Ember";
        ember.transform.SetParent(transform);
        ember.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);

        ResetEmberPosition(ember.transform);

        Renderer r = ember.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(1f, Random.Range(0.3f, 0.6f), 0.1f);
        r.material = mat;

        Object.Destroy(ember.GetComponent<Collider>());

        embers.Add(new AmbientParticle
        {
            transform = ember.transform,
            velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1.5f), 0),
            lifetime = Random.Range(3f, 8f),
            maxLifetime = 8f
        });
    }

    void ResetEmberPosition(Transform t)
    {
        t.position = new Vector3(
            Random.Range(-15f, 15f),
            Random.Range(-1f, 0f),
            Random.Range(-2f, 5f)
        );
    }

    void CreateDust()
    {
        GameObject dustObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dustObj.name = "Dust";
        dustObj.transform.SetParent(transform);
        dustObj.transform.localScale = Vector3.one * Random.Range(0.02f, 0.05f);

        dustObj.transform.position = new Vector3(
            Random.Range(-15f, 15f),
            Random.Range(0f, 5f),
            Random.Range(-2f, 5f)
        );

        Renderer r = dustObj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(0.3f, 0.25f, 0.2f, 0.3f);
        r.material = mat;

        Object.Destroy(dustObj.GetComponent<Collider>());

        dust.Add(new AmbientParticle
        {
            transform = dustObj.transform,
            velocity = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), 0),
            lifetime = Random.Range(5f, 15f),
            maxLifetime = 15f
        });
    }

    void Update()
    {
        for (int i = 0; i < fogPlanes.Count; i++)
        {
            Vector3 pos = fogPlanes[i].position;
            pos.x += fogSpeed * (0.5f + i * 0.2f) * Time.deltaTime;
            if (pos.x > 20) pos.x = -20;
            fogPlanes[i].position = pos;
        }

        UpdateParticles(embers, true);
        UpdateParticles(dust, false);
    }

    void UpdateParticles(List<AmbientParticle> particles, bool isEmber)
    {
        for (int i = 0; i < particles.Count; i++)
        {
            AmbientParticle p = particles[i];
            if (p.transform == null) continue;

            p.transform.position += p.velocity * Time.deltaTime;
            p.lifetime -= Time.deltaTime;

            if (isEmber)
            {
                p.velocity.x += Random.Range(-0.5f, 0.5f) * Time.deltaTime;
            }

            if (p.lifetime <= 0 || p.transform.position.y > 8)
            {
                if (isEmber)
                {
                    ResetEmberPosition(p.transform);
                    p.lifetime = Random.Range(3f, 8f);
                    p.velocity = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1.5f), 0);
                }
                else
                {
                    p.transform.position = new Vector3(
                        Random.Range(-15f, 15f),
                        Random.Range(0f, 5f),
                        Random.Range(-2f, 5f)
                    );
                    p.lifetime = Random.Range(5f, 15f);
                }
            }

            particles[i] = p;
        }
    }
}


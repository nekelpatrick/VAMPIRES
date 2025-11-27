using UnityEngine;
using System.Collections.Generic;

public class BloodParticleSystem : MonoBehaviour
{
    public static BloodParticleSystem Instance { get; private set; }

    [Header("Blood Splash Settings")]
    [SerializeField] private int particlesPerSplash = 12;
    [SerializeField] private float splashForce = 8f;
    [SerializeField] private float particleLifetime = 1.5f;
    [SerializeField] private float gravity = 15f;

    [Header("Colors")]
    [SerializeField] private Color bloodColorLight = new Color(0.7f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color bloodColorDark = new Color(0.4f, 0.05f, 0.05f, 1f);

    [Header("Gore Decals")]
    [SerializeField] private int maxDecals = 30;
    [System.NonSerialized] public float decalLifetime = 10f;

    [Header("VFX Graph Integration")]
    [SerializeField] private bool preferVFXGraph = true;

    private List<BloodParticle> particles = new List<BloodParticle>();
    private List<GameObject> decals = new List<GameObject>();
    private Queue<GameObject> decalPool = new Queue<GameObject>();

    private struct BloodParticle
    {
        public GameObject obj;
        public Vector3 velocity;
        public float lifetime;
        public float maxLifetime;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnBloodSplash(Vector3 position, Vector3 direction, int damage)
    {
        if (preferVFXGraph && VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnBloodSplash(position, direction, damage);
            return;
        }

        SpawnBloodSplashFallback(position, direction, damage);
    }

    public void SpawnDeathExplosion(Vector3 position)
    {
        if (preferVFXGraph && VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnDeathExplosion(position);
            return;
        }

        SpawnBloodSplashFallback(position, Vector3.up, 100);
        SpawnBloodSplashFallback(position, Vector3.left, 50);
        SpawnBloodSplashFallback(position, Vector3.right, 50);
    }

    private void SpawnBloodSplashFallback(Vector3 position, Vector3 direction, int damage)
    {
        int count = Mathf.Clamp(particlesPerSplash + damage / 20, 5, 25);

        for (int i = 0; i < count; i++)
        {
            SpawnParticle(position, direction);
        }
    }

    void SpawnParticle(Vector3 position, Vector3 direction)
    {
        GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        particle.name = "BloodParticle";
        particle.transform.position = position + Random.insideUnitSphere * 0.2f;

        float size = Random.Range(0.05f, 0.15f);
        particle.transform.localScale = Vector3.one * size;

        Renderer r = particle.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = Color.Lerp(bloodColorLight, bloodColorDark, Random.value);
        r.material = mat;

        Object.Destroy(particle.GetComponent<Collider>());

        Vector3 randomDir = (direction + Random.insideUnitSphere * 0.8f).normalized;
        Vector3 velocity = randomDir * splashForce * Random.Range(0.5f, 1.5f);
        velocity.y = Mathf.Abs(velocity.y) * 0.5f + Random.Range(2f, 5f);

        particles.Add(new BloodParticle
        {
            obj = particle,
            velocity = velocity,
            lifetime = particleLifetime * Random.Range(0.8f, 1.2f),
            maxLifetime = particleLifetime
        });
    }

    void Update()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            BloodParticle p = particles[i];

            if (p.obj == null)
            {
                particles.RemoveAt(i);
                continue;
            }

            p.velocity.y -= gravity * Time.deltaTime;
            p.obj.transform.position += p.velocity * Time.deltaTime;
            p.lifetime -= Time.deltaTime;

            float alpha = p.lifetime / p.maxLifetime;
            Renderer r = p.obj.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }

            if (p.obj.transform.position.y < -0.4f)
            {
                SpawnDecal(new Vector3(p.obj.transform.position.x, -0.45f, p.obj.transform.position.z));
                Object.Destroy(p.obj);
                particles.RemoveAt(i);
                continue;
            }

            if (p.lifetime <= 0)
            {
                Object.Destroy(p.obj);
                particles.RemoveAt(i);
            }
            else
            {
                particles[i] = p;
            }
        }
    }

    void SpawnDecal(Vector3 position)
    {
        if (Random.value > 0.3f) return;

        if (preferVFXGraph && VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnGroundDecal(position);
            return;
        }

        SpawnDecalFallback(position);
    }

    private void SpawnDecalFallback(Vector3 position)
    {
        GameObject decal;
        if (decalPool.Count > 0)
        {
            decal = decalPool.Dequeue();
            decal.SetActive(true);
        }
        else
        {
            decal = GameObject.CreatePrimitive(PrimitiveType.Quad);
            decal.name = "BloodDecal";
            Object.Destroy(decal.GetComponent<Collider>());

            Renderer r = decal.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(0.3f, 0.02f, 0.02f, 0.7f);
            r.material = mat;
        }

        decal.transform.position = position;
        decal.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        decal.transform.localScale = Vector3.one * Random.Range(0.3f, 0.8f);

        decals.Add(decal);

        while (decals.Count > maxDecals)
        {
            GameObject old = decals[0];
            decals.RemoveAt(0);
            old.SetActive(false);
            decalPool.Enqueue(old);
        }
    }

    public void SpawnSlashEffect(Vector3 position, Vector3 direction)
    {
        GameObject slash = new GameObject("SlashEffect");
        slash.transform.position = position;

        for (int i = 0; i < 3; i++)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.SetParent(slash.transform);
            line.transform.localPosition = Vector3.up * (i - 1) * 0.15f;
            line.transform.localScale = new Vector3(1.5f, 0.03f, 0.01f);
            line.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

            Renderer r = line.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.color = new Color(1f, 0.9f, 0.8f, 1f);
            r.material = mat;

            Object.Destroy(line.GetComponent<Collider>());
        }

        slash.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        Object.Destroy(slash, 0.15f);
    }
}

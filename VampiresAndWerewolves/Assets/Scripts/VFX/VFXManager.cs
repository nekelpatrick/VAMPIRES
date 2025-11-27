using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [SerializeField] private VFXConfig config;
    [SerializeField] private int poolSizePerEffect = 10;

    private Dictionary<VisualEffectAsset, Queue<VisualEffect>> pools = new();
    private List<ActiveVFX> activeEffects = new();

    private struct ActiveVFX
    {
        public VisualEffect effect;
        public float expireTime;
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

    void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (Time.time >= activeEffects[i].expireTime)
            {
                ReturnToPool(activeEffects[i].effect);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public void SpawnBloodSplash(Vector3 position, Vector3 direction, int damage)
    {
        if (config == null || config.bloodSplash == null)
        {
            BloodParticleSystem.Instance?.SpawnBloodSplash(position, direction, damage);
            return;
        }

        var vfx = GetFromPool(config.bloodSplash);
        if (vfx == null) return;

        vfx.transform.position = position;
        vfx.transform.rotation = Quaternion.LookRotation(direction);
        vfx.SetInt("ParticleCount", Mathf.Clamp(10 + damage / 10, 5, 30));
        vfx.SetVector3("Direction", direction.normalized);
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 2f });
    }

    public void SpawnDeathExplosion(Vector3 position)
    {
        if (config == null || config.deathExplosion == null)
        {
            BloodParticleSystem.Instance?.SpawnBloodSplash(position, Vector3.up, 100);
            return;
        }

        var vfx = GetFromPool(config.deathExplosion);
        if (vfx == null) return;

        vfx.transform.position = position;
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 3f });
    }

    public void SpawnGroundDecal(Vector3 position)
    {
        if (config == null || config.groundDecal == null) return;

        var vfx = GetFromPool(config.groundDecal);
        if (vfx == null) return;

        vfx.transform.position = new Vector3(position.x, -0.45f, position.z);
        vfx.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 10f });
    }

    public void SpawnBleedEffect(Transform target)
    {
        if (config == null || config.bleedEffect == null) return;

        var vfx = GetFromPool(config.bleedEffect);
        if (vfx == null) return;

        vfx.transform.SetParent(target);
        vfx.transform.localPosition = Vector3.up * 0.5f;
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 3f });
    }

    public void SpawnLifestealEffect(Vector3 from, Vector3 to)
    {
        if (config == null || config.lifestealEffect == null) return;

        var vfx = GetFromPool(config.lifestealEffect);
        if (vfx == null) return;

        vfx.transform.position = from;
        vfx.SetVector3("TargetPosition", to);
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 1.5f });
    }

    public void SpawnStunEffect(Vector3 position)
    {
        if (config == null || config.stunEffect == null) return;

        var vfx = GetFromPool(config.stunEffect);
        if (vfx == null) return;

        vfx.transform.position = position + Vector3.up;
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 2f });
    }

    public void SpawnRageAura(Transform target, float duration)
    {
        if (config == null || config.rageAura == null) return;

        var vfx = GetFromPool(config.rageAura);
        if (vfx == null) return;

        vfx.transform.SetParent(target);
        vfx.transform.localPosition = Vector3.zero;
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + duration });
    }

    public void SpawnHowlWave(Vector3 position, float radius)
    {
        if (config == null || config.howlWave == null) return;

        var vfx = GetFromPool(config.howlWave);
        if (vfx == null) return;

        vfx.transform.position = position;
        vfx.SetFloat("Radius", radius);
        vfx.Play();

        activeEffects.Add(new ActiveVFX { effect = vfx, expireTime = Time.time + 1.5f });
    }

    private VisualEffect GetFromPool(VisualEffectAsset asset)
    {
        if (asset == null) return null;

        if (!pools.ContainsKey(asset))
        {
            pools[asset] = new Queue<VisualEffect>();
        }

        if (pools[asset].Count > 0)
        {
            var vfx = pools[asset].Dequeue();
            vfx.gameObject.SetActive(true);
            vfx.transform.SetParent(null);
            return vfx;
        }

        var go = new GameObject($"VFX_{asset.name}");
        var effect = go.AddComponent<VisualEffect>();
        effect.visualEffectAsset = asset;
        return effect;
    }

    private void ReturnToPool(VisualEffect vfx)
    {
        if (vfx == null) return;

        vfx.Stop();
        vfx.transform.SetParent(transform);
        vfx.gameObject.SetActive(false);

        var asset = vfx.visualEffectAsset;
        if (asset != null && pools.ContainsKey(asset) && pools[asset].Count < poolSizePerEffect)
        {
            pools[asset].Enqueue(vfx);
        }
        else
        {
            Destroy(vfx.gameObject);
        }
    }
}


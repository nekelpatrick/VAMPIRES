using UnityEngine;

public class CoinDropper : MonoBehaviour
{
    public static CoinDropper Instance { get; private set; }

    [SerializeField] private CoinDrop coinPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private int minCoins = 3;
    [SerializeField] private int maxCoins = 6;
    [SerializeField] private float spreadRange = 1.5f;

    private ObjectPool<CoinDrop> pool;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (coinPrefab == null)
        {
            CreateDefaultPrefab();
        }

        pool = new ObjectPool<CoinDrop>(coinPrefab, transform, poolSize);
    }

    void CreateDefaultPrefab()
    {
        GameObject prefabObj = new GameObject("CoinPrefab");
        prefabObj.SetActive(false);
        prefabObj.transform.SetParent(transform);

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(prefabObj.transform);
        quad.transform.localPosition = Vector3.zero;
        quad.transform.localScale = Vector3.one * 0.25f;
        Destroy(quad.GetComponent<Collider>());

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.85f, 0.2f);
        quad.GetComponent<Renderer>().material = mat;

        CoinDrop cd = prefabObj.AddComponent<CoinDrop>();
        coinPrefab = cd;
    }

    public static void SpawnCoins(Vector3 position, int wave, bool isElite)
    {
        if (Instance == null || Instance.pool == null) return;

        int count = Random.Range(Instance.minCoins, Instance.maxCoins + 1);
        if (isElite) count += 2;

        for (int i = 0; i < count; i++)
        {
            CoinDrop coin = Instance.pool.Get();

            Vector3 dir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                0
            ).normalized;

            float spread = Random.Range(0.5f, Instance.spreadRange);
            coin.Launch(position, dir, spread);
        }
    }
}


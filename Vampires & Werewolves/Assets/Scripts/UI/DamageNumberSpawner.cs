using UnityEngine;
using TMPro;

public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }

    [SerializeField] private DamageNumber prefab;
    [SerializeField] private int poolSize = 30;

    private ObjectPool<DamageNumber> pool;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (prefab == null)
        {
            CreateDefaultPrefab();
        }

        pool = new ObjectPool<DamageNumber>(prefab, transform, poolSize);
    }

    void CreateDefaultPrefab()
    {
        GameObject prefabObj = new GameObject("DamageNumberPrefab");
        prefabObj.SetActive(false);
        prefabObj.transform.SetParent(transform);

        TextMeshPro tmp = prefabObj.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 5;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.sortingOrder = 100;

        DamageNumber dn = prefabObj.AddComponent<DamageNumber>();

        var textField = typeof(DamageNumber).GetField("textMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        textField?.SetValue(dn, tmp);

        prefab = dn;
    }

    public void Spawn(int damage, Vector3 position, bool isCritical = false)
    {
        if (pool == null) return;

        DamageNumber dn = pool.Get();
        dn.Show(damage, position, isCritical);
    }
}

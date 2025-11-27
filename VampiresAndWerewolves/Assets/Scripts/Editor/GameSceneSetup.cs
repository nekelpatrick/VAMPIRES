#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameSceneSetup : MonoBehaviour
{
    [MenuItem("Vampires/Setup Game Scene")]
    public static void SetupScene()
    {
        SetupManagers();
        SetupBattlefield();
        SetupThrall();
        SetupHUD();

        Debug.Log("[Vampires] Game scene setup complete! Press Play to test idle combat.");
    }

    static void SetupManagers()
    {
        GameObject managers = GameObject.Find("[Managers]");
        if (managers == null)
        {
            managers = new GameObject("[Managers]");
        }

        if (managers.GetComponent<GameManager>() == null)
            managers.AddComponent<GameManager>();

        if (managers.GetComponent<CombatManager>() == null)
            managers.AddComponent<CombatManager>();

        if (managers.GetComponent<CurrencyManager>() == null)
            managers.AddComponent<CurrencyManager>();

        if (managers.GetComponent<RewardHandler>() == null)
            managers.AddComponent<RewardHandler>();

        if (managers.GetComponent<DamageNumberSpawner>() == null)
            managers.AddComponent<DamageNumberSpawner>();
    }

    static void SetupBattlefield()
    {
        GameObject battlefield = GameObject.Find("[BATTLEFIELD]");
        if (battlefield == null)
        {
            battlefield = new GameObject("[BATTLEFIELD]");
        }

        HordeSpawner spawner = battlefield.GetComponent<HordeSpawner>();
        if (spawner == null)
        {
            spawner = battlefield.AddComponent<HordeSpawner>();
        }

        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(battlefield.transform);
            spawnPoint.transform.position = new Vector3(0, 0, 0);
        }

        SerializedObject spawnerObj = new SerializedObject(spawner);
        spawnerObj.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        spawnerObj.ApplyModifiedProperties();

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(battlefield.transform);
        floor.transform.position = new Vector3(0, -0.6f, 0);
        floor.transform.localScale = new Vector3(20, 0.2f, 3);
        floor.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
        DestroyImmediate(floor.GetComponent<Collider>());
    }

    static void SetupThrall()
    {
        GameObject existingThrall = GameObject.FindWithTag("Player");
        if (existingThrall != null) return;

        GameObject thrall = new GameObject("Werewolf");
        thrall.tag = "Player";
        thrall.transform.position = new Vector3(-3, 0, 0);

        ThrallController controller = thrall.AddComponent<ThrallController>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(thrall.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.GetComponent<Renderer>().material.color = new Color(0.2f, 0.4f, 0.9f);
        DestroyImmediate(body.GetComponent<Collider>());
    }

    static void SetupHUD()
    {
        GameObject hudRoot = GameObject.Find("[HUD]");
        if (hudRoot == null)
        {
            hudRoot = new GameObject("[HUD]");
        }

        Canvas existingCanvas = hudRoot.GetComponentInChildren<Canvas>();
        if (existingCanvas != null) return;

        GameObject canvasObj = new GameObject("HUD Canvas");
        canvasObj.transform.SetParent(hudRoot.transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        HUD hud = canvasObj.AddComponent<HUD>();

        GameObject topPanel = new GameObject("TopPanel");
        topPanel.transform.SetParent(canvasObj.transform);
        RectTransform topRect = topPanel.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.anchoredPosition = new Vector2(0, -20);
        topRect.sizeDelta = new Vector2(0, 100);

        Image bgImage = topPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);

        HorizontalLayoutGroup layout = topPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50;
        layout.padding = new RectOffset(30, 30, 10, 10);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;

        TextMeshProUGUI waveText = CreateHUDText(topPanel.transform, "WaveText", "WAVE 1");
        waveText.fontSize = 36;
        waveText.fontStyle = FontStyles.Bold;
        waveText.rectTransform.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI duskenText = CreateHUDText(topPanel.transform, "DuskenText", "[DUSKEN COIN]: 0");
        duskenText.color = new Color(1f, 0.85f, 0.4f);
        duskenText.rectTransform.sizeDelta = new Vector2(300, 50);

        TextMeshProUGUI shardsText = CreateHUDText(topPanel.transform, "ShardsText", "[BLOOD SHARDS]: 0");
        shardsText.color = new Color(0.9f, 0.2f, 0.3f);
        shardsText.rectTransform.sizeDelta = new Vector2(300, 50);

        TextMeshProUGUI healthText = CreateHUDText(topPanel.transform, "HealthText", "[THRALL] HP: 500/500");
        healthText.color = new Color(0.4f, 0.9f, 0.4f);
        healthText.rectTransform.sizeDelta = new Vector2(350, 50);

        SerializedObject hudObj = new SerializedObject(hud);
        hudObj.FindProperty("waveText").objectReferenceValue = waveText;
        hudObj.FindProperty("duskenCoinText").objectReferenceValue = duskenText;
        hudObj.FindProperty("bloodShardsText").objectReferenceValue = shardsText;
        hudObj.FindProperty("thrallHealthText").objectReferenceValue = healthText;
        hudObj.ApplyModifiedProperties();
    }

    static TextMeshProUGUI CreateHUDText(Transform parent, string name, string text)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        return tmp;
    }

    [MenuItem("Vampires/Create Enemy Prefab")]
    public static void CreateEnemyPrefab()
    {
        string path = "Assets/Prefabs/Enemies/Enemy.prefab";

        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";

        enemy.AddComponent<EnemyController>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(enemy.transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        body.GetComponent<Renderer>().material.color = new Color(0.8f, 0.2f, 0.2f);
        DestroyImmediate(body.GetComponent<Collider>());

        PrefabUtility.SaveAsPrefabAsset(enemy, path);
        DestroyImmediate(enemy);

        Debug.Log("[Vampires] Enemy prefab created at " + path);
    }
}
#endif


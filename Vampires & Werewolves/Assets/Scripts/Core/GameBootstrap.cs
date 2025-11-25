using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene")
            return;

        Debug.Log("[Vampires] Bootstrapping idle combat...");

        SetupCamera();
        SetupManagers();
        SetupBattlefield();
        SetupThrall();
        SetupHUD();

        Debug.Log("[Vampires] Idle combat initialized! Watch the [THRALL] fight the [HORDE]!");
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.transform.position = new Vector3(0, 1, -10);
            cam.backgroundColor = new Color(0.06f, 0.06f, 0.1f);
        }
    }

    static void SetupManagers()
    {
        GameObject managers = GameObject.Find("[Managers]");
        if (managers == null)
        {
            managers = new GameObject("[Managers]");
        }

        CleanMissingScripts(managers);

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

        if (managers.GetComponent<CoinDropper>() == null)
            managers.AddComponent<CoinDropper>();
    }

    static void CleanMissingScripts(GameObject go)
    {
        var components = go.GetComponents<Component>();
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] == null)
            {
                Debug.Log($"[Vampires] Removing missing script from {go.name}");
            }
        }
    }

    static void SetupBattlefield()
    {
        GameObject battlefield = GameObject.Find("[BATTLEFIELD]");
        if (battlefield == null)
        {
            battlefield = new GameObject("[BATTLEFIELD]");
        }

        CleanMissingScripts(battlefield);

        if (battlefield.GetComponent<HordeSpawner>() == null)
        {
            battlefield.AddComponent<HordeSpawner>();
        }

        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint == null)
        {
            spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(battlefield.transform);
            spawnPoint.transform.position = Vector3.zero;
        }

        CreateFloor(battlefield.transform);
    }

    static void CreateFloor(Transform parent)
    {
        GameObject existingFloor = GameObject.Find("Floor");
        if (existingFloor != null) return;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent);
        floor.transform.position = new Vector3(0, -0.6f, 0);
        floor.transform.localScale = new Vector3(30, 0.2f, 3);

        Renderer r = floor.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.12f, 0.12f, 0.15f);
        r.material = mat;

        Object.Destroy(floor.GetComponent<Collider>());
    }

    static void SetupThrall()
    {
        GameObject existingThrall = GameObject.FindWithTag("Player");
        if (existingThrall != null) return;

        GameObject thrall = new GameObject("Werewolf");
        thrall.tag = "Player";
        thrall.transform.position = new Vector3(-5, 0, 0);
        thrall.transform.localScale = Vector3.one * 0.4f;

        ThrallController controller = thrall.AddComponent<ThrallController>();
        thrall.AddComponent<AttackAnimator>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(thrall.transform);
        body.transform.localPosition = new Vector3(0, 1.25f, 0);
        body.transform.localScale = Vector3.one;

        Renderer r = body.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.4f, 0.9f);
        r.material = mat;

        Object.Destroy(body.GetComponent<Collider>());

        CreateHealthBar(thrall.transform);
    }

    static void CreateHealthBar(Transform parent)
    {
        GameObject hpBarRoot = new GameObject("HPBar");
        hpBarRoot.transform.SetParent(parent);
        hpBarRoot.transform.localPosition = new Vector3(0, 3.5f, 0);
        hpBarRoot.transform.localScale = Vector3.one * 2.5f;

        GameObject bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "Background";
        bgQuad.transform.SetParent(hpBarRoot.transform);
        bgQuad.transform.localPosition = Vector3.zero;
        bgQuad.transform.localScale = new Vector3(1.2f, 0.15f, 1);
        bgQuad.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        Object.Destroy(bgQuad.GetComponent<Collider>());

        GameObject fillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillQuad.name = "Fill";
        fillQuad.transform.SetParent(hpBarRoot.transform);
        fillQuad.transform.localPosition = new Vector3(0, 0, -0.01f);
        fillQuad.transform.localScale = new Vector3(1.1f, 0.1f, 1);
        fillQuad.GetComponent<Renderer>().material.color = new Color(0.8f, 0.2f, 0.2f);
        Object.Destroy(fillQuad.GetComponent<Collider>());

        hpBarRoot.AddComponent<WorldSpaceHealthBar>();
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

        GameObject topPanel = new GameObject("TopPanel");
        topPanel.transform.SetParent(canvasObj.transform);
        RectTransform topRect = topPanel.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.anchoredPosition = new Vector2(0, 0);
        topRect.sizeDelta = new Vector2(0, 80);

        Image bgImage = topPanel.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

        HorizontalLayoutGroup layout = topPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 40;
        layout.padding = new RectOffset(30, 30, 15, 15);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;

        TextMeshProUGUI waveText = CreateText(topPanel.transform, "WaveText", "WAVE 1", 32, Color.white, FontStyles.Bold, 180);
        TextMeshProUGUI duskenText = CreateText(topPanel.transform, "DuskenText", "[DUSKEN COIN]: 0", 22, new Color(1f, 0.85f, 0.4f), FontStyles.Normal, 280);
        TextMeshProUGUI shardsText = CreateText(topPanel.transform, "ShardsText", "[BLOOD SHARDS]: 0", 22, new Color(0.9f, 0.2f, 0.3f), FontStyles.Normal, 280);
        TextMeshProUGUI healthText = CreateText(topPanel.transform, "HealthText", "[THRALL] HP: 500/500", 22, new Color(0.4f, 0.9f, 0.4f), FontStyles.Normal, 320);

        HUD hud = canvasObj.AddComponent<HUD>();

        var hudType = typeof(HUD);
        var waveField = hudType.GetField("waveText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var duskenField = hudType.GetField("duskenCoinText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var shardsField = hudType.GetField("bloodShardsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var healthField = hudType.GetField("thrallHealthText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        waveField?.SetValue(hud, waveText);
        duskenField?.SetValue(hud, duskenText);
        shardsField?.SetValue(hud, shardsText);
        healthField?.SetValue(hud, healthText);
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color, FontStyles style, float width)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 50);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = color;

        return tmp;
    }
}

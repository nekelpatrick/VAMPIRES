using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnGameStart()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene")
            return;

        Debug.Log("[Vampires] Bootstrapping The Nine Circles...");

        CleanupOldObjects();
        SetupEventSystem();
        SetupCamera();
        SetupManagers();
        SetupMobileUI();
        SetupAtmosphere();
        SetupBattlefield();
        SetupThrall();
        SetupVFXSystems();
        SetupAdSystems();
        SetupQuestSystems();
        SetupHUD();
        SetupDebugOverlay();

        Debug.Log("[Vampires] Welcome to the [BATTLEFIELD]. The [HORDE] awaits!");
    }

    static void SetupEventSystem()
    {
        EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing != null)
        {
            Debug.Log($"[Vampires] Found existing EventSystem on '{existing.gameObject.name}', destroying to replace...");
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject eventSystemObj = new GameObject("[EventSystem]");
        EventSystem eventSystem = eventSystemObj.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventSystemObj.AddComponent<InputSystemUIInputModule>();

        var inputActions = UnityEngine.InputSystem.InputActionAsset.FromJson(GetDefaultUIActionsJson());
        inputActions.Enable();

        var uiMap = inputActions.FindActionMap("UI");
        if (uiMap != null)
        {
            uiMap.Enable();

            var pointAction = uiMap.FindAction("Point");
            var clickAction = uiMap.FindAction("Click");
            var scrollAction = uiMap.FindAction("ScrollWheel");

            if (pointAction != null)
            {
                pointAction.Enable();
                inputModule.point = UnityEngine.InputSystem.InputActionReference.Create(pointAction);
            }
            if (clickAction != null)
            {
                clickAction.Enable();
                inputModule.leftClick = UnityEngine.InputSystem.InputActionReference.Create(clickAction);
            }
            if (scrollAction != null)
            {
                scrollAction.Enable();
                inputModule.scrollWheel = UnityEngine.InputSystem.InputActionReference.Create(scrollAction);
            }

            Debug.Log($"[Vampires] UI actions enabled - Point: {pointAction?.enabled}, Click: {clickAction?.enabled}");
        }

        Debug.Log($"[Vampires] EventSystem created: {eventSystem.name}");
        Debug.Log($"[Vampires] InputModule: {inputModule.GetType().Name}, enabled: {inputModule.enabled}");
    }

    static string GetDefaultUIActionsJson()
    {
        return @"{
            ""name"": ""DefaultUIActions"",
            ""maps"": [{
                ""name"": ""UI"",
                ""id"": ""272f6d14-89ba-496f-b7ff-215263d3219f"",
                ""actions"": [
                    { ""name"": ""Point"", ""type"": ""PassThrough"", ""id"": ""73b301e9-2b4a-4c42-8b76-ff0d13f3c29a"", ""expectedControlType"": ""Vector2"" },
                    { ""name"": ""Click"", ""type"": ""PassThrough"", ""id"": ""4faf7dc9-4fb8-4559-b5f1-e8b6c6962e12"", ""expectedControlType"": ""Button"" },
                    { ""name"": ""ScrollWheel"", ""type"": ""PassThrough"", ""id"": ""0489e84a-4833-4c40-bfae-cea84b696689"", ""expectedControlType"": ""Vector2"" }
                ],
                ""bindings"": [
                    { ""path"": ""<Pointer>/position"", ""action"": ""Point"" },
                    { ""path"": ""<Mouse>/leftButton"", ""action"": ""Click"" },
                    { ""path"": ""<Pen>/tip"", ""action"": ""Click"" },
                    { ""path"": ""<Touchscreen>/touch*/press"", ""action"": ""Click"" },
                    { ""path"": ""<Touchscreen>/primaryTouch/position"", ""action"": ""Point"" },
                    { ""path"": ""<Mouse>/scroll"", ""action"": ""ScrollWheel"" }
                ]
            }]
        }";
    }

    static void CleanupOldObjects()
    {
        string[] objectsToClean = {
            "SceneBootstrap",
            "[EventSystem]",
            "[Managers]",
            "[MobileUI]",
            "[BATTLEFIELD]",
            "[Atmosphere]",
            "[HUD]",
            "[VFX]",
            "[ADS]",
            "[QUESTS]",
            "[UIDebug]",
            "Floor",
            "SpawnPoint"
        };

        foreach (string name in objectsToClean)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.Destroy(obj);
            }
        }

        GameObject existingThrall = GameObject.FindWithTag("Player");
        if (existingThrall != null)
        {
            Object.Destroy(existingThrall);
        }
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0, 1.5f, -10);
        cam.backgroundColor = new Color(0.02f, 0.01f, 0.02f);

        if (cam.GetComponent<CameraEffects>() == null)
        {
            cam.gameObject.AddComponent<CameraEffects>();
        }
    }

    static void SetupManagers()
    {
        GameObject managers = new GameObject("[Managers]");
        managers.AddComponent<GameManager>();
        managers.AddComponent<CombatManager>();
        managers.AddComponent<CurrencyManager>();
        managers.AddComponent<RewardHandler>();
        managers.AddComponent<DamageNumberSpawner>();
        managers.AddComponent<CoinDropper>();
    }

    static void SetupMobileUI()
    {
        GameObject mobileUI = new GameObject("[MobileUI]");
        mobileUI.AddComponent<MobileUIScaler>();
    }

    static void SetupAtmosphere()
    {
        GameObject atmosphere = new GameObject("[Atmosphere]");
        atmosphere.AddComponent<AtmosphereSystem>();
    }

    static void SetupBattlefield()
    {
        GameObject battlefield = new GameObject("[BATTLEFIELD]");
        battlefield.AddComponent<HordeSpawner>();

        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.SetParent(battlefield.transform);
        spawnPoint.transform.position = new Vector3(8f, 0, 0);

        CreateGoreBattlefloor(battlefield.transform);
    }

    static void CreateGoreBattlefloor(Transform parent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent);
        floor.transform.position = new Vector3(0, -0.55f, 0);
        floor.transform.localScale = new Vector3(40, 0.3f, 5);

        Renderer r = floor.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.08f, 0.05f, 0.06f);
        r.material = mat;

        Object.Destroy(floor.GetComponent<Collider>());

        CreateBloodStains(parent);

        for (int i = 0; i < 8; i++)
        {
            GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bone.name = "ScatteredBone_" + i;
            bone.transform.SetParent(parent);
            bone.transform.position = new Vector3(
                Random.Range(-12f, 12f),
                -0.35f,
                Random.Range(-1f, 1f)
            );
            bone.transform.localScale = new Vector3(0.05f, Random.Range(0.1f, 0.3f), 0.05f);
            bone.transform.rotation = Quaternion.Euler(
                Random.Range(60, 90),
                Random.Range(0, 360),
                0
            );

            Renderer br = bone.GetComponent<Renderer>();
            Material boneMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boneMat.color = new Color(0.7f, 0.65f, 0.55f);
            br.material = boneMat;

            Object.Destroy(bone.GetComponent<Collider>());
        }
    }

    static void CreateBloodStains(Transform parent)
    {
        for (int i = 0; i < 12; i++)
        {
            GameObject stain = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stain.name = "BloodStain_" + i;
            stain.transform.SetParent(parent);
            stain.transform.position = new Vector3(
                Random.Range(-15f, 15f),
                -0.38f,
                Random.Range(-1.5f, 1.5f)
            );
            stain.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);
            stain.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);

            Renderer sr = stain.GetComponent<Renderer>();
            Material stainMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            float darkness = Random.Range(0.1f, 0.25f);
            stainMat.color = new Color(darkness, 0.02f, 0.02f, Random.Range(0.3f, 0.6f));
            sr.material = stainMat;

            Object.Destroy(stain.GetComponent<Collider>());
        }
    }

    static void SetupThrall()
    {
        GameObject thrall = new GameObject("[THRALL] Werewolf");
        thrall.tag = "Player";
        thrall.transform.position = new Vector3(-6, 0, 0);
        thrall.transform.localScale = Vector3.one * 0.5f;

        thrall.AddComponent<ThrallController>();
        thrall.AddComponent<AttackAnimator>();

        UnitVisualFactory.CreateWerewolfVisual(thrall.transform);

        CreateWorldSpaceHealthBar(thrall.transform, new Color(0.2f, 0.8f, 0.3f));
    }

    static void CreateWorldSpaceHealthBar(Transform parent, Color barColor)
    {
        GameObject hpBarRoot = new GameObject("HPBar");
        hpBarRoot.transform.SetParent(parent);
        hpBarRoot.transform.localPosition = new Vector3(0, 5f, 0);
        hpBarRoot.transform.localScale = Vector3.one * 2f;

        GameObject bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "Background";
        bgQuad.transform.SetParent(hpBarRoot.transform);
        bgQuad.transform.localPosition = Vector3.zero;
        bgQuad.transform.localScale = new Vector3(1.4f, 0.18f, 1);

        Renderer bgRenderer = bgQuad.GetComponent<Renderer>();
        Material bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        bgMat.color = new Color(0.05f, 0.03f, 0.03f, 0.9f);
        bgRenderer.material = bgMat;
        Object.Destroy(bgQuad.GetComponent<Collider>());

        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
        border.name = "Border";
        border.transform.SetParent(hpBarRoot.transform);
        border.transform.localPosition = new Vector3(0, 0, -0.005f);
        border.transform.localScale = new Vector3(1.5f, 0.22f, 1);

        Renderer borderRenderer = border.GetComponent<Renderer>();
        Material borderMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        borderMat.color = new Color(0.5f, 0.35f, 0.2f, 0.8f);
        borderRenderer.material = borderMat;
        Object.Destroy(border.GetComponent<Collider>());

        GameObject fillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillQuad.name = "Fill";
        fillQuad.transform.SetParent(hpBarRoot.transform);
        fillQuad.transform.localPosition = new Vector3(0, 0, -0.01f);
        fillQuad.transform.localScale = new Vector3(1.3f, 0.12f, 1);

        Renderer fillRenderer = fillQuad.GetComponent<Renderer>();
        Material fillMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        fillMat.color = barColor;
        fillRenderer.material = fillMat;
        Object.Destroy(fillQuad.GetComponent<Collider>());

        hpBarRoot.AddComponent<WorldSpaceHealthBar>();
    }

    static void SetupHUD()
    {
        GameObject hudRoot = new GameObject("[HUD]");
        hudRoot.AddComponent<GothicHUD>();
    }

    static void SetupVFXSystems()
    {
        GameObject vfxSystems = new GameObject("[VFX]");
        vfxSystems.AddComponent<BloodParticleSystem>();
        vfxSystems.AddComponent<ScreenEffects>();
    }

    static void SetupAdSystems()
    {
        GameObject adSystems = new GameObject("[ADS]");
        adSystems.AddComponent<AdService>();
        adSystems.AddComponent<AdRewardManager>();
        adSystems.AddComponent<AdPromptPanel>();
    }

    static void SetupQuestSystems()
    {
        GameObject questSystems = new GameObject("[QUESTS]");
        questSystems.AddComponent<DailyQuestManager>();
        questSystems.AddComponent<QuestPanel>();
        questSystems.AddComponent<QuestNotification>();
    }

    static void SetupDebugOverlay()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameObject debugObj = new GameObject("[UIDebug]");
        debugObj.AddComponent<UIDebugOverlay>();
        Debug.Log("[Vampires] UI Debug overlay enabled (Press F1 to toggle)");
#endif
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.Collections.Generic;

public class UIDebugOverlay : MonoBehaviour
{
    public static UIDebugOverlay Instance { get; private set; }

    private TextMeshProUGUI debugText;
    private Canvas debugCanvas;
    private bool isVisible = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CreateDebugCanvas();
    }

    void CreateDebugCanvas()
    {
        GameObject canvasObj = new GameObject("DebugCanvas");
        canvasObj.transform.SetParent(transform);

        debugCanvas = canvasObj.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(canvasObj.transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(600, 400);

        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 18;
        debugText.color = Color.yellow;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        debugText.raycastTarget = false;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(textObj.transform);
        bgObj.transform.SetAsFirstSibling();

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-10, -10);
        bgRect.offsetMax = new Vector2(10, 10);

        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        bg.raycastTarget = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            isVisible = !isVisible;
            debugCanvas.gameObject.SetActive(isVisible);
        }

        if (!isVisible) return;

        UpdateDebugInfo();
    }

    void UpdateDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=#00FF00>[UI DEBUG] Press F1 to hide</color>");
        sb.AppendLine("");

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            sb.AppendLine($"<color=#FFFFFF>EventSystem:</color> {eventSystem.gameObject.name}");
            sb.AppendLine($"  Enabled: {eventSystem.enabled}");
            sb.AppendLine($"  IsPointerOverGO: {eventSystem.IsPointerOverGameObject()}");

            BaseInputModule inputModule = eventSystem.currentInputModule;
            if (inputModule != null)
            {
                sb.AppendLine($"  InputModule: {inputModule.GetType().Name}");
                sb.AppendLine($"  Module enabled: {inputModule.enabled}");

                if (inputModule is InputSystemUIInputModule isim)
                {
                    sb.AppendLine($"  Point action: {(isim.point?.action != null ? isim.point.action.name : "NULL")}");
                    sb.AppendLine($"  Click action: {(isim.leftClick?.action != null ? isim.leftClick.action.name : "NULL")}");
                }
            }
            else
            {
                sb.AppendLine("  <color=#FF0000>InputModule: NULL</color>");
            }
        }
        else
        {
            sb.AppendLine("<color=#FF0000>EventSystem: NOT FOUND</color>");
        }

        sb.AppendLine("");

        Vector2 pointerPos = Vector2.zero;
        bool hasPointer = false;

        if (Pointer.current != null)
        {
            pointerPos = Pointer.current.position.ReadValue();
            hasPointer = true;
            sb.AppendLine($"<color=#FFFFFF>Pointer Position:</color> {pointerPos}");
        }

        if (Mouse.current != null)
        {
            sb.AppendLine($"<color=#FFFFFF>Mouse:</color>");
            sb.AppendLine($"  Position: {Mouse.current.position.ReadValue()}");
            sb.AppendLine($"  Left: {(Mouse.current.leftButton.isPressed ? "PRESSED" : "released")}");
        }

        if (Touchscreen.current != null)
        {
            sb.AppendLine($"<color=#FFFFFF>Touchscreen:</color>");
            sb.AppendLine($"  Touches: {Touchscreen.current.touches.Count}");
            if (Touchscreen.current.primaryTouch.isInProgress)
            {
                sb.AppendLine($"  Primary: {Touchscreen.current.primaryTouch.position.ReadValue()}");
                hasPointer = true;
                pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
        }

        sb.AppendLine("");

        if (hasPointer && eventSystem != null)
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = pointerPos;

            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            sb.AppendLine($"<color=#FFFFFF>Raycast Hits ({results.Count}):</color>");
            for (int i = 0; i < Mathf.Min(results.Count, 5); i++)
            {
                var result = results[i];
                string objName = result.gameObject != null ? result.gameObject.name : "null";
                sb.AppendLine($"  [{i}] {objName} (depth: {result.depth})");
            }

            if (results.Count == 0)
            {
                sb.AppendLine("  <color=#FF8800>No UI elements hit</color>");
            }
        }

        sb.AppendLine("");

        GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        sb.AppendLine($"<color=#FFFFFF>GraphicRaycasters ({raycasters.Length}):</color>");
        foreach (var raycaster in raycasters)
        {
            Canvas canvas = raycaster.GetComponent<Canvas>();
            string order = canvas != null ? canvas.sortingOrder.ToString() : "?";
            sb.AppendLine($"  {raycaster.gameObject.name} (order: {order})");
        }

        debugText.text = sb.ToString();
    }
}


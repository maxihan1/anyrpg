// 터치 조작이 없는 AnyRPG 에 폰에서만 화면 조이스틱 · 단추를 띄운다 — 가상 게임패드로 넣어 게임 코드를 고치지 않는다
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public static class MobilePad {
    // 게임 화면보다 위에 그려야 HUD 가 조이스틱을 가리지 않는다.
    private const int SortingOrder = 1000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create() {
        if (!Application.isMobilePlatform) return;

        var root = new GameObject("MobilePad", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        // 이름은 control_cli 테스트가 찾는 주소다 — 바꾸면 케이스도 같이 바꾼다.
        Stick(root.transform, "PadMove", "<Gamepad>/leftStick", new Vector2(0f, 0f), new Vector2(260f, 260f));
        Stick(root.transform, "PadLook", "<Gamepad>/rightStick", new Vector2(1f, 0f), new Vector2(-260f, 260f));
        Button(root.transform, "PadJump", "Jump", "<Gamepad>/buttonWest", new Vector2(1f, 0f), new Vector2(-560f, 140f));
        Button(root.transform, "PadAccept", "OK", "<Gamepad>/buttonSouth", new Vector2(1f, 0f), new Vector2(-560f, 300f));
    }

    private static void Stick(Transform parent, string name, string path, Vector2 anchor, Vector2 position) {
        var ring = Box(parent, name + "Ring", anchor, position, 260f, new Color(1f, 1f, 1f, 0.12f));
        ring.GetComponent<Image>().raycastTarget = false;
        var knob = Box(parent, name, anchor, position, 150f, new Color(1f, 1f, 1f, 0.35f));
        knob.SetActive(false);
        // 켜기 전에 경로를 정해야 처음 켜질 때 엉뚱한 장치를 만들지 않는다.
        var stick = knob.AddComponent<OnScreenStick>();
        stick.controlPath = path;
        stick.movementRange = 110f;
        knob.SetActive(true);
    }

    private static void Button(Transform parent, string name, string label, string path, Vector2 anchor, Vector2 position) {
        var go = Box(parent, name, anchor, position, 130f, new Color(1f, 1f, 1f, 0.35f));
        go.SetActive(false);
        go.AddComponent<OnScreenButton>().controlPath = path;
        go.SetActive(true);

        var text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        text.transform.SetParent(go.transform, false);
        var rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 34;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private static GameObject Box(Transform parent, string name, Vector2 anchor, Vector2 position, float size, Color color) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(size, size);
        go.GetComponent<Image>().color = color;
        return go;
    }
}

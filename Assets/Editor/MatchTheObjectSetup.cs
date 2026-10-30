using System;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Editor-only helper. The resulting objects and button events are saved in BasicScene.
public static class MatchTheObjectSetup
{
    private const string ScenePath = "Assets/Scenes/BasicScene.unity";
    private static Font font;

    public static void Preview()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var preview = new GameObject("Preview Camera").AddComponent<Camera>();
        preview.transform.position = new Vector3(0, 1.6f, -0.6f);
        preview.transform.LookAt(new Vector3(0, 1.48f, 2));
        preview.fieldOfView = 48;
        preview.clearFlags = CameraClearFlags.SolidColor;
        preview.backgroundColor = new Color(0.18f, 0.2f, 0.23f);
        var texture = new RenderTexture(1400, 1000, 24);
        preview.targetTexture = texture;
        Canvas.ForceUpdateCanvases();
        preview.Render();
        preview.Render();
        RenderTexture.active = texture;
        var image = new Texture2D(1400, 1000, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1400, 1000), 0, 0);
        image.Apply();
        System.IO.File.WriteAllBytes("Logs/MatchTheObjectPreview.png", image.EncodeToPNG());
        RenderTexture.active = null;
        preview.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(image);
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(preview.gameObject);
        Debug.Log("MATCH_OBJECT_PREVIEW_OK");
    }

    [MenuItem("Match the Object/Build BasicScene Interface")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var previous = GameObject.Find("Match the Object");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var root = new GameObject("Match the Object");
        var game = root.AddComponent<MatchTheObject>();
        System.IO.Directory.CreateDirectory("Assets/Materials/MatchTheObject");
        game.colors = new[] { Material("Red", new Color(0.9f, 0.06f, 0.06f)),
            Material("Green", new Color(0.05f, 0.75f, 0.15f)), Material("Blue", new Color(0.05f, 0.25f, 1)) };
        game.targetObject = ObjectGroup(root.transform, "Target Object", new Vector3(-0.48f, 1.85f, 2));
        game.playerObject = ObjectGroup(root.transform, "Your Object", new Vector3(0.48f, 1.85f, 2));
        game.ShowObject(game.targetObject, game.targetShape, game.targetColor, game.targetSize, game.targetRotation);
        game.ShowObject(game.playerObject, MatchTheObject.Shape.Cube, MatchTheObject.ObjectColor.Red, MatchTheObject.Size.Medium, Vector3.zero);

        var canvasObject = new GameObject("Controls Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TrackedDeviceGraphicRaycaster));
        canvasObject.transform.SetParent(root.transform);
        canvasObject.transform.position = new Vector3(0, 1.1f, 1.8f);
        canvasObject.transform.localScale = Vector3.one * 0.0015f;
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 620);
        var panel = Rect(canvas.transform, "Panel", 0, 0, 1000, 620);
        panel.gameObject.AddComponent<Image>().color = new Color(0.045f, 0.065f, 0.1f, 1);
        Label(panel, "Title", "MATCH THE OBJECT", 0, 265, 900, 44, 32);
        Label(panel, "Instructions", "Match the object on the right to the target on the left.", 0, 222, 950, 36, 23);
        string[][] choices = { new[] { "Cube", "Sphere", "Cylinder" }, new[] { "Red", "Green", "Blue" } };
        UnityAction<int>[] actions = { game.SetShape, game.SetColor };
        string[] headings = { "Shape", "Color" };
        for (int row = 0; row < 2; row++)
        {
            float y = 163 - row * 62;
            Label(panel, headings[row], headings[row], -380, y, 160, 48, 25);
            for (int col = 0; col < 3; col++)
            {
                var button = Button(panel, choices[row][col], -165 + col * 230, y, 215);
                UnityEventTools.AddIntPersistentListener(button.onClick, actions[row], col);
            }
        }
        game.sizeSlider = SliderRow(panel, "Size", 40, 0.18f, 0.34f, 0.26f, game.SetSize);
        game.selectionText = Label(panel, "Selection", "Your object: Cube / Red / Size 0.26", 0, -7, 950, 32, 22);
        game.xSlider = SliderRow(panel, "X", -55, 0, 360, 0, game.RotateX);
        game.ySlider = SliderRow(panel, "Y", -100, 0, 360, 0, game.RotateY);
        game.zSlider = SliderRow(panel, "Z", -145, 0, 360, 0, game.RotateZ);
        game.rotationText = Label(panel, "Rotation", "Rotation: X 0 / Y 0 / Z 0", 0, -186, 950, 30, 22);
        UnityEventTools.AddPersistentListener(Button(panel, "Reset", -230, -230, 300).onClick, game.ResetObject);
        UnityEventTools.AddPersistentListener(Button(panel, "Confirm", 230, -230, 300).onClick, game.Confirm);
        game.feedbackText = Label(panel, "Feedback", "When you are ready, press Confirm.", 0, -282, 950, 45, 22);
        Label(canvas.transform, "Target Label", "TARGET", -320, 690, 380, 45, 28);
        Label(canvas.transform, "Player Label", "YOUR OBJECT", 320, 690, 380, 45, 28);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Verify(game);
        Debug.Log("MATCH_OBJECT_SETUP_OK: BasicScene saved; 8 buttons, 4 sliders, 2 grab interactables; checks passed.");
    }

    private static Material Material(string name, Color color)
    {
        string path = "Assets/Materials/MatchTheObject/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Transform ObjectGroup(Transform parent, string name, Vector3 position)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent);
        root.position = position;
        foreach (var type in new[] { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder })
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.transform.SetParent(root, false);
            // Unity's default cylinder is twice as tall as the other primitives.
            if (type == PrimitiveType.Cylinder) shape.transform.localScale = new Vector3(1, 0.5f, 1);
        }
        var body = root.gameObject.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        var grab = root.gameObject.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        // Register all three shapes, even those hidden when the scene starts.
        grab.colliders.Clear();
        grab.colliders.AddRange(root.GetComponentsInChildren<Collider>(true));
        return root;
    }

    private static Slider SliderRow(Transform parent, string name, float y, float min, float max, float value, UnityAction<float> action)
    {
        Label(parent, name + " Label", name, -380, y, 160, 40, 25);
        var rect = Rect(parent, name + " Slider", 65, y, 650, 40);
        var background = rect.gameObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.28f, 0.42f);
        var slider = rect.gameObject.AddComponent<Slider>();
        var handleArea = Rect(rect, "Handle Area", 0, 0, 610, 40);
        var handle = Rect(handleArea, "Handle", 0, 0, 40, 40);
        var handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = new Color(0.65f, 0.85f, 1);
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(value);
        UnityEventTools.AddPersistentListener(slider.onValueChanged, action);
        return slider;
    }

    private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
        return rect;
    }

    private static Text Label(Transform parent, string name, string words, float x, float y, float width, float height, int size)
    {
        var text = Rect(parent, name, x, y, width, height).gameObject.AddComponent<Text>();
        text.font = font;
        text.text = words;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button Button(Transform parent, string words, float x, float y, float width)
    {
        var rect = Rect(parent, words, x, y, width, 52);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.28f, 0.42f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.65f, 0.85f, 1);
        colors.pressedColor = new Color(0.4f, 0.65f, 0.9f);
        button.colors = colors;
        Label(rect, "Text", words, 0, 0, width, 52, 25);
        return button;
    }

    private static void Verify(MatchTheObject game)
    {
        void Check(bool pass, string message) { if (!pass) throw new Exception(message); }
        Check(game.GetComponentsInChildren<Button>().Length == 8, "Expected 8 buttons");
        Check(game.GetComponentsInChildren<Slider>().Length == 4, "Expected 4 sliders");
        foreach (var slider in game.GetComponentsInChildren<Slider>())
            Check(slider.onValueChanged.GetPersistentEventCount() == 1, "Unwired slider");
        foreach (var grab in game.GetComponentsInChildren<XRGrabInteractable>())
            Check(grab.colliders.Count == 3 && !grab.throwOnDetach && !grab.GetComponent<Rigidbody>().useGravity, "Grab setup");
        foreach (var button in game.GetComponentsInChildren<Button>())
            Check(button.onClick.GetPersistentEventCount() == 1, "Unwired button: " + button.name);
        Check(MatchTheObject.RotationMatches(MatchTheObject.Shape.Cube, Quaternion.Euler(0, 90, 0), Quaternion.identity, 15), "Cube symmetry");
        Check(!MatchTheObject.RotationMatches(MatchTheObject.Shape.Cube, Quaternion.Euler(0, 45, 0), Quaternion.identity, 15), "Cube mismatch");
        Check(MatchTheObject.RotationMatches(MatchTheObject.Shape.Cylinder, Quaternion.Euler(180, 60, 0), Quaternion.identity, 15), "Cylinder symmetry");
        Check(!MatchTheObject.RotationMatches(MatchTheObject.Shape.Cylinder, Quaternion.Euler(30, 0, 0), Quaternion.identity, 15), "Cylinder mismatch");
        Check(MatchTheObject.RotationMatches(MatchTheObject.Shape.Cube, Quaternion.Euler(0, 10, 0), Quaternion.identity, 15), "Tolerance");
        game.SendMessage("Start");
        game.Confirm();
        Check(game.feedbackText.text.StartsWith("Not quite"), "Initial mismatch");
        game.SetShape((int)game.targetShape); game.SetColor((int)game.targetColor); game.SetSize(0.34f);
        game.RotateX(30); game.RotateZ(45); game.Confirm();
        Check(game.feedbackText.text.StartsWith("Correct"), "Correct match");
        game.RotateX(45); game.RotateY(300); game.RotateZ(345); game.Confirm();
        Check(game.feedbackText.text.StartsWith("Correct"), "Screenshot near match");
        game.SetSize(0.28f); game.Confirm();
        Check(game.feedbackText.text.StartsWith("Correct"), "Forgiving size");
        game.SetSize(0.18f); game.Confirm();
        Check(game.feedbackText.text.StartsWith("Not quite"), "Clearly wrong size");
        var start = game.playerObject.position;
        game.playerObject.position += Vector3.right;
        game.playerObject.rotation = Quaternion.Euler(12, 23, 34);
        game.SetColor(1);
        Check(Vector3.Distance(game.playerObject.position, start + Vector3.right) < 0.001f, "Color preserves grabbed position");
        Check(Quaternion.Angle(game.playerObject.rotation, Quaternion.Euler(12, 23, 34)) < 0.01f, "Color preserves grabbed rotation");
        game.ResetObject();
        Check(game.playerObject.localRotation == Quaternion.identity, "Reset rotation");
        Check(Vector3.Distance(game.playerObject.position, start) < 0.001f && game.sizeSlider.value == 0.26f, "Reset position and slider");
    }
}

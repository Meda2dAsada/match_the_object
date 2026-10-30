using UnityEngine;
using UnityEngine.UI;

public class MatchTheObject : MonoBehaviour
{
    public enum Shape { Cube, Sphere, Cylinder }
    public enum ObjectColor { Red, Green, Blue }
    public enum Size { Small, Medium, Large }

    [Header("Objects (children ordered Cube, Sphere, Cylinder)")]
    public Transform playerObject;
    public Transform targetObject;
    public Material[] colors;

    [Header("Target - change these to make another challenge")]
    public Shape targetShape = Shape.Cylinder;
    public ObjectColor targetColor = ObjectColor.Blue;
    public Size targetSize = Size.Large;
    public Vector3 targetRotation = new Vector3(30, 0, 45);
    [Range(1, 40)] public float rotationTolerance = 35;
    [Range(0, 0.4f)] public float sizeTolerance = 0.25f;

    [Header("Canvas text")]
    public Text selectionText;
    public Text rotationText;
    public Text feedbackText;
    public Slider sizeSlider;
    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    private Shape shape;
    private ObjectColor objectColor;
    private float size = 0.26f;
    private Vector3 rotation;
    private Vector3 playerStartPosition;
    private Vector3 targetStartPosition;
    private Quaternion targetStartRotation;

    private void Start()
    {
        ShowObject(targetObject, targetShape, targetColor, targetSize, targetRotation);
        playerStartPosition = playerObject.position;
        targetStartPosition = targetObject.position;
        targetStartRotation = targetObject.rotation;
        ResetObject();
    }

    public void SetShape(int value) { shape = (Shape)value; UpdatePlayer(); }
    public void SetColor(int value) { objectColor = (ObjectColor)value; UpdatePlayer(); }
    public void SetSize(float value) { size = value; UpdatePlayer(); }
    public void RotateX(float degrees) { rotation.x = degrees; ApplyRotation(); }
    public void RotateY(float degrees) { rotation.y = degrees; ApplyRotation(); }
    public void RotateZ(float degrees) { rotation.z = degrees; ApplyRotation(); }

    private void ApplyRotation()
    {
        playerObject.localRotation = Quaternion.Euler(rotation);
        UpdatePlayer();
    }

    private void LateUpdate()
    {
        // Grabbing can rotate the object too. Keep sliders in sync without firing their events.
        if (Quaternion.Angle(Quaternion.Euler(rotation), playerObject.localRotation) > 0.1f)
        {
            rotation = playerObject.localEulerAngles;
            UpdatePlayer();
        }
    }

    public void ResetObject()
    {
        shape = Shape.Cube;
        objectColor = ObjectColor.Red;
        size = 0.26f;
        rotation = Vector3.zero;
        playerObject.SetPositionAndRotation(playerStartPosition, Quaternion.identity);
        targetObject.SetPositionAndRotation(targetStartPosition, targetStartRotation);
        UpdatePlayer();
        feedbackText.text = "Object reset. Look at the target and try to match it.";
    }

    private void UpdatePlayer()
    {
        // Color/shape changes must not move or rotate an object that was grabbed.
        ShowObject(playerObject, shape, objectColor, Size.Medium, playerObject.localEulerAngles);
        playerObject.localScale = Vector3.one * size;
        if (Quaternion.Angle(Quaternion.Euler(rotation), playerObject.localRotation) > 0.1f)
            rotation = playerObject.localEulerAngles;
        sizeSlider.SetValueWithoutNotify(size);
        xSlider.SetValueWithoutNotify(rotation.x);
        ySlider.SetValueWithoutNotify(rotation.y);
        zSlider.SetValueWithoutNotify(rotation.z);
        selectionText.text = $"Your object: {shape} / {objectColor} / Size {size:0.00}";
        rotationText.text = $"Rotation: X {rotation.x:0} / Y {rotation.y:0} / Z {rotation.z:0}";
        feedbackText.color = Color.white;
        feedbackText.text = "When you are ready, press Confirm.";
    }

    public void ShowObject(Transform root, Shape chosenShape, ObjectColor chosenColor, Size chosenSize, Vector3 angles)
    {
        // Each shape has its own mesh; only the selected shape is visible.
        for (int i = 0; i < root.childCount; i++)
        {
            root.GetChild(i).gameObject.SetActive(i == (int)chosenShape);
            root.GetChild(i).GetComponent<Renderer>().sharedMaterial = colors[(int)chosenColor];
        }
        float scale = chosenSize == Size.Small ? 0.18f : chosenSize == Size.Medium ? 0.26f : 0.34f;
        root.localScale = Vector3.one * scale;
        root.localRotation = Quaternion.Euler(angles);
    }

    public void Confirm()
    {
        bool correct = shape == targetShape && objectColor == targetColor
            && Mathf.Abs(size - targetObject.localScale.x) <= targetObject.localScale.x * sizeTolerance
            && RotationMatches(shape, playerObject.rotation, targetObject.rotation, rotationTolerance);
        feedbackText.color = correct ? new Color(0.45f, 1, 0.6f) : new Color(1, 0.8f, 0.4f);
        feedbackText.text = correct ? "Correct! Your object matches the target." : "Not quite a match yet. Look again and try another change.";
    }

    public static bool RotationMatches(Shape shape, Quaternion player, Quaternion target, float tolerance)
    {
        // A plain sphere looks the same from every direction.
        if (shape == Shape.Sphere) return true;
        float minimumDot = Mathf.Cos(tolerance * Mathf.Deg2Rad);
        // A cylinder can spin around its length or be upside down and still match.
        if (shape == Shape.Cylinder)
            return Mathf.Abs(Vector3.Dot(player * Vector3.up, target * Vector3.up)) >= minimumDot;

        // For a cube, match each axis with any target axis (including opposite directions).
        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward };
        foreach (Vector3 axis in axes)
        {
            float best = 0;
            foreach (Vector3 targetAxis in axes)
                best = Mathf.Max(best, Mathf.Abs(Vector3.Dot(player * axis, target * targetAxis)));
            if (best < minimumDot) return false;
        }
        return true;
    }
}

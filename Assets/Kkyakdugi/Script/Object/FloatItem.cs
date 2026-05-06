using UnityEngine;

public class FloatItem : MonoBehaviour {
    public float amplitude = 0.25f;
    public float frequency = 2f;

    Vector3 startPos;

    void Awake() {
        startPos = transform.position;
    }

    void Update() {
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);
    }
}
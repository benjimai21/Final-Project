using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    float min;

    private void Start()
    {
        min = Mathf.Infinity;

    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        Rect rect2 = new Rect(0, h * 2 / 100, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1f, 1f, 1f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        min = Mathf.Min(min, fps);
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        string text2 = string.Format("Min: {0:0.}", min);
        GUI.Label(rect, text, style);
        GUI.Label(rect2, text2, style);

        if (GUI.Button(new Rect(0, h * 2 / 100 * 2, 100, h * 3 / 100), "Reset Min"))
        {
            min = Mathf.Infinity;
        }
    }
}
    
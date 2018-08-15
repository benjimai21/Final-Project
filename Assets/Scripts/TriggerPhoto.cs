using UnityEngine;
using System.Collections;

public class TriggerPhoto : MonoBehaviour
{
    private SteamVR_TrackedController trackedObj;

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedController>();
    }

    // Update is called once per frame
    void Update() {
        // 1
        if (trackedObj.triggerPressed) {
            ScreenCapture.CaptureScreenshot("Screenshot.png");
        }
    }
}

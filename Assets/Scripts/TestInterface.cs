using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInterface : MonoBehaviour {

    // Variables to hold query values
    private float p0;
    private float p1;
    private float p2;
    private float p3;
    private float p4;
    private float p5;

    public CortexDrawer m_CortexDrawer;

    // Use this for initialization
    void Start () {
		
	}

    void OnGUI()
    {
        string p0str = GUI.TextField(new Rect(50, 145, 40, 30), p0.ToString(), 10);
        float p0Temp;
        if(float.TryParse(p0str, out p0Temp))
        {
            p0 = p0Temp;
        }

        string p1str = GUI.TextField(new Rect(50, 175, 40, 30), p1.ToString(), 10);
        float p1Temp;
        if (float.TryParse(p1str, out p1Temp))
        {
            p1 = p1Temp;
        }

        string p2str = GUI.TextField(new Rect(50, 205, 40, 30), p2.ToString(), 10);
        float p2Temp;
        if (float.TryParse(p2str, out p2Temp))
        {
            p2 = p2Temp;
        }

        string p3str = GUI.TextField(new Rect(50, 235, 40, 30), p3.ToString(), 10);
        float p3Temp;
        if (float.TryParse(p3str, out p3Temp))
        {
            p3 = p3Temp;
        }

        string p4str = GUI.TextField(new Rect(50, 265, 40, 30), p4.ToString(), 10);
        float p4Temp;
        if (float.TryParse(p4str, out p4Temp))
        {
            p4 = p4Temp;
        }

        string p5str = GUI.TextField(new Rect(50, 295, 40, 30), p5.ToString(), 10);
        float p5Temp;
        if (float.TryParse(p5str, out p5Temp))
        {
            p5 = p5Temp;
        }

        if (GUI.Button(new Rect(50, 335, 100, 30), "Perform Query"))
        {
            m_CortexDrawer.DrawNewQuery(p0, p1, p2, p3, p4, p5);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

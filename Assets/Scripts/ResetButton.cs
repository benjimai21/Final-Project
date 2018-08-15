using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour {

    public NewtonVR.NVRButton m_button;
    public CortexDrawer m_drawer;
	
	// Update is called once per frame
    private void Update()
    {
        if (m_button.ButtonDown)
        {
            m_drawer.Reset();
        }
    }

}

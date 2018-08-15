﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxMessage : MonoBehaviour {

    public FullLineModelRenderer m_LineModelRenderer;
    public CortexDrawer m_cortexDrawer;
 
    //public GameObject m_LeftController;
   // private SteamVR_TrackedController m_LeftTrackedContr;

    //public GameObject m_RightController;
    //private SteamVR_TrackedController m_RightTrackedContr;

    public Transform m_lowerSphere;
    public Transform m_upperSphere;

    public ControlModeManager m_controlManager;

    private bool m_boxVisible;

    // Use this for initialization
    void Start () {
        /*
        if (m_LeftController)
            m_LeftTrackedContr = m_LeftController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing left controller reference in MessageBox!");

        if (m_RightController)
            m_RightTrackedContr = m_RightController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing right controller reference in MessageBox!");

        if (!m_LeftTrackedContr || !m_RightTrackedContr)
            print("ERROR: Couldn't retrieve vrTracked controller components. Make sure they're attached to both controllers");

        //Set up controller action listeners
        if (m_LeftTrackedContr)
        {
            m_LeftTrackedContr.TriggerClicked += new ClickedEventHandler(TriggerPulled);

        }

        if (m_RightTrackedContr)
        {
            m_RightTrackedContr.TriggerClicked += new ClickedEventHandler(TriggerPulled);
        }
        */
        m_boxVisible = true;
    }

    public void TriggerPulled()
    {
        if (m_controlManager.GetCurrentControlMode() == ControlModeManager.CONTROL_MODE.MESSAGE_BOX)
            StartMessage();
    }

    private void StartMessage()
    {
        if (m_lowerSphere && m_upperSphere)
        {
            Vector3 offset = m_cortexDrawer.GetMessageBoxOffset();
            //Transform query points to world, then to model frame
            Transform sphereHoldertrans = m_lowerSphere.transform.parent;
            Vector3 lower = m_cortexDrawer.transform.InverseTransformPoint(sphereHoldertrans.TransformPoint(m_lowerSphere.localPosition)) + offset;
            Vector3 upper = m_cortexDrawer.transform.InverseTransformPoint(sphereHoldertrans.TransformPoint(m_upperSphere.localPosition)) + offset;

            m_LineModelRenderer.BeginMessage(lower, upper);
        }
        else
        {
            print("ERROR: bounding spheres not assigned on BoxMessage");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_controlManager.GetCurrentControlMode() == ControlModeManager.CONTROL_MODE.MESSAGE_BOX && !m_boxVisible)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(true);
            m_boxVisible = true;
        }
        else if (m_controlManager.GetCurrentControlMode() != ControlModeManager.CONTROL_MODE.MESSAGE_BOX && m_boxVisible)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);
            m_boxVisible = false;
        }

    }
}
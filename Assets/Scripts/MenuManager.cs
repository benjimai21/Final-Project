using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public ControlModeManager m_ControlModeManager;
    public FullLineModelRenderer m_FullLineModelRenderer;

    // All menu buttons 
    public MenuButton LeftQuery;
    public MenuButton LeftMessage;
    public MenuButton LeftConnectivity;
    public MenuButton LeftModel;
    public MenuButton LeftBox;

    public MenuButton RightQuery;
    public MenuButton RightMessage;
    public MenuButton RightConnectivity;
    public MenuButton RightModel;
    public MenuButton RightBox;

    public SliderButton LeftMessageLength;
    public SliderButton RightMessageLength;
    public SliderButton LeftMessageSpeed;
    public SliderButton RightMessageSpeed;
    public SliderButton LeftConnectionRange;
    public SliderButton RightConnectionRange;

    public GameObject LeftOptionsMenu;
    public GameObject RightOptionsMenu;

    // Constant values used in the exponential calculation of the raneg slider value
    private static int m_rangeSliderMin = 100;
    private static int m_rangeSliderMid = 1000;
    private static int m_rangeSliderMax = 10000;

    private float a = (m_rangeSliderMin * m_rangeSliderMax - m_rangeSliderMid * m_rangeSliderMid) 
        / (m_rangeSliderMin - 2 * m_rangeSliderMid + m_rangeSliderMax);
    private float b = (m_rangeSliderMid - m_rangeSliderMin) * (m_rangeSliderMid - m_rangeSliderMin)
        / (m_rangeSliderMin - 2 * m_rangeSliderMid + m_rangeSliderMax);
    private float c = 2 * Mathf.Log((m_rangeSliderMax - m_rangeSliderMid)
        / (m_rangeSliderMid - m_rangeSliderMin));

    public void SliderFunction(SliderButton.SLIDERFUNCTION func, float val)
    {
        switch (func)
        {
            case SliderButton.SLIDERFUNCTION.MESSAGELENGTH:
                // val is between -5 and 5. (where -5 is max) We want a value between 2 and 12
                int lenVal = (int) -val + 7;
                m_FullLineModelRenderer.m_steps = lenVal;
                return;
            case SliderButton.SLIDERFUNCTION.MESSAGESPEED:
                // Here we want a delay value between 1 and 11 (1 is fastest, 11 is slowest)
                int speedVal = (int)val + 6;
                m_FullLineModelRenderer.m_iterationDelay = speedVal;
                return;
            case SliderButton.SLIDERFUNCTION.CONNECTIONRANGE:
                // Here we want a range value between 100 and 10000, with 1000 as halfway along the slider. 
                float sliderVal = (-val + 5) / 10;
                int rangeVal = (int) (a + b * Mathf.Exp(c * sliderVal));
                m_FullLineModelRenderer.m_connectionRange = rangeVal;
                return;
        }
    }

    public void ChangeMode(MenuButton.BUTTONFUNCTION func)
    {
        ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();
        switch (func)
        {
            case MenuButton.BUTTONFUNCTION.QUERY:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL 
                    || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL
                    || curMode == ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_BOX);
                return;

            case MenuButton.BUTTONFUNCTION.MESSAGE:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL 
                    || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL
                    || curMode == ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_BOX);
                return;

            case MenuButton.BUTTONFUNCTION.CONNECTIVITY:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL
                    || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL
                    || curMode == ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.CONNECTIVITY_BOX);
                return;

            case MenuButton.BUTTONFUNCTION.MODEL:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_BOX || curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_MODEL);
                else if (curMode == ControlModeManager.CONTROL_MODE.QUERY_BOX || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_MODEL);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL);
                return;

            case MenuButton.BUTTONFUNCTION.BOX:
                if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_BOX || curMode == ControlModeManager.CONTROL_MODE.MESSAGE_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.MESSAGE_BOX);
                else if (curMode == ControlModeManager.CONTROL_MODE.QUERY_BOX || curMode == ControlModeManager.CONTROL_MODE.QUERY_MODEL)
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.QUERY_BOX);
                else
                    m_ControlModeManager.SetControlMode(ControlModeManager.CONTROL_MODE.CONNECTIVITY_BOX);
                return;
        }
    }

    private void Update()
    {
        ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();
        switch(curMode)
        {
            case ControlModeManager.CONTROL_MODE.QUERY_MODEL:
                LeftQuery.Activate();
                RightQuery.Activate();
                LeftModel.Activate();
                RightModel.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.QUERY_BOX:
                LeftQuery.Activate();
                RightQuery.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.MESSAGE_MODEL:
                LeftModel.Activate();
                RightModel.Activate();
                LeftMessage.Activate();
                RightMessage.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.MESSAGE_BOX:
                LeftMessage.Activate();
                RightMessage.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.CONNECTIVITY_MODEL:
                LeftConnectivity.Activate();
                RightConnectivity.Activate();
                LeftModel.Activate();
                RightModel.Activate();
                break;
            case ControlModeManager.CONTROL_MODE.CONNECTIVITY_BOX:
                LeftConnectivity.Activate();
                RightConnectivity.Activate();
                LeftBox.Activate();
                RightBox.Activate();
                break;
        }

        if (LeftOptionsMenu.activeInHierarchy || RightOptionsMenu.activeInHierarchy)
        {

            int messageLen = m_FullLineModelRenderer.m_steps - 7;
            LeftMessageLength.SetPosition((float)-messageLen);
            RightMessageLength.SetPosition((float)-messageLen);

            int messageSpeed = m_FullLineModelRenderer.m_iterationDelay - 6;
            LeftMessageSpeed.SetPosition((float)messageSpeed);
            RightMessageSpeed.SetPosition((float)messageSpeed);

            float rangeVal = Mathf.Log((m_FullLineModelRenderer.m_connectionRange - a) / b) / c;
            float sliderVal = -(rangeVal * 10) + 5;
            LeftConnectionRange.SetPosition(sliderVal);
        }
    }
}

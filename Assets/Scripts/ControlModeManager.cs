using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlModeManager : MonoBehaviour {

    public enum CONTROL_MODE { QUERY_MODEL, QUERY_BOX,  MESSAGE_MODEL, MESSAGE_BOX, CONNECTIVITY_MODEL, CONNECTIVITY_BOX };
    private CONTROL_MODE m_curControlMode;

    //public GameObject m_LeftController;
   // private SteamVR_TrackedController m_LeftTrackedContr;

    //public GameObject m_RightController;
   // private SteamVR_TrackedController m_RightTrackedContr;

    public GameObject m_meshParts;
    public GameObject m_LineModel;
    public CortexDrawer m_CortexDrawer;

    public Display m_ScreenDisplay;

    // Use this for initialization
    void Start () {
        // Default control mode
        m_curControlMode = CONTROL_MODE.QUERY_MODEL;
        /*
        if (m_LeftController)
            m_LeftTrackedContr = m_LeftController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing left controller reference in ControlModeManager!");

        if (m_RightController)
            m_RightTrackedContr = m_RightController.GetComponent<SteamVR_TrackedController>();
        else
            print("ERROR: Missing right controller reference in ControlModeManager!");

        if (!m_LeftTrackedContr || !m_RightTrackedContr)
            print("ERROR: Couldn't retrieve vrTracked controller components. Make sure they're attached to both controllers");

        //Set up controller action listeners
        if (m_LeftTrackedContr)
        {
            m_LeftTrackedContr.MenuButtonClicked += new ClickedEventHandler(MenuPressed);
        }

        if (m_RightTrackedContr)
        {
            m_RightTrackedContr.MenuButtonClicked += new ClickedEventHandler(MenuPressed);
        }
        */
    }
    /*
    private void MenuPressed(object sender, ClickedEventArgs e)
    {
        ToggleMode();
    }

    private void ToggleMode()
    {
        m_curControlMode = (CONTROL_MODE)(((int)m_curControlMode + 1) % 6);

        SetCorrectModel();
    }
    */
    private void SetCorrectModel()
    {
        if (m_curControlMode == CONTROL_MODE.QUERY_MODEL)
        {
            m_ScreenDisplay.SetQueryMode();
            m_ScreenDisplay.SetModelControl();
            if (m_CortexDrawer.IsQueryShown())
            {
                //m_meshParts.SetActive(true);
                //m_LineModel.SetActive(false);
                m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeOut();
                m_CortexDrawer.StartQueryFadeIn();
                m_CortexDrawer.RestoreQueryScale();
            }
            else
            {
                //m_meshParts.SetActive(false);
                //m_LineModel.SetActive(true);
                m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
                m_CortexDrawer.StartQueryFadeOut();
            }
        }
        else if (m_curControlMode == CONTROL_MODE.QUERY_BOX)
        {
            m_ScreenDisplay.SetQueryMode();
            m_ScreenDisplay.SetBoxControl();
            if (m_CortexDrawer.IsQueryShown())
            {
                //m_meshParts.SetActive(true);
                //m_LineModel.SetActive(false);
                m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeOut();
                m_CortexDrawer.StartQueryFadeIn();
            }
            else
            {
                //m_meshParts.SetActive(false);
                //m_LineModel.SetActive(true);
                m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
                m_CortexDrawer.StartQueryFadeOut();
            }
        }
        else if (m_curControlMode == CONTROL_MODE.MESSAGE_MODEL)
        {
            m_ScreenDisplay.SetMessageMode();
            m_ScreenDisplay.SetModelControl();
            //m_meshParts.SetActive(false);
            //m_LineModel.SetActive(true);
            m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
            m_CortexDrawer.StartQueryFadeOut();

            if (m_CortexDrawer.IsQueryShown())
            {
                m_CortexDrawer.SetModelToLineScale();
            }
        }
        else if (m_curControlMode == CONTROL_MODE.MESSAGE_BOX)
        {
            m_ScreenDisplay.SetMessageMode();
            m_ScreenDisplay.SetBoxControl();
            //m_meshParts.SetActive(false);
            //m_LineModel.SetActive(true);
            m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
            m_CortexDrawer.StartQueryFadeOut();

            if (m_CortexDrawer.IsQueryShown())
            {
                m_CortexDrawer.SetModelToLineScale();
            }
        }
        else if (m_curControlMode == CONTROL_MODE.CONNECTIVITY_MODEL)
        {
            m_ScreenDisplay.SetConnectivityMode();
            m_ScreenDisplay.SetModelControl();
            m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
            m_CortexDrawer.StartQueryFadeOut();

            if (m_CortexDrawer.IsQueryShown())
            {
                m_CortexDrawer.SetModelToLineScale();
            }
        }
        else if (m_curControlMode == CONTROL_MODE.CONNECTIVITY_BOX)
        {
            m_ScreenDisplay.SetConnectivityMode();
            m_ScreenDisplay.SetBoxControl();
            m_LineModel.GetComponent<FullLineModelRenderer>().StartFadeIn();
            m_CortexDrawer.StartQueryFadeOut();

            if (m_CortexDrawer.IsQueryShown())
            {
                m_CortexDrawer.SetModelToLineScale();
            }
        }
    }

    public CONTROL_MODE GetCurrentControlMode()
    {
        return m_curControlMode;
    }

    public void SetControlMode(CONTROL_MODE mode)
    {
        m_curControlMode = mode;
        SetCorrectModel();
    }
       
}

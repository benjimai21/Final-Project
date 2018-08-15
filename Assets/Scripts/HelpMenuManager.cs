using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenuManager : MonoBehaviour {

    public GameObject m_ControlScreen;
    public GameObject m_ModeScreen;

    public void ShowControlScreen()
    {
        m_ModeScreen.SetActive(false);
        m_ControlScreen.SetActive(true);
    }

    public void ShowModeScreen()
    {
        m_ModeScreen.SetActive(true);
        m_ControlScreen.SetActive(false);
    }
}

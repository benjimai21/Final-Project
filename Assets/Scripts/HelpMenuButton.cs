using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenuButton : MonoBehaviour {

    public enum HELPBUTTONFUNCTION { TO_MODE_PAGE, TO_CONTROLS_PAGE }

    public HelpMenuManager m_HelpMenuManager;
    public HELPBUTTONFUNCTION m_func;

    private void Function()
    {
        if (m_func == HELPBUTTONFUNCTION.TO_CONTROLS_PAGE)
            m_HelpMenuManager.ShowControlScreen();
        else if (m_func == HELPBUTTONFUNCTION.TO_MODE_PAGE)
            m_HelpMenuManager.ShowModeScreen();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finger")
            Function();
    }
}

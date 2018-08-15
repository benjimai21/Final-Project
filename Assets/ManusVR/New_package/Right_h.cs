using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ManusVR.PhysicalInteraction
{
    public class Right_h : MonoBehaviour
    {
        //reference to all scripts of objects to control
        public ModelManipulator[] objects;

        //reference to all MATERIAL objects to control 
        public GameObject queryCube;
        public GameObject messageCube;
        public GameObject connectivityCube;
        public GameObject cortexModel;

        private bool rotationMode = false;

        private bool touchCube;

        Vector3 objectPosition;
        Quaternion objectRotation;

        public ControlModeManager m_ControlModeManager;

        private void Start() {
        }

        void OnTriggerEnter(Collider collider) {

        
            if (collider.gameObject.tag == "Fist_r") {
                
                for (int j = 0; j < objects.Length; j++) {
                    if (objects[j].GetDistance())
                        objects[j].ChangeZoneFistActivation(true);

                 //   objects[j].ChangeIsTouched(); //SUBJECT TO CHANGES
                }
                
                //activate fist mode 
                for (int i = 0; i < objects.Length; i++) {
                    objects[i].RightGrip();
                    objects[i].ChangeStatutFistClosed();
                }

                //if scalemode is activated, save position of the object at this time frame
              if (objects[0].GetScaleMode())
                  AssignPositionWhenHandOpens();              
            }

            if (collider.gameObject.tag == "cube")
                touchCube = true;

            //if elbow is touched activate rotation mode
            if (collider.gameObject.tag == "elbow_l")
                rotationMode = true;

        }

        void OnTriggerExit(Collider collider) {
     
            if (collider.gameObject.tag == "Fist_r") {

                ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();

                //deactivate fist mode           
                for (int i = 0; i < objects.Length; i++) {
                    objects[i].RightUngrip();
                    objects[i].ChangeStatutFistOpen();
                }

                //get position and rotation of the object when hand opens to fix the object position and rotation
                AssignPositionWhenHandOpens();          
            }


            if (collider.gameObject.tag == "cube")
                touchCube = false;

            if (collider.gameObject.tag == "elbow_l")
                rotationMode = false;
        }

        public void AssignPositionWhenHandOpens() {

            ControlModeManager.CONTROL_MODE curMode = m_ControlModeManager.GetCurrentControlMode();

            //position of the object manipulated when fist opens 
            if (curMode == ControlModeManager.CONTROL_MODE.QUERY_BOX) 
                objectPosition = queryCube.transform.position;                        
            else if (curMode == ControlModeManager.CONTROL_MODE.MESSAGE_BOX) 
                objectPosition = messageCube.transform.position;                            
            else if (curMode == ControlModeManager.CONTROL_MODE.CONNECTIVITY_BOX)
                objectPosition = connectivityCube.transform.position;
            else {
                print("get position of cortex");
                objectPosition = cortexModel.transform.position;
                objectRotation = cortexModel.transform.rotation; //get rotation of cortex
            }

        }

        //return position of object 
        public Vector3 GetObjectPosition() {

           return objectPosition;
        }

        //return rotation of the cortex
        public Quaternion GetObjectRotation() {

            return objectRotation;
        }

        public bool GetRotationMode() {

            return rotationMode;
        }

        public bool GetTouchCube() {

            return touchCube;
        }

    }
}


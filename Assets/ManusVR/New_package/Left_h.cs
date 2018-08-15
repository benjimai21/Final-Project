using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace ManusVR.PhysicalInteraction
{
    public class Left_h : MonoBehaviour
    {

        //reference to all scripts of objects to control
        public ModelManipulator[] objects;

        //reference to all MATERIAL objects to control #
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

           

            if (collider.gameObject.tag == "Fist_l") {
                
                for (int j = 0; j < objects.Length; j++) {
                    if (objects[j].GetDistance())
                        objects[j].ChangeZoneFistActivation(true);

                 //   objects[j].ChangeIsTouched(); //SUBJECT TO CHANGES
                }
                
                //activate fist mode
                for (int i = 0; i < objects.Length; i++) {
                    objects[i].LeftGrip();
                    objects[i].ChangeStatutFistClosed();
                }

              if (objects[1].GetScaleMode())
                  AssignPositionWhenHandOpens();
            }


            if (collider.gameObject.tag == "cube")
                touchCube = true;

            //if elbow is touched activate rotation mode
            if (collider.gameObject.tag == "elbow_r") {
                rotationMode = true;
                print("IN");
            }

        }

        void OnTriggerExit(Collider collider) {

            
            if (collider.gameObject.tag == "Fist_l") {

             
                //deactivate fist mode               
                for (int i = 0; i < objects.Length; i++) {
                    objects[i].LeftUngrip();
                    objects[i].ChangeStatutFistOpen();                 
                }

                //get position and rotation of the object when hand opens to fix the object position and rotation
                AssignPositionWhenHandOpens();

            }

            if (collider.gameObject.tag == "cube")
                touchCube = false;

            if (collider.gameObject.tag == "elbow_r") {
                print("OUT");
                rotationMode = false;
            }
        }

        //assign to objectposition the position of the object in the current mode 
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

        //return whether rotation mode is activated or not
        public bool GetRotationMode() {

            return rotationMode;
        }

        public bool GetTouchCube() {

            return touchCube;
        }

    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ManusVR.PhysicalInteraction
{

    public class ModelManipulator : MonoBehaviour
    {
        public InteractableItem item;
    
        public GameObject m_LeftController;
        public GameObject m_RightController;
      
        public device_type_t DeviceTypeLeft;
        public device_type_t DeviceTypeRight;

        public Left_h left_h;
        public Right_h right_h;
     
        Vector3 objectPosition;

        public enum State { standard, fistClosed, fistOpen };
        State state;
        // public GrabbingZone zone;

        //turns true once when get in scale mode to get position of object when scale mode is started
        private bool scaleModePosition = true;
        private bool enterContactMode = true;
        private bool ZoneFistActivation = true;
        private bool distanceZone = true;
        private bool isTouched;

        private bool rightFist;
        private bool leftFist;

        [SerializeField] float grabbingDistance = 0.2f;

        Vector3 objectPositionEnterZone;
        Quaternion objectRotationEnterZone;

        public float m_scaleFactor;
        public float m_moveFactor;
        public float m_conactScaleFactor;

        // Set 0 for no max scale
        public Vector3 m_maxScale;

        public bool m_AutoSetMoveScale;
        public bool m_allowRotation;
        public CortexDrawer m_drawer;
        // WHich control mode allows for this manipulation
        public ControlModeManager.CONTROL_MODE m_activeMode;
        public ControlModeManager.CONTROL_MODE m_secondActiveMode;
        public ControlModeManager.CONTROL_MODE m_thirdActiveMode;
        public ControlModeManager m_controlManager;

        private enum SIDE { LEFT, RIGHT };
        private bool m_MoveModeActive;
        private SIDE m_rotatingHand;
        private Quaternion m_startingModelRotation;
        private Quaternion m_startingControllerRotation;
        private Vector3 m_startingModelPos;
        private Vector3 m_startingControllerPos;

        private bool m_ScaleModeActive;
        private Vector3 m_startingScale;
        private float m_startingDist;



        // Use this for initialization
        void Start() {

            state = State.standard;

        }

        //function to be used in Right_h and Left_h to get the position of cube when close the second hand
        public bool GetScaleMode() {

            return m_ScaleModeActive;
        }
      
        //SUBJECT TO CHANGES
       /* public void ChangeIsTouched() {

            isTouched = true;
        }
        */

        //change statut when hand closes
        public void ChangeStatutFistClosed() {

                state = State.fistClosed;
        }

        //change statut when hand opens
        public void ChangeStatutFistOpen() {

                state = State.fistOpen;
        }

        //change statut back to standard when touching an object
        public void ChangeStatutStandard() {

            state = State.standard;
        }


        //activates when left hand makes a fist
        public void LeftGrip() {

            SetGrip(true, SIDE.LEFT);
            leftFist = true;
        }

        //activates when left hand removes fist
        public void LeftUngrip() {

            SetGrip(false, SIDE.LEFT);
            leftFist = false;
        }

        //activates when right hand makes a fist
        public void RightGrip() {

            SetGrip(true, SIDE.RIGHT);
            rightFist = true;
        }

        //activates when right hand removes fist
        public void RightUngrip() {

            SetGrip(false, SIDE.RIGHT);
            rightFist = false;
        }

        //function that specifies which mode to activate or deactivate (move mode and scale mode)
        private void SetGrip(bool gripped, SIDE side) {
         
                if (gripped) {
                    //Other hand is already moving. switch to scale
                    if (m_MoveModeActive) {
                        InitScaleMode();
                    }
                    //This shouldn't happen...
                    else if (m_ScaleModeActive) {
                        print("Gripped while scaling...");
                    }
                    // Else must be first hand to grip. begin moving
                    else {
                        InitMoveMode(side);
                    }
                }
                else {
                    //hand released. Now rotating with other hand.
                    if (m_ScaleModeActive) {
                        InitMoveMode((SIDE)Math.Abs((int)side - 1));
                    }
                    // Stop rotating
                    else if (m_MoveModeActive && m_rotatingHand == side) {
                        m_MoveModeActive = false;
                    }
                }        
        }

        //activate scale mode and save initial position of right and left hand and initial scale of the object
        private void InitScaleMode() {
            m_MoveModeActive = false;
            m_ScaleModeActive = true;

            Vector3 leftPos = m_LeftController.transform.position;
            Vector3 rightPos = m_RightController.transform.position;
            m_startingDist = Vector3.Distance(leftPos, rightPos);
            m_startingScale = transform.localScale;
        }

        //activate mode move and save initial position of right and left hand and position of the object to be moved
        private void InitMoveMode(SIDE side) {
            m_MoveModeActive = true;
            m_ScaleModeActive = false;

            scaleModePosition = true;

            m_startingModelRotation = transform.rotation;
            m_startingModelPos = transform.position;
            m_rotatingHand = side;

            if (side == SIDE.LEFT) {
                m_startingControllerRotation = m_LeftController.transform.rotation;
                m_startingControllerPos = m_LeftController.transform.position;
                //VibrateHandLeft();
            }
            else {
                m_startingControllerRotation = m_RightController.transform.rotation;
                m_startingControllerPos = m_RightController.transform.position;
                //VibrateHandRight();
            }
        }

        //vibrate left hand
        private void VibrateHandLeft() {
            Manus.ManusSetVibration(HandData.Instance.Session, DeviceTypeLeft, 0.7f, 150);
        }

        //vibrate right hand
        private void VibrateHandRight() {
            Manus.ManusSetVibration(HandData.Instance.Session, DeviceTypeRight, 0.7f, 150);
        }

        // Update is called once per frame
        void Update() {


            // If we're in wrong control mode, return
            if (m_controlManager.GetCurrentControlMode() != m_activeMode
                && m_controlManager.GetCurrentControlMode() != m_secondActiveMode
                && m_controlManager.GetCurrentControlMode() != m_thirdActiveMode)
                return;

            //check if object is the contact zone
            Distance();

            //if object enters contact zone
            if (distanceZone == false) {
                
                //if object is not touched directly but is in contact zone
                if (!item.GetTouch()) {
                    
                    //Enter once when object enters contact zone to get object position
                    if (enterContactMode) {
                        print("this should happen once");
                        objectPositionEnterZone = transform.position;
                        objectRotationEnterZone = transform.rotation;
                        enterContactMode = false;
                    }
                    //fix the object position when object enters contact zone
                    print("keep going");
                    transform.position = objectPositionEnterZone;                   
                    transform.rotation = objectRotationEnterZone;
                   
                    return;
                }
                //if object is touched directly then not fix its position anymore
                else {
                    print("still in zone");
                    ContactScaleMode();
                    //isTouched = true;
                    return;
                }
            }

            /*
            if (!isTouched) {
                transform.position = objectPositionEnterZone;
                transform.rotation = objectRotationEnterZone;
                print(transform.rotation);
                print("DONT MOVE");
                return;
            }
            */

            if (!ZoneFistActivation) {
                print("waiting for fist");
                return;
            }
            
           

            // We're in move/rotate mode
            if (m_MoveModeActive) {
                Quaternion curContrRot;
                Vector3 curContrPos;
                bool rotateEnabledLeft = false;
                bool rotateEnabledRight = false;
                if (m_rotatingHand == SIDE.LEFT) {
                    curContrRot = m_LeftController.transform.rotation;
                    curContrPos = m_LeftController.transform.position;
                    print("left");
                    rotateEnabledLeft = right_h.GetRotationMode();
                    print(rotateEnabledLeft);
                }
                else {
                    curContrRot = m_RightController.transform.rotation;
                    curContrPos = m_RightController.transform.position;

                    rotateEnabledRight = left_h.GetRotationMode();
                }

                Quaternion rotationDiff = Quaternion.Inverse(m_startingControllerRotation) * curContrRot;
                Vector3 posDiff = curContrPos - m_startingControllerPos;

                //allow rotation of the cortex while fixing the object (this should not concern cube objects)
                if ((rotateEnabledLeft || rotateEnabledRight) && m_allowRotation) {                 
                    print("COUCOU");
                    transform.rotation = m_startingModelRotation * rotationDiff;
                }
                //allow the moving of the object
                else  {
                    if (m_AutoSetMoveScale) {
                        print("YALLLLLA");
                        float modelScale = transform.parent.localScale.x;
                        m_moveFactor = GetMoveScale(modelScale);
                    }
                    //update position of the object at each time frame
                    transform.position = m_startingModelPos + posDiff * m_moveFactor;
                    //fix rotation to zero if object is a cube
                    if (!m_allowRotation) {
                        print("this should not be here");
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                }

            }

            // We are in scale mode
            else if (m_ScaleModeActive) {
                Vector3 curLeftPos = m_LeftController.transform.localPosition;
                Vector3 curRightPos = m_RightController.transform.localPosition;
                float curContrDist = Vector3.Distance(curLeftPos, curRightPos);

                float scale = (curContrDist / m_startingDist) * m_scaleFactor;

                //scale cubes 
                if (m_maxScale != Vector3.zero) {
                    print("moveMoveActice");
                    transform.localScale = Vector3.Min(m_startingScale * scale, m_maxScale);
                   
                    //fix the object at the exact position when the second hand makes a fist and therefore launching scale mode
                    if (scaleModePosition)
                        GetObjectPositionScaleMode();

                    FixPositionRotation();
                }
                //else scale cortex
                else {
                    print("moveModelActive");
                    transform.localScale = m_startingScale * scale;

                    //fix the object at the exact position when the second hand makes a fist and therefore launching scale mode
                    if (scaleModePosition)
                        GetObjectPositionScaleMode();

                    FixPositionRotation();
                }
            }

            //When neither move mode nor scale mode is activated
            else if(state == State.fistOpen) {
                print("standard");
                FixPositionRotation(); //fix position of the object where it was left when opening the hand
            }
        }

        // Predicts a good move scale for QueryCube for different scales
        private float GetMoveScale(float modelScale) {
            return Mathf.Max(-1919.1919191919f * modelScale + 1134.338f, 10f);
        }
        
        public void ChangeZoneFistActivation(bool sign) {
            ZoneFistActivation = sign;

        }
        
        public void Distance() {

            float dist_left = (transform.position - m_LeftController.transform.position).magnitude;
            float dist_right = (transform.position - m_RightController.transform.position).magnitude;

            if (dist_left <= grabbingDistance || dist_right <= grabbingDistance) {
               // ZoneFistActivation = false;
                distanceZone = false;
                state = State.standard;
               // isTouched = false;


            }
            else {
                distanceZone = true;
                enterContactMode = true;
                item.UpdateTouch();              
            }
        }


        //return bool distanceZone to know whether object is in or out of the contact zone
        public bool GetDistance() {
            return distanceZone;
        }

        //Assign position of the object when scale mode is activated
        private void GetObjectPositionScaleMode() {
            scaleModePosition = false;
            if (m_rotatingHand == SIDE.LEFT) {
                left_h.AssignPositionWhenHandOpens();
            }
            else {
                right_h.AssignPositionWhenHandOpens();
            }
        }

        private void ContactScaleMode() {

            if(rightFist && leftFist && right_h.GetTouchCube() && left_h.GetTouchCube()) {

                Vector3 curLeftPos = m_LeftController.transform.localPosition;
                Vector3 curRightPos = m_RightController.transform.localPosition;
                float curContrDist = Vector3.Distance(curLeftPos, curRightPos);

                float scale = (curContrDist / m_startingDist) * m_conactScaleFactor;
                transform.localScale = m_startingScale * scale;


            }

        }

        //get position of the object at a given moment to make it static 
        private void FixPositionRotation() {
            if (m_rotatingHand == SIDE.LEFT) {
                objectPosition = left_h.GetObjectPosition();  //get the position of the object when hand opens
                transform.position = objectPosition;

                if (m_allowRotation) {
                    print("keep rotation");
                    transform.rotation = left_h.GetObjectRotation(); //fix rotation of the cortex once rotation mode is exit
                }
                else
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f); //fix rotation of the object when in move mode
            }

            if (m_rotatingHand == SIDE.RIGHT) {             
                objectPosition = right_h.GetObjectPosition();
                transform.position = objectPosition;

                if (m_allowRotation) {
                    print("keep rotation");
                    transform.rotation = right_h.GetObjectRotation(); //fix rotation of the cortex once rotation mode is exit                
                }
                else
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);//fix rotation of the object when in move mode
            }
        }
    }
}

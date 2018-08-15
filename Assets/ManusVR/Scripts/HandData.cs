using System;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace ManusVR
{
    public enum Finger
    {
        thumb,
        index,
        middle,
        ring,
        pink
    }

    /// <summary>
    /// List of values/states to check for each hand
    /// Go from big to small numbers
    /// </summary>
    public enum CloseValue
    {
        Fist = 65,
        Small = 30,
        Tiny = 15,
        Open = 5
    }

    /// <summary>
    /// State of each hand
    /// </summary>
    public struct HandValue
    {
        public CloseValue CloseValue;
        public bool IsClosed;
        public bool IsOpen;
        public bool HandOpened;
        public bool HandClosed;
        public ToggleEvent OnValueChanged;
    }

    [System.Serializable]
    public class ToggleEvent : UnityEvent<CloseValue> { }

    public class HandData : MonoBehaviour
    {
        public IntPtr Session { get { return session; } }
        private IntPtr session;

        // Saving the data retrieved from the hand
        private manus_hand_t _leftHand;
        private manus_hand_t _rightHand;

        // Save if the last data was retrieved correctly
        private manus_ret_t _leftRet;
        private manus_ret_t _rightRet;

        public static HandData Instance;
        
        string fileNameL = "leftHand" + DateTime.Now.ToString("yyyyMMddThhmmss") + ".txt";
        string fileNameR = "rightHand" + DateTime.Now.ToString("yyyyMMddThhmmss") + ".txt";
        StreamWriter srl, srr;
        ulong counter = 0;

        /// <summary>
        /// Get the close value of the hand
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public CloseValue GetCloseValue(device_type_t deviceType)
        {
            return _handValues[(int)deviceType].CloseValue;
        }

        public UnityEvent<CloseValue> GetOnValueChanged(device_type_t deviceType)
        {
            return _handValues[(int)deviceType].OnValueChanged;
        }
        /// <summary>
        /// Check if the hand just opened
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public bool HandOpened(device_type_t deviceType)
        {
            return _handValues[(int)deviceType].HandOpened;
        }
        /// <summary>
        /// Check if the hand just closed
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public bool HandClosed(device_type_t deviceType)
        {
            return _handValues[(int)deviceType].HandClosed;
        }
        private HandValue[] _handValues = new HandValue[2];

        // Left hand values

        // Use this for initialization
        void Start()
        {
            Manus.ManusInit(out session);
            Manus.ManusSetCoordinateSystem(session, coor_up_t.COOR_Y_UP, coor_handed_t.COOR_LEFT_HANDED);

            if (Instance == null)
                Instance = this;

            for (int i = 0; i < 2; i++)
            {
                _handValues[i].CloseValue = CloseValue.Open;
                _handValues[i].OnValueChanged = new ToggleEvent();
            }

            Manus.ManusGetHand(session, (device_type_t)0, out _leftHand);
            Manus.ManusGetHand(session, (device_type_t)1, out _rightHand);


            srl = File.CreateText(fileNameL);
            srr = File.CreateText(fileNameR);
        }



        // Update is called once per frame
        void Update()
        {
            manus_hand_t leftHand;
            manus_hand_t rightHand;

            // if the retrieval of the handdata is succesfull update the local value and wether the hand is closed
            if (Manus.ManusGetHand(session, device_type_t.GLOVE_LEFT, out leftHand) == manus_ret_t.MANUS_SUCCESS)
            {
                _leftHand = leftHand;
                UpdateCloseValue(AverageFingerValue(_leftHand), device_type_t.GLOVE_LEFT);
                //Debug.Log(_leftHand.raw.finger_sensor[0]);
                //Debug.Log(_leftHand.wrist.x + " " + _leftHand.wrist.y + " " + _leftHand.wrist.z + " " + _leftHand.wrist.w);

                ++counter;
               // if ((counter % 10) == 0) {
                    srl.WriteLine(DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss:ffff") + " " + _leftHand.wrist.x + " " + _leftHand.wrist.y + " " + _leftHand.wrist.z + " " + _leftHand.wrist.w + " " + _leftHand.raw.finger_sensor[0] + " " + _leftHand.raw.finger_sensor[1] + " " + _leftHand.raw.finger_sensor[2] + " " + _leftHand.raw.finger_sensor[3] + " " + _leftHand.raw.finger_sensor[4] + _leftHand.raw.finger_sensor[5] + " " + _leftHand.raw.finger_sensor[6] + " " + _leftHand.raw.finger_sensor[7] + " " + _leftHand.raw.finger_sensor[8] + " " + _leftHand.raw.finger_sensor[9]);
                //}


            }

            if (Manus.ManusGetHand(session, device_type_t.GLOVE_RIGHT, out rightHand) == manus_ret_t.MANUS_SUCCESS)
            {
                _rightHand = rightHand;
                
                UpdateCloseValue(AverageFingerValue(_rightHand), device_type_t.GLOVE_RIGHT);
                srr.WriteLine(DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss:ffff") + " " + _rightHand.wrist.x + " " + _rightHand.wrist.y + " " + _rightHand.wrist.z + " " + _rightHand.wrist.w + " " + _rightHand.raw.finger_sensor[0] + " " + _rightHand.raw.finger_sensor[1] + " " + _rightHand.raw.finger_sensor[2] + " " + _rightHand.raw.finger_sensor[3] + " " + _rightHand.raw.finger_sensor[4] + _rightHand.raw.finger_sensor[5] + " " + _rightHand.raw.finger_sensor[6] + " " + _rightHand.raw.finger_sensor[7] + " " + _rightHand.raw.finger_sensor[8] + " " + _rightHand.raw.finger_sensor[9]);
            }
        }

        /// <summary>
        /// Check if there is valid output for the given device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public bool ValidOutput(device_type_t deviceType)
        {
            if (deviceType == device_type_t.GLOVE_LEFT)
                return _leftRet == manus_ret_t.MANUS_SUCCESS;
            else
                return _rightRet == manus_ret_t.MANUS_SUCCESS;
        }

        /// <summary>
        /// Get the thumb imu rotation of the given device
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public Quaternion GetImuRotation(device_type_t deviceType)
        {
            if (deviceType == device_type_t.GLOVE_LEFT)
                return transform.parent.rotation * _leftHand.raw.imu[1];
            else if (deviceType == device_type_t.GLOVE_RIGHT)
                return transform.parent.rotation * _rightHand.raw.imu[1];
            return transform.parent.rotation;
        }

        /// <summary>
        /// Get the wrist rotation of a given device
        /// </summary>
        /// <param name="deviceType">The device type</param>
        /// <returns></returns>
        public Quaternion GetWristRotation(device_type_t deviceType)
        {
            if (deviceType == device_type_t.GLOVE_LEFT)
                return transform.parent.rotation * _leftHand.wrist;
            else if (deviceType == device_type_t.GLOVE_RIGHT)
                return transform.parent.rotation * _rightHand.wrist;
            else
                return transform.parent.rotation;
        }

        /// <summary>
        /// Get the rotation of the given finger
        /// </summary>
        /// <param name="finger"></param>
        /// <param name="deviceType"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        public Quaternion GetFingerRotation(Finger finger, device_type_t deviceType, int pose)
        {
            manus_hand_t hand;
            if (deviceType == device_type_t.GLOVE_LEFT)
                hand = _leftHand;
            else
                hand = _rightHand;

            Quaternion fingerRotation = hand.fingers[(int)finger].joints[pose].rotation;
            if (fingerRotation == null)
                return Quaternion.identity;
            return fingerRotation;
        }

        /// <summary>
        /// Get the average value of all fingers combined on the given hand
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        private double AverageFingerValue(manus_hand_t hand)
        {
            int sensors = 0;
            double total = 0;
            // Loop through all of the finger values
            for (int bendPosition = 0; bendPosition < 10; bendPosition++)
            {
                // Only get the sensor values of the first bending point without the thumb (1,3,5,7)
                if (bendPosition < 8)
                {
                    sensors++;
                    total += hand.raw.finger_sensor[bendPosition];
                }
            }
            return total / sensors;
        }

        //TODO REMOVE
        public double Average(device_type_t deviceType)
        {
            if (deviceType == device_type_t.GLOVE_RIGHT)
                return AverageFingerValue(_rightHand);

                return AverageFingerValue(_leftHand);
        }
        private void UpdateCloseValue(double averageSensorValue, device_type_t deviceType)
        {
            var values = Enum.GetValues(typeof(CloseValue));
            HandValue handValue;
            if (deviceType == device_type_t.GLOVE_LEFT)
                handValue = _handValues[0];
            else
                handValue = _handValues[1];

            CloseValue closest = CloseValue.Open;
            // Save the old value for comparisment
            CloseValue oldClose = handValue.CloseValue;

            // Get the current close value
            foreach (CloseValue item in values)
            {
                // Div by 100.0 is used because an enum can only contain ints
                if (averageSensorValue > (double)item / 100.0)
                    closest = item;
            }
            handValue.CloseValue = closest;

            // Invoke the on value changed event
            if (oldClose != handValue.CloseValue && handValue.OnValueChanged != null)
                handValue.OnValueChanged.Invoke(handValue.CloseValue);

            // Check if the hand just closed
            handValue.HandClosed = oldClose != handValue.CloseValue && handValue.CloseValue == CloseValue.Fist;
            // Check if the hand just opened
            handValue.HandOpened = (oldClose == CloseValue.Small && handValue.CloseValue == CloseValue.Open);

            if (deviceType == device_type_t.GLOVE_LEFT)
                _handValues[0] = handValue;
            else
                _handValues[1] = handValue;
        }
    }
}
/*
   Copyright 2015 Manus VR

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using System;
using UnityEngine;

namespace ManusVR
{
    public class IKSimulator : MonoBehaviour
    {
        private IntPtr session;
        private float[] handYawOffsets = { 0.0f, 0.0f };

        [SerializeField]
        private Transform cameraRig;

        public Transform rootTransform;
        private Transform[] wristTransforms;
        private Transform[][][] fingerTransforms;

        private Transform leftLowerArm;
        private Transform rightLowerArm;

        public Transform HMD;
        public Transform leftController;
        public Transform rightController;

        bool useTrackers = true;

        public KeyCode calibrateKey;
        public enum HandAlignmentKeys
        {
            None,
            QW,
            AS
        }
        public HandAlignmentKeys leftAlignmentKeys = HandAlignmentKeys.QW;
        public HandAlignmentKeys rightAlignmentKeys = HandAlignmentKeys.AS;

        public Vector3 postRotLeft;
        public Vector3 postRotRight;
        public Vector3 postRotThumbLeft;
        public Vector3 postRotThumbRight;

        public enum IMUCorrectionMethod
        {
            DontCorrect,
            LeftCorrect,
            RightCorrect
        }
        public IMUCorrectionMethod imuCorrectionMethod;

        private float lowerArmLength;

        /// <summary>
        /// Finds a deep child in a transform
        /// </summary>
        /// <param name="aParent">Transform to be searched</param>
        /// <param name="aName">Name of the (grand)child to be found</param>
        /// <returns></returns>
        private static Transform FindDeepChild(Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = FindDeepChild(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        void scaleBone(Transform child, float lenNew, float thicknessScale = 1)
        {
            var lenOld = child.localPosition.magnitude;
            if (lenOld == 0.0f) // Prevent a divide-by-zero for bones with localPosition (0, 0, 0), like the wrist bones.
                lenOld = 1.0f;

            var scaleX = lenNew / lenOld;
            var parent = child.parent;

            Transform newNode = new GameObject(parent.name + "_unscale").transform;

            var childPos = child.localPosition;
            var childRot = child.localRotation;

            newNode.parent = parent;
            child.parent = newNode;

            newNode.localPosition = childPos;
            child.localPosition = Vector3.zero;
            newNode.localRotation = Quaternion.identity;
            child.localRotation = childRot;

            parent.localScale = new Vector3(scaleX, thicknessScale, thicknessScale);
            newNode.localScale = new Vector3(1 / scaleX, 1 / thicknessScale, 1 / thicknessScale);
        }

        /// <summary>
        /// Constructor which loads the HandModel
        /// </summary>
        void Start()
        {
            Manus.ManusInit(out session);
            Manus.ManusSetCoordinateSystem(session, coor_up_t.COOR_Y_UP, coor_handed_t.COOR_LEFT_HANDED);

            string[] fingers =
            {
                "thumb_0",
                "index_0",
                "middle_0",
                "ring_0",
                "pinky_0"
            };

            if (!cameraRig)
            {
                cameraRig = Camera.main.transform.root;
                Debug.LogWarning("CameraRig reference not set, automatically retrieved root transform of main camera. To avoid usage of wrong transform, consider setting this reference.");
            }
            cameraRig.parent = transform.root;
            cameraRig.localPosition = Vector3.zero;

            if (!rootTransform)
                rootTransform = FindDeepChild(transform, "root");

            leftLowerArm = FindDeepChild(rootTransform, "lowerarm_l");
            rightLowerArm = FindDeepChild(rootTransform, "lowerarm_r");

            wristTransforms = new Transform[2];
            wristTransforms[0] = FindDeepChild(rootTransform, "hand_l");
            wristTransforms[1] = FindDeepChild(rootTransform, "hand_r");

            TrackingManager manager = GetComponent<TrackingManager>();
            if (manager)
                useTrackers = manager.trackingToUse == TrackingManager.EUsableTracking.GenericTracker;

            ik_profile_t profile = new ik_profile_t();
            Manus.ManusGetProfile(session, out profile);
            lowerArmLength = (float)profile.lowerArmLength;
            scaleBone(leftLowerArm, (float)profile.upperArmLength);
            scaleBone(rightLowerArm, (float)profile.upperArmLength);
            scaleBone(wristTransforms[0], lowerArmLength);
            scaleBone(wristTransforms[1], lowerArmLength);

            // Associate the game transforms with the skeletal model.
            fingerTransforms = new Transform[2][][];
            fingerTransforms[0] = new Transform[5][];
            fingerTransforms[1] = new Transform[5][];
            for (int i = 0; i < 5; i++)
            {
                fingerTransforms[0][i] = new Transform[5];
                fingerTransforms[1][i] = new Transform[5];
                for (int j = 1; j < 5; j++)
                {
                    string left = fingers[i] + j.ToString() + "_l";
                    string right = fingers[i] + j.ToString() + "_r";
                    fingerTransforms[0][i][j] = FindDeepChild(rootTransform, left);
                    fingerTransforms[1][i][j] = FindDeepChild(rootTransform, right);
                }
            }
        }

        /// <summary>
        /// Updates a skeletal from glove data
        /// </summary>
        void Update()
        {
            // Update the hands. Most of this data is based directly on the sensors.
            // Hand 0 is the left one, hand 1 the right.
            Transform[] controller = new Transform[2] { leftController, rightController };
            Transform[] lowerArm = new Transform[2] { leftLowerArm, rightLowerArm };
            float[] rotationDirection = new float[2] { 1.0f, -1.0f };
            float[] heightOffsetDirection = new float[2] { -1.0f, 1.0f };
            float[] positionOffsetDirection = new float[2] { useTrackers ? -1.0f : 1.0f, 1.0f };
            float lowerArmLengthOffset = useTrackers ? 0.07f : 0.05f;
            Vector3 rotLowerArm = useTrackers ? new Vector3(180.0f, 180.0f, 0.0f) : new Vector3(0.0f, 90.0f, 0.0f);
            Vector3 heightOffsetVec = useTrackers ? Vector3.back : Vector3.right;
            Vector3 lengthOffsetVec = useTrackers ? Vector3.left : Vector3.forward;
            HandAlignmentKeys[] alignmentKeys = new HandAlignmentKeys[2] { leftAlignmentKeys, rightAlignmentKeys };
            Vector3[] postRotEuler = new Vector3[2] { postRotLeft, postRotRight };
            Vector3[] postRotThumbEuler = new Vector3[2] { postRotThumbLeft, postRotThumbRight };

            for (int handNum = 0; handNum < 2; handNum++)
            {
                device_type_t deviceType = (device_type_t)handNum;
                // Set the position of the arms.
                lowerArm[handNum].rotation = controller[handNum].rotation * Quaternion.Euler(new Vector3(rotLowerArm.x, rotationDirection[handNum] * rotLowerArm.y, rotLowerArm.z));
                Vector3 offset = controller[handNum].TransformVector(lengthOffsetVec).normalized * (lowerArmLength - lowerArmLengthOffset) - controller[handNum].TransformVector(heightOffsetVec).normalized * heightOffsetDirection[handNum] * 0.03f;
                lowerArm[handNum].position = controller[handNum].position - positionOffsetDirection[handNum] * offset;

                if (!HandData.Instance.ValidOutput(deviceType))
                    continue;

                // Adjust the default orientation of the hand when the calibrateKey is pressed.
                if (Input.GetKeyDown(calibrateKey))
                {
                    Debug.Log("Calibrated a hand.");
                    handYawOffsets[handNum] = -(HandData.Instance.GetWristRotation(deviceType)).eulerAngles.y + wristTransforms[handNum].parent.eulerAngles.y;
                    //if ((device_type_t) handNum == device_type_t.GLOVE_LEFT)
                    handYawOffsets[handNum] += 180;
                }

                // Manually rotate the hands with the HandAlignmentKeys.
                switch (alignmentKeys[handNum])
                {
                    case HandAlignmentKeys.None:
                        break;

                    case HandAlignmentKeys.QW:
                        if (Input.GetKey(KeyCode.Q))
                            handYawOffsets[handNum] -= 1.0f;
                        else if (Input.GetKey(KeyCode.W))
                            handYawOffsets[handNum] += 1.0f;
                        break;

                    case HandAlignmentKeys.AS:
                        if (Input.GetKey(KeyCode.A))
                            handYawOffsets[handNum] -= 1.0f;
                        else if (Input.GetKey(KeyCode.S))
                            handYawOffsets[handNum] += 1.0f;
                        break;

                    default:
                        Debug.LogWarning("The alignment keys for the " + (handNum == 0 ? "left" : "right") + " hand are set to an unknown value.");
                        break;
                }


                // Set the wrist rotation.
                Quaternion postRot = Quaternion.Euler(postRotEuler[handNum]);
                wristTransforms[handNum].rotation = Quaternion.Euler(0.0f, handYawOffsets[handNum], 0.0f) * HandData.Instance.GetWristRotation(deviceType) * postRot;

                // Set the rotation of the fingers.
                for (int joint = 3; joint >= 1; joint--)
                {
                    // Set this joint for all the fingers.
                    for (int finger = 0; finger < 5; finger++)
                        fingerTransforms[handNum][finger][joint].localRotation = HandData.Instance.GetFingerRotation((Finger)finger, deviceType, joint);

                    // Handle joint 1 differently from the rest for the thumb.
                    if (joint == 1)
                    {
                        Quaternion postRotThumb = Quaternion.Euler(postRotThumbEuler[handNum]);
                        Quaternion rot1 = Quaternion.Euler(0.0f, handYawOffsets[handNum], 0.0f) * HandData.Instance.GetImuRotation(deviceType) * postRotThumb;
                        Quaternion rot2 = fingerTransforms[handNum][0][2].localRotation;
                        Quaternion rot;

                        switch (imuCorrectionMethod)
                        {
                            case IMUCorrectionMethod.DontCorrect:
                                rot = rot1;
                                break;
                            case IMUCorrectionMethod.LeftCorrect:
                                rot = Quaternion.Inverse(rot2) * rot1;
                                break;
                            case IMUCorrectionMethod.RightCorrect:
                                rot = rot1 * Quaternion.Inverse(rot2);
                                break;
                            default:
                                throw new System.ArgumentException(string.Format("imuCorrectionMethod has value {0}.", imuCorrectionMethod.ToString()));
                        }

                        fingerTransforms[handNum][0][joint].rotation = rot;
                    }
                } // for loop for the joints
            } // for loop for the hands
        } // void Update()
    }
}

using UnityEngine;
using Valve.VR;

namespace ManusVR
{
    public class TrackingManager : MonoBehaviour
    {
        public enum EIndex
        {
            None = -1,
            Hmd = (int) OpenVR.k_unTrackedDeviceIndex_Hmd,
            Limit = (int) OpenVR.k_unMaxTrackedDeviceCount
        }

        public enum EUsableTracking
        {
            Controller,
            GenericTracker
        }

        private enum ERole
        {
            HMD,
            LeftHand,
            RightHand
        }

        private class TrackedDevice
        {
            public int index;
            public bool isValid;
        }

        public EUsableTracking trackingToUse = EUsableTracking.GenericTracker;
        private ETrackedDeviceClass pTrackingToUse = ETrackedDeviceClass.GenericTracker;

        public Transform HMD;
        public Transform leftTracker;
        public Transform rightTracker;
        private Transform[] trackerTransforms;

        private TrackedDevice[] devices;

        SteamVR_Events.Action newPosesAction;

        // Manus Custom vars
        public Vector3 leftRotOffset = new Vector3(0, 0, 0);
        public Vector3 rightRotOffset = new Vector3(0, 0, 0);

        public Vector3 leftPosOffset = new Vector3(0, 0, 0);
        public Vector3 rightPosOffset = new Vector3(0, 0, 0);

        public KeyCode switchArmsButton = KeyCode.None;

        // Use this for initialization
        void Start()
        {
            pTrackingToUse = trackingToUse == EUsableTracking.Controller ? ETrackedDeviceClass.Controller : ETrackedDeviceClass.GenericTracker;

            trackerTransforms = new Transform[3];
            trackerTransforms[(int) ERole.HMD] = HMD;
            trackerTransforms[(int) ERole.LeftHand] = leftTracker;
            trackerTransforms[(int) ERole.RightHand] = rightTracker;

            int num = System.Enum.GetNames(typeof(ERole)).Length;
            devices = new TrackedDevice[num];

            for (int i = 0; i < num; i++)
            {
                devices[i] = new TrackedDevice();
                devices[i].index = new int();
                devices[i].index = (int) EIndex.None;
                devices[i].isValid = new bool();
                devices[i].isValid = false;

                GetIndex(i);
            }
        }

        void Awake()
        {
            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
        }

        void OnEnable()
        {
            var render = SteamVR_Render.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            newPosesAction.enabled = true;
        }

        void OnDisable()
        {
            newPosesAction.enabled = false;

            for (int i = 0; i < devices.Length; i++)
                devices[i].isValid = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(switchArmsButton))
            {
                // Switch the indices around.
                int leftNum = (int) ERole.LeftHand;
                int rightNum = (int) ERole.RightHand;
                int OldLeftIndex = devices[leftNum].index;

                devices[leftNum].index = devices[rightNum].index;
                devices[rightNum].index = OldLeftIndex;
            }
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            for (int deviceNum = 0; deviceNum < devices.Length; deviceNum++)
            {
                // if no role is set or the tracked object is the head
                //if (myRole == ETrackedControllerRole.Invalid && !isHead)
                if (devices[deviceNum].index == (int) EIndex.None)
                    continue;

                int intIndex = (int) devices[deviceNum].index;
                ERole role = (ERole) deviceNum;
                devices[deviceNum].isValid = false;

                if (poses.Length <= intIndex)
                    continue;
                try
                {
                    if (!poses[intIndex].bDeviceIsConnected)
                        continue;
                }
                catch (System.IndexOutOfRangeException)
                {
                    // retry to get the glove index
                    GetIndex(deviceNum);
                    continue;
                }

                if (!poses[intIndex].bPoseIsValid)
                    continue;
                devices[deviceNum].isValid = true;

                var pose = new SteamVR_Utils.RigidTransform(poses[intIndex].mDeviceToAbsoluteTracking);

                // make sure the offset is localized
                trackerTransforms[deviceNum].localPosition = pose.pos + pose.rot * (role == ERole.LeftHand ? leftPosOffset : rightPosOffset);
                trackerTransforms[deviceNum].localRotation = pose.rot * Quaternion.Euler(role == ERole.LeftHand ? leftRotOffset : rightRotOffset);
            }
        }
    
        void GetIndex(int deviceNum)
        {
            ERole role = (ERole) deviceNum;

            if (role == ERole.HMD)
            {
                devices[deviceNum].index = (int) EIndex.Hmd;
                return;
            }

            int DeviceCount = 0;

            for (uint i = 0; i < (uint) EIndex.Limit; i++)
            {
                ETrackedPropertyError error = new ETrackedPropertyError();
                ETrackedDeviceClass type = (ETrackedDeviceClass) OpenVR.System.GetInt32TrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_DeviceClass_Int32, ref error);

                if (pTrackingToUse == ETrackedDeviceClass.Controller && type == ETrackedDeviceClass.Controller
                    || pTrackingToUse == ETrackedDeviceClass.GenericTracker && type == ETrackedDeviceClass.GenericTracker)
                {
                    if (role == ERole.LeftHand && DeviceCount == 0 || role == ERole.RightHand && DeviceCount == 1)
                    {
                        devices[deviceNum].index = (int) i;
                        return;
                    }

                    DeviceCount++;
                }
            }
        }
    }
}

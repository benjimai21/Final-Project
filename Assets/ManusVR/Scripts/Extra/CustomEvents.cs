using ManusVR.PhysicalInteraction;
using UnityEngine.Events;

namespace ManusVR.Extra
{
    //-------------------------------------------------------------------------
    public static class CustomEvents
    {
        //-------------------------------------------------
        [System.Serializable]
        public class UnityEventBool : UnityEvent<bool>
        {
        }

        //-------------------------------------------------
        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float>
        {
        }

        //-------------------------------------------------
        [System.Serializable]
        public class UnityEventHand : UnityEvent<device_type_t>
        {
        }

        //-------------------------------------------------
        [System.Serializable]
        public class UnityEventPhalange : UnityEvent<Phalange>
        {
        }
    }
}

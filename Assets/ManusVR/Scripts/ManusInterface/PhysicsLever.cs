using System;
using ManusVR.PhysicalInteraction;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace ManusVR.ManusInterface
{
    public class PhysicsLever : MonoBehaviour
    {
        public Action<float> OnValueChanged;
        [SerializeField]
        private ManusVR.Extra.CustomEvents.UnityEventFloat _valueChangedEvent;

        private Quaternion _minRotation, _midRotation, _maxRotation;
        private float _angleRange;

        [SerializeField, HideInInspector]
        private float _initialValue;
        public Vector2 MinMaxValue;

        private HingeJoint _hingeJoint;

        private float _currentValue = Single.NaN;
        
        public float CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (float.IsNaN(value) || FloatComparer.AreEqual(_currentValue, value, 0.0001f))
                    return;

                _currentValue = value;
                OnValueChanged.Invoke(_currentValue);
            }
        }

        void Awake()
        {
            _hingeJoint = gameObject.GetComponent<HingeJoint>();
            _midRotation = _hingeJoint.transform.localRotation;
            _minRotation = _midRotation * Quaternion.AngleAxis(_hingeJoint.limits.min, _hingeJoint.axis);
            _maxRotation = _midRotation * Quaternion.AngleAxis(_hingeJoint.limits.max, _hingeJoint.axis);
            _angleRange = Mathf.Max(_hingeJoint.limits.max, _hingeJoint.limits.min) -
                          Mathf.Min(_hingeJoint.limits.max, _hingeJoint.limits.min);
        }

        protected void Start()
        {
            OnValueChanged += _valueChangedEvent.Invoke;
            CurrentValue = _initialValue;
            RotateToValue(CurrentValue);
        }

        void OnValidate()
        {
            if (MinMaxValue.y < MinMaxValue.x)
                MinMaxValue.y = MinMaxValue.x;
            _initialValue = Mathf.Clamp(_initialValue, MinMaxValue.x, MinMaxValue.y);
        }

        void Update()
        {
            float angle = _hingeJoint.angle - _hingeJoint.limits.min;
            CurrentValue = Mathf.Lerp(MinMaxValue.x, MinMaxValue.y, angle / _angleRange);
            //switch (RotationAxis)
            //{
            //    case Axis.X:
            //        var euler = transform.localEulerAngles.x;
            //        while (euler > 180)
            //            euler -= 360;
            //        while (euler < -180)
            //            euler += 360;

            //        angle = Mathf.InverseLerp(_initialLocalEulerAngles.x + RotationLimits.x,
            //            _initialLocalEulerAngles.x + RotationLimits.y, euler);
            //        break;
            //    case Axis.Y:
            //        euler = transform.localEulerAngles.y;
            //        while (euler > 180)
            //            euler -= 360;
            //        while (euler < -180)
            //            euler += 360;

            //        angle = Mathf.InverseLerp(_initialLocalEulerAngles.y + RotationLimits.x,
            //            _initialLocalEulerAngles.y + RotationLimits.y, euler);
            //        break;
            //    case Axis.Z:
            //        euler = transform.localEulerAngles.z;
            //        while (euler > 180)
            //            euler -= 360;
            //        while (euler < -180)
            //            euler += 360;

            //        angle = Mathf.InverseLerp(_initialLocalEulerAngles.z + RotationLimits.x,
            //            _initialLocalEulerAngles.z + RotationLimits.y, euler);
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
            //CurrentValue = Mathf.Lerp(MinValue, MaxValue, angle);
        }

        //Add or get the HingeJoint component and set the correct values.
        void InitializeHingeJoint()
        {
            //_hingeJoint = GetComponent<HingeJoint>();
            //if (_hingeJoint == null)
            //    _hingeJoint = gameObject.AddComponent<HingeJoint>();

            ////Set anchor
            //_hingeJoint.anchor = Vector3.zero;

            ////Set axis
            //_hingeJoint.axis = new Vector3(Convert.ToInt32(RotationAxis == Axis.X), Convert.ToInt32(RotationAxis == Axis.Y), Convert.ToInt32(RotationAxis == Axis.Z));

            ////Setup limits
            //_hingeJoint.useLimits = true;
            //var limits = _hingeJoint.limits;
            //limits.min = RotationLimits.x;
            //limits.max = RotationLimits.y;
            //_hingeJoint.limits = limits;
        }

        //void SetRigidbodyConstraints()
        //{
        //    var rb = GetComponent<Rigidbody>();
        //    int constraints = (int)RigidbodyConstraints.FreezeAll;
        //    switch (RotationAxis)
        //    {
        //        case Axis.X:
        //            constraints -= (int)RigidbodyConstraints.FreezeRotationX;
        //            break;
        //        case Axis.Y:
        //            constraints -= (int)RigidbodyConstraints.FreezeRotationY;
        //            break;
        //        case Axis.Z:
        //            constraints -= (int)RigidbodyConstraints.FreezeRotationZ;
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //    rb.constraints = (RigidbodyConstraints)constraints;
        //}

        public void RotateToValue(float value)
        {
            
            var valuePercent = Mathf.InverseLerp(MinMaxValue.x, MinMaxValue.y, value);
            var newAngle = _angleRange* valuePercent;
            transform.rotation = _minRotation * Quaternion.AngleAxis(newAngle, _hingeJoint.axis);
            //var euler = transform.localEulerAngles;
            //switch (RotationAxis)
            //{
            //    case Axis.X:
            //       euler.x = Mathf.Lerp(_initialLocalEulerAngles.x + RotationLimits.x,
            //            _initialLocalEulerAngles.x + RotationLimits.y, valuePercent);
            //        break;
            //    case Axis.Y:
            //        euler.y = Mathf.Lerp(_initialLocalEulerAngles.y + RotationLimits.x,
            //            _initialLocalEulerAngles.y + RotationLimits.y, valuePercent);
            //        break;
            //    case Axis.Z:
            //        euler.z = Mathf.Lerp(_initialLocalEulerAngles.z + RotationLimits.x,
            //            _initialLocalEulerAngles.z + RotationLimits.y, valuePercent);
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
            //transform.localEulerAngles = euler;
        }
    }
}

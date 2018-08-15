using System;
using System.Collections;
using ManusVR.Extra;
using ManusVR.PhysicalInteraction;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;

namespace ManusVR.ManusInterface
{
    public abstract class Slider : InterfaceItem
    {
        public Action<float> OnValueChanged;

        [SerializeField, HideInInspector]
        protected CustomEvents.UnityEventFloat ValueChangedEvent;

        [SerializeField, HideInInspector]
        public Vector2 MinMaxValue;
        [SerializeField, HideInInspector]
        protected float InitialValue;
        
        private float _currentValue;
        public float CurrentValue
        {
            get { return _currentValue; }
            protected set
            {
                if (float.IsNaN(value) || FloatComparer.AreEqual(_currentValue, value, 0.0001f))
                    return;
                if (MinMaxValue == null)
                    return;
                _currentValue = Mathf.Clamp(value, MinMaxValue.x, MinMaxValue.y);
                OnValueChanged.Invoke(_currentValue);
            }
        }

        void OnValidate()
        {
            if (MinMaxValue.x > MinMaxValue.y)
                MinMaxValue.y = MinMaxValue.x;
            InitialValue = Mathf.Clamp(InitialValue, MinMaxValue.x, MinMaxValue.y);
        }

        protected override void Start()
        {
            base.Start();

            OnValueChanged += ValueChangedEvent.Invoke;

            CurrentValue = InitialValue;
            SetSliderPosition(InitialValue);
        }

        void Update()
        {
            CurrentValue = Mathf.Lerp(MinMaxValue.x, MinMaxValue.y, GetCurrentInverseLerpValue());
        }
        protected abstract float GetCurrentInverseLerpValue();
        protected abstract void SetSliderPosition(float value);
    }
}

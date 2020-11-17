using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public class HandleValueChange<T>
    {
        [SerializeField, HideInInspector] private T _value;
        [SerializeField, HideInInspector] protected T prevValue;

        private bool _isValueType;

        public event Action<T> onChange;

        public HandleValueChange()
        {
            _isValueType = typeof(T).IsValueType;
        }

        [ShowInInspector]
        public T Value
        {
            get => _value;
            set
            {
                if (!_isValueType && value != null)
                {
                    prevValue = TypeUtils.DeepCopy(value);
                }
                
                if (!IsEqual(_value, value))
                {
                    _value = value;
                    
                    if (_isValueType)
                    {
                        prevValue = value;
                    }
                    
                    onChange?.Invoke(value);
                }
            }
        }

        public virtual bool IsEqual(T prev, T current)
        {
            return false;
        }

        public virtual void HandleChange(T value)
        {
            _value = value;

            if (!_isValueType && value != null)
            {
                prevValue = TypeUtils.DeepCopy(value);
            }

            onChange?.Invoke(Value);
        }
    }
}
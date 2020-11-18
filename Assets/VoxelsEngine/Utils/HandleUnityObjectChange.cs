using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VoxelsEngine.Utils
{
    /// <summary>
    /// Used for handling of value change of Unity build in types which inherits from UnityEngine.Object
    /// </summary>
    [Serializable]
    public class HandleUnityObjectChange<T> where T : Object
    {
        [SerializeField, HideInInspector] private T _value;
        [SerializeField, HideInInspector] protected T prevValue;

        public event Action<T> onChange;

        [ShowInInspector]
        public T Value
        {
            get => _value;
            set
            {
                if (value != null)
                {
                    prevValue = Object.Instantiate(value);
                }
                
                if (!IsEqual(_value, value))
                {
                    _value = value;

                    onChange?.Invoke(value);
                }
            }
        }

        public virtual bool IsEqual(T prev, T current)
        {
            return false;
        }

        /// <summary>
        /// Used for reference types in OnValidate of MonoBehavior
        /// </summary>
        /// <param name="value"></param>
        public virtual void HandleChange(T value)
        {
            _value = value;

            if (value != null)
            {
                prevValue = Object.Instantiate(value);
            }

            onChange?.Invoke(Value);
        }
    }
}
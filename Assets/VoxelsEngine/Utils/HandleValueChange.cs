﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    [Serializable]
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

        [DelayedProperty, ShowInInspector]
        public T Value
        {
            get => _value;
            set
            {
                if (!IsValid(value)) return;
                
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

        /// <summary>
        /// Used for reference types in OnValidate of MonoBehavior
        /// </summary>
        /// <param name="value"></param>
        public virtual void HandleChange(T value)
        {
            if (!IsValid(value)) return;
            
            _value = value;

            if (!_isValueType && value != null)
            {
                prevValue = TypeUtils.DeepCopy(value);
            }

            onChange?.Invoke(Value);
        }

        public virtual bool IsValid(T value)
        {
            return true;
        }
    }
}
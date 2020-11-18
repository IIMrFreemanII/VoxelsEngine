using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class TypeUtils
    {
        public static Dictionary<string, object> GetFields(object type)
        {
            if (type == null) return new Dictionary<string, object>();
            Type t = type.GetType();
            FieldInfo[] fields =
                t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            
            Dictionary<string, object> dict = new Dictionary<string, object>();
            
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(type);
                dict.Add(field.Name, value);
            }

            return dict;
        }

        public static T DeepCopy<T>(T original)
        {
            T copy = Activator.CreateInstance<T>();

            FieldInfo[] originalFields = original.GetType().GetFields();

            foreach (FieldInfo originalField in originalFields)
            {
                originalField.SetValue(copy, originalField.GetValue(original));
            }

            return copy;
        }
        
        // public static T DeepCopy<T>(T original) where T : ScriptableObject
        // {
        //     T copy = ScriptableObject.CreateInstance<T>();
        //
        //     FieldInfo[] originalFields = original.GetType().GetFields();
        //
        //     foreach (FieldInfo originalField in originalFields)
        //     {
        //         originalField.SetValue(copy, originalField.GetValue(original));
        //     }
        //
        //     return copy;
        // }
        
        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
    }
}
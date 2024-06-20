using System;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI
{
    public class Modal : MonoBehaviour
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();
        
        public void SetValue<T>(string name, T value)
        {
            if (values.ContainsKey(name))
            {
                values[name] = value;
            }
            else
            {
                values.Add(name, value);
            }
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            if (values.TryGetValue(name, out var storedValue) && storedValue is T)
            {
                value = (T)storedValue;
                return true;
            }
            value = default;
            return false;
        }

        public T GetValueOrDefault<T>(string name, T defaultValue = default)
        {
            return TryGetValue(name, out T value) ? value : defaultValue;
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDisable()
        {
            
        }

        public virtual void Refresh()
        {
            
        }
    }
}
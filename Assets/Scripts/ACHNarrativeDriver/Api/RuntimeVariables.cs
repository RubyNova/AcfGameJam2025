using System.Collections.Generic;
using System.Collections.ObjectModel;
using ACHNarrativeDriver.ScriptableObjects;
using UnityEngine;

namespace ACHNarrativeDriver.Api
{
    public class RuntimeVariables : MonoBehaviour
    {
        [SerializeField] private PredefinedVariables _eagerInitialisationValues;
        
        private Dictionary<string, string> _backingDictionary;

        public ReadOnlyDictionary<string, string> ReadOnlyVariableView { get; private set; }

        private void Awake()
        {
            _backingDictionary = new();

            if (_eagerInitialisationValues is not null)
            {
                foreach (var variablePair in _eagerInitialisationValues.Variables)
                {
                    _backingDictionary.Add(variablePair.Key, variablePair.Value);
                }
            }
            
            ReadOnlyVariableView = new ReadOnlyDictionary<string, string>(_backingDictionary);
            DontDestroyOnLoad(gameObject);
        }

        public void UpdateVariable(string variableName, string newValue)
        {
            if (!_backingDictionary.ContainsKey(variableName))
            {
                _backingDictionary.Add(variableName, newValue);
                return;
            }
        
            _backingDictionary[variableName] = newValue;
        }
    }
}

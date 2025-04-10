using System;
using System.Collections.Generic;
using UnityEngine;

namespace ACHNarrativeDriver.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewNarrativeSequence", menuName = "ACH Narrative Driver / PredefinedVariables")]
    public class PredefinedVariables : ScriptableObject
    {
        [Serializable]
        public class VariableNameValuePair
        {
            [SerializeField] private string _key;
            [SerializeField] private string _value;

            public string Key => _key;
            public string Value => _value;
        }
        
        [SerializeField] private List<VariableNameValuePair> _variables;

        public IReadOnlyList<VariableNameValuePair> Variables => _variables.AsReadOnly();
    }
}

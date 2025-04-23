using UnityEngine;

public class MaterialValueDriverVector2 : MonoBehaviour
{
    [SerializeField]
    private Material _targetMaterial;

    [SerializeField]
    private string[] _parameterNames;

    [SerializeField]
    private Vector2[] _deltaValues;     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_parameterNames.Length != _deltaValues.Length)
        {
            throw new System.Exception("Parameter names and values length don't match. This is not allowed.");   
        } 
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _parameterNames.Length; i++)
        {
            var parameterName = _parameterNames[i];
            _targetMaterial.SetVector(parameterName, (Vector2)_targetMaterial.GetVector(parameterName) + (_deltaValues[i] * Time.deltaTime)); 
        }    
    }
}

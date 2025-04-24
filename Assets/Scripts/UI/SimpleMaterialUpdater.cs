using UnityEngine;

public class SimpleMaterialUpdater : MonoBehaviour
{
    public Material MaterialToUpdate;
    public string NameOfProperty;
    public float PropertyValue = 0f;


    // Update is called once per frame
    void Update()
    {
        if(MaterialToUpdate is not null)
        {
            MaterialToUpdate.SetFloat(NameOfProperty, PropertyValue);
        }        
    }
}

using UnityEngine;

public class Resident : MonoBehaviour, IContactable
{
    public string contactableName = "Resident";

    public void Contact()
    {
        Debug.Log("Contact with " + contactableName);
    }

    public string GetContactableName()
    {
        return contactableName;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}

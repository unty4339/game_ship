using UnityEngine;

public interface IContactable
{
    void Contact();
    string GetContactableName();
    GameObject GetGameObject();
}

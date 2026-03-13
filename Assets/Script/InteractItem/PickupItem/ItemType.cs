using UnityEngine;

public enum ItemType
{
    PhysicsOnly,
    Crowbar,
    StunGun,
    doorID,
    Key
}

public class Item : MonoBehaviour
{
    public string itemName;
    public ItemType itemType;
    public bool isUsable = true;
    public string keyID = "";
}
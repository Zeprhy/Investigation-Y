using UnityEngine;

public enum ItemType
{
    PhysicsOnly,
    Key,
    StunGun
}

public class Item : MonoBehaviour
{
    public string itemName;
    public ItemType itemType;
    public bool isUsable = true;
}
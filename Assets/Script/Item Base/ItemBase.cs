using UnityEngine;

public class ItemBase : MonoBehaviour
{
    [Header("Base Item Information")]
    public string itemName;
    public ItemType itemType;
    public bool isUsable = true;

    // just for a while
    public string KeyID;

    public virtual void OnEquip() { }
    public virtual void OnDrop() { }
    public virtual void UseItem() { }
}

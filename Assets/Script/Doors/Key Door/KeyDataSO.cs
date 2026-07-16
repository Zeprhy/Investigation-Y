using UnityEngine;

[CreateAssetMenu(fileName = "New Key Data", menuName = "Game Data/Key Data")]
public class KeyDataSO : ScriptableObject
{
    public string keyID;
    public string keyName;
    public Sprite keyIcon;
    public GameObject KeyPrefabs;
}
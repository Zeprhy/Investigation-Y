using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{

    public Dictionary<string, bool> debrisFallenStatus;
    public GameData()
    {
        this.debrisFallenStatus = new Dictionary<string, bool>();
    }
}

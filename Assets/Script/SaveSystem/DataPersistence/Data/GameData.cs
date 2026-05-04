using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
    public class DebrisData {
            public string id;
            public bool hasTriggered;

            public DebrisData(string id, bool hasTriggered) {
                this.id = id;
                this.hasTriggered = hasTriggered;
            }
        }

public class GameData
{
    public List<DebrisData> debrisListData = new List<DebrisData>();
    
    public GameData()
    {

    }
}

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


[System.Serializable]
public class AudioSettings
{
    public float masterVolume = 0.75f;
    public float musicVolume = 0.75f;
    public float sfxVolume = 0.75f;
    public float ambientVolume = 0.75f;

    public AudioSettings()
    {
        // Default values already set above
    }
}

public class GameData
{
    public List<DebrisData> debrisListData = new List<DebrisData>();
    public AudioSettings audioSettings = new AudioSettings();
    
    public GameData()
    {
         audioSettings = new AudioSettings();
    }
}

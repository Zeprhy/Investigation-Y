using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "HorrorGame/Objective")]
public class ObjectiveSO : ScriptableObject
{
    public string objectiveID;
    [TextArea] public string Description;
    public bool iscompleted = false;

    public void Reset() => iscompleted = false;
}

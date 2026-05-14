using UnityEngine;

public class PowerSystem : MonoBehaviour
{
    public static bool Instance {get; private set; }
    public static bool IsPowerOn = true;

    private void Awake()
    {
        IsPowerOn = true;
    }

    public static void CutPower()
    {
        IsPowerOn = false;
    }

    public static void RestorePower()
    {
        IsPowerOn = true;
    }
}

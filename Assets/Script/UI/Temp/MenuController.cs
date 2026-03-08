using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
   public RectTransform subPanel;
   public Animator animator;
   private string currentActiveMenu = "None";
   private bool isSwitching = false;

  public void ClickSettings() {
        StartCoroutine(SwitchMenuProcess("ShowSettings", "Settings"));
    }

    public void ClickConfirm() {
        StartCoroutine(SwitchMenuProcess("ShowSmall", "Confirm"));
    }

    public void ClickResume() {
        animator.SetTrigger("HideAll");
        currentActiveMenu = "None";
    }

    public void ResetMenuState()
    {
        StopAllCoroutines();
        currentActiveMenu = "None";
    }

    IEnumerator SwitchMenuProcess(string targetTrigger, string menuName)
    {
        if (isSwitching || currentActiveMenu == menuName) yield break;
        isSwitching = true;

        if(currentActiveMenu != "None")
        {
        animator.SetTrigger("HideAll");
        yield return new WaitForSecondsRealtime(0.25f);
        }
        
        animator.SetTrigger(targetTrigger);
        currentActiveMenu = menuName;

        isSwitching = false;
    }
}

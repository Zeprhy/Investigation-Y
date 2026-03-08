using System.Collections;
using UnityEngine;

public class MenuController : MonoBehaviour
{
   public RectTransform subPanel;
   public Animator animator;
   private string currentActiveMenu = "None";

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

    IEnumerator SwitchMenuProcess(string targetTrigger, string menuName)
    {
        if (currentActiveMenu == menuName)
        yield break;
        if(currentActiveMenu != "None")
        {
        animator.SetTrigger("HideAll");
        yield return new WaitForSeconds(0.2f);
        }
        
        animator.SetTrigger(targetTrigger);
        currentActiveMenu = menuName;
    }
}

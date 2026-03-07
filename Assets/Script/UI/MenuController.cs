using UnityEngine;

public class MenuController : MonoBehaviour
{
   public RectTransform subPanel;
   public Animator animator;

   public void OpenSettings()
    {
        animator.SetTrigger("ShowSettings");
    }
    public void OpenSmallPanel()
    {
        animator.SetTrigger("ShowSmall");
    }
    public void CloseAllPanel()
    {
        animator.SetTrigger("HideAll");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    public List<CanvasGroup> pageList;
    public CanvasGroup currentPage;

    void Start()
    {
        CloseAllPages();
        OpenPage(0);
    }

    public void CloseAllPages()
    {
        foreach (CanvasGroup cg in pageList)
        {
            ClosePage(cg);
        }
        currentPage = null;
    }

    public void OpenPage(int index)
    {
        if (currentPage != null)
        {
            ClosePage(currentPage);
        }

        if (index > -1 && index < pageList.Count)
        {
            CanvasGroup cg = pageList[index];
            if (cg != null)
            {
                cg.alpha = 1;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                currentPage = cg;
            }
        }
    }

    public void ClosePage(int index)
    {
        if (index > -1 && index < pageList.Count)
        {
            CanvasGroup cg = pageList[index];
        }
    }

    public void ClosePage(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        currentPage = null;
    }
}

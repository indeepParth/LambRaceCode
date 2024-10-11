using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPopUp : UIScreen
{

    public Image backroundPanel;
    public RectTransform popuop;

    [Header("Optional")]
    public RectTransform closeButton;
    public bool autoShowAnimate = true;
    public float autoBackgroundAlpha = 1;
    public virtual void OnEnable()
    {
        if (autoShowAnimate)
        {
            ShowPopUpMenu(autoBackgroundAlpha);
        }

    }

    #region ShowPopUp

    public void ShowPopUpMenu(float backgroundFade = 1f)
    {
        ShowPopUpMenu(backgroundFade, null);
    }

    public void ShowPopUpMenu(float backgroundFade = 1f, UIVoid callback = null)
    {
        //if (gameObject.activeSelf) return;
        // OnUIScreenBlendAnimationComplete = callback;
        backroundPanel.DOFade(0, 0);
        popuop.gameObject.SetActive(false);
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
            closeButton.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0);
        }
        popuop.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0);
        gameObject.SetActive(true);
        backroundPanel.DOFade(backgroundFade, backgroundFade == 0 ? 0 : 0.2f).OnComplete(() =>
              {
                  popuop.gameObject.SetActive(true);
                  popuop.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
                  {
                      if (callback != null)
                          callback();
                  });
                  if (closeButton != null)
                  {
                      closeButton.gameObject.SetActive(true);
                      closeButton.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
                  }
              });
    }

    public void ShowPopUp()
    {
        ShowPopUp(null);
    }

    public void ShowPopUp(UIVoid callback = null)
    {
        popuop.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0).OnComplete(() =>
        {
            popuop.gameObject.SetActive(true);
            popuop.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        });
        //popuop.gameObject.SetActive(true);
        //popuop.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
        //{
        //    if (callback != null)
        //        callback();
        //});
    }
    #endregion

    #region HidePopUp

    public void HidePopUpMenu()
    {
        HidePopUpMenu(null);
    }

    public void HidePopUpMenu(UIVoid callback = null)
    {
        if (!gameObject.activeSelf) return;
        // OnUIScreenBlendAnimationComplete = callback;
        float amount = 0.8f;
        popuop.DOScale(Vector3.one, 0);
        popuop.DOScale(Vector3.one * amount, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popuop.gameObject.SetActive(false);
            backroundPanel.DOFade(0, 0.1f).SetDelay(0.1f).OnComplete(() =>
            {
                gameObject.SetActive(false);

                if (callback != null)
                    callback();
            });
        });

        if (closeButton != null)
        {
            closeButton.DOScale(Vector3.one * amount, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                closeButton.gameObject.SetActive(false);
            });
        }

    }
    public void HidePopUp()
    {
        HidePopUp(null);
    }
    public void HidePopUp(UIVoid callback = null)
    {
        // OnUIScreenBlendAnimationComplete = callback;
        float amount = 0.8f;
        popuop.DOScale(Vector3.one, 0);
        popuop.DOScale(Vector3.one * amount, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            popuop.gameObject.SetActive(false);
            if (callback != null)
                callback();
        });

    }
    #endregion

}
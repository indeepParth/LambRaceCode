using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Video;

public class UIScrollViewLoading : MonoBehaviour
{
    public Image bgPanel;
    public TMP_Text loadingText;
    public float duration = 1;
    private bool enable;
    public Ease easeSet = Ease.Linear;
    public Image spriteAnimation;

    public TextMeshProUGUI scrollBarEmptyText;



    public void ScrollBarEmptyText(string msgg, bool show)
    {
        scrollBarEmptyText.text = msgg;
        scrollBarEmptyText.DOFade(show ? 1 : 0, 0.2f);
    }

    public void ShowLoading(string text = "Loading", float dur = 0.2f)
    {
        if (enable)
        {
            return;
        }
        enable = true;
        loadingText.text = text;
        StartAnim(text, dur);
    }

    public void ShowTextLoading(string text = "")
    {
        loadingText.text = text;
        loadingText.DOFade(1, 0.1f);
    }

    public void UpdateTextLoading(string text = "")
    {
        loadingText.text = text;
    }

    public void HideLoading()
    {
        if (!enable)
        {
            return;
        }

        spriteAnimation.DOFade(0, 0.2f).OnComplete(() =>
        {
            bgPanel.DOFade(0, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                bgPanel.gameObject.SetActive(false);
                DOTween.Kill(gameObject.name);
            });
        });
        loadingText.DOFade(0, 0.2f);
        enable = false;
    }


    private void StartAnim(string text, float dur)
    {
        loadingText.DOFade(0, 0);
        //rawImage.DOFade(0, 0);
        spriteAnimation.DOFade(0, 0f);
        spriteAnimation.rectTransform.DOLocalRotate(Vector3.zero, 0);
        bgPanel.DOFade(0, 0).OnComplete(() =>
        {
            bgPanel.gameObject.SetActive(true);
            spriteAnimation.DOFade(1, 0.1f).SetDelay(0.1f);
            bgPanel.DOFade(0.95f, dur).SetEase(Ease.Linear).OnComplete(() => // ppp 0.9f
            {
                spriteAnimation.rectTransform.DOLocalRotate(-Vector3.forward * 360, 1.5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetId(gameObject.name).SetEase(Ease.Linear); // loading
                ShowTextLoading("Loading");
            });
        });
    }

    IEnumerator LoadingText(string text)
    {
        loadingText.DOFade(1, 0.2f);
        while (enable)
        {
            loadingText.text = text;
            yield return new WaitForSecondsRealtime(duration / 4);

            loadingText.text = text + ".";
            yield return new WaitForSecondsRealtime(duration / 4);

            loadingText.text = text + "..";
            yield return new WaitForSecondsRealtime(duration / 4);

            loadingText.text = text + "...";
            yield return new WaitForSecondsRealtime(duration / 4);
        }
    }
}

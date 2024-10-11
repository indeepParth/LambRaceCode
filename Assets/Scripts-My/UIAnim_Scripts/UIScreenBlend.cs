using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class UIScreenBlend : MonoBehaviour {
	public delegate void UISVoid ();
	public static UISVoid OnUIScreenBlendAnimationComplete;

	public static UIScreenBlend Instance;
	public Image blendImage;
	void Awake () {

		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy (gameObject);
		}
		
	}
	public void DarkInInsta () {
		blendImage.gameObject.SetActive (true);
		blendImage.DOFade (1f, 0f);
	}
	public void DarkIn (UISVoid callback = null) {
		blendImage.gameObject.SetActive (true);
		blendImage.DOFade (1f, 0.2f).SetEase (Ease.Linear).OnComplete (() => {
			if (callback != null) {
				callback ();
			}
		});
	}
	public void DarkOut (UISVoid callback = null) {
		blendImage.gameObject.SetActive (true);
		blendImage.DOFade (0f, 0.2f).SetEase (Ease.Linear).OnComplete (() => {
			blendImage.gameObject.SetActive (false);
			if (callback != null) {
				callback ();
			}
		});
	}
	public void DarkInOut (UISVoid callback = null) {
		DarkIn (() => {
			if (callback != null) {
				callback ();
			}
			DarkOut ();
		});
	}
}
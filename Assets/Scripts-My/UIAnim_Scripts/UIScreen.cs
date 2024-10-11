using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class UIScreen : MonoBehaviour {
	public delegate void UIVoid ();
	public static UIVoid OnUIScreenBlendAnimationComplete;

	#region ShowScreen

	public virtual void ShowScreen () {
		ShowScreen (null, false);
	}
	public virtual void ShowScreen (bool dontActiveCheck) {
		ShowScreen (null, dontActiveCheck);
	}
	public virtual void ShowScreen (UIVoid callback) {
		ShowScreen (callback, false);
	}
	public virtual void ShowScreen (UIVoid callback = null, bool dontActiveCheck = false) {
		if (!dontActiveCheck && gameObject.activeSelf) return;
		UIScreenBlend.Instance.DarkIn (() => {
			gameObject.SetActive (true);
			if (callback != null)
				callback ();
			UIScreenBlend.Instance.DarkOut (null);
		});
	}

	#endregion

	#region HideScreen

	public virtual void HideOutScreen () {
		HideOutScreen (null);
	}
	public virtual void HideOutScreen (UIVoid callback) {
		if (!gameObject.activeSelf) return;
		// OnUIScreenBlendAnimationComplete = callback;
		UIScreenBlend.Instance.DarkIn (() => {
			if (callback != null)
				callback ();
			gameObject.SetActive (false);
			UIScreenBlend.Instance.DarkOut (null);
		});
	}
	public virtual void HideScreen () {
		HideScreen (null);
	}
	public virtual void HideScreen (UIVoid callback) {
		if (!gameObject.activeSelf) return;
		// OnUIScreenBlendAnimationComplete = callback;
		UIScreenBlend.Instance.DarkIn (() => {
			if (callback != null)
				callback ();
			gameObject.SetActive (false);
		});
	}
	#endregion

}
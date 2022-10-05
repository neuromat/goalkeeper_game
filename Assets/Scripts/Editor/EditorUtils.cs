using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

public class EditorUtils : MonoBehaviour 
{

	[MenuItem( "Tools/Clear PlayerPrefs" )]
	static void ClearPrefs()
	{
		PlayerPrefs.DeleteAll();
		//Debug.Log("PlayerPrefs cleared!"); //Josi commented
	}

	[MenuItem("Tools/Anchors to Corners %[")]
	static void AnchorsToCorners(){
		RectTransform t = Selection.activeTransform as RectTransform;
		RectTransform pt = Selection.activeTransform.parent as RectTransform;
		
		if(t == null || pt == null) return;
		
		Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
		                                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
		Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
		                                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);
		
		t.anchorMin = newAnchorsMin;
		t.anchorMax = newAnchorsMax;
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
	}
	
	[MenuItem("Tools/Corners to Anchors %]")]
	static void CornersToAnchors(){
		RectTransform t = Selection.activeTransform as RectTransform;
		
		if(t == null) return;
		
		t.offsetMin = t.offsetMax = new Vector2(0, 0);
	}

	[MenuItem("Tools/ApplyCornerToAnchorsOnAllText")]
	static void CornersToAnchorsToAllTexts(){

		RectTransform [] transforms = Selection.activeGameObject.GetComponentsInChildren<RectTransform>();

		foreach (var t in transforms) 
		{
			var text = t.GetComponent<Text>();
			if(text != null)
			{
//				t.offsetMin = t.offsetMax = new Vector2(0, 0);
				text.alignment = TextAnchor.MiddleCenter;
			}
		}

	}

}

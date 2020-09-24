using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSizer : MonoBehaviour {

	
	
	// Update is called once per frame
	void Update ()
    {
        RectTransform textRect = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        Text text = transform.GetChild(0).gameObject.GetComponent<Text>();
        if (textRect != null && text != null && textRect.sizeDelta.y >= 20 && text.text != "")
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(520f, textRect.sizeDelta.y);
            Destroy(this);
        }
        
	}
}

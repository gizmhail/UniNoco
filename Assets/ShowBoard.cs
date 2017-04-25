using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Noco;
using EasyCaching;

public class ShowBoard : MonoBehaviour {
	public Image coverIMage;
	public Text titleText;
	public NocoShow show;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadShow(NocoShow show){
		this.show = show;
		titleText.text = show.show_TT;
	}

	public bool IsCoverCached(){
		CachedWWW coverRequest = new CachedWWW (show.screenshot_1024x576, 3600*24, "Show_"+show.id_show);
		return coverRequest.IsCached ();
	}

	public IEnumerator LoadShowAdditionalInfo(){
		Debug.Log ("LoadShowAdditionalInfo: "+show.screenshot_1024x576);
		CachedWWW coverRequest = new CachedWWW (show.screenshot_1024x576, 3600*24, "Show_"+show.id_show);
		yield return coverRequest.Load();
		Texture2D texture = coverRequest.www.texture;
		Debug.Log (texture);
		coverIMage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
	}
}

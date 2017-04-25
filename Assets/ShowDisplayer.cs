using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Noco;

public class ShowDisplayer : MonoBehaviour {
	public GameObject showBoardPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DisplayShows(NocoShowsSet showSet, Vector3 center, bool loadImages = false){
		StartCoroutine (LaunchDisplayShows(showSet, center, loadImages));
	}

	public IEnumerator LaunchDisplayShows(NocoShowsSet showSet, Vector3 center, bool loadImages = false){
		Vector3 position = center;
		float step = 0.0f;
		float scale = 0.3f;
		float spraying = 0.20f;
		float initialSpraying = 3f;
		float itemByTurn = 3;
		List<ShowBoard> boards = new List<ShowBoard> ();
		foreach(NocoShow show in showSet.shows){
			GameObject showBoardObject = Instantiate (showBoardPrefab);
			showBoardObject.transform.localScale = new Vector3 (scale, scale, scale);
			showBoardObject.transform.position = position;
			position = center;
			float angle = step * Mathf.PI / itemByTurn;
			float radius = step * spraying;
			if (step > 0) {
				radius += initialSpraying;
			}
			position = new Vector3 (
				center.x + radius*Mathf.Cos(angle), 
				center.y + radius*Mathf.Sin(angle), 
				center.z);
			step++;
			itemByTurn += 1.0f/9.0f;
			ShowBoard showBoard = showBoardObject.GetComponent<ShowBoard> ();
			showBoard.LoadShow (show);
			boards.Add (showBoard);
		}
		int fetched = 0;
		int simultaneousFetch = 5;
		int waitDurationBetweenGroupFetch = 1;
		foreach (ShowBoard showBoard in boards) {
			bool imageCached = showBoard.IsCoverCached ();
			if (loadImages || imageCached ) {
				yield return showBoard.LoadShowAdditionalInfo ();
				if (!imageCached) {
					fetched++;
					if (fetched >= simultaneousFetch) {
						fetched = 0;
						yield return new WaitForSeconds (waitDurationBetweenGroupFetch);
					}
				}
			}
		}		
	}
}

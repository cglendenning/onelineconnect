using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextM : MonoBehaviour {

	private TextMesh tm;
	private List<string> texts;
	private int index;
	private float duration;
	private float startTime;
	// Use this for initialization
	void Awake () {
		texts = new List<string> ();
		tm = GetComponent<TextMesh> ();


		duration = 0.8f;//after second change
		startTime = Time.time;

	}
	
	// Update is called once per frame
	void Update () {

		float timePassed = Time.time - startTime;
		if (timePassed > duration) {

			changeText ();
		}
		
	}

	public void addString(string str){

		texts.Add (str);
		changeText ();
	}

	public void changeText(){

		startTime = Time.time;

		if (texts.Count == 0)
			return;
		
		index++;

		if (index > (texts.Count - 1)) {

			index = 0;
		}

		tm.text = texts [index];
	}
}

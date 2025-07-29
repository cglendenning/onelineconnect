using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBackground : MonoBehaviour {

	public float scrollSpeed;
	private Vector2 savedOffset;

	void Start ()
    {
		savedOffset = GetComponent<Renderer>().material.GetTextureOffset ("_MainTex");
	}

	void Update ()
    {
		float x = Mathf.Repeat (Time.time * scrollSpeed, 1);
		Vector2 offset = new Vector2 (x , savedOffset.y);
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
	}
}

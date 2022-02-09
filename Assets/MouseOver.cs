using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOver : MonoBehaviour
{
	private Color basicColor = Color.grey;
	private Color hoverColor = Color.red;

	void Start()
	{
		GetComponent<Renderer>().material.color = basicColor;
	}

	void OnMouseEnter()
	{
		GetComponent<Renderer>().material.color = hoverColor;
		transform.localScale += new Vector3(0.2F,0.2F,0.2F);
	}

	void OnMouseExit()
	{
		GetComponent<Renderer>().material.color = basicColor;
		transform.localScale -= new Vector3(0.2F, 0.2F, 0.2F);
	}
}

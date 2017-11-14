using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickBehavior : MonoBehaviour {

	void OnMouseOver(){
		if(Input.GetMouseButtonDown(0)){
			print("Clicked " + gameObject.name);
		}
	}
}

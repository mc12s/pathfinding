using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMouseMovement : MonoBehaviour {

	public GameObject player_pre;
	public GameObject target_pre;
	public float H_scale = 1f;
	private Camera MainCam;

	private GameObject player = null; 
	private GameObject target = null; 
	private GameObject target_icon = null; 
	int listindex = 0;
	List<Vector2> movelist = new List<Vector2>();
	List<GameObject> targetlist = new List<GameObject>();

	// Use this for initialization
	void Start () {
		MainCam = Camera.main;
	}

	void Update(){

		if(Input.GetMouseButtonDown(0)){// && !player){
			if(player){
				Destroy(player.gameObject);
			}
			Vector2 mouse_pos = MainCam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			player = Instantiate(player_pre, mouse_pos, Quaternion.identity);
			print(mouse_pos.x + ", " + mouse_pos.y);
		}

		if(Input.GetKeyDown(KeyCode.LeftShift)){
			movelist.Clear();
		}

		if(Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1) && player){
			Vector2 mouse_pos = MainCam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			target = Instantiate(target_pre, mouse_pos, Quaternion.identity);
			movelist.Add(target.transform.position);
			targetlist.Add(target);
		}

		else if(Input.GetMouseButtonDown(1) && player){
			Vector2 mouse_pos = MainCam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			print(mouse_pos.x + ", " + mouse_pos.y);
			Destroy(target);
			target = Instantiate(target_pre, mouse_pos, Quaternion.identity);
			movelist.Clear();
			movelist.Add(target.transform.position);
			targetlist.Add(target);

		}

		if( ( Input.GetMouseButtonUp(1) && !Input.GetKey(KeyCode.LeftShift) )|| Input.GetKeyUp(KeyCode.LeftShift)){
			listindex = 0;
			StartCoroutine(TravelToTarget(movelist, targetlist, listindex));
		}		
	}


	IEnumerator TravelToTarget(List<Vector2> movelist, List<GameObject> targetlist, int listindex){
		Vector2 player_pos = player.transform.position;
		float sqrRemainingDistance = (player_pos - movelist[listindex]).sqrMagnitude;

		while(listindex<=movelist.Count){
			Vector2 dir = player_pos - movelist[listindex];
			float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
			player.transform.rotation = Quaternion.AngleAxis(angle-180, Vector3.back);

			player.transform.position = Vector2.MoveTowards(player.transform.position, movelist[listindex], 5f * Time.deltaTime);
			//print("moving... " + player_pos + " to " + movelist[listindex]);
			player_pos = player.transform.position;
			sqrRemainingDistance = (player_pos - movelist[listindex]).sqrMagnitude;
			if(sqrRemainingDistance<=float.Epsilon){
				player_pos = player.transform.position;
				sqrRemainingDistance = (player_pos - movelist[listindex]).sqrMagnitude;
				Destroy(targetlist[listindex]);
				listindex++;
			}
			yield return null;
		}
	}
}

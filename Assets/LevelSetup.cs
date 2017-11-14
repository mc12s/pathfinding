using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetup : MonoBehaviour {

	public int rows = 8;
	public int cols = 16;
	public GameObject walls;
	public GameObject player_pre;
	public GameObject target_pre;
	public Transform WallContainer;
	public float H_scale = 1f;
	private Camera MainCam;

	private GameObject player = null; 
	private GameObject target = null; 
	private GameObject target_icon = null; 
	int listindex = 0;
	List<Vector2> movelist = new List<Vector2>();

	// Use this for initialization
	void Start () {

		MainCam = Camera.main;

		for(int i=0; i<=cols; i++){
			for(int j=0; j<=rows; j++){
				GameObject tile = Instantiate(walls, new Vector2(i, j), Quaternion.identity);
				tile.transform.SetParent(WallContainer);
				foreach(Transform child in tile.transform){
					if(Random.Range(0,10)<7){
						child.gameObject.SetActive(false);
					}

				}
			}
		}
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

		else if(Input.GetMouseButtonDown(1) && player){
			Vector2 mouse_pos = MainCam.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
			print(mouse_pos.x + ", " + mouse_pos.y);
			Destroy(target_icon);
			target = Instantiate(target_pre, mouse_pos, Quaternion.identity);
			movelist.Clear();
		}

		if(target){
			bool target_found = false;
			Vector2 start = new Vector2(Mathf.Round(player.transform.position.x), Mathf.Round(player.transform.position.y));
			Vector2 target_loc = new Vector2(Mathf.Round(target.transform.position.x), Mathf.Round(target.transform.position.y));
			Dictionary<Vector2, int[]> closed = new Dictionary<Vector2, int[]>();
			Dictionary<Vector2, int[]> open = new Dictionary<Vector2, int[]>();
			Dictionary<Vector2, Vector2> came_from = new Dictionary<Vector2, Vector2>();

			int G = 0;
			int F = G + (int)(target_loc.x-start.x)+(int)(target_loc.y-start.y);
			int[] S = {G,F};
			open.Add(start, S);
			came_from.Add(start, new Vector2(-9999, -9999));
			Vector2 cur_space = start;

			while(open.Count!=0){
				FindOpenSpace(cur_space, ref closed, ref open, G, target_loc, ref came_from);
				int min = 999;
				foreach (KeyValuePair<Vector2, int[]> kvp in open) {
					//print(cur_space.x + " " + cur_space.y + " G:" + G + " F:" + kvp.Value[1]);
			        if (kvp.Value[1] <= min){
			            cur_space = kvp.Key;
        				min = kvp.Value[1]; //F score
        				G = kvp.Value[0]; //steps from start
        				print(cur_space.x + " " + cur_space.y + " G:" + G + " F:" + kvp.Value[1]);
    				}
				}
				if(cur_space == target_loc){
					movelist.Add(cur_space);
					target_found = true;
					print("Target Found");
					open.Clear();
					closed.Clear();
				}
				print("Open Count:" + open.Count);
			}


			if(target_found){
				while(came_from[cur_space]!=new Vector2(-9999, -9999)){
					cur_space=came_from[cur_space];
					movelist.Add(cur_space);
					listindex = movelist.Count-2;
				}
			}
			else{
				print("No path to target");
			}
			target_icon = Instantiate(target_pre, target.transform.position, Quaternion.identity);
			Destroy(target.gameObject);
			//target = null;
		}



		if(Input.GetMouseButtonDown(2) || Input.GetButtonDown("Jump")){
			if(listindex>0){
				StartCoroutine(TravelToTarget(movelist, listindex));
				Destroy(target_icon.gameObject);
			}
		}		

	}


	void FindOpenSpace(Vector2 cur_space, ref Dictionary<Vector2, int[]> closed, ref Dictionary<Vector2, int[]> open, int G, Vector2 target, ref Dictionary<Vector2, Vector2> came_from){

		closed.Add(cur_space, open[cur_space]);
		open.Remove(cur_space);

		Vector2[] cardinal_dirs = {Vector2.up, Vector2.right, Vector2.down, Vector2.left};
		foreach (Vector2 dir in cardinal_dirs){		
			Vector2 dest = cur_space+dir;
			RaycastHit2D hit = Physics2D.Linecast(cur_space, dest);		//check if there's a wall in the way

			if(hit.collider){
				if(hit.collider.CompareTag("target")){
					open.Add(dest,new int[2]{G+1,0});
					came_from.Add(dest, cur_space);
					return;
				}
			}
			if(!hit.collider){					//if not calculate the F score and add it to the open list

				Debug.DrawLine(cur_space, dest, Color.red, 5);
				if(!closed.ContainsKey(dest)){	//if the space is not already in the closed list
					int H = (int)(Mathf.Abs(target.x-dest.x)+Mathf.Abs(target.y-dest.y));	//manhattan norm
					//print(H);
					int F = (G+1) +  H;

					if(!open.ContainsKey(dest)){ //not in the open list
						open.Add(dest, new int[2]{G+1,F});
						came_from.Add(dest, cur_space);						
					}

					else{	//update the open list score with the new value if it's less than the stored value
						int[] val;
						val = open[dest];
						if(val[1]>=F){
							open[dest] = new int[2]{(G+1),F};
							came_from[dest] = cur_space;
						}
					}

				}
			}
		}
		//foreach(KeyValuePair<Vector2, int[]> kvp in open){
		//	print("open spaces:" + kvp.Key.x + " " + kvp.Key.y + " G:");
		//}
		//foreach(KeyValuePair<Vector2, int[]> kvp in closed){
		//	print("closed spaces:" + kvp.Key.x + " " + kvp.Key.y);
		//}

	}



	IEnumerator TravelToTarget(List<Vector2> movelist, int listindex){
		Vector2 player_pos = player.transform.position;
		float sqrRemainingDistance = (player_pos - movelist[listindex]).sqrMagnitude;

		while(listindex>=0){
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
				listindex--;
			}
			yield return null;
		}
	}



}

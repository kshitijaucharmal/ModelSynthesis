using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEditor.Callbacks;

public class ContainerScript : MonoBehaviour {

	[SerializeField] private Outline outline;
	[SerializeField] private new Renderer renderer;
	[SerializeField] private Canvas inventoryCanvas;
	[SerializeField] private TMP_Text inventoryText;
	
	[HideInInspector] public bool isCollapsed = false;
	[HideInInspector] public HashSet<int> allowedTiles = new HashSet<int>();

	void Start(){
		outline.enabled = false;
		renderer.enabled = true;
		inventoryCanvas.enabled = false;
	}

	public void AddAllTiles(int tileCount){
		for(int i = 0; i < tileCount; i++){
			allowedTiles.Add(i);
		}
	}

	public void AddValidTiles(HashSet<int> nowAllowed, GameObject[] tiles){
		if(isCollapsed) return;
		allowedTiles.RemoveWhere(item => !nowAllowed.Contains(item));
		int left = allowedTiles.Count;
		if(left == 0){
			Debug.Log("Empty");
		}
		if(left == 1){
			Collapse(tiles, -1);
		}
		UpdateInventory();
	}

	void OnMouseEnter(){
		outline.enabled = true;
		inventoryCanvas.enabled = true;
	}
	void OnMouseExit(){
		outline.enabled = false;
		inventoryCanvas.enabled = false;
	}

	public void UpdateInventory(){
		inventoryText.text = string.Join(",", allowedTiles);
	}

	public void Collapse(GameObject[] tiles, int tileIndex){
		if(tileIndex == -1){
			var poss = allowedTiles.ToArray();
			if (poss.Length == 1) tileIndex = poss[0];
			else if(poss.Length == 0) return;
			else tileIndex = poss[Random.Range(0, poss.Length)];
		}
		// Disable rendering for this container
		renderer.enabled = false;
		// Set isCollapsed = true
		isCollapsed = true;
		// Set only allowedd tile to be tileIndex
		allowedTiles.Clear();
		allowedTiles.Add(tileIndex);
		UpdateInventory();
		// Spawn the new tile
		if(tiles[tileIndex] == null) return;
		Instantiate(tiles[tileIndex], transform.position, Quaternion.identity, transform.parent);
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MSManager : MonoBehaviour {

	// Index in this array represent Values in map
	[SerializeField] private GameObject[] tiles;
	[SerializeField] private GameObject containerTile;

	[Header("Map Dimensions")]
	[SerializeField] private int iMax = 4;
	[SerializeField] private int jMax = 4;
	[SerializeField] private int kMax = 4;

	private int[,,] sampleMap;

	// Jagged Array Containing All Rules
	// Hierarcy tileType -> direction -> allowedTiles (Hashset)
	private HashSet<int>[,] rules;

	private int[,,] incompleteMap;
	private ContainerScript[,,] containers;

	// Start is called before the first frame update
	void Start() {
		// Initilize
		sampleMap = new int[iMax, jMax, kMax];
		incompleteMap = new int[iMax, jMax, kMax];
		containers = new ContainerScript[iMax, jMax, kMax];

		// Sample Map
		sampleMap[2, 3, 2] = 1;
		sampleMap[2, 2, 2] = 2;
		sampleMap[2, 1, 2] = 2;
		sampleMap[2, 0, 2] = 3;
		GenerateMap("SampleMap", sampleMap, Vector3.zero);

		// Main Map
		for(int i = 0; i < incompleteMap.GetLength(0); i++){
			for(int j = 0; j < incompleteMap.GetLength(1); j++){
				for(int k = 0; k < incompleteMap.GetLength(2); k++){
					incompleteMap[i, j, k] = -1;
				}
			}
		}
		Vector3 offset = new Vector3(5, 0, 0);
		GenerateMap("MainMap", incompleteMap, offset);

		// Calculate Rules from sample
		CalculateNeighborRules();

		// Print Rules (For Debugging)
		PrintRules();
	}
	
	void CollapseTile(int i, int j, int k, int tileIndex){
		ContainerScript tile = containers[i,j,k];
		if(tile == null){
			// Not empty
			return;
		}

		// Collapse the tile
		tile.Collapse(tiles, tileIndex);

		// Clean
		CleanNeighbors(i, j, k);
	}

	// This is gonna be a recursive function
	void CleanNeighbors(int i, int j, int k){
		ContainerScript tile = containers[i,j,k];
		if(tile == null){
			// Not empty
			return;
		}

		ContainerScript up, down, right, left, forward, backward;
		
		// Get Neighbors
		if (j > 0)      up          = containers[i, j - 1, k];
		if (j < jMax-1) down        = containers[i, j + 1, k];
		if (i < iMax-1) right       = containers[i + 1, j, k];
		if (i > 0)      left        = containers[i - 1, j, k];
		if (k < kMax-1) forward     = containers[i, j, k + 1];
		if (k > 0)      backward	= containers[i, j, k - 1];
	}

	// Generate Map Based on map matrix
	void GenerateMap(string mapName, int[,,] map, Vector3 offset){
		Transform mapParent = Instantiate(new GameObject(name:mapName), transform).transform;

		for(int i = 0; i < map.GetLength(0); i++){
			for(int j = 0; j < map.GetLength(1); j++){
				for(int k = 0; k < map.GetLength(2); k++){

					int tileIndex = map[i, j, k];
					Vector3 pos = new Vector3(i + offset.x, j + offset.y, k + offset.z);
					Quaternion rot = Quaternion.identity;
					
					if (tileIndex < 0){
						// Create tile, parent to mapParent
						ContainerScript tile = Instantiate(containerTile, pos, rot, mapParent).GetComponent<ContainerScript>();
						containers[i,j,k] = tile;
					}
					else{
						if(tiles[tileIndex] == null) continue;
						Instantiate(tiles[tileIndex], pos, rot, mapParent);
					}
				}
			}
		}
		
	}

	// Calculate Rules based on sample map
	void CalculateNeighborRules(){
		rules = new HashSet<int>[tiles.Length, 6];
		
		// Initialize Hashsets
		for(int i = 0; i < rules.GetLength(0); i++){
			for(int j = 0; j < rules.GetLength(1); j++){
				rules[i, j] = new HashSet<int>();
			}
		}

		for(int i = 0; i < iMax; i++){
			for(int j = 0 ; j < jMax; j++){
				for(int k = 0 ; k < kMax; k++){
					int tileIndex = sampleMap[i, j, k];

					if (tileIndex < 0){
						continue;
					}
					
					int up, down, right, left, forward, backward;

					if (j > 0){
						up = sampleMap[i, j - 1, k];
						rules[tileIndex, 0].Add(up);
					}
					if (j < jMax-1){
						down = sampleMap[i, j + 1, k];
						rules[tileIndex, 1].Add(down);
					}
					if (i < iMax-1){
						right = sampleMap[i + 1, j, k];
						rules[tileIndex, 2].Add(right);
					}
					if (i > 0){
						left = sampleMap[i - 1, j, k];
						rules[tileIndex, 3].Add(left);
					}
					if (k < kMax-1){
						forward = sampleMap[i, j, k + 1];
						rules[tileIndex, 4].Add(forward);
					}
					if (k > 0){
						backward = sampleMap[i, j, k - 1];
						rules[tileIndex, 5].Add(backward);
					}
				}
			}
		}
	}

	// Print Rules for each Tile
	void PrintRules(){
		for(int i = 0; i < rules.GetLength(0); i++){
			StringBuilder tileRules = new StringBuilder();
			tileRules.Append("Tile " + i + ": { ");
			for(int j = 0; j < rules.GetLength(1); j++){
				tileRules.Append(j + ": ( " + string.Join(",", rules[i, j]) + " ), ");
			}
			tileRules.Append(" }");
			Debug.Log(tileRules.ToString());
		}
	}
}

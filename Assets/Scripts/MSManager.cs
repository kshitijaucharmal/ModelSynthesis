using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MSManager : MonoBehaviour {

	// Index in this array represent Values in map
	[SerializeField] private GameObject[] tiles;
	[SerializeField] private GameObject containerTile;

	[Header("Map Dimensions")]
	[SerializeField] private Vector3Int sampleDim = new Vector3Int(4, 4, 4);
	[SerializeField] private Vector3Int mainDim = new Vector3Int(6, 6, 6);

	private int[,,] sampleMap;

	// Jagged Array Containing All Rules
	// Hierarcy tileType -> direction -> allowedTiles (Hashset)
	private HashSet<int>[,] rules;

	private int[,,] incompleteMap;
	private bool[,,] visitedMap;
	private ContainerScript[,,] containers;

	// Start is called before the first frame update
	void Start() {
		// Initialize
		sampleMap = new int[sampleDim.x, sampleDim.y, sampleDim.y];

		incompleteMap = new int[mainDim.x, mainDim.y, mainDim.z];
		visitedMap = new bool[mainDim.x, mainDim.y, mainDim.z];
		containers = new ContainerScript[mainDim.x, mainDim.y, mainDim.z];

		// Sample Map
		sampleMap[2, 3, 2] = 1;
		sampleMap[2, 2, 2] = 2;
		sampleMap[2, 1, 2] = 2;
		sampleMap[2, 0, 2] = 3;
		GenerateMap("SampleMap", sampleMap, offset:Vector3.zero);

		// Main Map
		for(int i = 0; i < incompleteMap.GetLength(0); i++){
			for(int j = 0; j < incompleteMap.GetLength(1); j++){
				for(int k = 0; k < incompleteMap.GetLength(2); k++){
					incompleteMap[i, j, k] = -1;
				}
			}
		}
		Vector3 offset = new Vector3(sampleDim.x + 1, 0, 0);
		GenerateMap("MainMap", incompleteMap, offset);

		// Calculate Rules from sample
		CalculateNeighborRules();

		// Print Rules (For Debugging)
		PrintRules();

		// Collapse 
		for(int x = 0; x < mainDim.x; x++){
			for(int y = 0; y < mainDim.y; y++){
				for(int z = 0; z < mainDim.z; z++){
					CollapseTile(x, y, z, -1);
				}
			}
		}
	}

	// Vector3Int lowestEntropyTile(){
		
	// }
	
	void CollapseTile(int i, int j, int k, int tileIndex){
		ContainerScript tile = containers[i,j,k];
		if(tile == null || tile.isCollapsed){
			// Not empty
			return;
		}

		// Collapse the tile
		tile.Collapse(tiles, tileIndex);

		// Clean
		CleanNeighbors(i, j, k);
		
		// Reset the visited map
		visitedMap = new bool[mainDim.x, mainDim.y, mainDim.z];

	}

	// This is gonna be a recursive function
	void CleanNeighbors(int i, int j, int k){
		ContainerScript tile = containers[i,j,k];
		if(tile.allowedTiles.Count == 0 || visitedMap[i,j,k]){
			// Already Collapsed
			return;
		}
		visitedMap[i,j,k] = true;

		int[] possibleStates = tile.allowedTiles.ToArray();

		ContainerScript up = null, 
						down = null, 
						right = null, 
						left = null, 
						forward = null, 
						backward = null;

		// Get Neighbors
		if (j < mainDim.y-1) up        	= containers[i, j + 1, k];
		if (j > 0)      down        = containers[i, j - 1, k];
		if (i < mainDim.x-1) right       = containers[i + 1, j, k];
		if (i > 0)      left        = containers[i - 1, j, k];
		if (k < mainDim.z-1) forward     = containers[i, j, k + 1];
		if (k > 0)      backward	= containers[i, j, k - 1];

		// Calculate Possibilties
		if(up != null){
			var r = CalculateRemovals(possibleStates, 1);
			up.AddValidTiles(r, tiles);
			CleanNeighbors(i, j + 1, k);
		}
		if(down != null){
			var r = CalculateRemovals(possibleStates, 0);
			down.AddValidTiles(r, tiles);
			CleanNeighbors(i, j - 1, k);
		}
		if(right != null){
			var r = CalculateRemovals(possibleStates, 3);
			right.AddValidTiles(r, tiles);
			CleanNeighbors(i + 1, j, k);
		}
		if(left != null){
			var r = CalculateRemovals(possibleStates, 2);
			left.AddValidTiles(r, tiles);
			CleanNeighbors(i - 1, j, k);
		}
		if(forward != null){
			var r = CalculateRemovals(possibleStates, 5);
			forward.AddValidTiles(r, tiles);
			CleanNeighbors(i, j, k + 1);
		}
		if(backward != null){
			var r = CalculateRemovals(possibleStates, 4);
			backward.AddValidTiles(r, tiles);
			CleanNeighbors(i, j, k - 1);
		}

	}

	HashSet<int> CalculateRemovals(int[] possibleStates, int dir){
		HashSet<int> poss = new HashSet<int>();
		foreach(int t in possibleStates){
			var p = rules[t, dir];
			foreach(int f in p){
				poss.Add(f);
			}
		}
		return poss;
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
						tile.AddAllTiles(tiles.Length);
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

		for(int i = 0; i < sampleDim.x; i++){
			for(int j = 0 ; j < sampleDim.y; j++){
				for(int k = 0 ; k < sampleDim.z; k++){
					int tileIndex = sampleMap[i, j, k];

					if (tileIndex < 0){
						continue;
					}
					
					int up, down, right, left, forward, backward;

					if (j < sampleDim.y-1){
						up = sampleMap[i, j + 1, k];
						rules[tileIndex, 1].Add(up);
					}
					if (j > 0){
						down = sampleMap[i, j - 1, k];
						rules[tileIndex, 0].Add(down);
					}
					if (i < sampleDim.x-1){
						right = sampleMap[i + 1, j, k];
						rules[tileIndex, 2].Add(right);
					}
					if (i > 0){
						left = sampleMap[i - 1, j, k];
						rules[tileIndex, 3].Add(left);
					}
					if (k < sampleDim.z-1){
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
		// Add an air tile where nothing present
		for(int i = 0; i < rules.GetLength(0); i++){
			for(int j = 0; j < rules.GetLength(1); j++){
				if(rules[i,j].Count == 0){
					// 0 is Air tile
					rules[i,j].Add(0);
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

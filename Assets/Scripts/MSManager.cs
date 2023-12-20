using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MSManager : MonoBehaviour {

    // Index in this array represent Values in map
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private GameObject containerTile;

    private int[,,] sampleMap = new int[4,4,4];

    // Jagged Array Containing All Rules
    // Hierarcy tileType -> direction -> allowedTiles (Hashset)
    private HashSet<int>[,] rules;

    private int[,,] incompleteMap = new int[4,4,4];
    private ContainerScript[,,] incompleteMapTransforms = new ContainerScript[4,4,4];

    // Start is called before the first frame update
    void Start() {
        // Sample Map
        sampleMap[1, 1, 1] = 1;
        sampleMap[1, 2, 1] = 2;
        sampleMap[1, 1, 2] = 3;

        // Calculate Rules from sample Map
        CalculateNeighborRules();

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
        // incompleteMap[1, 1, 2] = 1;
        GenerateMap("MainMap", incompleteMap, offset);

        // Print Rules (For Debugging)
        PrintRules();

        // Collapse Tile
        CollapseTile(1, 1, 1, 1);

    }

    void CollapseTile(int i, int j, int k, int tileIndex){
        ContainerScript tile = incompleteMapTransforms[i, j, k];
        if(tile == null){
            Debug.Log("Already Collpsed");
            return;
        }
        incompleteMap[i, j, k] = tileIndex;
        tile.Collapse(tiles, tileIndex);

        // Clean Neighbors
        CleanNeighbors(i, j, k);
    }

    void CleanNeighbors(int i, int j, int k){
        ContainerScript tile = incompleteMapTransforms[i, j, k];

        // if(tile.isVisited) return;

        int iMax = incompleteMap.GetLength(0);
        int jMax = incompleteMap.GetLength(1);
        int kMax = incompleteMap.GetLength(2);

        List<int> possibleTiles;
        if (tile != null){
            possibleTiles = new List<int>(tile.allowedTiles);
        }
        else{
            possibleTiles = new List<int> { incompleteMap[i, j, k] };
        }

        Debug.Log(string.Join(",",possibleTiles));

        for (int t = 0; t < possibleTiles.Count; t++){
            int tileIndex = possibleTiles[t];
            ContainerScript up, down, right, left, forward, backward;
            // Get Neighbors
            if (j > 0){
                up = incompleteMapTransforms[i, j - 1, k];
                up.CleanTiles(rules[tileIndex, 0], tiles);
                // Update the incompleteMap if tile is collapsed
                if(up.isCollapsed) incompleteMap[i, j, k] = up.tileNo;
                CleanNeighbors(i, j - 1, k);
            }
            if (j < jMax-1){
                down = incompleteMapTransforms[i, j + 1, k];
                down.CleanTiles(rules[tileIndex, 1], tiles);
                // Update the incompleteMap if tile is collapsed
                if(down.isCollapsed) incompleteMap[i, j, k] = down.tileNo;
                CleanNeighbors(i, j + 1, k);
            }
            if (i < iMax-1){
                right = incompleteMapTransforms[i + 1, j, k];
                right.CleanTiles(rules[tileIndex, 2], tiles);
                if(right.isCollapsed) incompleteMap[i, j, k] = right.tileNo;
                CleanNeighbors(i + 1, j, k);
            }
            if (i > 0){
                left = incompleteMapTransforms[i - 1, j, k];
                left.CleanTiles(rules[tileIndex, 3], tiles);
                if(left.isCollapsed) incompleteMap[i, j, k] = left.tileNo;
                CleanNeighbors(i - 1, j, k);
            }
            if (k < kMax-1){
                forward = incompleteMapTransforms[i, j, k + 1];
                forward.CleanTiles(rules[tileIndex, 4], tiles);
                if(forward.isCollapsed) incompleteMap[i, j, k] = forward.tileNo;
                CleanNeighbors(i, j, k + 1);
            }
            if (k > 0){
                backward = incompleteMapTransforms[i, j, k - 1];
                backward.CleanTiles(rules[tileIndex, 5], tiles);
                if(backward.isCollapsed) incompleteMap[i, j, k] = backward.tileNo;
                CleanNeighbors(i, j, k - 1);
            }
        }

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
                        ContainerScript tile = Instantiate(containerTile, pos, rot, mapParent).GetComponent<ContainerScript>();
                        for(int t = 0; t < tiles.Length; t++){
                            tile.AddAllowedTile(t);
                        }
                        incompleteMapTransforms[i,j,k] = tile;
                    }else{

                        // Create tile, parent to mapParent
                        Transform tile = Instantiate(tiles[tileIndex], pos, rot, mapParent).transform;
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

        int iMax = sampleMap.GetLength(0);
        int jMax = sampleMap.GetLength(1);
        int kMax = sampleMap.GetLength(2);

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

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MSManager : MonoBehaviour {

    // Index in this array represent Values in map
    [SerializeField] private GameObject[] tiles;
    [SerializeField] private GameObject containerTile;
    [SerializeField] private float sampleHeightInWorld = 2;
    [SerializeField] private float mainHeightInWorld = 0;

    private int[,] sampleMap = {
        {0, 0, 0, 0},
        {0, 2, 3, 0},
        {0, 1, 0, 0},
        {0, 0, 0, 0},
    };

    // Jagged Array Containing All Rules
    // Hierarcy tileType -> direction -> allowedTiles (Hashset)
    private HashSet<int>[,] rules;

    private int[,] incompleteMap = {
        {-1, -1, -1, -1},
        {-1, -1, -1, -1},
        {-1, -1,  1, -1},
        {-1, -1, -1, -1},
    };

    // Start is called before the first frame update
    void Start() {
        // Sample Map
        StartCoroutine(GenerateMap("SampleMap", sampleMap, sampleHeightInWorld));

        // Main Map
        StartCoroutine(GenerateMap("MainMap", incompleteMap, mainHeightInWorld));

        // Calculate Rules from sample
        CalculateNeighborRules();

        // Print Rules (For Debugging)
        PrintRules();
    }

    // Generate Map Based on map matrix
    IEnumerator GenerateMap(string mapName, int[, ] map, float yPos){
        Transform mapParent = Instantiate(new GameObject(name:mapName), Vector3.zero, Quaternion.identity, transform).transform;

        for(int i = 0; i < map.GetLength(0); i++){
            for(int j = 0; j < map.GetLength(1); j++){

                int tileIndex = map[i, j];
                
                GameObject model = tileIndex < 0 ? containerTile : tiles[tileIndex];
                Vector3 pos = new Vector3(i, yPos, j);
                Quaternion rot = Quaternion.Euler(90, 0, 0);

                // Create tile, parent to mapParent
                Transform tile = Instantiate(model, pos, rot, mapParent).transform;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
    }

    // Calculate Rules based on sample map
    void CalculateNeighborRules(){
        rules = new HashSet<int>[tiles.Length, 4];
        
        // Initialize Hashsets
        for(int i = 0; i < rules.GetLength(0); i++){
            for(int j = 0; j < rules.GetLength(1); j++){
                rules[i, j] = new HashSet<int>();
            }
        }

        int iMax = sampleMap.GetLength(0);
        int jMax = sampleMap.GetLength(1);

        for(int i = 0; i < iMax; i++){
            for(int j = 0 ; j < jMax; j++){
                int tileIndex = sampleMap[i, j];

                if (tileIndex < 0){
                    continue;
                }
                
                int up, down, right, left;

                if (j > 0){
                    up = sampleMap[i, j - 1];
                    rules[tileIndex, 0].Add(up);
                }
                if (j < jMax-1){
                    down = sampleMap[i, j + 1];
                    rules[tileIndex, 1].Add(down);
                }
                if (i < iMax-1){
                    right = sampleMap[i + 1, j];
                    rules[tileIndex, 2].Add(right);
                }
                if (i > 0){
                    left = sampleMap[i - 1, j];
                    rules[tileIndex, 3].Add(left);
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

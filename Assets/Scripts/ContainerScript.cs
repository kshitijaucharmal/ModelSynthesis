using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class ContainerScript : MonoBehaviour {

    [SerializeField] private Canvas canvas;
    [SerializeField] private Outline outline;
    [SerializeField] private TMP_Text allowedTilesText;

    [HideInInspector] public HashSet<int> allowedTiles = new HashSet<int>();

    [HideInInspector] public bool isVisited = false;
    [HideInInspector] public bool isCollapsed = false;
    [HideInInspector] public int tileNo = -1;

    public void AddAllowedTile(int tileIndex){
        allowedTiles.Add(tileIndex);
        allowedTilesText.text = string.Join(",", allowedTiles);
    }

    public void AddAllowedTiles(HashSet<int> tiles){
        allowedTiles = new HashSet<int>(tiles);
        allowedTilesText.text = string.Join(",", allowedTiles);
    }

    public void CleanTiles(HashSet<int> nowAllowed, GameObject[] tiles){
        if(isVisited) return;
        isVisited = true;
        var temp = new HashSet<int>(allowedTiles);
        allowedTiles.RemoveWhere(item => !nowAllowed.Contains(item));
        if (allowedTiles.Count == 0){
            Debug.Log("This is not good " + string.Join(",", nowAllowed) + " : " + string.Join(",", temp));
        }
        if (allowedTiles.Count == 1){
            Collapse(tiles, allowedTiles.ToArray()[0]);
        }
        allowedTilesText.text = string.Join(",", allowedTiles);
    }

    public void Collapse(GameObject[] tiles, int tileIndex){
        isVisited = true;
        if (tileIndex == -1){
            if(allowedTiles.Count == 1) tileIndex = allowedTiles.ToArray()[0];
            else return;
        }
        Instantiate(tiles[tileIndex], transform.position, Quaternion.identity, transform.parent);
        allowedTiles = new HashSet<int>{tileIndex};
        isCollapsed = true;
        tileNo = tileIndex;
    }

    void Start(){
        outline.enabled = false;
        canvas.enabled = false;
    }

    void OnMouseEnter(){
        outline.enabled = true;
        canvas.enabled = true;
    }
    void OnMouseExit(){
        outline.enabled = false;
        canvas.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerScript : MonoBehaviour {

    [SerializeField] private Outline outline;
    [SerializeField] private Renderer renderer;
    
    [HideInInspector] public bool isCollapsed = false;
    [HideInInspector] public HashSet<int> allowedTiles = new HashSet<int>();

    void Start(){
        outline.enabled = false;
        renderer.enabled = true;
    }

    void OnMouseEnter(){
        outline.enabled = true;
    }
    void OnMouseExit(){
        outline.enabled = false;
    }

    public void Collapse(GameObject[] tiles, int tileIndex){
        // Disable rendering for this container
        renderer.enabled = false;
        // Set isCollapsed = true
        isCollapsed = true;
        // Spawn the new tile
        Instantiate(tiles[tileIndex], transform.position, Quaternion.identity, transform.parent);
    }
}

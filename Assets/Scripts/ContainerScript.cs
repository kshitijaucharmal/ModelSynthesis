using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerScript : MonoBehaviour {

    [SerializeField] private Outline outline;

    void Start(){
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void OnMouseEnter(){
        outline.enabled = true;
    }
    void OnMouseExit(){
        outline.enabled = false;
    }


}

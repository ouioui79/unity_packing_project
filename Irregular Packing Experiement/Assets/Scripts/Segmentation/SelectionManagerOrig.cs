using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using NewtonVR;

public class SelectionManagerOrig : MonoBehaviour
{
    private NVRPlayer player = null;
    public GameObject selection = null;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private string selectableTag = "Interactable";

    void Start()
    {
        player = NVRPlayer.Instance;
        if ( player == null )
        {
            Debug.LogError( "Teleport: No Player instance found in map." );
            Destroy( this.gameObject );
            return;
        }
    }
    public void SelectObject()
    {
        Debug.Log("OBJECT SELECTED");
        selection = player.LeftHand.CurrentlyInteracting.gameObject;
        /*
        var selectionRenderer = selection.GetComponentInChildren<Renderer>();
        if (selectionRenderer != null)
        {
            selectionRenderer.material = highlightMaterial;
        }
        */
    }

    public void DeselectObject()
    {
        //var selectionRenderer = selection.GetComponentInChildren<Renderer>(); //Problem here
        //selectionRenderer.material = defaultMaterial;
        selection = null;
    }

    public GameObject GetSelection()
    {
        return selection;
    }
}
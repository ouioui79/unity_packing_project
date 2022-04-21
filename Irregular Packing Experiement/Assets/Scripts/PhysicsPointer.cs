using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewtonVR;

public class PhysicsPointer : MonoBehaviour
{
    public float defaultLength = 3.0f;
    private NVRPlayer player = null;
    private GameObject selection = null;
    private GameObject currentlyHitting = null;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material defaultMaterial;

    private LineRenderer lineRenderer = null;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        player = NVRPlayer.Instance;
        
        if ( player == null )
        {
            Debug.LogError( "Teleport: No Player instance found in map." );
            Destroy( this.gameObject );
            return;
        }
    }

    private void Update()
    {
        UpdateLength();
        if (player.LeftHand.Inputs[NVRButtons.Touchpad].IsPressed)
        {
            if (selection == null)
            {
                SelectObject();
            } else if (selection.name == currentlyHitting.name)
            {
                DeselectObject();
            }
        }
    }

    private void UpdateLength()
    { 
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, CalculateEnd());
    } 

    private Vector3 CalculateEnd()
    {
        RaycastHit hit = CreateForwardRaycast();
        Vector3 endPosition = DefaultEnd(defaultLength);

        if (hit.collider)
        {
            endPosition = hit.point;
            currentlyHitting = hit.transform.gameObject;
        }

        return endPosition;
    }

    private RaycastHit CreateForwardRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        Physics.Raycast(ray, out hit, defaultLength);
        return hit;
    }

    private Vector3 DefaultEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
    
    private void SelectObject()
    {
        selection = currentlyHitting;
        
        var selectionRenderer = selection.GetComponentInChildren<Renderer>();
        if (selectionRenderer != null)
        {
            selectionRenderer.material = highlightMaterial;
        }
    }

    private void DeselectObject()
    {
        var selectionRenderer = selection.GetComponentInChildren<Renderer>();
        selectionRenderer.material = defaultMaterial;
        selection = null;
    }

    public GameObject GetHit()
    {
        return selection;
    }
}

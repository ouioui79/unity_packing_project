using System;
using Plawius.NonConvexCollider;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider.Editor;
using UnityEngine;
using UnityEngine.UI;
using NewtonVR;
public class ObjectManagerRay : MonoBehaviour
{
    private PhysicsPointer physicsPointer;
    private bool rotating = true;
    private string object_mode = "packing";
    
    private NVRPlayer player = null;

    private GameObject selection = null;
    private Text object_mode_text;
    private MeshCollider mc_for_slicing;
    public Timer t;

    public GameObject cuttingPlane;
    public GameObject pointer;
    
    private PlaneSlicer slicer;

    public List<GameObject> Wastes = new List<GameObject>();

    private void Start()
    {
        //t.SetUpTimer(10);
        player = NVRPlayer.Instance;
        
        if ( player == null )
        {
            Debug.LogError( "Teleport: No Player instance found in map." );
            Destroy( this.gameObject );
            return;
        }

        if (object_mode == "packing")
        {
            cuttingPlane.SetActive(false);
        }
        
        physicsPointer = player.LeftHand.GetComponentInChildren<PhysicsPointer>();
        slicer = gameObject.GetComponent<PlaneSlicer>();
        object_mode_text = GameObject.Find("Mode").GetComponentInChildren<Text>();
        
        Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
    }

    void Update() 
    {
        selection = physicsPointer.GetHit();

        if (object_mode == "packing")
        {
            for (int i = 0; i < Wastes.Count; ++i)
            {
                if (Wastes[i].GetComponent<NonConvexColliderComponent>() == null)
                {
                    UnityExtensions.DeleteAllColliders(Wastes[i]);
                    Mesh m = Wastes[i].GetComponent<MeshFilter>().mesh;
                    AddNonConvexColliders(m, selection);
                    Wastes[i].GetComponent<Property>().RecalculateProperties();
                    Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                }
            }	
        } 
        else if (object_mode == "segmenting")
        {
            if (selection != null) {
                if (selection.GetComponent<NonConvexColliderComponent>() != null)
                {
                    Destroy(selection.GetComponent<NonConvexColliderComponent>());
                    UnityExtensions.DeleteAllColliders(selection);
                    mc_for_slicing = selection.AddComponent<MeshCollider>();
                    mc_for_slicing.convex = true;
                }
                if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed)
                {
                    slicer.PrepareSegmentation(cuttingPlane.transform.up, cuttingPlane.transform.position, selection);
                    slicer.SliceObject();
                    Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
                    for (int i = 0; i < Wastes.Count; i++)
                    {
                        Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                    }
                }
            }
        }
    } 
        
    public void SwitchObjectMode()
    {
        if (object_mode == "packing")
        {
            object_mode = "segmenting";
            object_mode_text.text = "Mode: Segmentation";
            slicer.enabled = true;
            pointer.GetComponent<LineRenderer>().enabled = true;
            cuttingPlane.SetActive(true);
        }
        else if (object_mode == "segmenting")
        {
            object_mode = "packing";
            object_mode_text.text = "Mode: Packing";
            // todo: delete
            slicer.enabled = false;
            pointer.GetComponent<LineRenderer>().enabled = false;
            cuttingPlane.SetActive(false);
        }
    }

    void AddNonConvexColliders(Mesh m, GameObject selected_object)
    {
        Mesh[] meshes;
        meshes = API.GenerateConvexMeshes(m, Parameters.Default());

        var collider_asset = NonConvexColliderAsset.CreateAsset(meshes);
        var non_convex = selected_object.AddComponent<NonConvexColliderComponent>();
        non_convex.SetPhysicsCollider(collider_asset);
    }
}

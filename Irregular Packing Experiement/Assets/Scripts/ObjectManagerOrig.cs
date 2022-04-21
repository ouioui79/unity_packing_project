using System;
using Plawius.NonConvexCollider;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider.Editor;
using UnityEngine;
using UnityEngine.UI;
using NewtonVR;

public class ObjectManagerOrig : MonoBehaviour
{
    private CollidersCounter counter;
    private SelectionManagerSeg segmentation_selection_manager;
    private bool rotating = true;
    private string object_mode = "packing";
    
    private NVRPlayer player = null;

    private GameObject selection = null;
    private GameObject old_selection = null;
    private Text object_mode_text;
    private MeshCollider mc_for_slicing;
    public Timer t;

    public GameObject cuttingPlane;
    
    private PlaneSlicer slicer;

    public List<GameObject> Wastes = new List<GameObject>();

    private void Awake()
    {
        segmentation_selection_manager = gameObject.GetComponent<SelectionManagerSeg>();
        counter = GameObject.Find("counter").GetComponent<CollidersCounter>();
        slicer = gameObject.GetComponent<PlaneSlicer>();
        object_mode_text = GameObject.Find("Mode").GetComponentInChildren<Text>();
    }

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
        Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
    }

    void Update() 
    {
        if (object_mode == "packing")
        {
                for (int i = 0; i < Wastes.Count; i++)
                {
                    if (Wastes[i].GetComponent<NonConvexColliderComponent>() == null)
                    {
                        UnityExtensions.DeleteAllColliders(Wastes[i]);
                        Mesh m = Wastes[i].GetComponent<MeshFilter>().mesh;
                        AddNonConvexColliders(m, Wastes[i]);
                        Wastes[i].GetComponent<Property>().SetUp();
                        Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                    }
                }
        }
        else if (object_mode == "segmenting")
        {
            if (player.LeftHand.Inputs[NVRButtons.Touchpad].IsPressed && player.LeftHand.CurrentlyInteracting != null)
            {
                if (selection == null)
                {
                    segmentation_selection_manager.SelectObject();
                }
                else if (selection.name == player.LeftHand.CurrentlyInteracting.gameObject.name)
                {
                    segmentation_selection_manager.DeselectObject();
                }
            }

            selection = segmentation_selection_manager.GetSelection();

            if (selection != null && selection.GetComponent<NonConvexColliderComponent>() != null)
            {
                Destroy(selection.GetComponent<NonConvexColliderComponent>());
                UnityExtensions.DeleteAllColliders(selection);
                mc_for_slicing = selection.AddComponent<MeshCollider>();
                mc_for_slicing.convex = true;
                selection.GetComponent<NVRInteractableItem>().UpdateColliders();
            }

            if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed && selection != null)
            {
                slicer.PrepareSegmentation(cuttingPlane.transform.up, cuttingPlane.transform.position, selection);

                segmentation_selection_manager.DeselectObject();
                
                slicer.SliceObject();
                counter.ResetWastes();
                Wastes = counter.GetWastes();//new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
                for (int i = 0; i < Wastes.Count; i++)
                {
                    if (Wastes[i].GetComponent<Property>().GetVolume() == 0.0f)
                    {
                        Wastes[i].GetComponent<NVRInteractableItem>().DeregisterObject();
                        //Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                        UnityExtensions.DeleteAllColliders(Wastes[i]);
                        mc_for_slicing = Wastes[i].AddComponent<MeshCollider>();
                        mc_for_slicing.convex = true;
                        Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                        Wastes[i].GetComponent<Property>().SetUp();
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
            cuttingPlane.SetActive(true);
        }
        else if (object_mode == "segmenting")
        {
            object_mode = "packing";
            object_mode_text.text = "Mode: Packing";
            slicer.enabled = false;
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

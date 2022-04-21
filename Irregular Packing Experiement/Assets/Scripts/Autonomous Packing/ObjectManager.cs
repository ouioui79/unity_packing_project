using Plawius.NonConvexCollider;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider.Editor;
using UnityEngine;
using UnityEngine.UI;
using NewtonVR;
using UnityEditor.Sprites;

public class ObjectManager : MonoBehaviour
{
    
    private List<string> selections = new List<string>();
    private bool ga_ran = false;
    private NVRPlayer player = null;
    private GameObject lid;

    private CollidersCounter counter;
    private SelectionManager selection_manager;
    private BLFPacking packer;
    private GAController ga_controller;
    private ConfigReporter reporter;
    private string curr_mode = "packing";
    private GameObject segmenting_obj;
    private MeshCollider mc_for_slicing;
    private PlaneSlicer slicer;
    private Vector3 orig_scale;

    public Timer t;
    public List<GameObject> Wastes;
    public GameObject cuttingPlane;
    public GameObject scaler;
    public GameObject select_button;
    
    private void Awake()
    {
        selection_manager = gameObject.GetComponent<SelectionManager>();
        packer = gameObject.GetComponent<BLFPacking>();
        ga_controller = gameObject.GetComponent<GAController>();
        reporter = gameObject.GetComponent<ConfigReporter>();

        player = NVRPlayer.Instance;
        
        if ( player == null )
        {
            Debug.LogError( "Teleport: No Player instance found in map." );
            Destroy( this.gameObject );
            return;
        }

        lid = GameObject.Find("wall6");
        lid.SetActive(false);
        
        slicer = gameObject.GetComponent<PlaneSlicer>();
        scaler.SetActive(false);
        select_button.SetActive(false);
        cuttingPlane.SetActive(false);
    }

    void Update()
    {
        if (curr_mode == "packing")
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
            // if in packing mode, run GA selection code
            if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed)
            {
                var interacting = player.RightHand.CurrentlyInteracting;
                if (interacting != null && !selections.Contains(interacting.name))
                {
                    selection_manager.SelectObject(interacting.gameObject);
                }
                selections = selection_manager.GetSelections();
            } else if (player.LeftHand.Inputs[NVRButtons.Touchpad].IsPressed)
            {
                var interacting = player.LeftHand.CurrentlyInteracting;
                if (interacting != null && selections.Contains(interacting.name))
                {
                    selection_manager.DeselectObject(interacting.gameObject);
                }
                selections = selection_manager.GetSelections();
            }

            if (!ga_ran && ga_controller.Finished())
            {
                gameObject.GetComponent<ConfigReporter>().SetUp();
                Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
                for(int i = 0; i < Wastes.Count; i++)
                {
                    if (selections.Contains(Wastes[i].name))
                    {
                        selection_manager.DeselectObject(Wastes[i]);
                    }
                }
                lid.SetActive(false);
                ga_ran = true;
            }
        }
        else
        {
            // if in segmenting mode, run segmentation selection code
            if (segmenting_obj != null && segmenting_obj.GetComponent<NonConvexColliderComponent>() != null)
            {
                Destroy(segmenting_obj.GetComponent<NonConvexColliderComponent>());
                UnityExtensions.DeleteAllColliders(segmenting_obj);
                mc_for_slicing = segmenting_obj.AddComponent<MeshCollider>();
                mc_for_slicing.convex = true;
                segmenting_obj.GetComponent<NVRInteractableItem>().UpdateColliders();
            }

            if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed && segmenting_obj != null)
            {
                slicer.PrepareSegmentation(cuttingPlane.transform.up, cuttingPlane.transform.position, segmenting_obj);

                slicer.SliceObject();
                counter.ResetWastes();
                Wastes = counter.GetWastes();//new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
                for (int i = 0; i < Wastes.Count; i++)
                {
                    if (Wastes[i].GetComponent<Property>().GetVolume() == 0.0f)
                    {
                        Wastes[i].GetComponent<NVRInteractableItem>().DeregisterObject();
                        Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
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

    public List<string> GetSelections()
    {
        return selections;
    }

    public void RunGA()
    {
        Debug.LogFormat("SELECTED FOR GA: {0}", selections.Count);
        lid.SetActive(true);
        packer.SetUp(selections);
        ga_controller.SetUp(selections.Count);
        ga_controller.StartGA();
    }

    public void SwitchMode()
    {
        if (curr_mode == "packing")
        {
            curr_mode = "segmenting";
            // TODO: turn scaler, select button, and cutting plane on
            scaler.SetActive(true);
            select_button.SetActive(true);
            cuttingPlane.SetActive(true);
        }
        else
        {
            curr_mode = "packing";
            // TODO: turn scaler, select button, and cutting plane off
            scaler.SetActive(false);
            select_button.SetActive(false);
            cuttingPlane.SetActive(false);
        }
    }

    public void SelectForSegmenting()
    {
        var interacting = player.LeftHand.CurrentlyInteracting;
        // TODO: set this object for scaling and segmenting
        segmenting_obj = player.LeftHand.CurrentlyInteracting.gameObject;
        orig_scale = segmenting_obj.transform.localScale;
    }
    
    public void ScaleObject()
    {
        Slider slider = scaler.GetComponent<Slider>();
        segmenting_obj.transform.localScale = new Vector3(orig_scale.x/slider.value,orig_scale.y/slider.value,orig_scale.z/slider.value);
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
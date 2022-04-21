using Plawius.NonConvexCollider;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider.Editor;
using UnityEngine;
using UnityEngine.UI;
using NewtonVR;
using UnityEditor.Sprites;

public class ObjectManagerGA : MonoBehaviour
{
    
    [HideInInspector] public List<string> selections = new List<string>();
    private bool ga_ran = false;
    private NVRPlayer player = null;
    public GameObject lid;
    private Text object_mode_text;

    private CollidersCounter counter;
    private SelectionManager selection_manager;
    private BLFPacking packer;
    private BLFPackingInwards inwardsPacker;
    private GAController ga_controller;
    private ConfigReporter reporter;
    GameObject currentInteractingObj;

    public Timer t;
    
    public List<GameObject> Wastes;
    
    private void Awake()
    {
        selection_manager = gameObject.GetComponent<SelectionManager>();
        packer = gameObject.GetComponent<BLFPacking>();
        inwardsPacker = gameObject.GetComponent<BLFPackingInwards>();
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
    }

    void Update()
    {
        /*object_mode_text = GameObject.Find("Mode").GetComponentInChildren<Text>();
        if (object_mode_text.text == "Mode: Packing")
        {
            if (player.RightHand.Inputs[NVRButtons.Touchpad].PressDown)
            {

                if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed)
                {
                    var interacting = player.RightHand.CurrentlyInteracting.gameObject;// gameObject

                    if (interacting != null && !selections.Contains(interacting.name))
                    {
                        selection_manager.SelectObject(interacting.gameObject);
                    }
                    selections = selection_manager.GetSelections();

                }
                else if (player.LeftHand.Inputs[NVRButtons.Touchpad].IsPressed)
                {
                    var interacting = player.LeftHand.CurrentlyInteracting;
                    if (interacting != null && selections.Contains(interacting.name))
                    {
                        selection_manager.DeselectObject(interacting.gameObject);
                    }
                    selections = selection_manager.GetSelections();
                }
            }
        }*/

       /*if (!ga_ran && ga_controller.Finished())
        {
            gameObject.GetComponent<ConfigReporter>().SetUp();
            Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
            for(int i = 0; i < Wastes.Count; i++)
            {
                if (selections.Contains(Wastes[i].name))
                {
                    //selection_manager.DeselectObject(Wastes[i]);
                    selections.Remove(Wastes[i].name);
                }
            }
            lid.SetActive(false);
            ga_ran = true;
        }*/
    }

    public List<string> GetSelections()
    {
        return selections;
    }

    public void RunGA()
    {
        Debug.LogFormat("SELECTED FOR GA: {0}", selections.Count);
        lid.SetActive(true);
        inwardsPacker.SetUp(selections);
        ga_controller.SetUp(selections.Count);
        if (ga_controller.has_started = true)
        {
            ga_controller.has_started = false;
        }
        if (ga_controller.finished = true)
        {
            ga_controller.finished = false;
        }
        ga_controller.StartGA();
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
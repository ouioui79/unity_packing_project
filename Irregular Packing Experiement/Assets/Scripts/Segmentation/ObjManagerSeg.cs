using System;
using Plawius.NonConvexCollider;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider.Editor;
using UnityEngine;
using UnityEngine.UI;
using NewtonVR;

public class ObjManagerSeg : MonoBehaviour
{
    public CollidersCounter counter;
    private SelectionManagerOrig selection_manager;
    private bool rotating = true;
    private string object_mode = "packing";
    private NVRPlayer player = null;
    private GameObject selection = null;
    public string selectionName;
    private GameObject old_selection = null;
    private Text object_mode_text;
    private MeshCollider mc_for_slicing;
    private Vector3 orig_scale;
    private PlaneSlicer slicer;

    public Timer t;
    public GameObject cuttingPlane;
    public GameObject scaler;

    public List<GameObject> Wastes = new List<GameObject>();
    public List<Color> oldColors = new List<Color>();

    public bool slicerSpawned = false;
    GameObject slicerHolder;
    GameObject cuttingPlaneControllable;
    public float speed = 0.05f;

    public bool cutComplete = false;
    public bool cutBigObjects = false;
    GameObject currentInteractingObj;
    public float sphereRadius = 0.01f;
    public GameObject leftHand;
    public Material combinedMeshMaterial;

    public GameObject objOfInterest; //for debugging use during runtime

    public int downsizeResolution = 15000;

    private void Awake()
    {
        selection_manager = gameObject.GetComponent<SelectionManagerOrig>();
        if (counter == null) counter = GameObject.Find("counter").GetComponent<CollidersCounter>();
        slicer = gameObject.GetComponent<PlaneSlicer>();
        object_mode_text = GameObject.Find("Mode").GetComponentInChildren<Text>();
    }

    private void Start()
    {
        player = NVRPlayer.Instance;

        if (player == null)
        {
            Debug.LogError("Teleport: No Player instance found in map.");
            Destroy(this.gameObject); //(CLOSED FOR NOW, WORKING ON A LAPTOP WITHOUT THE VR HEADSET)
            return;
        }

        if (object_mode == "packing")
        {
            cuttingPlane.SetActive(false);
        }
    }

    void Update()
    {
        FixWastesList();

        if (cutComplete)
        {
            int count = 0;
            for (int i = 0; i < Wastes.Count; i++)
            {
                if (Wastes[i].GetComponent<NonConvexColliderComponent>() == null)
                {
                    UnityExtensions.DeleteAllColliders(Wastes[i]);
                    Mesh m = Wastes[i].GetComponent<MeshFilter>().mesh;
                    AddNonConvexColliders(m, Wastes[i]); //maybe here? component.resolution = 50000;
                    Wastes[i].GetComponent<Property>().SetUp(); //sets up collider
                    Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                    count++;
                }
            }

            cutComplete = false;
        }
        if (!slicerSpawned) GetControllablePlane(); //Instantiate the controllable slicer


        if (selection != null) selectionName = selection.name;

        if (slicerSpawned && object_mode == "segmenting") //PLACES THE SLICING PLANE ON THE RIGHT HAND CONTROLLER
        {

            slicerHolder.transform.SetParent(player.RightHand.gameObject.transform);
            slicerHolder.transform.position = player.RightHand.gameObject.transform.position;
            slicerHolder.transform.rotation = player.RightHand.gameObject.transform.rotation;
            slicerHolder.transform.Rotate(90f, 0f, 0f);
        }

        if (object_mode == "packing" && !cutComplete)
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
            if (player.LeftHand.Inputs[NVRButtons.Touchpad].PressDown && player.LeftHand.CurrentlyInteracting != null)
            {
                Debug.Log("Freed object " + player.LeftHand.CurrentlyInteracting.gameObject.name);
                currentInteractingObj = player.LeftHand.CurrentlyInteracting.gameObject;
                currentInteractingObj.GetComponent<Rigidbody>().isKinematic = false;
            }
            else if (player.LeftHand.CurrentlyInteracting == null && currentInteractingObj != null)
            {
                selection_manager.DeselectObject();
                currentInteractingObj = null;
            }
            
                        
            if (player.LeftHand.Inputs[NVRButtons.Trigger].PressDown && player.LeftHand.CurrentlyInteracting != null)
            {
                player.LeftHand.CurrentlyInteracting.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                if (selection == null)
                {
                    selection_manager.SelectObject();

                }
            }

            leftHand.GetComponent<NVRHand>().objPreparedForCut = true;

            selection = selection_manager.GetSelection();


            if (selection != null && selection.GetComponent<NonConvexColliderComponent>() != null) //Prepares object to be sliced
            {
                Destroy(selection.GetComponent<NonConvexColliderComponent>());
                UnityExtensions.DeleteAllColliders(selection);
                mc_for_slicing = selection.AddComponent<MeshCollider>();
                mc_for_slicing.convex = true;
                selection.GetComponent<NVRInteractableItem>().UpdateColliders();
                //Debug.Log("Object " + selection.name + " is ready to be sliced.");
            }
            SliceObject();
        }    
    }

    //This method fixes the Wastes list any time this method is called. Meant to be called when a new object is instantiated, or imported.
    public void FixWastesList()
    {
        int numOfObjets = GameObject.FindGameObjectsWithTag("Interactable").Length;
        if (Wastes.Count != numOfObjets || cutComplete)
        {
            Wastes = new List<GameObject>(GameObject.FindGameObjectsWithTag("Interactable"));
            oldColors = new List<Color>();
            for (int i = 0; i < Wastes.Count; i++)
            {
                //Debug.Log("#" + i + " adding color for obj " + Wastes[i].name);
                if (Wastes[i].GetComponent<Renderer>() != null)
                    oldColors.Add(Wastes[i].GetComponent<Renderer>().material.color);
                else if (Wastes[i].GetComponent<MeshRenderer>() != null)
                    oldColors.Add(Wastes[i].GetComponent<MeshRenderer>().material.color);
            }

            if (object_mode == "packing")
            {
                for (int i = 0; i < Wastes.Count; i++)
                {
                    Wastes[i].GetComponent<Rigidbody>().isKinematic = false; //Keep fixed for now (RELEASED AFTER PACKING)
                }
            }
            else if (object_mode == "segmenting")
            {
                for (int i = 0; i < Wastes.Count; i++)
                {
                    Wastes[i].GetComponent<Rigidbody>().isKinematic = true; //Set all objects isKinematic
                }
            }

            //Debug.Log("# OF OBJ CHANGED, NOW THERE ARE " + Wastes.Count + " OBJECTS AND " + oldColors.Count + " COLORS.");
        }
    }

    //Used to switch the mode during play mode. Segmentation mode is meant to be used while segmenting objects. When done,
    //switch the mode to packing, which will setup all the objects for packing (centering their pivots etc.)
    public void SwitchObjectMode()
    {
        if (object_mode == "packing")
        {
            object_mode = "segmenting";
            object_mode_text.text = "Mode: Segmentation";
            slicer.enabled = true;
            slicerHolder.SetActive(true);
            cuttingPlaneControllable.SetActive(true);

            for (int i = 0; i < Wastes.Count; i++)
            {
                Wastes[i].GetComponent<Rigidbody>().isKinematic = true; //Set all objects isKinematic
                Wastes[i].GetComponent<NVRInteractableItem> ().EnableKinematicOnDetach = true;
            }
        }
        else if (object_mode == "segmenting")
        {
            object_mode = "packing";
            object_mode_text.text = "Mode: Packing";
            slicer.enabled = false;
            cuttingPlaneControllable.SetActive(false);
            slicerHolder.SetActive(false);

            ClusterObjectsUnderParent(); //Combines the sliced objects into a single object so that they can be moved/packed later

            for (int i = 0; i < Wastes.Count; i++)
            {
                Wastes[i].GetComponent<Rigidbody>().isKinematic = false; //Free all objects (DISABLED FOR NOW!)
                Wastes[i].GetComponent<NVRInteractableItem>().EnableKinematicOnDetach = false;
                /*if(Wastes[i].name.Contains("_neg") || Wastes[i].name.Contains("_pos")) //not used anymore because it causes controll issues when switching back from segmentation mode
                 {
                     Debug.Log("Centered " + Wastes[i].name);
                     SetPivot(Wastes[i]);
                 }*/
            }
            Debug.Log("Downsized all the NonConvexColliders to " + downsizeResolution);
            //SimplifyColliders(downsizeResolution); //not used anymore because it causes controll issues when switching back from segmentation mode
        }
    }
    
    //NEVER USED.
    public void ScaleObject()
    {
        Slider slider = scaler.GetComponent<Slider>();
        if (selection != null)
        {
            orig_scale = selection.transform.localScale;
            selection.transform.localScale =
                new Vector3(1 / slider.value, 1 / slider.value,
                    1 / slider.value); //orig_scale.x/slider.value,orig_scale.y/slider.value,orig_scale.z/slider.value);
        }
    }

    //Adds the NonConvexCollider component to the selected_object, also generates the collider for the component using the mesh m
    void AddNonConvexColliders(Mesh m, GameObject selected_object)
    {
        Mesh[] meshes;
        meshes = API.GenerateConvexMeshes(m, Parameters.Default());
        var collider_asset = NonConvexColliderAsset.CreateAsset(meshes);
        var non_convex = selected_object.AddComponent<NonConvexColliderComponent>();
        non_convex.SetPhysicsCollider(collider_asset);
    }

    //This method instantiates the plane that is used for segmenting in the beginning
    public void GetControllablePlane()
    {
        Debug.Log("New Slicer Spawned.");
        slicerHolder = new GameObject("Slicer Holder");  //the parent game object, that holds the slicer plane
        cuttingPlaneControllable = Instantiate(cuttingPlane, new Vector3(0, 0.5f, 0), Quaternion.Euler(90, -180, 0));
        cuttingPlaneControllable.name = "My Slicer";
        cuttingPlaneControllable.SetActive(true);
        cuttingPlaneControllable.transform.SetParent(slicerHolder.transform);
        slicerSpawned = true;
    }

    //The main method for segmenting objects. The object is first selected, and then segmented. Each new "segmented" object is also setup in this method.
    public void SliceObject()
    {

        //cut the object
        bool initiateCut = false;
     
        initiateCut = player.RightHand.Inputs[NVRButtons.Trigger].PressDown;
        if (initiateCut)
        {
            slicer.PrepareSegmentation(cuttingPlaneControllable.transform.up, cuttingPlaneControllable.transform.position, selection);
            selection_manager.DeselectObject();
            slicer.SliceObject();
            counter.ResetWastes();
            Wastes = counter.GetWastes();
            oldColors = new List<Color>();
            for (int i = 0; i < Wastes.Count; i++)
            {
                oldColors.Add(Wastes[i].GetComponent<Renderer>().material.color);
                Wastes[i].GetComponent<Rigidbody>().isKinematic = true;

                if (Wastes[i].GetComponent<Property>().GetVolume() == 0.0f)
                {
                    Debug.Log("this object has no volume: " + Wastes[i].name);
                    Wastes[i].GetComponent<NVRInteractableItem>().DeregisterObject();
                    UnityExtensions.DeleteAllColliders(Wastes[i]);
                    mc_for_slicing = Wastes[i].AddComponent<MeshCollider>();
                    mc_for_slicing.convex = true;
                    Wastes[i].GetComponent<NVRInteractableItem>().UpdateColliders();
                    Wastes[i].GetComponent<Property>().SetUp();
                }

                Wastes[i].GetComponent<Renderer>().material.SetColor("_Color", Color.gray); //Color everything back to gray
            }

            cutComplete = true;
            leftHand.GetComponent<NVRHand>().cutComplete = true;

            Debug.Log("Slicing of object " + selection + " is complete.");
        }
    }

    //This method is used to find the "neighbour" objects, meaning objects that are still physically in contact with the obj.
    //The method returns a list of game objects that are still in contact
    public List<GameObject> FindNeighbourObjects(GameObject obj)
    {
        List<GameObject> neighbouringObjects = new List<GameObject>();

        Vector3 obj_bounds = Vector3.Scale(obj.GetComponent<MeshFilter>().sharedMesh.bounds.size, obj.transform.localScale);
        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z) * 1.05f; //Multiplied because they aren't actually touching
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals("Counter"));
        Collider[] obj_cols = obj.GetComponents<Collider>();

        foreach (Collider hitcollider in colliding_objs)
        {
            List<Collider> curr = new List<Collider>(colliding_objs.FindAll(s => s.gameObject.name.Equals(hitcollider.gameObject.name)));
            foreach (Collider curr_colliding in curr)
            {
                Vector3 dir;
                float dis;
                // Check collision for all colliders of the object
                foreach (Collider col in obj_cols)
                {
                    if (Physics.ComputePenetration(col, obj.transform.position,
                        obj.transform.rotation, curr_colliding, curr_colliding.transform.position,
                        curr_colliding.transform.rotation, out dir, out dis))
                    {
                        if (curr_colliding.tag == "Interactable")
                        {
                            neighbouringObjects.Add(curr_colliding.gameObject);
                        }
                    }
                }
            }
        }

        string objList = "";
        for (int i = 0; i < neighbouringObjects.Count; i++)
        {
            if (string.IsNullOrEmpty(objList)) objList = neighbouringObjects[i].name;
            else objList = objList + ", " + neighbouringObjects[i].name;
        }

        //Debug.Log("The object " + obj.name + " is in collision with: " + objList);
        return neighbouringObjects;
    }


    //Collects all gameobjects that have the same "true" name (See TrueName() method), and that are physically touching under a new parent gameobject
    //NOTE: Because of the limitations of segmentation sometimes unintended cuts might be done. When switching back to "packing" mode, ANY number of
    //objects that were the same object initially (hence the same true name), and that are still physically in contact will be combined back together.
    public void ClusterObjectsUnderParent() 
    {
        List<List<GameObject>> clusters = new List<List<GameObject>>(); //All clusters that will be formed are stored here

        for (int i = 0; i < Wastes.Count; i++)
        {
            List<GameObject> neighbours = FindNeighbourObjects(Wastes[i]); //Gets all the neighbouring objects (physically touching)
            if (neighbours.Count == 0) continue; //Object has no neighbours, won't be clustered

            //FIND NEIGHBOURS TO BE ADDED, WHICH ARE NEIGHBOURS THAT ARE NOT PART OF A CLUSTER ALREADY
            List<GameObject> neighboursToBeAdded = new List<GameObject>(); //Neighbours that have the same name, and that are not already a part of a cluster
            for (int k = 0; k < neighbours.Count; k++)
            {
                for (int j = 0; j < clusters.Count; j++)
                {
                    if (clusters[j].Contains(neighbours[k])) continue; //If neighbour[k] is already part of any cluster, skip it
                }

                if (TrueName(Wastes[i].name) != TrueName(neighbours[k].name)) continue; //It must be the same original object

                neighboursToBeAdded.Add(neighbours[k]);
            }

            //IF WASTES[i] IS ALREADY IN A CLUSTER, ADD THE NEIGHBOURS THERE. IF NOT, FORM A NEW CLUSTER WITH THEM
            bool alreadyInACluster = false; //Is Wastes[i] already in a cluster?

            for (int k = 0; k < clusters.Count; k++)
            {
                if (clusters[k].Contains(Wastes[i]))
                {
                    alreadyInACluster = true; //The object is already in a cluster, so its neighboursToBeAdded will be added to that cluster

                    for (int j = 0; j < neighboursToBeAdded.Count; j++) //Add all the neighbours to the cluster of Wastes[i]
                    {
                        if (clusters[k].Contains(neighboursToBeAdded[j])) continue; //If the cluster already contains the neighbour, don't add it twice

                        clusters[k].Add(neighboursToBeAdded[j]);
                    }
                }
            }

            if (!alreadyInACluster) //Wastes[i] is not in any cluster, so form a new one with it and its neighbours
            {
                List<GameObject> newCluster = new List<GameObject>(); //Object has neighbours, so start a cluster
                newCluster.Add(Wastes[i]);

                for (int j = 0; j < neighboursToBeAdded.Count; j++) //Add all the neighbours to the new cluster
                {
                    newCluster.Add(neighboursToBeAdded[j]);
                }

                clusters.Add(newCluster);
            }
        }

        if (clusters.Count == 0) Debug.Log("No clusters were formed.");

        //FOR EACH CLUSTER, PLACE THEM UNDER A PARENT OBJECT
        for (int i = 0; i < clusters.Count; i++)
        {
            GameObject clusterParent = new GameObject();
            clusterParent.name = "ClusterObject" + i; //Give cluster a name
            clusterParent.tag = "Interactable"; //Tag it interactable for later steps

            for (int k = 0; k < clusters[i].Count; k++)
            {
                clusters[i][k].transform.SetParent(clusterParent.transform);
            }

            //ADD ALL THE NECESSARY COMPONENTS TO THE PARENT OBJECT (Needed for later steps)

            if (clusterParent.GetComponent<MeshFilter>() == null) clusterParent.AddComponent<MeshFilter>();

            if (clusterParent.GetComponent<MeshRenderer>() == null) clusterParent.AddComponent<MeshRenderer>();

            if (clusterParent.GetComponent<Rigidbody>() == null) clusterParent.AddComponent<Rigidbody>();
            Rigidbody rb = clusterParent.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (clusterParent.GetComponent<Property>() == null) clusterParent.AddComponent<Property>();

            if (clusterParent.GetComponent<NVRInteractableItem>() == null) clusterParent.AddComponent<NVRInteractableItem>();
            clusterParent.GetComponent<NVRInteractableItem>().DisableKinematicOnAttach = false;

            CombineChildMeshes(clusterParent);            
        }
    }

    //After the objects are segmented, they receive new names such as name_pos or name_neg, this method returns their initial (true) name
    public string TrueName(string name) 
    {
        string returnName = "";

        if (name.Contains("pos") || name.Contains("neg"))
        {
            string[] splitName = name.Split('_');
            returnName = splitName[0];
        }
        else returnName = name;

        return returnName;
    }

    //Combines all the meshes, submeshes and the object's childrens' meshes into a single mesh. 
    //After combining, also generates the NonConvexCollider component, and the collider in that component
    //NOTE: This is meant to be used with ClusterObjectsUnderParent()
    public void CombineChildMeshes(GameObject obj)
    {
        // (1) COMBINING ALL THE MESHES AND SUBMESHES UNDER A SINGLE COMBINED MESH

        Quaternion oldRot = obj.transform.rotation; //Save old transform
        Vector3 oldPos = obj.transform.position;

        obj.transform.rotation = Quaternion.identity; //Set transform to zero
        obj.transform.position = Vector3.zero;

        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>(); //Get all children mesh filters
        List<CombineInstance> combiners = new List<CombineInstance>();

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].transform == obj.transform) //Skip the parent object
                continue;

            //combine submeshes
            for (int j = 0; j < filters[i].mesh.subMeshCount; j++)
            {
                CombineInstance ci = new CombineInstance();

                ci.mesh = filters[i].mesh;
                ci.subMeshIndex = j;
                ci.transform = filters[i].transform.localToWorldMatrix; //wherever is it in the real world, thats where we want it in the combined mesh

                combiners.Add(ci);
            }

            //Disable child gameObject
            filters[i].gameObject.SetActive(false);
        }

        MeshFilter filter = obj.GetComponent<MeshFilter>(); //Get the parent objects mesh filter
        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combiners.ToArray(), true, true); //Combine all the child meshes/submeshes into one
        filter.mesh.name = "Combined Mesh";

        obj.transform.position = oldPos; //Set old values back
        obj.transform.rotation = oldRot;

        /////////////////////////////////////////////////////////////////

        // (2) GENERATING THE NON CONVEX COLLIDER
        Mesh m = filter.mesh; //Get the combined mesh
        Mesh[] meshes; //The NonConvexCollider is made up of many smaller convex colliders, they will be stored here
        Parameters uniqueParameters = new Parameters { resolution = filters.Length * 50000, //Set the resolution higher depending on the number of submeshes
                                                       maxConvexHulls = 32,                 //...rest of the parameters are the same as Parameters.Default();
                                                       concavity = 0.0025,
                                                       planeDownsampling = 4,
                                                       convexhullApproximation = 4,
                                                       alpha = 0.05,
                                                       beta = 0.05,
                                                       pca = 1,
                                                       mode = 0,
                                                       maxNumVerticesPerCH = 64,
                                                       minVolumePerCH = 0.0001,
                                                       callback = null };

        meshes = API.GenerateConvexMeshes(m, uniqueParameters); //In script NonConvexColliderAPI
        var collider_asset = NonConvexColliderAsset.CreateAsset(meshes); //Create the nonconvexcollider asset
        collider_asset.name = "NonConvexCollider of " + obj.name; //Name it
        var non_convex_comp = obj.AddComponent<NonConvexColliderComponent>(); //Add the necessary component to the parent game objet
        non_convex_comp.SetPhysicsCollider(collider_asset); //Set the created nonconvex collider asset for the added component
    }   

    //For each object in the scene, it combines all the submshes into a single mesh.
    void ClearAllSubmeshes()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Interactable"))
            CombineChildMeshes(obj);
    }

    //(not used) After segmentation, the new objects might not have the correct pivot point. This method sets the pivot point of the object at the centre of its
    //Renderer's bounding box (which means the centre of the object)
   /* void SetPivot(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true; //stop object from moving

        //(0) Store the objects "real" position (meaning the centre of the renderer, not the pivot), to place the object back here
        Vector3 truePosition = obj.GetComponent<Renderer>().bounds.center;

        //(1) Centre the pivot
        PivotManager.selectedObject = obj; //object
        PivotManager.selectedObjectMesh = obj.GetComponent<MeshFilter>().sharedMesh;//mesh
        PivotManager.selectedObjectPivot = PivotManager.FindObjectPivot(PivotManager.selectedObjectMesh.bounds); //pivot
        PivotManager.CenterObjectPivot();

        //(2) Recreate the non convex collider
       MeshFilter filter = obj.GetComponent<MeshFilter>();
        Mesh m = filter.mesh; //Get the combined mesh
        Mesh[] meshes; //The NonConvexCollider is made up of many smaller convex colliders, they will be stored here
        int objectResolution = 50000;
        if (obj.name.Contains("Lab")) objectResolution = 100000;
        Parameters uniqueParameters = new Parameters { resolution = objectResolution, //Set the resolution higher depending on the number of submeshes
                                                       maxConvexHulls = 32,                 //...rest of the parameters are the same as Parameters.Default();
                                                       concavity = 0.0025,
                                                       planeDownsampling = 4,
                                                       convexhullApproximation = 4,
                                                       alpha = 0.05,
                                                       beta = 0.05,
                                                       pca = 1,
                                                       mode = 0,
                                                       maxNumVerticesPerCH = 64,
                                                       minVolumePerCH = 0.0001,
                                                       callback = null };

        meshes = API.GenerateConvexMeshes(m, uniqueParameters); //In script NonConvexColliderAPI
        var collider_asset = NonConvexColliderAsset.CreateAsset(meshes); //Create the nonconvexcollider asset
        collider_asset.name = "NonConvexCollider of " + obj.name; //Name it
        var non_convex_comp = obj.GetComponent<NonConvexColliderComponent>(); //Add the necessary component to the parent game objet
        non_convex_comp.SetPhysicsCollider(collider_asset); //Set the created nonconvex collider asset for the added component
        
        obj.GetComponent<Rigidbody>().isKinematic = false; //set the object free (CHANGED TO TRUE FOR NOW)

        obj.transform.position = truePosition; //Set the object back to where it was in real world space

    //(Not used)For all Interactable objects, downsizes the resolution of the NonConvexCollider to the resolutionInput, the value should be at least 10000
    //(This method is automatically called when the segmentation mode changes to packing)
    void SimplifyColliders(int resolutionInput)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Interactable"))
        {   
            // GENERATING THE NON CONVEX COLLIDER
            Mesh m = obj.GetComponent<MeshFilter>().mesh;
            Mesh[] meshes; //The NonConvexCollider is made up of many smaller convex colliders, they will be stored here
            Parameters uniqueParameters = new Parameters { resolution = resolutionInput, //Set the resolution higher depending on the number of submeshes
                                                           maxConvexHulls = 32,                 //...rest of the parameters are the same as Parameters.Default();
                                                           concavity = 0.0025,
                                                           planeDownsampling = 4,
                                                           convexhullApproximation = 4,
                                                           alpha = 0.05,
                                                           beta = 0.05,
                                                           pca = 1,
                                                           mode = 0,
                                                           maxNumVerticesPerCH = 64,
                                                           minVolumePerCH = 0.0001,
                                                           callback = null };

            meshes = API.GenerateConvexMeshes(m, uniqueParameters); //In script NonConvexColliderAPI
            var collider_asset = NonConvexColliderAsset.CreateAsset(meshes); //Create the nonconvexcollider asset
            collider_asset.name = "NonConvexCollider of " + obj.name; //Name it
            var non_convex_comp = obj.GetComponent<NonConvexColliderComponent>(); //Add the necessary component to the parent game objet
            non_convex_comp.SetPhysicsCollider(collider_asset); //Set the created nonconvex collider asset for the added component
        }
    }*/
}
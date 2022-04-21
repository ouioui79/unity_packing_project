using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using NewtonVR;

public class BLFTesting : MonoBehaviour
{
    public bool fullWorkFlow;
    public bool setSpecificObjects = false;
    public BLFPackingInwards BLFPacker;
    public GameObject sliceables;
    public List<string> selected_objects = new List<string>();
    List<int> objectsToBeActivated = new List<int>();

    [HideInInspector] public string mode;
    public int numberOfObjects; //If mode == "Random Objects", choose the number of objects to be packed
    public bool controlWithCubes; //If true, will instentiate 20 cubes used to test algorithms
    [Range(3, 30)]
    public int numberOfCubes;
    public GameObject controlCube; //The cube that is generated 20 times
    public bool showObjectList; //If true, will print out the list of objects (in order) that is going to be packed
    public bool checkBetterPositions;
    public bool betterPosWiggleRoom;
    public bool rotateMeshBounds;
    private NVRPlayer player = null;
    private Text object_mode_text;

    //For storing initial position/rotation of all objects, and restoring these positions/rotations if needed
    List<Vector3> initialChildPositions = new List<Vector3>(); //list of all initial positions
    List<Quaternion> initialChildRotations = new List<Quaternion>(); //list of all initial rotations
    int numOfChildren = 0; //stores the number of objects under sliceables

    List<string> thisList;

    //GA Algorithm
    public GAController ga;
    public ObjectManagerGA objManGA;

    //For importing objects
    public GameObject controller;
    public Import import;
    public CollidersCounter counter;

    //Resetting object positions in the full work flow scene
    List<GameObject> objectsThatWerePacked = new List<GameObject>();
    List<Vector3> packedObjectPositions = new List<Vector3>();
    List<Quaternion> packedObjectRotations = new List<Quaternion>();

    public GameObject objOfInterest;
    GameObject objManualSelectecd;

    void Start()
    {
        player = NVRPlayer.Instance;
        if (player == null)
        {
            Debug.LogError("Teleport: No Player instance found in map.");
            // Destroy(this.gameObject); //(CLOSED FOR NOW, WORKING ON A LAPTOP WITHOUT THE VR HEADSET)
            return;
        }

        if (controlWithCubes) InstantiateCubes();
        ga = this.GetComponent<GAController>();
        objManGA = this.GetComponent<ObjectManagerGA>();
        import = controller.GetComponent<Import>();

        BLFPacker = this.GetComponent<BLFPackingInwards>();
        if (!fullWorkFlow) StoreInitialPositions();
        //CompareBLF();

        thisList = new List<string>();
        //List<string> thisList = new List<string> { "O11", "O12", "O13", "CO1", "CO2", "CO3", "O13", "O5", "O1" };
        //SetSpecificObjects(thisList); //Use this method if a specific list of objects needs to be packed
        //setSpecificObjects = true;
    }

    void Update()
    {
        object_mode_text = GameObject.Find("Mode").GetComponentInChildren<Text>();
        //Debug.Log(object_mode_text);

        if (object_mode_text.text == "Mode: Packing")
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (fullWorkFlow)
                {
                    SelectAllInteractables();
                    counter.ResetWastes();
                }
                else
                {
                    if (setSpecificObjects) SetSpecificObjects(thisList);
                    else SetObjects(); //Selects objects for the BLF Packer
                }
            }
            else
            {
                if (player.LeftHand.Inputs[NVRButtons.Touchpad].IsPressed && player.LeftHand.CurrentlyInteracting != null)
                {

                    objManualSelectecd = player.LeftHand.CurrentlyInteracting.gameObject;
                    objManualSelectecd.GetComponent<Rigidbody>().isKinematic = true;
                    if (!selected_objects.Contains(player.LeftHand.CurrentlyInteracting.gameObject.name))
                    {
                        selected_objects.Add(player.LeftHand.CurrentlyInteracting.gameObject.name);

                    }
                    BLFPacker.object_list.Clear();
                    BLFPacker.SetUp(selected_objects); //setup the objects in the BLF Packer
                    AddObjectsToBLFPacker();
                    ShowObjectList();

                }
                if (player.RightHand.Inputs[NVRButtons.Touchpad].IsPressed && player.RightHand.CurrentlyInteracting != null)
                {
                    Debug.Log("I want to remove" + player.RightHand.CurrentlyInteracting.gameObject.name);
                    objManualSelectecd = player.RightHand.CurrentlyInteracting.gameObject;
                    objManualSelectecd.GetComponent<Rigidbody>().isKinematic = false;
                    if (selected_objects.Contains(player.RightHand.CurrentlyInteracting.gameObject.name))
                    {
                        selected_objects.Remove(player.RightHand.CurrentlyInteracting.gameObject.name);

                    }
                    BLFPacker.object_list.Clear();
                    BLFPacker.SetUp(selected_objects); //setup the objects in the BLF Packer
                    AddObjectsToBLFPacker();
                    foreach (string Obj in selected_objects)
                    {
                        Debug.Log("After Remove" + Obj);
                    }
                    ShowObjectList();

                }
            }
        }
       

        if (Input.GetKeyDown(KeyCode.T)) //For debugging
        {
            if (BLFPacker.CheckCollision(objOfInterest) == 1)
                Debug.Log("The object " + objOfInterest.name + " is colliding with something");
            else
                Debug.Log("The object " + objOfInterest.name + " is NOT colliding with something");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) Invoke("GetResultsInwards", 1); //Runs the inwards BLF algorithm

        if (Input.GetKeyDown(KeyCode.Alpha4)) Invoke("GetResultsOutwards", 1); //Runs the outwards BLF algorithm

        if (Input.GetKeyDown(KeyCode.Alpha5)) ResetPositions(); //Resets the position of all objects (use inbetween algorithms)

        if (checkBetterPositions) BLFPacker.checkBetterPositions = true;
        if (!checkBetterPositions) BLFPacker.checkBetterPositions = false;

        if (betterPosWiggleRoom) BLFPacker.wiggleRoomBool = true;
        if (!betterPosWiggleRoom) BLFPacker.wiggleRoomBool = false;

        if (rotateMeshBounds) BLFPacker.rotateMeshBounds = true;
        if (!rotateMeshBounds) BLFPacker.rotateMeshBounds = false;

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            // objManGA.selections = selected_objects;
            //objManGA.RunGA();
            RunGA();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            import.ImportMeshes();
        }
    }

    #region Methods

    void SetObjects()
    {
        //clear all lists
        selected_objects.Clear();
        objectsToBeActivated.Clear();
        BLFPacker.object_list.Clear();

        if (!controlWithCubes) ActivateSliceablesObjects(numberOfObjects); //activate n random objects
        GetInteractableObjects(); //populates selected_objects with all the active children under slicebles game object      
        BLFPacker.SetUp(selected_objects); //setup the objects in the BLF Packer
        AddObjectsToBLFPacker();

        ShowObjectList();
    }

    void SetSpecificObjects(List<string> specificObjList)
    {
        //clear all lists
        selected_objects.Clear();
        objectsToBeActivated.Clear();
        BLFPacker.object_list.Clear();

        BLFPacker.SetUp(specificObjList);
        BLFPacker.packing_objs.Clear();

        for (int i = 0; i < specificObjList.Count; i++)
        {
            selected_objects.Add(specificObjList[i]); //Populate selected_objects
            GameObject objToBeAdded = sliceables.transform.Find(specificObjList[i]).gameObject; //Find game object with name
            BLFPacker.packing_objs.Add(objToBeAdded); //Add directly to BLF Packer's packing_objs list
        }

        ShowObjectList(); //Shows the list of objects in the console
    }

    public void SelectAllInteractables()
    {
        //clear all lists
        selected_objects.Clear();
        objectsToBeActivated.Clear();
        BLFPacker.object_list.Clear();

        foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("Interactable")) //Get all interactable objects in the scene
        {
            if (interactable.activeSelf) selected_objects.Add(interactable.name); //Add them to the list if they are active right now
        }

        BLFPacker.SetUp(selected_objects); //setup the objects in the BLF Packer
        AddObjectsToBLFPacker();

        ShowObjectList();
    }

    void GetResultsInwards()
    {
        if (fullWorkFlow) StoreInteractablePositions();
        var startTime = System.DateTime.Now;
        Vector3 iResults = BLFPacker.RunBLF(false); //runs the "inwards" BLF algorithm
        var funcTime = (System.DateTime.Now.Subtract(startTime));
        int numOfObjectsPacked = BLFPacker.colliders_counter.count;
        Debug.Log("Inwards BLF Results: 1.Pack. Eff.: " + iResults.z + ", 2. Num. of Obj packed: " + numOfObjectsPacked
                                                                     + ", 3. Time: " + funcTime.ToString(@"mm\:ss\.ff"));
    }

    void GetResultsOutwards()
    {
        if (fullWorkFlow) StoreInteractablePositions();
        var startTime = System.DateTime.Now;
        Vector3 oResults = BLFPacker.RunBLF(true); //runs the "outwards" BLF algorithm
        var funcTime = (System.DateTime.Now.Subtract(startTime));
        int numOfObjectsPacked = BLFPacker.colliders_counter.count;
        Debug.Log("Outwards BLF Results: 1.Pack. Eff.: " + oResults.z + ", 2. Num. of Obj packed: " + numOfObjectsPacked
                                                                      + ", 3. Time: " + funcTime.ToString(@"mm\:ss\.ff"));
    }

    void ShowObjectList()
    {
        int lastIndex = BLFPacker.packing_objs.Count - 1;

        if (showObjectList) //Prints the list of objects (in order) that will be packed
        {
            string myString = "-> ";
            for (int k = 0; k < BLFPacker.packing_objs.Count; k++)
            {
                if (k == lastIndex) myString = myString + BLFPacker.packing_objs[k].name + ".";
                else myString = myString + BLFPacker.packing_objs[k].name + ", ";
            }

            Debug.Log("-OBJ_LIST: " + myString);
        }
    }

    void StoreInitialPositions()
    {
        if (!fullWorkFlow)
        {
            foreach (Transform child in sliceables.transform)
            {
                initialChildPositions.Add(child.position);
                initialChildRotations.Add(child.rotation);
                numOfChildren++;
            }
        }
        else
        {
            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("Interactable"))
            {
                initialChildPositions.Add(interactable.transform.position);
                initialChildRotations.Add(interactable.transform.rotation);
                numOfChildren++;
            }
        }
    }

    void StoreInteractablePositions()
    {
        objectsThatWerePacked.Clear();
        packedObjectPositions.Clear();
        packedObjectRotations.Clear();

        foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("Interactable"))
        {
            objectsThatWerePacked.Add(interactable);
            packedObjectPositions.Add(interactable.transform.position);
            packedObjectRotations.Add(interactable.transform.rotation);
        }
    }

    void ResetPositions()
    {
        if (!fullWorkFlow)
        {
            for (int i = 0; i < numOfChildren; i++)
            {
                Transform child = sliceables.transform.GetChild(i);
                child.position = initialChildPositions[i];
                child.rotation = initialChildRotations[i];
                child.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        else
        {
            for (int i = 0; i < objectsThatWerePacked.Count; i++)
            {
                Transform packedObjTransform = objectsThatWerePacked[i].transform;
                packedObjTransform.position = packedObjectPositions[i];
                packedObjTransform.rotation = packedObjectRotations[i];
                packedObjTransform.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

    }

    void ActivateSliceablesObjects(int num)
    {
        if (mode == "One Cube" || mode == "Random Objects")
        {
            foreach (Transform child in sliceables.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        if (mode == "One Cube")
        {
            foreach (Transform child in sliceables.transform)
            {
                if (child.gameObject.name == "CO1")
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        if (mode == "Random Objects")
        {
            List<int> possibleNums = new List<int>();

            for (int i = 0; i < sliceables.transform.childCount; i++)
            {
                possibleNums.Add(i);
            }

            for (int i = 0; i < num; i++)
            {
                int index = Random.Range(0, possibleNums.Count);
                objectsToBeActivated.Add(possibleNums[index]);
                possibleNums.RemoveAt(index);
            }

            for (int i = 0; i < num; i++)
            {
                Transform objectToBeActivated = sliceables.transform.GetChild(objectsToBeActivated[i]);
                objectToBeActivated.gameObject.SetActive(true);
            }
        }

        if (mode == "All Objects" || mode == "All Objects Randomly")
        {
            foreach (Transform child in sliceables.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void GetInteractableObjects() //Gets all active children from sliceables gameobject, and adds them to the list of objects that will be packed by BLF
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Interactable"))
        {
            if (obj.activeSelf) selected_objects.Add(obj.name);
        }
    }

    void AddObjectsToBLFPacker()
    {
        for (int k = 0; k < selected_objects.Count; k++) //adds selected objects to the packing_objs list of BLF
        {
            BLFPacker.packing_objs.Add(BLFPacker.object_list[k].obj);
        }

        if (mode == "All Objects Randomly") //if allObjectsRandomly is true, it will add all the objects randomly to the BLF Packer
        {
            int totalCount = BLFPacker.packing_objs.Count;
            List<GameObject> randomizedList = new List<GameObject>();

            for (int i = 0; i < totalCount; i++)
            {
                int index = Random.Range(0, BLFPacker.packing_objs.Count);
                randomizedList.Add(BLFPacker.packing_objs[index]);
                BLFPacker.packing_objs.RemoveAt(index);
            }

            BLFPacker.packing_objs = randomizedList;
        }
    }

    void InstantiateCubes()
    {
        Vector3 initialPosition = GameObject.Find("CO1").transform.position;
        int ticker = 0;
        float addY = 0;
        float addX = 0;

        for (int i = 0; i < numberOfCubes - 3; i++)
        {
            if (ticker > 23)
            {
                addY = 0.9f;
                addX = 0.3f * (i - 23);
            }
            else if (ticker > 15)
            {
                addY = 0.6f;
                addX = 0.3f * (i - 15);
            }
            else if (ticker > 7 && 15 >= ticker)
            {
                addY = 0.3f;
                addX = 0.3f * (i - 7);
            }
            else
            {
                addX = 0.3f * (i + 1);
            }

            Vector3 nextPosition = new Vector3((initialPosition.x - addX), initialPosition.y + addY, initialPosition.z);
            GameObject newCube = Instantiate(controlCube, nextPosition, Quaternion.identity);
            newCube.transform.parent = sliceables.transform;
            newCube.name = "CO" + (i + 4).ToString();
            ticker++;
        }

        //turn off all gameobjects that aren't cubes
        foreach (Transform child in sliceables.transform)
        {
            if (!child.name.Contains("C"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    public void RunGA()
    {
        if (fullWorkFlow) StoreInteractablePositions();
        objManGA.selections=selected_objects;
        objManGA.RunGA();
    }

    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleContainers : MonoBehaviour
{
    public bool useMultipleContainers;
    public Vector3[] boxMultipliers; //Here create as many vector3s, for the number of boxes and their sizes
    public GameObject counter;
    public List<GameObject> counters;

    //Resetting object positions in the full work flow scene
    List<GameObject> objectsThatWerePacked = new List<GameObject>();
    List<Vector3> packedObjectPositions = new List<Vector3>();
    List<Quaternion> packedObjectRotations = new List<Quaternion>();

    public BLFPackingInwards BLFPacker;
    public BLFTesting tester;

    void Awake()
    {
        if (!useMultipleContainers) this.enabled = false;
        tester = GetComponent<BLFTesting>();
        BLFPacker = GetComponent<BLFPackingInwards>();

        counters = new List<GameObject>();
        counters.Add(counter);
    }

    void Start()
    {
        //EXPAND TOWARDS NEGATIVE-Z AND POSITIVE-X (x-direction first)

        for (int i = 0; i < boxMultipliers.Length; i++)
        {
            Vector3 lastBoxCentre = new Vector3();
            Vector3 lastBoxScale = new Vector3();

            if (i == 0) //FOR THE FIRST ADDITIONAL BOX, POSITION IT AWAY FROM THE COUNTER THAT IS ALREADY IN THE SCENE
            {
                lastBoxCentre = GameObject.Find("Counter").transform.position;
                lastBoxScale = GameObject.Find("Counter").transform.localScale;
            }
            else //PREVIOUS BOX (They're named Counter, Counter 2, Counter 3...)
            {
                lastBoxCentre = GameObject.Find("Counter " + (i + 1)).transform.position;
                lastBoxScale = GameObject.Find("Counter " + (i + 1)).transform.localScale;
            }

            Vector3 newPosition = new Vector3(); //Decide where to put the next block. Currently creates rows of 3 boxes

            if (i % 3 == 0)
            {
                newPosition = new Vector3(lastBoxCentre.x,
                                          0.33f + ((boxMultipliers[i].y - 1) / 2),
                                          lastBoxCentre.z - (lastBoxScale.z / 2) - 0.2f - (boxMultipliers[i].z / 2));
            }
            else if (((i - (i % 3)) / 3) % 2 == 1)
            {
                newPosition = new Vector3(lastBoxCentre.x + (lastBoxScale.x / 2) + 0.2f + (boxMultipliers[i].x / 2),
                          0.33f + ((boxMultipliers[i].y - 1) / 2),
                          lastBoxCentre.z);
            }
            else
            {
                newPosition = new Vector3(lastBoxCentre.x - (lastBoxScale.x / 2) - 0.2f - (boxMultipliers[i].x / 2),
                                          0.33f + ((boxMultipliers[i].y - 1) / 2),
                                          lastBoxCentre.z);
            }

            GameObject nextCounter = Instantiate(counter, newPosition, Quaternion.identity);
            nextCounter.name = "Counter " + (i + 2);
            nextCounter.transform.localScale = Vector3.Scale(nextCounter.transform.localScale, boxMultipliers[i]);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            List<GameObject> packedObjs = new List<GameObject>(); //Add all objects that already belong to a counter here

            for (int i = 0; i < counters.Count; i++)
            {
                List<GameObject> thisCountersObjects = counters[i].GetComponent<CollidersCounter>().GetCurrentlyPackedWastes();

                for (int j = 0; j < thisCountersObjects.Count; j++)
                {
                    packedObjs.Add(thisCountersObjects[j]);
                }
            }

            if (packedObjs.Count == 0)
            {
                Debug.Log("No objects were packed, not moving on to the next counter.");
                return;
            }

            Debug.Log("Currently packed objects are: " + string.Join(", ", packedObjs));
            LabelAlreadyPackedObjects(packedObjs);
            MoveToNextCounter();
            tester.SelectAllInteractables();
        }
    }

    void MultiBoxPacking()
    {

    }

    void MoveToNextCounter()
    {
        int boxNum;

        //Get current box numbers
        string currentBoxName = BLFPacker.box.name;
        if (currentBoxName == "Counter") boxNum = 1;
        else
        {
            string onlyDigits = "";

            for (int i=0; i< currentBoxName.Length; i++)
            {
                if (Char.IsDigit(currentBoxName[i]))
                    onlyDigits += currentBoxName[i];
            }

            boxNum = Convert.ToInt32(onlyDigits);
        }

        //Cycle to the next box
        GameObject nextBox = GameObject.Find("Counter " + (boxNum + 1));
        tester.counter = nextBox.GetComponent<CollidersCounter>();
        BLFPacker.box = nextBox;
        BLFPacker.colliders_counter = nextBox.GetComponent<CollidersCounter>();
        counters.Add(nextBox);
        Debug.Log("Currently packing to " + nextBox.name);
        nextBox.GetComponent<CollidersCounter>().ResetWastes();
    }

    void LabelAlreadyPackedObjects(List<GameObject> alreadyPacked)
    {
        foreach (GameObject obj in alreadyPacked)
            obj.tag = "Packed Interactable";
    }

        #region unused code
        /*

        void GetResultsInwards()
        {
            if (fullWorkFlow) StoreInteractablePositions();
            var startTime = System.DateTime.Now;
            Vector3 iResults = BLFPacker.RunBLF(false); //runs the "inwards" BLF algorithm
            var funcTime = (System.DateTime.Now.Subtract(startTime));
            int numOfObjectsPacked = BLFPacker.colliders_counter.count;
            Debug.Log("Inwards BLF Results: 1.Pack. Eff.: " + iResults.z + ", 2. Num. of Obj packed: " + numOfObjectsPacked + ", 3. Time: " + funcTime.Seconds + ":" + funcTime.Milliseconds);
        }

        void GetResultsOutwards()
        {
            if (fullWorkFlow) StoreInteractablePositions();
            var startTime = System.DateTime.Now;
            Vector3 oResults = BLFPacker.RunBLF(true); //runs the "outwards" BLF algorithm
            var funcTime = (System.DateTime.Now.Subtract(startTime));
            int numOfObjectsPacked = BLFPacker.colliders_counter.count;
            Debug.Log("Outwards BLF Results: 1.Pack. Eff.: " + oResults.z + ", 2. Num. of Obj packed: " + numOfObjectsPacked + ", 3. Time: " + funcTime.Seconds + ":" + funcTime.Milliseconds);
        }

        void SelectAllInteractables() //Get all interactable objects
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

        void StoreInteractablePositions() //Store the objects positions to be able to reset them later
        {
            foreach (GameObject interactable in GameObject.FindGameObjectsWithTag("Interactable"))
            {
                objectsThatWerePacked.Add(interactable);
                packedObjectPositions.Add(interactable.transform.position);
                packedObjectRotations.Add(interactable.transform.rotation);
            }

            counter.ResetWastes();
        }
        */
        #endregion


}

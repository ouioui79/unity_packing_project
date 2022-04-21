using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLFPackingInwards : MonoBehaviour
{
    #region Variables 
    public bool fullWorkFlow;

    public bool outwards = false;

    public float cubeSize;
    public GameObject box;
    private int max_iter = 10;
    public CollidersCounter colliders_counter;

    [HideInInspector] public bool checkBetterPositions = false;
    [HideInInspector] public bool wiggleRoomBool = false;
    [HideInInspector] public bool rotateMeshBounds = false;

    // Object list used to keep track of object information including (id, original transformation)
    public List<ObjectStruct> object_list = new List<ObjectStruct>();

    // Object list used in BLF algorithm
    public List<GameObject> packing_objs = new List<GameObject>();

    private float[] Angles = new float[8] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

    public GameObject wallSix;

    public GameObject objOfInterest;

    #endregion

    #region Setup Methods

    public void SetUp(List<string> selected_objects)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Interactable");
        packing_objs = new List<GameObject>(selected_objects.Count);
        for (int i = 0; i < objects.Length; i++)
        {
            if (selected_objects.Contains(objects[i].name))
            {
                ObjectStruct obj = new ObjectStruct(i, objects[i], objects[i].transform.position, objects[i].transform.eulerAngles);
                object_list.Add(obj);
            }
        }

        if (!fullWorkFlow) colliders_counter = GameObject.Find("Counter").GetComponent<CollidersCounter>();
    }

    public void SetUpPacking(PackingChromosome chromosome)
    {
        packing_objs.Clear();

        int ry;
        //int rx;
        //int rz;
        for (int i = 0; i < chromosome.Length; i++)
        {
            int[] gene = chromosome.GetGene(i).Value as int[];
            //Debug.Log(gene[0]);
            GameObject obj = object_list[gene[0]].obj; //index out of range
            packing_objs.Add(obj);
            ry = gene[1] + gene[2] * 2 + gene[3] * 2 * 2;
            //rx = gene[4] + gene[5] * 2 + gene[6] * 2 * 2;
            //rz = gene[7] + gene[8] * 2 + gene[9] * 2 * 2;
            obj.transform.eulerAngles = new Vector3(object_list[gene[0]].orig_rot.x, object_list[gene[0]].orig_rot.y + Angles[ry], object_list[gene[0]].orig_rot.z);
            obj.transform.position = object_list[gene[0]].orig_pos;
        }
    }

    #endregion

    void Update()
    {
        if(objOfInterest == null) return;
        //Debug.Log("Obj of interest render bounds: " + objOfInterest.GetComponent<Renderer>().bounds.size);

        /*
        if(CheckWallCollision(objOfInterest, new List<string> { "wall2" }) == 1)
        {
            Debug.Log("colliding with wall 2");
        }
        */
    }

    public Vector3 RunBLFAbstract()
    {
        return RunBLF(outwards);
    }

    //Packages the objects and returns the weight/radiation/packingefficiency values. 
    // If outwards = true, runs the outwards algorithm (starting from the bottom left front corner and moving outwards)
    // If outwards = false, starts the object from the opposite diagonal corner and moves it inwards until it collides
    public Vector3 RunBLF(bool outwards)
    {
        Debug.Log("BOX NAME: " + box.name);
        if (outwards) PackOutwards(box, packing_objs, 0.01f, 0.01f, 0.01f);
        else PackInwards(box, packing_objs, 0.01f, 0.01f, 0.01f);

        colliders_counter.CalculateTotalProperties();
        float Weight = colliders_counter.Totalweight / colliders_counter.weightLimitation;
        float Radiation = colliders_counter.Totalradioactivity / colliders_counter.doseLimitation;
        float PackingEfficiency = colliders_counter.Totalvolume / colliders_counter.colliderVolume;

        return new Vector3(Weight, Radiation, PackingEfficiency);
    }

    void PackOutwards(GameObject box, List<GameObject> object_list, float x_offset, float y_offset, float z_offset)
    {
        Vector3 box_blfposition = box.GetComponent<BoxCollider>().transform.position; //Stores the position of the box that the objects are placed in
        Vector3 box_size = box.GetComponent<BoxCollider>().size;
        box_size = Vector3.Scale(box_size, box.transform.localScale); //Resizes the box's collider depending on its local scale

        //Box's (Counter game object) outwards and inwards corner's position in all three dimensions.
        //The outwards (far) corner is where the iteration begins. The inwards corner is where all the objects are moving towards.
        Vector3 boxOutwardsCorner = box_blfposition + (box_size / 2);
        Vector3 boxInwardsCorner = box_blfposition - (box_size / 2);

        float x;
        float y;
        float z;
        float init_x;
        float init_y;
        float init_z;
        int num_max_reached = 0;

        foreach (GameObject curr_obj in object_list)
        {
            Vector3 pos_outside = curr_obj.transform.position;
            Vector3 prev_pos;
            int iter = 1;
            Vector3 initialPosition = boxInwardsCorner;
            Vector3 maximumPosition = boxOutwardsCorner;

            while (iter <= max_iter)
            {
                //Debug.Log("Current object: " + curr_obj.name + " ITERATION: " + iter.ToString());
                prev_pos = curr_obj.transform.position;
                if (iter == 1)
                {
                    //Place object in the initial position, fix the position with CheckInitialCollision, and then set the fixed position as the initial position
                    curr_obj.transform.position = initialPosition;
                    CheckInitialCollisionOutwards(curr_obj); //-> different for outwards
                    initialPosition = curr_obj.transform.position;

                    //Place object in the maximum position, fix the position with CheckInitialCollisionInwards, and then set the fixed position as the initial position
                    curr_obj.transform.position = maximumPosition;
                    CheckInitialCollisionInwards(curr_obj); //-> different for outwards
                    maximumPosition = curr_obj.transform.position;

                    maximumPosition = new Vector3(maximumPosition.x, maximumPosition.y + 0.01f, maximumPosition.z); //This modification allows packing 27 control cubes instead of 18

                    x = initialPosition.x;
                    z = initialPosition.z;
                }
                else
                {
                    x = curr_obj.transform.position.x;
                    z = curr_obj.transform.position.z;
                }

               
                // Bottom
                y = initialPosition.y;
                curr_obj.transform.position = new Vector3(x, y, z);
                while (CheckCollision(curr_obj) == 1)
                {
                    y += y_offset;
                    if (y >= maximumPosition.y)
                    {
                        // Restore the best y position
                        y -= y_offset;
                        num_max_reached++;
                        break;
                    }
                    curr_obj.transform.position = new Vector3(x, y, z);
                }
                //Debug.Log("AFTER Y MODIFICATION: " + curr_obj.name + " position " + curr_obj.transform.position.ToString("F5"));

                // Left
                x = initialPosition.x;
                curr_obj.transform.position = new Vector3(x, y, z);
                while (CheckCollision(curr_obj) == 1)
                {
                    x += x_offset;
                    if (x > maximumPosition.x)
                    {
                        // Restore the best x position
                        x -= x_offset;
                        num_max_reached++;
                        break;
                    }
                    curr_obj.transform.position = new Vector3(x, y, z);
                }
                //Debug.Log("AFTER X MODIFICATION: " + curr_obj.name + " position " + curr_obj.transform.position.ToString("F5"));

                // Front
                z = initialPosition.z;
                curr_obj.transform.position = new Vector3(x, y, z);
                while (CheckCollision(curr_obj) == 1)
                {
                    z += z_offset;
                    if (z > maximumPosition.z)
                    {
                        // Restore the best z position
                        z -= z_offset;
                        num_max_reached++;
                        break;
                    }
                    curr_obj.transform.position = new Vector3(x, y, z);
                }
                //Debug.Log("AFTER Z MODIFICATION: " + curr_obj.name + " position " + curr_obj.transform.position.ToString("F5"));

                // If object couldn't be packed within the limits, move it outside the box and continue to the next object
                if (num_max_reached == 3)
                {
                    curr_obj.transform.position = pos_outside;
                    num_max_reached = 0;
                    //Debug.Log("this object " + curr_obj.name + " couldn't be packed");
                    break;
                }

                // If there isn't an improvement compared to the previous position, continue to the next object
                if (Vector3.Distance(prev_pos, curr_obj.transform.position) < 0.01) break;
                num_max_reached = 0;

                iter++;
            }
        }
    }

    void PackInwards(GameObject box, List<GameObject> object_list, float x_offset, float y_offset, float z_offset)
    {
        wallSix.SetActive(true);

        Vector3 box_blfposition = box.GetComponent<BoxCollider>().transform.position; //Stores the position of the box that the objects are placed in
        Vector3 box_size = box.GetComponent<BoxCollider>().size; 
        box_size = Vector3.Scale(box_size, box.transform.localScale); //Resizes the box's collider depending on its local scale

        //Box's (Counter game object) outwards and inwards corner's position in all three dimensions.
        //The outwards (far) corner is where the iteration begins. The inwards corner is where all the objects are moving towards.
        Vector3 boxOutwardsCorner = box_blfposition + (box_size / 2.00f);
        Vector3 boxInwardsCorner = box_blfposition - (box_size / 2.00f);

        foreach (GameObject curr_obj in object_list)
        {
            //Set transform to manipulate
            Transform manipulatedTransform = curr_obj.transform;
            bool pivotChanged = false;

            Vector3 pos_outside = manipulatedTransform.position; //outside position recorded, if box can't be fit it will be placed here in the end
            Vector3 prev_pos; //stores the 1 previous position of the object
            int iter = 1;
            Vector3 initialPosition = boxOutwardsCorner;

            //Fix the initial position

            //Place them initially using the renderer bounds
            Vector3 obj_bounds = curr_obj.GetComponent<Renderer>().bounds.size;

            manipulatedTransform.position = boxOutwardsCorner 
                                            - (obj_bounds / 2.00f); //Half of obj bounds because initially they are in the centre


            //Debug.Log("For " + curr_obj.name + " rend bnds and outwds crnr: " + obj_bounds + ", " + boxOutwardsCorner);
 
            // Debug.Log("For " + curr_obj.name + " FIX: Before pushAway: " + manipulatedTransform.position.x + ", " 
            //                                                             + manipulatedTransform.position.y + ", " 
            //                                                            + manipulatedTransform.position.z);
            PushObjectAwayFromWalls(curr_obj);
            
            //Debug.Log("For " + curr_obj.name + " FIX: After pushAway: " + manipulatedTransform.position.x + ", " 
            //                                                            + manipulatedTransform.position.y + ", " 
              //                                                          + manipulatedTransform.position.z);
            initialPosition = manipulatedTransform.position;

            //Place object, check further positions, if no further position and it is in collision move it out

            while (iter <= max_iter)
            {
                prev_pos = manipulatedTransform.position;
                Vector3 iteratedPosition = manipulatedTransform.position; //This Vector3 will store the iterated positions throughout the algorithm

                int count = 0;
                // BOTTOM (y-axis)
                while (CheckCollision(curr_obj) == 0) //While the object is not colliding with anything
                {
                    iteratedPosition.y -= y_offset;
                    manipulatedTransform.position = iteratedPosition;
                    //Debug.Log("Iterated position (y): " + manipulatedTransform.position);
                    count++;

                    if (count > 100)
                    {
                        Debug.Log("For " + curr_obj.name + " check collision reached 100 repetitions, break.");
                        break;
                    }
                }

                iteratedPosition.y += y_offset; //The object had just collided, move it back 1 step (this is the best position)
                manipulatedTransform.position = iteratedPosition;
                if (checkBetterPositions)
                {
                    CheckFurtherPositions(curr_obj, "y", pivotChanged); //Check future possible positions in the y-direction that might be blocked
                    iteratedPosition = manipulatedTransform.position;
                }
                
                count = 0;
                // LEFT (x-axis)
                while (CheckCollision(curr_obj) == 0)
                {
                    iteratedPosition.x -= x_offset;
                    manipulatedTransform.position = iteratedPosition;
                    count++;

                    if (count > 100)
                    {
                        Debug.Log("For " + curr_obj.name + " check collision reached 100 repetitions, break.");
                        break;
                    }
                }
                
                iteratedPosition.x += x_offset;
                manipulatedTransform.position = iteratedPosition;
                if (checkBetterPositions)
                {
                    CheckFurtherPositions(curr_obj, "x", pivotChanged); //Check future possible positions in the x-direction that might be blocked
                    iteratedPosition = manipulatedTransform.position;
                }
                
                count = 0;
                // FRONT (z-axis)
                while (CheckCollision(curr_obj) == 0)
                {
                    iteratedPosition.z -= z_offset;
                    manipulatedTransform.position = iteratedPosition;
                    count++;

                    if (count > 100)
                    {
                        Debug.Log("For " + curr_obj.name + " check collision reached 100 repetitions, break.");
                        break;
                    }
                }
                
                iteratedPosition.z += z_offset;
                manipulatedTransform.position = iteratedPosition;
                if (checkBetterPositions)
                {
                    CheckFurtherPositions(curr_obj, "z", pivotChanged); //Check future possible positions in the z-direction that might be blocked
                    iteratedPosition = manipulatedTransform.position;
                }

                if (Vector3.Distance(prev_pos, manipulatedTransform.position) < 0.01)
                {
                    Debug.Log("For " + curr_obj.name + " no improvement found, break");
                    break; //If there is no improvement, no need to continue running the algorithm
                }
                
                if (iter == max_iter) //After all the iterations, if the objects final position is WORSE than its first one, move the object out
                {
                    //Least points = better score (more towards the inner corner)
                    float initPoints = initialPosition.x + initialPosition.y + initialPosition.z;
                    float finalPoints = manipulatedTransform.position.x + manipulatedTransform.position.y + manipulatedTransform.position.z;
                    //Debug.Log("the object " + curr_obj.name + " reached iteration 10."); 
                    if (initPoints <= finalPoints)
                    {
                        Debug.Log("For " + curr_obj.name + " final position was worse than first one, so moved it out.");
                        Debug.Log("For " + curr_obj.name + "init pos: " + initialPosition);
                        Debug.Log("For " + curr_obj.name + "final pos: " + manipulatedTransform.position);
                        manipulatedTransform.position = pos_outside;
                    }
                }
                
                iter++;
            }

            curr_obj.GetComponent<Rigidbody>().isKinematic = false;
        }

        wallSix.SetActive(false);
    }

    void PushObjectAwayFromWalls(GameObject curr_obj)
    {
        Vector3 iteratedPosition = curr_obj.transform.position;

        int count = 0;
        while(CheckWallCollision(curr_obj, new List<string> { "wall2" }) == 1)
        {
            iteratedPosition.z -= 0.01f;
            curr_obj.transform.position = iteratedPosition;
            
            count++;
            if (count > 100) 
            {
                Debug.Log("For " + curr_obj.name + " reached max count here.");
                break;
            }
        }

        count = 0;
        while(CheckWallCollision(curr_obj, new List<string> { "wall3" }) == 1)
        {
            iteratedPosition.x -= 0.01f;
            curr_obj.transform.position = iteratedPosition;
            
            count++;
            if (count > 100) 
            {
                Debug.Log("For " + curr_obj.name + " reached max count here.");
                break;
            }
        }

        count = 0;
        while(CheckWallCollision(curr_obj, new List<string> { "wall6" }) == 1)
        {
            iteratedPosition.y -= 0.01f;
            curr_obj.transform.position = iteratedPosition;
            
            count++;
            if (count > 100) 
            {
                Debug.Log("For " + curr_obj.name + " reached max count here.");
                break;
            }
        }

        //iteratedPosition.y += 0.03f;
        //curr_obj.transform.position = iteratedPosition;
    }

    void CheckFurtherPositions(GameObject curr_obj, string direction, bool pivotChanged)
    {
        Transform manipulatedTransform = curr_obj.transform;
        if (pivotChanged && curr_obj.transform.parent.name.Contains("(Pivot)")) manipulatedTransform = curr_obj.transform.parent;

        Vector3 stopPoint = new Vector3();
        Vector3 bestPosition = curr_obj.transform.position;
        List<Vector3> possiblePositions = new List<Vector3>();
        Vector3 castDirection = new Vector3(0, 0, 0);
        float rayLength = 1f;

        //Set the direction, if the input is anything else break
        if (direction == "x")
            castDirection = new Vector3(-1, 0, 0);
        else if (direction == "y")
            castDirection = new Vector3(0, -1, 0);
        else if (direction == "z")
            castDirection = new Vector3(0, 0, -1);
        else
            return;

        //Cast a ray between where the curr_obj is and where the stopping wall will be
        RaycastHit hit;
        if (Physics.Raycast(manipulatedTransform.position, castDirection, out hit, rayLength, LayerMask.GetMask("Ignore Raycast")))
        {
            if (hit.collider != null && hit.collider.gameObject != curr_obj)
            {
                //Debug.Log("From obj: " + curr_obj.name + " the ray hit " + hit.collider.name + " at pos " + hit.point);
                stopPoint = hit.point;
            }
        }

        //Generate points inbetween curr_obj and the wall at 0.01f distanced intervals
        if (direction == "x")
        {
            int numOfIterations = Mathf.RoundToInt((manipulatedTransform.position.x - hit.point.x) / 0.01f);
            for (int i = 0; i < numOfIterations; i++)
            {
                float xPos = manipulatedTransform.position.x - (0.01f * i);
                possiblePositions.Add(new Vector3(xPos, manipulatedTransform.position.y, manipulatedTransform.position.z));

                if (wiggleRoomBool)
                {
                    possiblePositions.Add(new Vector3(xPos, manipulatedTransform.position.y + 0.01f, manipulatedTransform.position.z));
                    possiblePositions.Add(new Vector3(xPos, manipulatedTransform.position.y - 0.01f, manipulatedTransform.position.z));
                    possiblePositions.Add(new Vector3(xPos, manipulatedTransform.position.y, manipulatedTransform.position.z + 0.01f));
                    possiblePositions.Add(new Vector3(xPos, manipulatedTransform.position.y, manipulatedTransform.position.z - 0.01f));
                }
            }
        }
        else if (direction == "y")
        {
            int numOfIterations = Mathf.RoundToInt((manipulatedTransform.position.y - hit.point.y) / 0.01f);
            for (int i = 0; i < numOfIterations; i++)
            {
                float yPos = manipulatedTransform.position.y - (0.01f * i);
                possiblePositions.Add(new Vector3(manipulatedTransform.position.x, yPos, manipulatedTransform.position.z));

                if (wiggleRoomBool)
                {
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x + 0.01f, yPos, manipulatedTransform.position.z));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x - 0.01f, yPos, manipulatedTransform.position.z));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x, yPos, manipulatedTransform.position.z + 0.01f));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x, yPos, manipulatedTransform.position.z - 0.01f));
                }
            }
        }
        else if (direction == "z")
        {
            int numOfIterations = Mathf.RoundToInt((manipulatedTransform.position.z - hit.point.z) / 0.01f);
            for (int i = 0; i < numOfIterations; i++)
            {
                float zPos = manipulatedTransform.position.z - (0.01f * i);
                possiblePositions.Add(new Vector3(manipulatedTransform.position.x, manipulatedTransform.position.y, zPos));

                if (wiggleRoomBool)
                {
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x + 0.01f, manipulatedTransform.position.y, zPos));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x - 0.01f, manipulatedTransform.position.y, zPos));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x, manipulatedTransform.position.y + 0.01f, zPos));
                    possiblePositions.Add(new Vector3(manipulatedTransform.position.x, manipulatedTransform.position.y - 0.01f, zPos));
                }
            }
        }

        for (int i = 0; i < possiblePositions.Count; i++)
        {
            manipulatedTransform.position = possiblePositions[i]; //Set object at possible position
            float totalBestPosition = bestPosition.x + bestPosition.y + bestPosition.z;
            float totalPossiblePosition = possiblePositions[i].x + possiblePositions[i].y + possiblePositions[i].z;

            //if it doesn't collide and it is a better position, it is the new best position
            //better position = sum of x,y,z components are smaller
            if (CheckCollision(curr_obj) == 0 && totalPossiblePosition < totalBestPosition) bestPosition = possiblePositions[i];
        }

        manipulatedTransform.position = bestPosition;
    }

    #region Collision Check Methods

    //This method checks if the obj is in collision with anything else
    public int CheckCollision(GameObject obj)
    {
        Vector3 obj_bounds = Vector3.Scale(obj.GetComponent<MeshFilter>().sharedMesh.bounds.size, obj.transform.localScale);

        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals(box.name));
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
                    if (Physics.ComputePenetration(col, 
                                                   obj.transform.position,
                                                   obj.transform.rotation, 
                                                   curr_colliding, 
                                                   curr_colliding.transform.position,
                                                   curr_colliding.transform.rotation, 
                                                   out dir, 
                                                   out dis))
                    {
                        return 1;
                    }
                }
            }
        }
        return 0;
    }

    //This method checks if obj is colliding with any of the walls in the list, returns 1 if true, 0 otherwise 
    int CheckWallCollision(GameObject obj, List<string> wallNames) 
    {
        Vector3 obj_bounds = Vector3.Scale(obj.GetComponent<MeshFilter>().sharedMesh.bounds.size, obj.transform.localScale);

        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals(box.name));
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
                    if (Physics.ComputePenetration(col, 
                                                   obj.transform.position,
                                                   obj.transform.rotation, 
                                                   curr_colliding, 
                                                   curr_colliding.transform.position,
                                                   curr_colliding.transform.rotation, 
                                                   out dir, 
                                                   out dis))
                    {
                        if(wallNames.Contains(hitcollider.name))
                        {
                            //Debug.Log("obj " + obj.name + " was colliding with wall " + hitcollider.name);
                            return 1;
                        } 
                    }
                }
            }
        }
        return 0;
    }

    //(NOT USED ANYMORE!) This is the old CheckInitialCollison method, new methods specifically for Outwards and Inwards BLF Algorithm has been developed
    void CheckInitialCollision(GameObject obj)
    {
        Vector3 obj_bounds = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size * obj.transform.localScale.x;
        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals(box.name));

        Collider wall_col = colliding_objs.Find(s => s.gameObject.name.Equals("wall2"));

        if (wall_col != null)
        {
            Vector3 dir;
            float dis;
            Collider[] obj_cols = obj.GetComponents<Collider>();
            foreach (Collider col in obj_cols)
            {
                while (Physics.ComputePenetration(col, obj.transform.position,
                    obj.transform.rotation, wall_col, wall_col.transform.position,
                    wall_col.transform.rotation, out dir, out dis))
                {
                    obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z - 0.01f);
                } 
            }
        }

        wall_col = colliding_objs.Find(s => s.gameObject.name.Equals("wall3"));

        if (wall_col != null)
        {
            Vector3 dir;
            float dis;
            Collider[] obj_cols = obj.GetComponents<Collider>();
            foreach (Collider col in obj_cols)
            {
                while (Physics.ComputePenetration(col, obj.transform.position,
                    obj.transform.rotation, wall_col, wall_col.transform.position,
                    wall_col.transform.rotation, out dir, out dis))
                {
                    obj.transform.position = new Vector3(obj.transform.position.x - 0.01f, obj.transform.position.y, obj.transform.position.z);
                }
            }
        }

        wall_col = colliding_objs.Find(s => s.gameObject.name.Equals("wall6"));

        if (wall_col != null)
        {
            Vector3 dir;
            float dis;
            Collider[] obj_cols = obj.GetComponents<Collider>();
            foreach (Collider col in obj_cols)
            {
                while (Physics.ComputePenetration(col, obj.transform.position,
                    obj.transform.rotation, wall_col, wall_col.transform.position,
                    wall_col.transform.rotation, out dir, out dis))
                {
                    obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - 0.01f, obj.transform.position.z);
                }
            }
        }
    }

    //(NOT USED ANYMORE!) This method is used with the Inwards BLF Algorithm. It "fixes" the initial positions of the objects that will be packed
    void CheckInitialCollisionInwards(GameObject obj)
    {
        int count = 0;

        wallSix.SetActive(true); //Activate top wall to be checked for collisions (deactivated later so that the objects inside the box can be seen)
        bool needToCheckCol = true; //Only works if this is true, is set the false at the end IF the object is not colliding with any walls

        Vector3 obj_bounds = Vector3.Scale(obj.GetComponent<MeshFilter>().sharedMesh.bounds.size, obj.transform.localScale);
        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals(box.name));

        Collider wall_col_6 = colliding_objs.Find(s => s.gameObject.name.Equals("wall6"));
        Collider wall_col_2 = colliding_objs.Find(s => s.gameObject.name.Equals("wall2"));
        Collider wall_col_3 = colliding_objs.Find(s => s.gameObject.name.Equals("wall3"));

        //These walls will be checked for collision at the end, if they are the function will be repeated
        List<string> wallsToBeChecked = new List<string> { "wall6", "wall2", "wall3" }; 
        while(needToCheckCol)
        {
            if (wall_col_6 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    int countStop = 0;
                    while (Physics.ComputePenetration(col, 
                                                      obj.transform.position,
                                                      obj.transform.rotation, 
                                                      wall_col_6, 
                                                      wall_col_6.transform.position,
                                                      wall_col_6.transform.rotation, 
                                                      out dir, 
                                                      out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x, 
                                                             obj.transform.position.y - 0.01f, 
                                                             obj.transform.position.z);

                        countStop++;
                        if (countStop > 100) 
                        {
                            Debug.Log("Reached countStop limit (wall 6)");
                            break;
                        }
                    }
                }

                count++;
            }

            if (wall_col_2 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    int countStop = 0;
                    while (Physics.ComputePenetration(col, 
                                                      obj.transform.position,
                                                      obj.transform.rotation, 
                                                      wall_col_2, 
                                                      wall_col_2.transform.position,
                                                      wall_col_2.transform.rotation, 
                                                      out dir, 
                                                      out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x, 
                                                             obj.transform.position.y, 
                                                             obj.transform.position.z - 0.01f);
                        
                        countStop++;
                        if (countStop > 100) 
                        {
                            Debug.Log("Reached countStop limit (wall 2)");
                            break;
                        }
                    }
                    //if (obj.name == "O10") Debug.Log("stopped colliding with wall2 at " + obj.transform.position);
                }

                count++;
            }

            if (wall_col_3 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    int countStop = 0;
                    while (Physics.ComputePenetration(col, 
                                                      obj.transform.position,
                                                      obj.transform.rotation, 
                                                      wall_col_3, 
                                                      wall_col_3.transform.position,
                                                      wall_col_3.transform.rotation, 
                                                      out dir, 
                                                      out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x - 0.01f, 
                                                             obj.transform.position.y, 
                                                             obj.transform.position.z);
                    
                        countStop++;
                        if (countStop > 100) 
                        {
                            Debug.Log("Reached countStop limit (wall 3)");
                            break;
                        }
                    }
                }

                count++;
            }

            needToCheckCol = false; //false means the function will not loop
            if (CheckWallCollision(obj, wallsToBeChecked) == 1) needToCheckCol = true; //If the object is still colliding with walls, the loop will continue
            count++;

            if (count > 100)
            {
                Debug.Log("The object " + obj.name + " reached the total count limit!"); ///return a different number
                break;
            }
        }
    }

    //(NOT USED ANYMORE!) The Outwards BLF Algorithm has been outdated for a while, this method was meant to be used with it, fixing the initial
    //positions of that objects that are packed
    void CheckInitialCollisionOutwards(GameObject obj)
    {
        bool needToCheckCol = true; //Only works if this is true, is set the false at the end IF the object is not colliding with any walls

        //Vector3 obj_bounds = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size * obj.transform.localScale.x;
        Vector3 obj_bounds = Vector3.Scale(obj.GetComponent<MeshFilter>().sharedMesh.bounds.size, obj.transform.localScale);
        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals(box.name));

        Collider wall_col_5 = colliding_objs.Find(s => s.gameObject.name.Equals("wall5"));
        Collider wall_col_4 = colliding_objs.Find(s => s.gameObject.name.Equals("wall4"));
        Collider wall_col_1 = colliding_objs.Find(s => s.gameObject.name.Equals("wall1"));

        //These walls will be checked for collision at the end, if they are the function will be repeated
        List<string> wallsToBeChecked = new List<string> { "wall5", "wall4", "wall1" };
        while (needToCheckCol)
        {
            if (wall_col_5 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    while (Physics.ComputePenetration(col, obj.transform.position,
                        obj.transform.rotation, wall_col_5, wall_col_5.transform.position,
                        wall_col_5.transform.rotation, out dir, out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + 0.01f, obj.transform.position.z);
                    }
                }
            }

            if (wall_col_4 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    while (Physics.ComputePenetration(col, obj.transform.position,
                        obj.transform.rotation, wall_col_4, wall_col_4.transform.position,
                        wall_col_4.transform.rotation, out dir, out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x + 0.01f, obj.transform.position.y, obj.transform.position.z);
                    }
                    //if (obj.name == "O10") Debug.Log("stopped colliding with wall2 at " + obj.transform.position);
                }
            }

            if (wall_col_1 != null)
            {
                Vector3 dir;
                float dis;
                Collider[] obj_cols = obj.GetComponents<Collider>();
                foreach (Collider col in obj_cols)
                {
                    while (Physics.ComputePenetration(col, obj.transform.position,
                        obj.transform.rotation, wall_col_1, wall_col_1.transform.position,
                        wall_col_1.transform.rotation, out dir, out dis))
                    {
                        obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z + 0.01f);
                    }
                }
            }

            needToCheckCol = false; //false means the function will not loop
            if (CheckWallCollision(obj, wallsToBeChecked) == 1) needToCheckCol = true; //If the object is still colliding with walls, the loop will continue
        }
    }

    #endregion
}


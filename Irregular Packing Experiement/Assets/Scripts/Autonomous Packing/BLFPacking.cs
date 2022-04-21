using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLFPacking : MonoBehaviour
{
    public GameObject box;

    private int max_iter = 10;

    // Object list used to keep track of object information including (id, original transformation)
    private List<ObjectStruct> object_list = new List<ObjectStruct>();

    // Object list used in BLF algorithm
    private List<GameObject> packing_objs = new List<GameObject>();

    private float[] Angles = new float[8] {0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f};

    private CollidersCounter colliders_counter;
    // Start is called before the first frame update
    public void SetUp(List<string> selected_objects)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Interactable");
        packing_objs = new List<GameObject>(selected_objects.Count);
        for (int i = 0; i < objects.Length; i++)
        {
            if (selected_objects.Contains(objects[i].name))
            {
                ObjectStruct obj = new ObjectStruct(i, objects[i], objects[i].transform.position, objects[i].transform.eulerAngles);
                object_list.Add(obj); //The objects in this list are packed with the GA
            }
        }

        colliders_counter = GameObject.Find("Counter").GetComponent<CollidersCounter>();
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
            GameObject obj = object_list[gene[0]].obj;
            packing_objs.Add(obj);
            ry = gene[1] + gene[2] * 2 + gene[3] * 2 * 2;
            //rx = gene[4] + gene[5] * 2 + gene[6] * 2 * 2;
            //rz = gene[7] + gene[8] * 2 + gene[9] * 2 * 2;
            obj.transform.eulerAngles = new Vector3(object_list[gene[0]].orig_rot.x, object_list[gene[0]].orig_rot.y + Angles[ry], object_list[gene[0]].orig_rot.z);
            obj.transform.position = object_list[gene[0]].orig_pos;
        }
    }
    public Vector3 RunBLF()
    {
        Pack(box, packing_objs, 0.01f, 0.01f, 0.01f);
        colliders_counter.CalculateTotalProperties();
        float Weight = colliders_counter.Totalweight / colliders_counter.weightLimitation;
        float Radiation = colliders_counter.Totalradioactivity / colliders_counter.doseLimitation;
        float PackingEfficiency = colliders_counter.Totalvolume / colliders_counter.colliderVolume;
        
        return new Vector3(Weight, Radiation, PackingEfficiency);
    }

    void Pack(GameObject box, List<GameObject> object_list, float x_offset, float y_offset, float z_offset)
    {
        float x;
        float y;
        float z;
        float init_x;
        float init_y;
        float init_z;
        float x_max;
        float y_max;
        float z_max;
        int num_max_reached = 0;
        Vector3 box_blfposition = box.GetComponent<BoxCollider>().transform.position;
        Vector3 box_size = box.GetComponent<BoxCollider>().size;
        box_size.x = box_size.x * box.transform.localScale.x;
        box_size.y = box_size.y * box.transform.localScale.y;
        box_size.z = box_size.z * box.transform.localScale.z;


        foreach (GameObject curr_obj in object_list)
        {
            Vector3 pos_outside = curr_obj.transform.position;
            Vector3 prev_pos;
            int iter = 1;

            while (iter <= max_iter)
            {
                //Debug.Log("Current object: " + curr_obj.name + " ITERATION: " + iter.ToString());
                prev_pos = curr_obj.transform.position;
                if (iter == 1)
                {
                    x = box_blfposition.x + box_size.x / 2 -
                        curr_obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
                        curr_obj.transform.localScale.x / 2;
                    z = box_blfposition.z + box_size.z / 2 -
                        curr_obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.z *
                        curr_obj.transform.localScale.z / 2;
                }
                else
                {
                    x = curr_obj.transform.position.x;
                    z = curr_obj.transform.position.z;
                }
                
                init_x = box_blfposition.x - box_size.x / 2;
                init_y = box_blfposition.y - box_size.y / 2;
                init_z = box_blfposition.z - box_size.z / 2;
                
                y_max = box_blfposition.y + box_size.y / 2 -
                        curr_obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.y *
                        curr_obj.transform.localScale.x / 2;
                x_max = box_blfposition.x + box_size.x / 2 -
                        curr_obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
                        curr_obj.transform.localScale.x / 2;
                z_max = box_blfposition.z + box_size.z / 2 -
                        curr_obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.z *
                        curr_obj.transform.localScale.z / 2;
                
                
                curr_obj.transform.position = new Vector3(x_max, y_max, z_max);

                if (iter == 1)
                {
                    CheckInitialCollision(curr_obj);
                    x = curr_obj.transform.position.x;
                    z = curr_obj.transform.position.z;
                    x_max = curr_obj.transform.position.x;
                    y_max = curr_obj.transform.position.y;
                    z_max = curr_obj.transform.position.z;

                    //Debug.Log("LIMIT POSITION: " + curr_obj.transform.position.ToString("F5"));
                }
                
                // Bottom
                y = init_y;
                curr_obj.transform.position = new Vector3(x, y, z);
                while (CheckCollision(curr_obj) == 1)
                {
                    y += y_offset;
                    if (y > y_max)
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
                x = init_x;
                curr_obj.transform.position = new Vector3(x, y, z);

                while (CheckCollision(curr_obj) == 1)
                {
                    x += x_offset;
                    if (x > x_max)
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
                z = init_z;
                curr_obj.transform.position = new Vector3(x, y, z);
                while(CheckCollision(curr_obj) == 1)
                {
                    z += z_offset;
                    if (z > z_max)
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
                    break;
                }
                
                // If there isn't an improvement compared to the previous position, continue to the next object
                if (Vector3.Distance(prev_pos, curr_obj.transform.position) < 0.01) break;
                num_max_reached = 0;
                
                iter++;
            }
        }
    }

    int CheckCollision(GameObject obj)
    {
        Vector3 obj_bounds;
        obj_bounds.x = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * obj.transform.localScale.x;
        obj_bounds.y = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * obj.transform.localScale.y;
        obj_bounds.z = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size.z * obj.transform.localScale.z;

        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
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
                        //Debug.Log("COLLIDING WITH: " + curr_colliding.name);
                        return 1;
                    }
                }
            }
        }
        
        return 0;
    }

    void CheckInitialCollision(GameObject obj)
    {
        Vector3 obj_bounds = obj.GetComponent<MeshFilter>().sharedMesh.bounds.size * obj.transform.localScale.x;
        float r = Mathf.Max(obj_bounds.x, obj_bounds.y, obj_bounds.z);
        Vector3 c = obj.transform.position;

        List<Collider> colliding_objs = new List<Collider>(Physics.OverlapSphere(c, r));
        colliding_objs.RemoveAll(s => s.gameObject.name.Equals(obj.name) || s.gameObject.name.Equals("Counter"));

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
}

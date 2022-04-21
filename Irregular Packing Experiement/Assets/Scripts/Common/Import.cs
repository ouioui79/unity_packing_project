﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using System;
 using System.IO;
using BzKovSoft.ObjectSlicer.Samples;
 using NewtonVR;
 using Plawius.NonConvexCollider;
 using UnityEngine.UI;

 public class Import : MonoBehaviour
{
    public Material default_mat;
    private int counter = 0;
    private FileInfo[] files;
    public GameObject parentObject;
    private int num_meshes =3; //number of meshes imported
    private int max_num_meshes;
    public Timer t;
    private int[] imported_meshes;
    public GameObject num_meshes_field;
    public GameObject import_button;
    private Text num_meshes_text;
    public GameObject decrement_button;
    public GameObject increment_button;
    private Vector3 play_area_pos;
   // public GameObject import_config_button;
    public CollidersCounter colliders_counter;

    void Start()
    {
        string myPath = "C:/Projects after July 2021/LabMesh";
        DirectoryInfo dir = new DirectoryInfo(myPath);
        files = dir.GetFiles("*.obj");
        Debug.Log("There are " + files.Length + " meshes in the folder.");
        max_num_meshes = files.Length;
        imported_meshes = new int[max_num_meshes];
        for (int i = 0; i < max_num_meshes; ++i)
        {
            imported_meshes[i] = 0;
        }
        num_meshes_text = GameObject.Find("NumMeshes").GetComponentInChildren<Text>();
        play_area_pos = GameObject.Find("Segmentation Ruler").transform.position;
    }

    public void ImportNumMeshes(int num)
    {
        if (num > max_num_meshes)
        {
            Debug.LogError("Not enough meshes in the folder.");
            return;
        }
        else if (max_num_meshes < 1)
        {
            Debug.LogError("No meshes in the folder.");
            return;
        }

        while (counter < num)
        {
            Debug.Log("Importing mesh #" + (counter + 1));
            int index = UnityEngine.Random.Range(0, max_num_meshes - 1);
            if (imported_meshes[index] == 0)
            {
                CreateMesh(index);
                counter++;
                imported_meshes[index] = 1;
            }
        }
        import_button.SetActive(false);
        num_meshes_field.SetActive(false);
        increment_button.SetActive(false);
        decrement_button.SetActive(false);
        //  import_config_button.SetActive(false);
        t.SetUpTimer(num);
        colliders_counter.ResetWastes();
    }


    public void ImportMeshes()
    {
        if (num_meshes > max_num_meshes)
        {
            Debug.LogError( "Not enough meshes in the folder." );
            return;
        } else if (max_num_meshes < 1)
        {
            Debug.LogError("No meshes in the folder.");
            return;
        } 
        
        while(counter < num_meshes)
        {
            Debug.Log("Importing mesh #" + (counter + 1));
            int index = UnityEngine.Random.Range(0, max_num_meshes - 1);
            if (imported_meshes[index] == 0)
            {
                CreateMesh(index);
                counter++;
                imported_meshes[index] = 1;
            }
        }
        import_button.SetActive(false);
        num_meshes_field.SetActive(false);
        increment_button.SetActive(false);
        decrement_button.SetActive(false);
      //  import_config_button.SetActive(false);
        t.SetUpTimer(num_meshes);
        colliders_counter.ResetWastes();
    }

    public void CreateMesh(int index)
    {
        GameObject new_mesh = new GameObject(files[index].Name);
        new_mesh.transform.parent = parentObject.transform;
        var rb = new_mesh.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var mf = new_mesh.AddComponent<MeshFilter>();
        var renderer = new_mesh.AddComponent<MeshRenderer>();
        var oss = new_mesh.AddComponent<ObjectSlicerSample>();
        var property = new_mesh.AddComponent<Property>();
        var interactable_item = new_mesh.AddComponent<NVRInteractableItem>();
        
        ObjImporter objImporter = new ObjImporter();
        mf.mesh = objImporter.ImportFile(files[index].FullName);
        renderer.material = default_mat;
        oss.defaultSliceMaterial = default_mat;
        property.SetUp();
        AddNonConvexColliders(mf.mesh, new_mesh);
       
        Vector3 random_pos = new Vector3(UnityEngine.Random.Range(play_area_pos.x , play_area_pos.x + 1.0f), play_area_pos.y+1.0f, UnityEngine.Random.Range(play_area_pos.z - 1.5f, play_area_pos.z));
        new_mesh.transform.position = random_pos;
        
        new_mesh.tag = "Interactable";

        GameObject holder = GameObject.Find("Imported Objects Holder");
        if (holder != null) new_mesh.transform.SetParent(holder.transform);
    }

    public void DecrementNumMeshes()
    {
        if (num_meshes == 1)
        {
            Debug.Log("Can't import less than 1 mesh!");
        }
        else
        {
            num_meshes--;
        }
        num_meshes_text.text = num_meshes.ToString();
    }
    public void IncrementNumMeshes()
    {
        if (num_meshes == max_num_meshes)
        {
            Debug.Log("Can't import more meshes than the max!");
        }
        else
        {
            num_meshes++;
        }
        num_meshes_text.text = num_meshes.ToString();
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using BzKovSoft.ObjectSlicer.Samples;
using Plawius.NonConvexCollider;
using UnityEngine.UI;
using NewtonVR;

public class ImportFromFiles : MonoBehaviour {

    public Material default_mat;
    public GameObject parentObject;
    private int num_meshes = 15;
    public Timer t;
    public GameObject num_meshes_field;
    public GameObject import_button;
    public GameObject decrement_button;
    public GameObject increment_button;
    private Vector3 play_area_pos;
    public GameObject import_config_button;
    public CollidersCounter colliders_couter;

	private List<string> filePaths = new List<string>();
	private List<Vector3> objPoses = new List<Vector3>();
	private List<Quaternion> objRots = new List<Quaternion>();
 
	void Start ()
	{
		play_area_pos = GameObject.Find("PlayArea").transform.position;
		var path = "C:/projects/meshes/";
		var dataset = Resources.Load<TextAsset>("Config");
        /*if (dataset == null)
        {
            Debug.LogError("No dataset");
        }*/
        
		string[,] data = CSVReader.SplitCsvGrid(dataset.text);
		Debug.Log(data.GetLength(1));
		for (int i = 1; i < data.GetLength(1); i++)
		{
			if (data[0, i] != null)
			{
				if (System.IO.File.Exists(path + data[0, i]))
				{
					filePaths.Add(path + data[0, i]);
					objPoses.Add(new Vector3(float.Parse(data[1, i]),float.Parse(data[2, i]),float.Parse(data[3, i])));
					objRots.Add(Quaternion.Euler(float.Parse(data[4, i]),float.Parse(data[5, i]),float.Parse(data[6, i])));
					Debug.Log(path + data[0, i]);
					Debug.Log(data[1, i] + ", " + data[2, i] + ", " + data[3, i] + ", " +data[4, i] + ", " + data[5, i] + ", " +data[6, i]);
				}
			}
		}
	}

	public void ImportFromConfig()
	{
		for(int i = 0; i < filePaths.Count; i++) {
			string path = filePaths[i];
			FileInfo f = new FileInfo(path);
			GameObject new_mesh = new GameObject(f.Name);
			new_mesh.transform.parent = parentObject.transform;
			var rb = new_mesh.AddComponent<Rigidbody>();
			rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

			var mf = new_mesh.AddComponent<MeshFilter>();
			var renderer = new_mesh.AddComponent<MeshRenderer>();
			var oss = new_mesh.AddComponent<ObjectSlicerSample>();
			var property = new_mesh.AddComponent<Property>();
            var interactable_item = new_mesh.AddComponent<NVRInteractableItem>();

            ObjImporter objImporter = new ObjImporter();
			mf.mesh = objImporter.ImportFile(f.FullName);
			renderer.material = default_mat;
			oss.defaultSliceMaterial = default_mat;
			property.SetUp();
			AddNonConvexColliders(mf.mesh, new_mesh);
        
			Vector3 random_pos = new Vector3(UnityEngine.Random.Range(play_area_pos.x-1.0f, play_area_pos.x+1.0f), play_area_pos.y, UnityEngine.Random.Range(play_area_pos.z - 1.0f, play_area_pos.z + 1.0f));
			new_mesh.transform.position = random_pos;
			new_mesh.tag = "Interactable";
			new_mesh.transform.position = objPoses[i];
			new_mesh.transform.rotation = objRots[i];
		}
        num_meshes = GameObject.FindGameObjectsWithTag("Interactable").Length;
        import_button.SetActive(false);
        num_meshes_field.SetActive(false);
        increment_button.SetActive(false);
        decrement_button.SetActive(false);
        import_config_button.SetActive(false);
        t.SetUpTimer(num_meshes);
        colliders_couter.ResetWastes();
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
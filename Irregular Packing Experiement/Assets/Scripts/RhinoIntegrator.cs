using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rhino.Compute;
using Rhino;

public class RhinoIntegrator : MonoBehaviour
{
    public string authToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwIjoiUEtDUyM3IiwiYyI6IkFFU18yNTZfQ0JDIiwiYjY0aXYiOiIzUUljS2hwWFlOQTRrWm1xMnVXRVl3PT0iLCJiNjRjdCI6ImVqOGY3dVdKTVhpNTBrMzA4azBGNmRGU3BsOEJiYkVrRiswOUtPZi9HVGxOYVV6TEZ6VWM1M3dSZ2YvaGNHc3VqdE9jTGEydmF0Zm4wd1FOVXZvRGRaaTA1NUVTUXRia2pZQ2ZlVzMwTWtKQXcvRkxRbVJuazd3aVdtZjFDMTRtUnF4MHpUcTZ4UnFKYVZLWkhBUkhrbENJUjdTVkQ5Vm9QV3UwMkxjS1lDVmNDZDdWdzZIQUlMVEVJTTBRZ2xaVjBlV1JOTXlPYURzTjBQeTNXLzRGbnc9PSIsImlhdCI6MTYwNTg1NzAwN30.rFKeBtJSRwBrbc4SmcTsGc30umOuHhz6swt4dH8rrt8";

    private Rhino.FileIO.File3dm model;
    public GameObject[] objects;
    private BoxCollider m_Collider;
    
    // Start is called before the first frame update
    void Start()
    {
        ComputeServer.AuthToken = authToken;
        m_Collider = GetComponent<BoxCollider>();
    }

    public void ExportToRhino()
    {
        Export();
    }

    private void Export()
    {
        objects = GameObject.FindGameObjectsWithTag("Interactable");
        model = new Rhino.FileIO.File3dm();

        string path = Application.dataPath + "/../packing_configuration" + System.DateTime.Now + ".3dm";
        foreach (GameObject obj in objects)
        {
            if (m_Collider.bounds.Contains(obj.transform.position))
            {
                Mesh mesh = obj.GetComponent<MeshFilter>().mesh;

                // Get vertices from Unity mesh object
                var vertices = mesh.vertices;
                Rhino.Geometry.Mesh rhino_mesh = new Rhino.Geometry.Mesh();
                // Transform each vertex for Rhino mesh type
                for (var i = 0; i < vertices.Length; i++)
                {
                    Vector3 world_v = obj.transform.TransformPoint(vertices[i]);
                    // z and y are switched as Rhino uses right-handed axes convention and Unity uses left-handed axes convention
                    var pt = new Rhino.Geometry.Point3d(world_v.x, world_v.z, world_v.y);
                    rhino_mesh.Vertices.Add(pt);

                }

                // Create faces for Rhino mesh
                var faces = mesh.triangles;
                for (int i = 0; i < faces.Length; i = i + 3)
                {
                    rhino_mesh.Faces.AddFace(faces[i], faces[i + 1], faces[i + 2]);
                }
                //var info = Rhino.Compute.AreaMassPropertiesCompute.Compute(rhino_mesh);
                model.Objects.AddMesh(rhino_mesh);
            }
        }
        
        model.Write(path, 5);
    }
}

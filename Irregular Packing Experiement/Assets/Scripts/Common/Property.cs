using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Property : MonoBehaviour
{
    // Start is called before the first frame update
    public float _obj_weight = 0.0f;
    public float _obj_volume = 0.0f;
    public float _radioactivity = 0.0f;
    //bool nonMeshVolumeCalculate;
    //NonConvexMeshCollider nonConvexMeshCollider;
    //float volume;
    private Vector3 localCenterOfGravity;
    private Vector3 centerofgravitytotal = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 centerOfGravity;
    public int lowOrHighRadiation = 0;

    private Mesh _mesh;
    public void SetUp()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _obj_volume = VolumeOfMesh(_mesh);
        _obj_weight = WeightCalculation(_obj_volume);
        _radioactivity = RadioactivityCalculation(_obj_volume);
        
        string msg = "object: " + this.name + "volume: " + _obj_volume + "\n weight: " + _obj_weight + "\n radioactivity: " + _radioactivity;
        //Debug.Log(msg);

        //Debug.Log(this.name + "'s mesh has " + _mesh.triangles.Length + " triangles and " + _obj_volume + " volume.");
    }

    public float WeightCalculation(float volume)
    {
        float weight = 0.0f;
        if (gameObject.name.Contains("pipe"))
        {
            weight = 7850 * volume;
        }
        else
        {
            weight = 2400 * volume;
        }
        
        return weight;
    }

    public float RadioactivityCalculation(float volume)
    {
        float radioactivity = 0f;
        if (gameObject.name.Contains("pipe"))
        {
            radioactivity = 24f * volume;
        }
        else
            radioactivity = 12f * volume;
        return radioactivity;
    }
    
    float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {                                         
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    float VolumeOfMesh(Mesh m)
    {
        float v = 0;
        Vector3[] vertices = m.vertices;
        int[] triangles = m.triangles;
        for (int i = 0; i < m.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            v += SignedVolumeOfTriangle(p1, p2, p3);
        }
        v = v * gameObject.transform.localScale.x * gameObject.transform.localScale.y * gameObject.transform.localScale.z;

        return Mathf.Abs(v);
    }

    void Start()
    {
        SetUp();
    }
    void Update()
    {
        centerOfGravity = gameObject.transform.position;
    }

    public float GetVolume()
    {
        return _obj_volume;
    }

    public float GetWeight()
    {
        return _obj_weight;
    }

    public float GetRadioactivity()
    {
        return _radioactivity;
    }

    public void RecalculateProperties()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        _obj_volume = VolumeOfMesh(_mesh);
        _obj_weight = WeightCalculation(_obj_volume);
        _radioactivity = RadioactivityCalculation(_obj_volume);
    }
}
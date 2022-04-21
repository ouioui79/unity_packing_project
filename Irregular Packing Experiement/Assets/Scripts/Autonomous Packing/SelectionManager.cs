using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewtonVR;

public class SelectionManager : MonoBehaviour
{
    private List<string> selections = new List<string>();
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material defaultMaterial;
    NVRPlayer pleyer;

    public void SelectObject(GameObject currently_interacting)
    {
        if (!selections.Contains(currently_interacting.name))
        {
            var selectionRenderer = currently_interacting.GetComponent<MeshRenderer>();
            selectionRenderer.material = highlightMaterial;
            currently_interacting.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            selections.Add(currently_interacting.name);
        }
    }

    public void DeselectObject(GameObject currently_interacting)
    {
        if (selections.Contains(currently_interacting.name))
        {
            var selectionRenderer = currently_interacting.GetComponent<MeshRenderer>();
            selectionRenderer.material = defaultMaterial;
            currently_interacting.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            selections.Remove(currently_interacting.name);
        }
    }

    public List<string> GetSelections()
    {
        return selections;
    }
}
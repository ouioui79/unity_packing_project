using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
    public class PlaneSlicer : MonoBehaviour
    {
        private Plane cutting_plane = new Plane(Vector3.zero, Vector3.zero);
        private GameObject selection;
        // Start is called before the first frame update
        private SelectionManager selection_manager;

        void Start()
        {
            selection_manager = gameObject.GetComponent<SelectionManager>();
        }

        public void PrepareSegmentation(Vector3 n, Vector3 d, GameObject s)
        {
            selection = s;
            //Debug.Log("PrepareSegmentation: the two vectors are " + n + " and " + d);
            cutting_plane.SetNormalAndPosition(n, d);
        }
    
        public void SliceObject()
        {
            if (selection != null)
            {
                //selection_manager.DeselectObject();
                var sliceableA = selection.GetComponentInParent<IBzSliceableNoRepeat>(); //no component?
                var sliceId = SliceIdProvider.GetNewSliceId();
                if (sliceableA != null)
                    sliceableA.Slice(cutting_plane, sliceId, null);
                else
                    Debug.Log("sliceableA is null");
            }
        }
    }

}

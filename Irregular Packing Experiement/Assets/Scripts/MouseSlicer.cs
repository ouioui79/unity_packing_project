﻿using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Test class for demonstration purpose
	/// </summary>
	public class MouseSlicer : MonoBehaviour
	{
		private SelectionManagerSeg segmentation_selection_manager;
		private LineRenderer line_rend;
		private GameObject selection;
		private Vector3 first_point_2d = new Vector3(0,0,0);
		private Vector3 second_point_2d = new Vector3(0,0,0);
		private Vector3 third_point_2d = new Vector3(0, 0, 0);
		private Vector3 first_point;
		private Vector3 second_point;
		private Vector3 third_point;
		private bool first_set = false;
		private bool second_set = false;
		private bool set_plane = false;

		private float distance;

		private void Start()
		{
			line_rend = GetComponent<LineRenderer>();
			segmentation_selection_manager = gameObject.GetComponent<SelectionManagerSeg>();
		}

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (set_plane)
				{
					if (!first_set)
					{
						first_set = true;
						first_point_2d = Input.mousePosition;
						first_point_2d.z = 0.5f;
						first_point = Camera.main.ScreenToWorldPoint(first_point_2d);
					}
					else if (!second_set)
					{
						second_set = true;
						second_point_2d = Input.mousePosition;
						second_point_2d.z = 0.5f;
						second_point = Camera.main.ScreenToWorldPoint(second_point_2d);

						third_point_2d = (first_point_2d + second_point_2d) * 0.5f;
						third_point_2d.z = 4;
						third_point = Camera.main.ScreenToWorldPoint(third_point_2d);
					
						line_rend.SetPosition(0, first_point);
						line_rend.SetPosition(1, second_point);
					}	
				}
			}

			selection = segmentation_selection_manager.GetSelection();
		}

		public void SliceObject()
		{
			if (selection != null)
			{
				segmentation_selection_manager.DeselectObject();
				var sliceableA = selection.GetComponentInParent<IBzSliceableNoRepeat>();
				var sliceId = SliceIdProvider.GetNewSliceId();
					
				Plane plane = new Plane(first_point, second_point, third_point);
				
				if (sliceableA != null)
					sliceableA.Slice(plane, sliceId, null);
			}

			first_set = false;
			second_set = false;
			set_plane = false;
		}

		public void SetPlane()
		{
			set_plane = true;
			first_set = false;
			second_set = false;
		}
	}
}
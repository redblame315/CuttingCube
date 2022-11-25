using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Direct mesh slice
	/// </summary>
	public class SampleManualSlicer : MonoBehaviour
	{
		public GameObject _target;

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				// prepare data
				var meshFilter = _target.GetComponent<MeshFilter>();
				var meshRenderer = _target.GetComponent<MeshRenderer>();
				var mesh = meshFilter.mesh;
				Plane plane = new Plane(Vector3.up, 0);
				Material[] materials = meshRenderer.sharedMaterials;
				Material sectionMaterial = new Material(Shader.Find("Diffuse"));
				var adapter = new BzManualMeshAdapter(mesh.vertices);

				// slice mesh
				var meshDissector = new BzMeshDataDissector(mesh, plane, materials, adapter, BzSliceConfiguration.GetDefault());
				meshDissector.DefaultSliceMaterial = sectionMaterial;
				SliceResult sliceResult = meshDissector.Slice();

				// apply result back to our object
				if (sliceResult == SliceResult.Sliced)
				{
					var result = meshDissector.SliceResultNeg;
					meshFilter.mesh = result.GenerateMesh();
					meshRenderer.materials = result.Materials;
				}
			}
		}
	}
}
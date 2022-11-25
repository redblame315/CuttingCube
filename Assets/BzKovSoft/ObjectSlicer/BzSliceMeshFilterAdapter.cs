using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public class BzSliceMeshFilterAdapter : IBzSliceAdapter, IXmlSerializable
	{
		Matrix4x4 _l2w;
		Matrix4x4 _w2l;
		Vector3[] _vertices;

		public BzSliceMeshFilterAdapter()
		{

		}

		public BzSliceMeshFilterAdapter(Vector3[] vertices, MeshRenderer renderer)
		{
			_vertices = vertices;
			_l2w = renderer.transform.localToWorldMatrix;
			_w2l = renderer.transform.worldToLocalMatrix;
		}

		public Vector3 GetWorldPos(int index)
		{
			Vector3 position = _vertices[index];
			return _l2w.MultiplyPoint3x4(position);
		}

		public Vector3 GetLocalPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			return _l2w.MultiplyPoint3x4(meshData.Vertices[index]);
		}

		public Vector3 InverseTransformDirection(Vector3 p)
		{
			return _w2l.MultiplyPoint3x4(p + _l2w.MultiplyPoint3x4(Vector3.zero));
		}

		public bool Check(BzMeshData meshData)
		{
			return true;
		}

		public void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer)
		{
			var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			meshRenderer.sharedMaterials = materials;
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return _l2w.MultiplyPoint3x4(Vector3.zero);
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadToDescendant("l2w");

			var l2wSerializer = new XmlSerializer(typeof(Matrix4x4), new XmlRootAttribute("l2w"));
			_l2w = (Matrix4x4)l2wSerializer.Deserialize(reader);

			var _vertSerializer = new XmlSerializer(typeof(Vector3[]), new XmlRootAttribute("vertices"));
			_vertices = (Vector3[])_vertSerializer.Deserialize(reader);
		}

		public void WriteXml(XmlWriter writer)
		{
			var l2wSerializer = new XmlSerializer(typeof(Matrix4x4), new XmlRootAttribute("l2w"));
			l2wSerializer.Serialize(writer, _l2w);

			var _vertSerializer = new XmlSerializer(typeof(Vector3[]), new XmlRootAttribute("vertices"));
			_vertSerializer.Serialize(writer, _vertices);
		}
	}
}
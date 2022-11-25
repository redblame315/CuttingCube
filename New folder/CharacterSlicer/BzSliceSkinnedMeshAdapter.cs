using BzKovSoft.ObjectSlicer;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace BzKovSoft.CharacterSlicer
{
	public class BzSliceSkinnedMeshAdapter : IBzSliceAdapter, IXmlSerializable
	{
		private Vector3 _position;
		private Matrix4x4 _w2l;
		private Matrix4x4 _l2w;
		private Vector3[] _vertices;

		private Matrix4x4[] _charToW;
		private BoneWeight[] _boneWeights;

		public BzSliceSkinnedMeshAdapter(SkinnedMeshRenderer renderer)
		{
			_position = renderer.gameObject.transform.position;
			var mesh = renderer.sharedMesh;
			_vertices = mesh.vertices;

			var bones = renderer.bones;
			var bindposes = mesh.bindposes;
			_charToW = new Matrix4x4[bones.Length];
			_w2l = renderer.gameObject.transform.worldToLocalMatrix;
			_l2w = renderer.gameObject.transform.localToWorldMatrix;
			for (int i = 0; i < bones.Length; i++)
			{
				var tr = bones[i];
				if (tr == null)
					continue;

				var toW = tr.localToWorldMatrix * bindposes[i];
				_charToW[i] = toW;
			}

			_boneWeights = mesh.boneWeights;
		}

		public Vector3 GetWorldPos(int index)
		{
			Vector3 position = _vertices[index];
			var boneWeight = _boneWeights[index];

			Vector3 newPosition = Vector3.zero;
			if (boneWeight.weight0 > 0f)
				newPosition += _charToW[boneWeight.boneIndex0].MultiplyPoint3x4(position) * boneWeight.weight0;
			if (boneWeight.weight1 > 0f)
				newPosition += _charToW[boneWeight.boneIndex1].MultiplyPoint3x4(position) * boneWeight.weight1;
			if (boneWeight.weight2 > 0f)
				newPosition += _charToW[boneWeight.boneIndex2].MultiplyPoint3x4(position) * boneWeight.weight2;
			if (boneWeight.weight3 > 0f)
				newPosition += _charToW[boneWeight.boneIndex3].MultiplyPoint3x4(position) * boneWeight.weight3;

			return newPosition;
		}

		public Vector3 GetLocalPos(BzMeshData meshData, int index)
		{
			var v = GetWorldPos(meshData, index);
			return _w2l.MultiplyPoint3x4(v);
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			Vector3 position = meshData.Vertices[index];
			var boneWeight = meshData.BoneWeights[index];

			Vector3 newPosition = Vector3.zero;
			if (boneWeight.weight0 > 0f)
				newPosition += _charToW[boneWeight.boneIndex0].MultiplyPoint3x4(position) * boneWeight.weight0;
			if (boneWeight.weight1 > 0f)
				newPosition += _charToW[boneWeight.boneIndex1].MultiplyPoint3x4(position) * boneWeight.weight1;
			if (boneWeight.weight2 > 0f)
				newPosition += _charToW[boneWeight.boneIndex2].MultiplyPoint3x4(position) * boneWeight.weight2;
			if (boneWeight.weight3 > 0f)
				newPosition += _charToW[boneWeight.boneIndex3].MultiplyPoint3x4(position) * boneWeight.weight3;

			return newPosition;
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
			((SkinnedMeshRenderer)meshRenderer).sharedMesh = mesh;
			meshRenderer.sharedMaterials = materials;
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return _position;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadToDescendant("position");

			var serializer = new XmlSerializer(typeof(Vector3), new XmlRootAttribute("position"));
			_position = (Vector3)serializer.Deserialize(reader);

			serializer = new XmlSerializer(typeof(Vector3[]), new XmlRootAttribute("vertices"));
			_vertices = (Vector3[])serializer.Deserialize(reader);

			serializer = new XmlSerializer(typeof(Matrix4x4[]), new XmlRootAttribute("charToW"));
			_charToW = (Matrix4x4[])serializer.Deserialize(reader);

			serializer = new XmlSerializer(typeof(BoneWeight[]), new XmlRootAttribute("boneWeights"));
			_boneWeights = (BoneWeight[])serializer.Deserialize(reader);
		}

		public void WriteXml(XmlWriter writer)
		{
			var serializer = new XmlSerializer(typeof(Vector3), new XmlRootAttribute("position"));
			serializer.Serialize(writer, _position);

			serializer = new XmlSerializer(typeof(Vector3[]), new XmlRootAttribute("vertices"));
			serializer.Serialize(writer, _vertices);

			serializer = new XmlSerializer(typeof(Matrix4x4[]), new XmlRootAttribute("charToW"));
			serializer.Serialize(writer, _charToW);

			serializer = new XmlSerializer(typeof(BoneWeight[]), new XmlRootAttribute("boneWeights"));
			serializer.Serialize(writer, _boneWeights);
		}
	}
}
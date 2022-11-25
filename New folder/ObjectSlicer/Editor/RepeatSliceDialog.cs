using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Editor
{
	public class RepeatSliceDialog : EditorWindow
	{
		private const string _adapterElementName = "adapter";
		public BzSliceableBase _sliceableFrom;
		public BzSliceableBase _sliceableTo;
		string _dataFilePath;
		string _meshFilePath;

		[MenuItem("Window/BzSoft/MeshSlicer/Repeat Slice")]
		private static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(RepeatSliceDialog), false, "Repeat Slice");
		}

		void OnGUI()
		{
			Handles.BeginGUI();

			_sliceableFrom = (BzSliceableBase)EditorGUILayout.ObjectField("Sliced to save", _sliceableFrom, typeof(BzSliceableBase), true);

			if (GUILayout.Button("Save data"))
			{
				string filePath = SaveDataFileDialog();
				if (filePath != null)
				{
					var sliceTry = _sliceableFrom.lastSuccessfulSlice;
					SaveSliceData(filePath, sliceTry);
				}
			}

			if (GUILayout.Button("Save mesh"))
			{
				string filePath = SaveMeshFileDialog();
				if (filePath != null)
				{
					var mf = _sliceableFrom.GetComponent<MeshFilter>();
					Mesh m = mf.mesh;
					string tmpPath = "Assets/my_super_mesh.asset";
					AssetDatabase.CreateAsset(m, tmpPath);
					File.Move(tmpPath, filePath);
				}
			}

			_sliceableTo = (BzSliceableBase)EditorGUILayout.ObjectField("Applay slice to", _sliceableTo, typeof(BzSliceableBase), true);
			if (GUILayout.Button("Apply slice"))
			{
				string filePath = OpenFileDialog();
				if (filePath != null)
				{
					Vector3 position;
					Quaternion rotation;
					Plane plane;
					var adapters = new List<IBzSliceAdapter>();
					ReadSavedData(filePath, adapters, out position, out rotation, out plane);

					Vector3 tmpPosition = _sliceableTo.transform.position;
					Quaternion tmpRotation = _sliceableTo.transform.rotation;
					_sliceableTo.transform.position = position;
					_sliceableTo.transform.rotation = rotation;
					_sliceableTo.RepeatSlice(plane, adapters.ToArray());
					_sliceableTo.transform.position = tmpPosition;
					_sliceableTo.transform.rotation = tmpRotation;
				}
			}

			Handles.EndGUI();
		}

		private static void SaveSliceData(string filePath, SliceTry sliceTry)
		{
			IBzSliceAdapter[] adapters = new IBzSliceAdapter[sliceTry.items.Length];

			using (XmlWriter writer = XmlWriter.Create(filePath, new XmlWriterSettings { Encoding = Encoding.UTF8 }))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("root");

				SerializeObject("position", sliceTry.position, writer);
				SerializeObject("rotation", sliceTry.position, writer);
				SerializeObject("plane", sliceTry.sliceData.plane, writer);

				writer.WriteStartElement("adapters");
				for (int i = 0; i < sliceTry.items.Length; i++)
				{
					// save adapter
					var adapter = sliceTry.items[i].meshDissector.Adapter;
					adapters[i] = adapter;

					writer.WriteStartElement(_adapterElementName);
					writer.WriteElementString("adapterType", adapter.GetType().AssemblyQualifiedName);
					SerializeObject("adapterData", adapter, writer);
					writer.WriteEndElement();
				}
				writer.WriteEndElement(); // end "adapters" element
				writer.WriteEndDocument();
			}
		}

		private static void ReadSavedData(string filePath, List<IBzSliceAdapter> adapters, out Vector3 position, out Quaternion rotation, out Plane plane)
		{
			position = Vector3.zero;
			rotation = Quaternion.identity;
			plane = new Plane();

			using (var reader = XmlReader.Create(filePath))
			{
				reader.ReadToDescendant("root");
				while (!reader.EOF)
				{
					if (reader.NodeType != XmlNodeType.Element)
					{
						reader.Read();
						continue;
					}
					switch (reader.Name)
					{
						case "position":
							position = ReadObject<Vector3>("position", reader);
							break;

						case "rotation":
							rotation = ReadObject<Quaternion>("rotation", reader);
							break;

						case "plane":
							plane = ReadObject<Plane>("plane", reader);
							break;

						case _adapterElementName:
							do
							{
								ReadAdapter(adapters, reader);
							}
							while ((reader.Name == _adapterElementName && reader.IsStartElement()) ||
								reader.ReadToNextSibling(_adapterElementName));
							break;

						default:
							reader.Read();
							break;
					}
				}
			}
		}

		private static void ReadAdapter(List<IBzSliceAdapter> adapters, XmlReader reader)
		{
			using (var r = reader.ReadSubtree())
			{
				string adapterTypeStr = null;
				IBzSliceAdapter adapter = null;
				while (!r.EOF)
				{
					if (r.NodeType != XmlNodeType.Element)
					{
						r.Read();
						continue;
					}

					switch (r.Name)
					{
						case "adapterType":
							adapterTypeStr = r.ReadElementContentAsString();
							break;

						case "adapterData":
							if (adapterTypeStr == null)
							{
								throw new InvalidOperationException("adapterType should be declared first");
							}
							Type adapterType = Type.GetType(adapterTypeStr);


							using (var rr = reader.ReadSubtree())
							{
								object o = new XmlSerializer(adapterType, new XmlRootAttribute(r.Name)).Deserialize(rr);
								adapter = (IBzSliceAdapter)o;
							}
							break;

						default:
							r.Read();
							break;
					}
				}

				if (adapter == null)
				{
					throw new InvalidOperationException("adapterData element not found");
				}
				adapters.Add(adapter);
			}
		}

		private static T ReadObject<T>(string paramName, XmlReader reader)
		{
			using (var rr = reader.ReadSubtree())
			{
				while (!rr.IsStartElement())
				{
					rr.Read();
				}
				object o = new XmlSerializer(typeof(T), new XmlRootAttribute(paramName)).Deserialize(rr);
				Debug.Log("#### " + paramName + " ## " + o.ToString());
				return (T)o;
			}
		}

		private static void SerializeObject(string name, object o, XmlWriter writer)
		{
			var serializer = new XmlSerializer(o.GetType(), new XmlRootAttribute(name));
			serializer.Serialize(writer, o);
		}

		private string SaveDataFileDialog()
		{
			string dir = null;
			try
			{
				dir = Path.GetDirectoryName(_dataFilePath);
			}
			catch { }

			string filePath = EditorUtility.SaveFilePanel("Save XML slice data", dir, "SliceData", "xml");
			if (!string.IsNullOrEmpty(filePath))
			{
				_dataFilePath = filePath;
				return _dataFilePath;
			}
			return null;
		}

		private string SaveMeshFileDialog()
		{
			string dir = null;
			try
			{
				dir = Path.GetDirectoryName(_meshFilePath);
			}
			catch { }

			string filePath = EditorUtility.SaveFilePanel("Save Mesh", dir, "mesh", "asset");
			if (!string.IsNullOrEmpty(filePath))
			{
				_meshFilePath = filePath;
				return _meshFilePath;
			}
			return null;
		}

		private string OpenFileDialog()
		{
			string dir = null;
			try
			{
				dir = Path.GetDirectoryName(_dataFilePath);
			}
			catch { }

			string filePath = EditorUtility.OpenFilePanel("Open XML slice data", dir, "xml");
			if (!string.IsNullOrEmpty(filePath))
			{
				_dataFilePath = filePath;
				return _dataFilePath;
			}
			return null;
		}
	}
}
using BzKovSoft.ObjectSlicer.EventHandlers;
using BzKovSoft.ObjectSlicer.Polygon;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class BzSliceableBase : MonoBehaviour, IBzSliceable
	{
		/// <summary>
		/// Material that will be applied after slicing
		/// </summary>
		public Material defaultSliceMaterial;
		public bool asynchronously = false;
		public bool useLazyRunner;

		Queue<SliceTry> _sliceTrys;
		[SerializeField]
		[HideInInspector]
		public SliceTry lastSuccessfulSlice;

		private void OnEnable()
		{
			_sliceTrys = new Queue<SliceTry>();
		}

		/// <summary>
		/// Start slicing process
		/// </summary>
		/// <param name="addData">You can pass any object. You will </param>
		/// <returns>Returns true if pre-slice conditions was succeeded and task was added to the queue</returns>
		private void StartSlice(BzSliceTryData sliceTryData, IBzSliceAdapter[] adapters, Action<BzSliceTryResult> callBack)
		{
			Renderer[] renderers = GetRenderers(gameObject);
			SliceTryItem[] items = new SliceTryItem[renderers.Length];

			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];

				var adapterAndMesh = GetAdapterAndMesh(renderer);

				if (adapterAndMesh == null)
					continue;

				Mesh mesh = adapterAndMesh.mesh;
				IBzSliceAdapter adapter = adapters == null ? adapterAndMesh.adapter : adapters[i];

				var configuration = renderer.gameObject.GetComponent<BzSliceConfiguration>();
				var confDto = configuration == null ? BzSliceConfiguration.GetDefault() : configuration.GetDto();
				var meshDissector = new BzMeshDataDissector(mesh, sliceTryData.plane, renderer.sharedMaterials, adapter, confDto);
				meshDissector.DefaultSliceMaterial = defaultSliceMaterial;

				SliceTryItem sliceTryItem = new SliceTryItem();
				sliceTryItem.meshRenderer = renderer;
				sliceTryItem.meshDissector = meshDissector;
				items[i] = sliceTryItem;
			}

			SliceTry sliceTry = new SliceTry();
			sliceTry.items = items;
			sliceTry.callBack = callBack;
			sliceTry.sliceData = sliceTryData;
			sliceTry.position = transform.position;
			sliceTry.rotation = transform.rotation;

			if (asynchronously)
			{
				StartWorker(WorkForWorker, sliceTry);
				_sliceTrys.Enqueue(sliceTry);
			}
			else
			{
				Work(sliceTry);
				SliceTryFinished(sliceTry);
			}
		}

		protected abstract AdapterAndMesh GetAdapterAndMesh(Renderer renderer);

		/// <summary>
		/// You need to override this to use your thead pool
		/// </summary>
		/// <param name="method">method that you need to call</param>
		/// <param name="obj">object that you need to pass to method</param>
		protected virtual void StartWorker(Action<object> method, object obj)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(method), obj);
		}

		void WorkForWorker(object obj)
		{
			try
			{
				var sliceTry = (SliceTry)obj;
				Work(sliceTry);
				sliceTry.Finished = true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
		}

		void Work(SliceTry sliceTry)
		{
			bool somethingOnNeg = false;
			bool somethingOnPos = false;
			for (int i = 0; i < sliceTry.items.Length; i++)
			{
				var sliceTryItem = sliceTry.items[i];

				if (sliceTryItem == null)
					continue;

				var meshDissector = sliceTryItem.meshDissector;
				sliceTryItem.SliceResult = meshDissector.Slice();

				if (sliceTryItem.SliceResult == SliceResult.Neg |
					sliceTryItem.SliceResult == SliceResult.Duplicate |
					sliceTryItem.SliceResult == SliceResult.Sliced)
				{
					somethingOnNeg = true;
				}

				if (sliceTryItem.SliceResult == SliceResult.Pos |
					sliceTryItem.SliceResult == SliceResult.Duplicate |
					sliceTryItem.SliceResult == SliceResult.Sliced)
				{
					somethingOnPos = true;
				}
			}

			sliceTry.sliced = somethingOnNeg & somethingOnPos;

			if (sliceTry.sliced)
			{
				sliceTry.sliceData.componentManager.OnSlicedWorkerThread(sliceTry.items);
			}

			OnSliceFinishedWorkerThread(sliceTry.sliced, sliceTry.sliceData.addData);
		}

		/// <summary>
		/// Called when sliced process finished. If the flag Asynchronously is true, then this method will also called asynchronously
		/// </summary>
		protected virtual void OnSliceFinishedWorkerThread(bool sliced, object addData) { }

		void Update()
		{
			Profiler.BeginSample("GetFinishedTask");
			var sliceTry = GetFinishedTask();
			Profiler.EndSample();

			if (sliceTry == null)
				return;

			Profiler.BeginSample("SliceTryFinished");
			SliceTryFinished(sliceTry);
			Profiler.EndSample();
		}

		private void SliceTryFinished(SliceTry sliceTry)
		{
			BzSliceTryResult result = null;
			if (sliceTry.sliced)
			{
				Profiler.BeginSample("ApplyChanges");
				result = ApplyChanges(sliceTry);
				Profiler.EndSample();
			}

			if (result == null)
			{
				result = new BzSliceTryResult(false, sliceTry.sliceData.addData);
			}
			else
			{
				lastSuccessfulSlice = sliceTry;
				result.outObjectNeg.GetComponent<BzSliceableBase>().lastSuccessfulSlice = sliceTry;
				result.outObjectPos.GetComponent<BzSliceableBase>().lastSuccessfulSlice = sliceTry;

				Profiler.BeginSample("InvokeEvents");
				InvokeEvents(result.outObjectNeg, result.outObjectPos);
				Profiler.EndSample();
			}

			Profiler.BeginSample("OnSliceFinished");
			OnSliceFinished(result);
			Profiler.EndSample();

			if (result.sliced)
			{
				var runnerNeg = result.outObjectNeg.GetComponent<LazyActionRunner>();
				if (runnerNeg != null)
					runnerNeg.RunLazyActions();
				var runnerPos = result.outObjectPos.GetComponent<LazyActionRunner>();
				if (runnerPos != null)
					runnerPos.RunLazyActions();
			}

			if (sliceTry.callBack != null)
			{
				Profiler.BeginSample("CallBackMethod");
				sliceTry.callBack(result);
				Profiler.EndSample();
			}
		}

		private void InvokeEvents(GameObject resultNeg, GameObject resultPos)
		{
			var events = resultNeg.GetComponents<IBzObjectSlicedEvent>();
			for (int i = 0; i < events.Length; i++)
				events[i].ObjectSliced(gameObject, resultNeg, resultPos);
		}

		private BzSliceTryResult ApplyChanges(SliceTry sliceTry)
		{
			// duplicate object
			GameObject resultObjNeg, resultObjPos;
			GetNewObjects(out resultObjNeg, out resultObjPos);
			var renderersNeg = GetRenderers(resultObjNeg);
			var renderersPos = GetRenderers(resultObjPos);
			if (useLazyRunner)
			{
				resultObjNeg.AddComponent<LazyActionRunner>();
				resultObjPos.AddComponent<LazyActionRunner>();
			}

			if (renderersNeg.Length != renderersPos.Length |
				renderersNeg.Length != sliceTry.items.Length)
			{
				// if something wrong happaned with object, and during slicing it was changed
				// reject this slice try
				return null;
			}

			Profiler.BeginSample("ComponentManager.OnSlicedMainThread");
			sliceTry.sliceData.componentManager.OnSlicedMainThread(resultObjNeg, resultObjPos, renderersNeg, renderersPos);
			Profiler.EndSample();

			BzSliceTryResult result = new BzSliceTryResult(true, sliceTry.sliceData.addData);
			result.meshItems = new BzMeshSliceResult[sliceTry.items.Length];

			for (int i = 0; i < sliceTry.items.Length; i++)
			{
				var sliceTryItem = sliceTry.items[i];
				if (sliceTryItem == null)
					continue;

				var rendererNeg = renderersNeg[i];
				var rendererPos = renderersPos[i];

				if (sliceTryItem.SliceResult == SliceResult.Sliced)
				{
					sliceTryItem.meshDissector.RebuildNegMesh(rendererNeg);
					sliceTryItem.meshDissector.RebuildPosMesh(rendererPos);

					var itemResult = GetItemResult(sliceTryItem, rendererNeg, rendererPos);
					result.meshItems[i] = itemResult;
				}

				if (sliceTryItem.SliceResult == SliceResult.Neg)
					DeleteRenderer(rendererPos);

				if (sliceTryItem.SliceResult == SliceResult.Pos)
					DeleteRenderer(rendererNeg);
			}

			result.outObjectNeg = resultObjNeg;
			result.outObjectPos = resultObjPos;

			return result;
		}

		/// <summary>
		/// Method must return two objects that will be negative and positive part of result
		/// </summary>
		protected virtual void GetNewObjects(out GameObject resultObjNeg, out GameObject resultObjPos)
		{
			resultObjNeg = this.gameObject;
			resultObjPos = Instantiate(this.gameObject, this.gameObject.transform.parent);

			resultObjPos.name = resultObjNeg.name + "_pos";
			resultObjNeg.name = resultObjNeg.name + "_neg";
		}

		private static void DeleteRenderer(Renderer renderer)
		{
			GameObject.Destroy(renderer);
			var mf = renderer.gameObject.GetComponent<MeshFilter>();
			if (mf != null)
			{
				GameObject.Destroy(mf);
			}
		}

		/// <summary>
		/// Prepare data that will bu used for slicing
		/// </summary>
		/// <param name="plane"></param>
		protected virtual BzSliceTryData PrepareData(Plane plane)
		{
			// colliders that will be participating in slicing
			var colliders = gameObject.GetComponentsInChildren<Collider>();

			// return data
			return new BzSliceTryData()
			{
				// componentManager: this class will manage components on sliced objects
				componentManager = new StaticComponentManager(gameObject, plane, colliders),
				plane = plane,
			};
		}

		/// <summary>
		/// Called when sliced process finished
		/// </summary>
		protected virtual void OnSliceFinished(BzSliceTryResult result)
		{

		}

		public void Slice(Plane plane, Action<BzSliceTryResult> callBack)
		{
			if (this == null)  // if this component was destroied
				return;

			if (defaultSliceMaterial == null)
				throw new InvalidOperationException("DefaultSliceMaterial == null");

			var data = PrepareData(plane);
			if (data == null)
			{
				if (callBack != null)
					callBack(null);
				return;
			}

			if (!data.componentManager.Success)
			{
				if (callBack != null)
					callBack(new BzSliceTryResult(false, data.addData));
				return;
			}

			StartSlice(data, null, callBack);
		}

		public void RepeatSlice(Plane plane, IBzSliceAdapter[] adapters)
		{
			var data = PrepareData(plane);
			if (data == null)
			{
				throw new InvalidOperationException("PrepareData returned null");
			}

			if (!data.componentManager.Success)
			{
				throw new InvalidOperationException("ComponentManager failure");
			}

			var asyncEnabled = asynchronously;
			asynchronously = false;
			StartSlice(data, adapters, null);
			asynchronously = asyncEnabled;
		}

		private SliceTry GetFinishedTask()
		{
			if (_sliceTrys.Count == 0)
				return null;

			var sliceTry = _sliceTrys.Peek();

			if (sliceTry == null || !sliceTry.Finished)
				return null;

			_sliceTrys.Dequeue();

			return sliceTry;
		}

		private static BzMeshSliceResult GetItemResult(SliceTryItem sliceTryItem, Renderer rendererNeg, Renderer rendererPos)
		{
			BzMeshSliceResult itemResult = new BzMeshSliceResult();
			itemResult.rendererNeg = rendererNeg;
			itemResult.rendererPos = rendererPos;

			if (sliceTryItem.meshDissector.Configuration.CreateCap)
			{
				var sliceEdgeNegResult = new BzSliceEdgeResult[sliceTryItem.meshDissector.CapsNeg.Count];
				for (int i = 0; i < sliceEdgeNegResult.Length; i++)
				{
					var edgeResult = MakeEdgeResult(sliceTryItem.meshDissector.CapsNeg[i]);
					sliceEdgeNegResult[i] = edgeResult;
				}
				itemResult.sliceEdgesNeg = sliceEdgeNegResult;

				var sliceEdgePosResult = new BzSliceEdgeResult[sliceTryItem.meshDissector.CapsPos.Count];
				for (int i = 0; i < sliceEdgePosResult.Length; i++)
				{
					var edgeResult = MakeEdgeResult(sliceTryItem.meshDissector.CapsPos[i]);
					sliceEdgePosResult[i] = edgeResult;
				}
				itemResult.sliceEdgesPos = sliceEdgePosResult;
			}

			return itemResult;
		}

		private static BzSliceEdgeResult MakeEdgeResult(PolyMeshData polyMeshData)
		{
			var result = new BzSliceEdgeResult();
			result.vertices = polyMeshData.vertices;
			result.normals = polyMeshData.normals;
			result.boneWeights = polyMeshData.boneWeights;
			return result;
		}

		private Renderer[] GetRenderers(GameObject gameObject)
		{
			return gameObject.GetComponentsInChildren<Renderer>();
		}

		public override string ToString()
		{
			// prevent from accessing the name in debuge mode.
			return GetType().Name;
		}

		protected class AdapterAndMesh
		{
			public IBzSliceAdapter adapter;
			public Mesh mesh;
		}
	}
}
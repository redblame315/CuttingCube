using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Runs some slice steps in different frames to avoid low frame rate
	/// </summary>
	[DisallowMultipleComponent]
	public class LazyActionRunner : MonoBehaviour
	{
		List<Action> _postponeActions;

		private void OnEnable()
		{
			_postponeActions = new List<Action>();
		}

		public void RunLazyActions()
		{
			if (_postponeActions == null)
				return;

			StartCoroutine(ProcessSlicePostponeActions(_postponeActions));
		}

		private IEnumerator ProcessSlicePostponeActions(List<Action> actions)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				yield return null;
				var action = actions[i];
				action();
			}

			Destroy(this);
		}

		public void AddLazyAction(Action action)
		{
			if (_postponeActions == null)
			{
				action();
			}
			else
			{
				_postponeActions.Add(action);
			}
		}
	}
}
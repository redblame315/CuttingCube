using System;
using UnityEngine;

namespace BzKovSoft.CharacterSlicer.Samples
{
	/// <summary>
	/// This component emits new character each time the last dead
	/// </summary>
	public class EnemyManager : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
		GameObject _enemyPrefab;

		[SerializeField]
		Transform _attachEnemiesTo;
#pragma warning restore 0649

		IDeadable _enemy;

		void Update()
		{
			if (_enemy == null)
			{
				var enemyGO = Instantiate(_enemyPrefab);
				enemyGO.transform.SetParent(_attachEnemiesTo, false);

				_enemy = enemyGO.GetComponent<IDeadable>();

				if (_enemy == null)
					throw new InvalidOperationException("Component IDeadable not found");

				return;
			}

			if (_enemy.IsDead)
			{
				var enemyGO = Instantiate(_enemyPrefab);
				enemyGO.transform.SetParent(_attachEnemiesTo, false);

				_enemy = enemyGO.GetComponent<IDeadable>();
			}
		}
	}
}
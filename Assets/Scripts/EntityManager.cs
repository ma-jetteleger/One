using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	public static EntityManager Instance;

	[SerializeField] private AnimationCurve _enemyMoveCurve = null;

	public Player Player { get; set; }
	public List<Enemy> Enemies { get; set; } = new List<Enemy>();

	private static Entity[] _entities;
	
	private float _enemyMoveValueModifier;
	private float _enemyMoveValueDamper;

	private void Awake()
	{
		Instance = this;
		
		_entities = FindObjectsOfType<Entity>();

		for (var i = 0; i < _entities.Length; i++)
		{
			if (_entities[i] is Player)
			{
				Player = _entities[i] as Player;

				continue;
			}

			_entities[i].Initialize();
		}

		Enemies = _entities.Where(x => x is Enemy).Select(x => x as Enemy).ToList();
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		_enemyMoveValueModifier += Time.deltaTime;

		if (_enemyMoveValueModifier > _enemyMoveCurve.length)
		{
			_enemyMoveValueModifier -= _enemyMoveCurve.length;
		}

		if (_enemyMoveValueModifier > 0f)
		{
			var value = _enemyMoveCurve.Evaluate(_enemyMoveValueModifier) * Time.deltaTime;

			for (var i = 0; i < Enemies.Count; i++)
			{
				if(!Enemies[i].gameObject.activeSelf)
				{
					continue;
				}

				Enemies[i].Move(value);
			}
		}
	}

	[Button]
	public static void PositionEntities()
	{
		_entities = FindObjectsOfType<Entity>();

		for (var i = 0; i < _entities.Length; i++)
		{
			_entities[i].Initialize();
		}
	}
}

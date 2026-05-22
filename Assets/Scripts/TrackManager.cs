using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
	public static TrackManager Instance;
	
	public static List<Track> PlayerTracks = new List<Track>();

	private static Track[] _playerTracks;
	private static Track[] _enemyTracks;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		InitializeTracks();

		for (var i = 0; i < _playerTracks.Length; i++)
		{
			_playerTracks[i].Initialize(
				i < _playerTracks.Length - 1 ? _playerTracks[i + 1] : null,
				i > 0 ? _playerTracks[i - 1] : null);
		}

		for (var i = 0; i < _enemyTracks.Length; i++)
		{
			_enemyTracks[i].Initialize(null, null);
		}

		var levels = FindObjectsOfType<Level>(); // eww

		for (var i = 0; i < levels.Length; i++)
		{
			levels[i].Initialize();
		}

		EntityManager.Instance.Player.Initialize();
	}

	private static void InitializeTracks()
	{
		var tracks = FindObjectsOfType<Track>(); // eww

		_playerTracks = tracks.Where(x => !x.EnemyTrack).OrderBy(x => x.transform.position.x).ToArray(); // eww
		_enemyTracks = tracks.Where(x => x.EnemyTrack).OrderBy(x => x.transform.position.x).ToArray(); // eww

		PlayerTracks = _playerTracks.ToList();
	}

	[Button]
	public static void TraceTracks()
	{
		InitializeTracks();

		for (var i = 0; i < _playerTracks.Length; i++)
		{
			_playerTracks[i].Trace(i < _playerTracks.Length - 1 ? _playerTracks[i + 1] : null);
		}

		for (var i = 0; i < _enemyTracks.Length; i++)
		{
			_enemyTracks[i].Trace(null);
		}
	}
}

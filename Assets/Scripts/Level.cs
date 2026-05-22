using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Level : MonoBehaviour
{
	[SerializeField] private bool _focusOnPlayer = false;
	[SerializeField] private float _focusSize = 0f;
	[SerializeField] private float _focusTime = 0f;
	[SerializeField] private Transform _cameraAnchor = null;
	[SerializeField] private AnimationCurve _focusCurve = null;
	[SerializeField] private Node _progressiveFocusStart = null;
	[SerializeField] private Node _progressiveFocusEnd = null;
	[SerializeField] private float _progressiveFocusSizeEnd = 0f;
	[SerializeField] private bool _companionMatchPlayerSpeed = false;
	[SerializeField] private GameObject _companionPrefab = null;
	
	public bool ProgressiveZoom
	{
		get
		{
			if(!_progressiveZoom)
			{
				_progressiveZoom = _progressiveFocusStart != null && _progressiveFocusEnd != null && _progressiveFocusStart.ReachedByPlayer && !_progressiveFocusEnd.ReachedByPlayer;
			}

			return _progressiveZoom;
		}
	}

	public bool FocusOnPlayer => _focusOnPlayer;
	public Node ProgressiveFocusStart => _progressiveFocusStart;
	public Node ProgressiveFocusEnd => _progressiveFocusEnd;
	public float FocusSize => _focusSize;
	public AnimationCurve FocusCurve => _focusCurve;
	public float ProgressiveFocusSizeEnd => _progressiveFocusSizeEnd;
	public bool CompanionMatchPlayerSpeed => _companionMatchPlayerSpeed;

	//public bool Active { get; set; }
	public Enemy Companion { get; set; }

	private bool _progressiveZoom;
	private Track _playerTrack;
	private Track _companionTrack;
	private Enemy[] _enemies;

	public void Initialize()
	{
		if (_playerTrack == null)
		{
			_playerTrack = GetComponentsInChildren<Track>(true).FirstOrDefault(x => !x.EnemyTrack);
			_companionTrack = GetComponentsInChildren<Track>(true).FirstOrDefault(x => x.EnemyTrack);
			_enemies = GetComponentsInChildren<Enemy>(true).Where(x => !x.Invincible).ToArray();
			Companion = GetComponentsInChildren<Enemy>(true).FirstOrDefault(x => x.Invincible);

			TryActivate();
		}
	}

	public void TryActivate()
	{
		if (EntityManager.Instance.Player.Level != this)
		{
			Deactivate();

			gameObject.SetActive(false);
		}
		else
		{
			Activate();
		}
	}

	public void Focus(bool instantiateCompanion = false)
	{
		if(_focusOnPlayer)
		{
			if (instantiateCompanion && Companion == null && _companionPrefab != null)
			{
				Companion = Instantiate(_companionPrefab, _companionTrack.transform).GetComponent<Enemy>();
				Companion.StartNode = _companionTrack.Nodes[0];
				Companion.Invincible = true;
				Companion.MovementSpeed = 9.5f;
				Companion.MatchPlayerSpeed = _companionMatchPlayerSpeed;
				Companion.transform.position = Companion.StartNode.transform.position;

				EntityManager.Instance.Enemies.Add(Companion);

				Companion.Initialize();
				Companion.Activate();
			}

			return;
		}

		var cameraDestination = _cameraAnchor.position;
		cameraDestination.z = Camera.main.transform.position.z;

		DOTween.To(x => Camera.main.orthographicSize = x, Camera.main.orthographicSize, _focusSize, _focusTime).SetEase(_focusCurve);
		Camera.main.transform.DOMove(cameraDestination, _focusTime).SetEase(_focusCurve).OnComplete(() =>
		{
			if (_playerTrack?.PreviousTrack != null)
			{
				_playerTrack?.PreviousTrack?.Level.Deactivate();

				if (_playerTrack?.PreviousTrack?.PreviousTrack != null)
				{
					_playerTrack?.PreviousTrack?.PreviousTrack.Level.gameObject.SetActive(false);
				}
			}
		});
	}
	
	public void Activate()
	{
		if (_playerTrack == null)
		{
			Initialize();
		}
		
		foreach (var enemy in _enemies)
		{
			enemy.gameObject.SetActive(true);
		}

		if(Companion != null)
		{
			Companion.gameObject.SetActive(true);
		}

		gameObject.SetActive(true);
	}

	public void Deactivate(bool setInactive = false)
	{
		foreach (var enemy in _enemies)
		{
			enemy.gameObject.SetActive(false);
		}

		Companion?.gameObject.SetActive(false);

		if(setInactive)
		{
			gameObject.SetActive(false);
		}
	}

	public void ResetEntitiesAndTracks(bool instant = false)
	{
		foreach (var node in _playerTrack.Nodes)
		{
			node.ReachedByPlayer = false;
		}

		Companion?.Deactivate();

		Companion?.transform.DOScale(0f, instant ? 0 : Companion.DeathTime).SetEase(Companion.DeathCurve).OnComplete(() =>
		{
			Companion?.Initialize();
			Companion?.Activate();

			foreach (var enemy in _enemies)
			{
				enemy.Activate();
			}
		});
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		foreach (var selectedObject in Selection.gameObjects)
		{
			if (selectedObject != gameObject && selectedObject.transform.root != transform)
			{
				return;
			}
		}
		
		var vertical = _focusSize;
		var horizontal = vertical * Camera.main.aspect;

		var topLeft = _cameraAnchor.position + new Vector3(-horizontal, -vertical, 0f);
		var topRight = _cameraAnchor.position + new Vector3(horizontal, -vertical, 0f);
		var bottomRight = _cameraAnchor.position + new Vector3(horizontal, vertical, 0f);
		var bottomLeft = _cameraAnchor.position + new Vector3(-horizontal, vertical, 0f);

		Gizmos.DrawLine(topLeft, topRight);
		Gizmos.DrawLine(topRight, bottomRight);
		Gizmos.DrawLine(bottomRight, bottomLeft);
		Gizmos.DrawLine(bottomLeft, topLeft);
	}
#endif
}

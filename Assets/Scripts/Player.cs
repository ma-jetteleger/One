using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : Entity
{
	public const string kSavedTrackId = "SavedTrackId";

	[SerializeField] private int _startTrackId = 0;
	[SerializeField] private Vector3 _initialCameraOffset = Vector3.zero;
	[SerializeField] private float _initialCameraSize = 0f;
	[SerializeField] private Text _titleText = null;
	[SerializeField] private Button _hudButton = null;
	[SerializeField] private Button _backButton = null;
	[SerializeField] private Button _restartButton = null;
	[SerializeField] private Button _clearButton = null;
	[SerializeField] private CanvasGroup _resetPanel = null;
	[SerializeField] private LineRenderer _erasableTrack = null;
	[SerializeField] private float _cameraFollowSpeed = 0f;
	[SerializeField] private float _outOfTrackFocusSize = 0f;
	[SerializeField] private float _accelerationFactor = 0f;
	[SerializeField] private float _decelerationFactor = 0f;
	[SerializeField] private AnimationCurve _spawnCurve = null;
	[SerializeField] private float _spawnTime = 0f;

	public Enemy Companion { get; set; }

	private int SavedTrackId
	{
		get
		{
			if (_savedTrackId == 0)
			{
				_savedTrackId = PlayerPrefs.GetInt(kSavedTrackId);
			}

			return _savedTrackId;
		}
		set
		{
			_savedTrackId = value;
			PlayerPrefs.SetInt(kSavedTrackId, _savedTrackId);
		}
	}

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	private float _moveValueModifier;
	private float? _startProgressiveZoom;
	private Vector3? _startProgressiveCameraPosition;
	private Vector3? _startProgressivePlayerPosition;
	private float? _progressiveZoomTotalDistance;
	private bool _dead;
	private bool _started;
	private int _savedTrackId;
	
	public override void Initialize()
	{
		_dead = true;

		if (_startTrackId > -1)
		{
			SavedTrackId = _startTrackId;
		}

		if (Application.isPlaying && StartNode == null)
		{
			StartNode = TrackManager.PlayerTracks[SavedTrackId].Nodes[0];

			if(SavedTrackId > 0)
			{
				TrackManager.PlayerTracks[SavedTrackId - 1].Level.gameObject.SetActive(true);
			}
		}

		base.Initialize();
		
		var newAspectRatio = 1280f / 720f;
		var variance = newAspectRatio / Camera.main.aspect;

		if (variance < 1f)
		{
			Camera.main.rect = new Rect((1f - variance) / 2f, 0, variance, 1f);
		}
		else
		{
			variance = 1f / variance;
			Camera.main.rect = new Rect(0, (1f - variance) / 2f, 1f, variance);
		}

		Camera.main.transform.position = transform.position + _initialCameraOffset;
		Camera.main.orthographicSize = _initialCameraSize;

		_resetPanel.DOFade(0f, 0f);
		_resetPanel.interactable = false;
		_resetPanel.blocksRaycasts = false;

		_titleText.DOFade(1f, 0f);

		ToggleButton(_clearButton, SavedTrackId > 0, 0, _spawnCurve);
		ToggleButton(_hudButton, false, 0, _spawnCurve);
		ToggleButton(_backButton, false, 0, _spawnCurve);
		ToggleButton(_restartButton, false, 0, _spawnCurve);
	}

	private void Update()
	{
		/*if (Input.GetKeyDown(KeyCode.R))
		{
			Die();
		}*/

		if (_dead)
		{
			if(!_started && Input.GetMouseButton(0) && !IsPointerOverUIObject())
			{
				_dead = false;
				_started = true;

				CurrentTrack?.Level.Focus(true);

				_titleText.DOFade(0f, DeathTime).SetEase(DeathCurve);
				
				ToggleButton(_clearButton, false, DeathTime, DeathCurve);
				ToggleButton(_hudButton, true, DeathTime, DeathCurve);
				ToggleButton(_restartButton, false, DeathTime, DeathCurve);
				ToggleButton(_backButton, false, DeathTime, DeathCurve);
			}

			return;
		}

		if(_hudButton.image.color.a < 1f && Input.GetMouseButton(0) && !IsPointerOverUIObject())
		{
			ToggleButton(_hudButton, true, DeathTime, DeathCurve);
			ToggleButton(_restartButton, false, DeathTime, DeathCurve);
			ToggleButton(_backButton, false, DeathTime, DeathCurve);
		}

		/*if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			JumpToTrack(CurrentTrack.NextTrack);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			JumpToTrack(CurrentTrack.PreviousTrack);
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			var cameraDestination = transform.position;
			cameraDestination.z = Camera.main.transform.position.z;

			if (CurrentTrack.Level.FocusOnPlayer && Companion != null)
			{
				cameraDestination.y = (transform.position.y + Companion.transform.position.y) / 2f;
			}

			Camera.main.transform.position = cameraDestination;
		}*/

		if ((Input.GetKey(KeyCode.Space) || (Input.GetMouseButton(0) && !IsPointerOverUIObject())) && _moveValueModifier <= 1f)
		{
			_moveValueModifier += Time.deltaTime * _accelerationFactor;
		}
		else if (_moveValueModifier >= 0f)
		{
			_moveValueModifier -= Time.deltaTime * _decelerationFactor;
		}

		_moveValueModifier = Mathf.Clamp01(_moveValueModifier);

		if (_moveValueModifier > 0f && CurrentNode.NextNode != null)
		{
			Move(_moveValueModifier * Time.deltaTime * MovementSpeed);
		}

		if (CurrentTrack != null && CurrentTrack.Level.ProgressiveZoom)
		{
			if (!_startProgressiveZoom.HasValue)
			{
				_startProgressiveZoom = Camera.main.orthographicSize;
				_startProgressiveCameraPosition = Camera.main.transform.position;
				_startProgressivePlayerPosition = transform.position;
				_progressiveZoomTotalDistance = Vector3.Distance(transform.position, CurrentTrack.Level.ProgressiveFocusEnd.transform.position);
			}
			
			var currentDistanceValue = Vector3.Distance(transform.position, _startProgressivePlayerPosition.Value);
			var lerpValue = Mathf.Clamp01(currentDistanceValue / _progressiveZoomTotalDistance.Value);

			var cameraDestination = Camera.main.transform.position;
			cameraDestination.x = Mathf.Lerp(cameraDestination.x, transform.position.x, _cameraFollowSpeed);
			cameraDestination.y = Mathf.Lerp(_startProgressiveCameraPosition.Value.y, transform.position.y, CurrentTrack.Level.FocusCurve.Evaluate(lerpValue));
			
			Camera.main.transform.position = cameraDestination;
			Camera.main.orthographicSize = Mathf.Lerp(_startProgressiveZoom.Value, CurrentTrack.Level.ProgressiveFocusSizeEnd, CurrentTrack.Level.FocusCurve.Evaluate(lerpValue));
			
			return;
		}

		if (CurrentTrack == null || CurrentTrack.Level.FocusOnPlayer)
		{
			var cameraDestination = transform.position;
			cameraDestination.z = Camera.main.transform.position.z;

			if (CurrentTrack?.Level != null && Companion != null && !CurrentTrack.Level.ProgressiveZoom && CurrentTrack.Level.FocusOnPlayer)
			{
				cameraDestination.y = (transform.position.y + Companion.transform.position.y) / 2f;
			}
			
			Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position, cameraDestination, _cameraFollowSpeed);
			Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, (CurrentTrack != null && CurrentTrack.Level.FocusOnPlayer) ? CurrentTrack.Level.FocusSize : _outOfTrackFocusSize, _cameraFollowSpeed);
		}
	}

	protected override void SetTrack(Track track)
	{
		if (track == null && CurrentTrack != null)
		{
			var newTrackId = TrackManager.PlayerTracks.IndexOf(CurrentTrack) + 1;

			if(newTrackId <= 29) // eww
			{
				SavedTrackId = newTrackId;
			}
		}

		base.SetTrack(track);

		CurrentTrack?.Level.Activate();

		if(!_dead)
		{
			CurrentTrack?.Level.Focus();
		}
		
		if(CurrentTrack?.Level?.Companion != null)
		{
			Companion = CurrentTrack?.Level?.Companion;
		}

		if (CurrentTrack?.NextTrack?.Level != null)
		{
			CurrentTrack.NextTrack.Level.Activate();
		}
	}

	protected override void SetNode(Node node)
	{
		base.SetNode(node);

		node.ReachedByPlayer = true;
	}

	/*private void JumpToTrack(Track track)
	{
		CurrentTrack.Level.Deactivate();

		SetTrack(track);
		CurrentTrack.Level.Activate();

		SetNode(CurrentTrack.Nodes[0]);

		Die();
	}*/

	private void Die(bool backToTitle = false)
	{
		_dead = true;
		_moveValueModifier = 0f;

		var respawnNode = (Node)null;

		if(CurrentTrack == null)
		{
			var track = TrackManager.PlayerTracks[SavedTrackId - 1];
			respawnNode = track.Nodes[track.Nodes.Length - 1];
		}
		else
		{
			respawnNode = CurrentTrack.Nodes[0];
			CurrentTrack.Level.ResetEntitiesAndTracks();
		}
		
		transform.DOScale(0f, DeathTime).SetEase(DeathCurve).OnComplete(() =>
		{
			SetNode(respawnNode);

			transform.position = CurrentNode.transform.position;

			if(backToTitle)
			{
				Camera.main.transform.DOMove(transform.position + _initialCameraOffset, _spawnTime).SetEase(_spawnCurve);
				DOTween.To(x => Camera.main.orthographicSize = x, Camera.main.orthographicSize, _initialCameraSize, _spawnTime).SetEase(_spawnCurve);

				_titleText.DOFade(1f, _spawnTime).SetEase(_spawnCurve);

				ToggleButton(_clearButton, SavedTrackId > 0, _spawnTime, _spawnCurve);
				ToggleButton(_hudButton, false, _spawnTime, _spawnCurve);
				ToggleButton(_backButton, false, _spawnTime, _spawnCurve);
				ToggleButton(_restartButton, false, _spawnTime, _spawnCurve);
			}

			transform.DOScale(1f, _spawnTime).SetEase(_spawnCurve).OnComplete(() =>
			{
				if (backToTitle)
				{
					_started = false;
				}
				else
				{
					_dead = false;
				}
			});
		});
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if(_dead)
		{
			return;
		}

		var enemy = collision.gameObject.GetComponent<Enemy>();

		if (enemy != null)
		{
			if(enemy.MatchPlayerSpeed)
			{
				enemy.InversePulse();
			}
			else
			{
				Die();

				enemy.Pulse();
			}
		}
	}

	private void ToggleButton(Button button, bool toggle, float time, AnimationCurve curve)
	{
		button.image.DOFade(toggle ? 1f : 0f, time).SetEase(curve);
		button.image.raycastTarget = toggle;
		button.interactable = toggle;
	}

	public void UI_OpenHud()
	{
		ToggleButton(_hudButton, false, _spawnTime, _spawnCurve);
		ToggleButton(_backButton, true, _spawnTime, _spawnCurve);
		ToggleButton(_restartButton, true, _spawnTime, _spawnCurve);
	}

	public void UI_BackToTitle()
	{
		Die(true);
	}

	public void UI_RestartLevel()
	{
		Die();
	}

	public void UI_OpenResetPanel()
	{
		_resetPanel.DOFade(1f, _spawnTime).SetEase(_spawnCurve);
		_resetPanel.interactable = true;
		_resetPanel.blocksRaycasts = true;
	}

	public void UI_CloseResetPanel()
	{
		_resetPanel.DOFade(0f, DeathTime).SetEase(DeathCurve);
		_resetPanel.interactable = false;
		_resetPanel.blocksRaycasts = false;
	}

	public void UI_ClearProgress()
	{
		for (var i = 0; i < SavedTrackId + 2; i++)
		{
			var track = TrackManager.PlayerTracks[i];

			track.Level.ResetEntitiesAndTracks(true);

			if (track.Level.gameObject.activeSelf)
			{
				track.Level.gameObject.SetActive(false);
			}
		}

		_erasableTrack.gameObject.SetActive(true);

		var startColor = new Color2(_erasableTrack.startColor, _erasableTrack.endColor);
		var endColor = new Color2(Color.clear, Color.clear);

		_erasableTrack.DOColor(startColor, endColor, _spawnTime).SetEase(_spawnCurve).OnComplete(() =>
		{
			_erasableTrack.gameObject.SetActive(false);
			_erasableTrack.startColor = startColor.ca;
			_erasableTrack.endColor = startColor.cb;
		});

		SavedTrackId = 0;
		StartNode = null;

		Initialize();
	}
}

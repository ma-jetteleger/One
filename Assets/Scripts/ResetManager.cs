using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour
{
	[SerializeField] private Player _player = null;
	[SerializeField] private Node _lastNode = null;
	[SerializeField] private float _inactivityTime = 60f;
	[SerializeField] private float _countdownDuration = 30f;
	[SerializeField] private Text _countdownText;
	[SerializeField] private CanvasGroup _canvasGroup;
	[SerializeField] private AnimationCurve _fadeCurve = null;
	[SerializeField] private float _fadeTime = 0f;

	private float idleTimer = 0f;
	private float countdownTimer;
	private bool countdownActive = false;

	void Start()
	{
		countdownTimer = _countdownDuration;

		if (_canvasGroup != null)
		{
			_canvasGroup.alpha = 0f;
		}
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Return))
		{
			ResetGame();

			return;
		}

		// Don't count inactivity on title screen
		if (!_player.Started)
		{
			if (idleTimer > 0f)
			{
				ResetIdleState();
			}

			return;
		}

		bool inputDetected = HasAnyInput();

		if (inputDetected)
		{
			idleTimer = 0f;

			// Cancel countdown if user interacts
			if (countdownActive)
			{
				countdownActive = false;
				countdownTimer = _countdownDuration;

				if (_countdownText != null)
				{
					_canvasGroup.DOFade(0f, _fadeTime).SetEase(_fadeCurve);
				}
			}
		}
		else
		{
			idleTimer += Time.deltaTime;
		}

		// Start countdown after inactivity period
		if (!countdownActive && idleTimer >= _inactivityTime)
		{
			countdownActive = true;
			countdownTimer = _countdownDuration;

			if (_countdownText != null)
			{
				_canvasGroup.DOFade(1f, _fadeTime).SetEase(_fadeCurve);
			}
		}

		// Handle countdown
		if (countdownActive)
		{
			countdownTimer -= Time.deltaTime;

			// Update text once per second
			if (_countdownText != null)
			{
				int secondsLeft = Mathf.CeilToInt(countdownTimer);
				_countdownText.text = _player.CurrentNode == _lastNode
					? $"Resetting for new player in\n{secondsLeft} seconds"
					: $"No input detected, resetting for new player in {secondsLeft} seconds";
			}

			// Reset game
			if (countdownTimer <= 0f)
			{
				_player.Started = false;

				countdownTimer = 0f;

				ResetGame();
			}
		}
	}

	bool HasAnyInput()
	{
		if(_player.CurrentNode == _lastNode)
		{
			return false;
		}

		// Keyboard / mouse buttons
		if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
		{
			return true;
		}

		return false;
	}

	void ResetIdleState()
	{
		idleTimer = 0f;
		countdownActive = false;
		countdownTimer = _countdownDuration;

		if (_countdownText != null)
		{
			_canvasGroup.DOFade(0f, _fadeTime).SetEase(_fadeCurve);
		}
	}

	void ResetGame()
	{
		_player.CurrentTrack = null;
		_player.CurrentNode = null;

		_player.UI_BackToTitle();
	}
}
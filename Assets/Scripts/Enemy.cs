using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
	[SerializeField] private bool _invincible = false;
	[SerializeField] private float _gatedDecelerationDistance = 0f;
	[SerializeField] private float _gatedDecelerationCutoff = 0f;
	[SerializeField] private float _playerMatchSpeedDistanceThreshold = 0f;
	[SerializeField] private float _playerMatchSpeedUpFactor = 0f;
	[SerializeField] private float _playerMatchSpeedDownFactor = 0f;
	[SerializeField] private Vector2 _playerMatchSpeedRange = Vector2.zero;
	[SerializeField] private float _punchValue = 0f;
	[SerializeField] private float _punchTime = 0f;
	[SerializeField] private int _punchVibration = 0;

	public bool Invincible 
	{
		get => _invincible;
		set => _invincible = value;
	}

	public bool MatchPlayerSpeed { get; set; }

	private CircleCollider2D _collider;
	private float _moveValueModifier;
	private float _moveValueDamper;

	private void Awake()
	{
		_collider = GetComponentInChildren<CircleCollider2D>(true);
	}

	private void Start()
	{
		_moveValueDamper = 1f;
	}

	public override void Move(float moveFactor)
	{
		if (CurrentNode.NextNode == null)
		{
			return;
		}

		var currentGated = CurrentNode.GatedBy != null && !CurrentNode.GatedBy.ReachedByPlayer || (!_collider.enabled && Invincible && !MatchPlayerSpeed);
		
		if (currentGated)
		{
			return;
		}

		var nextGated = CurrentNode.NextNode.ForcedGate || (CurrentNode.NextNode.GatedBy != null && !CurrentNode.NextNode.GatedBy.ReachedByPlayer || (!_collider.enabled && Invincible && !MatchPlayerSpeed));
		
		_moveValueModifier = 1;

		if (nextGated)
		{
			var distance = Vector3.Distance(transform.position, CurrentNode.NextNode.transform.position);

			if (distance <= _gatedDecelerationCutoff)
			{
				_moveValueModifier = 0f;
			}
			else if (distance <= _gatedDecelerationDistance)
			{
				_moveValueModifier = Mathf.Clamp01(distance / _gatedDecelerationDistance);
			}
		}
		
		if (MatchPlayerSpeed)
		{
			var distanceToPlayer = EntityManager.Instance.Player.transform.position.x - transform.position.x;

			if (distanceToPlayer > _playerMatchSpeedDistanceThreshold)
			{
				_moveValueDamper += Time.deltaTime * _playerMatchSpeedUpFactor * Mathf.Abs(distanceToPlayer);

				//Debug.Log("+");
			}
			else if (distanceToPlayer < -_playerMatchSpeedDistanceThreshold)
			{
				_moveValueDamper -= Time.deltaTime * _playerMatchSpeedDownFactor * Mathf.Abs(distanceToPlayer);

				//Debug.Log("-");
			}
			else if (_moveValueDamper - 1f > 0.1f || 1 - _moveValueDamper > 0.1f)
			{
				_moveValueDamper = Mathf.Lerp(_moveValueDamper, 1f, Time.deltaTime * Mathf.Abs(distanceToPlayer));
				
				//Debug.Log("=");
			}

			_moveValueDamper = Mathf.Clamp(_moveValueDamper, _playerMatchSpeedRange.x, _playerMatchSpeedRange.y);
		}
		
		base.Move(moveFactor * MovementSpeed * _moveValueModifier * _moveValueDamper);
	}

	public override void Activate()
	{
		base.Activate();
		
		_collider.enabled = true;

		transform.DOScale(1f, DeathTime).SetEase(DeathCurve);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		
		_collider.enabled = false;

		transform.DOScale(0f, DeathTime).SetEase(DeathCurve);
	}

	public void Pulse()
	{
		transform.DOPunchScale(Vector3.one * _punchValue, _punchTime, _punchVibration).SetEase(Ease.OutBounce).OnComplete(() =>
		{
			transform.DOScale(1f, _punchTime);
		});
	}

	public void InversePulse()
	{
		_collider.enabled = false;

		transform.DOScale(Vector3.zero, _punchTime / 2f).SetEase(Ease.OutBounce).OnComplete(() =>
		{
			transform.DOScale(1f, _punchTime / 2f).OnComplete(() =>
			{
				_collider.enabled = true;
			});
		});
	}

	protected override void SetTrack(Track track)
	{
		if (Invincible && CurrentTrack?.Level.Companion != null)
		{
			CurrentTrack.Level.Companion = null;
			
			EntityManager.Instance.Player.Companion = this;

			if (track?.Level != null)
			{
				track.Level.Companion = this;
				MatchPlayerSpeed = track.Level.CompanionMatchPlayerSpeed;
			}
		}
		
		base.SetTrack(track);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		var enemy = collision.gameObject.GetComponent<Enemy>();

		if (enemy && enemy.Invincible)
		{
			Deactivate();

			enemy.Pulse();
		}
	}
}

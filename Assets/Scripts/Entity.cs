using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
	[SerializeField] private Node _startNode = null;
	[SerializeField] private float _movementSpeed = 0f;
	[SerializeField] private AnimationCurve _deathCurve = null;
	[SerializeField] private float _deathTime = 0f;

	public Node StartNode
	{
		get => _startNode;
		set => _startNode = value;
	}

	public float MovementSpeed
	{
		get => _movementSpeed;
		set => _movementSpeed = value;
	}

	public AnimationCurve DeathCurve => _deathCurve;
	public float DeathTime => _deathTime;

	public Level Level { get; set; }

	protected Track CurrentTrack;
	protected Node CurrentNode;

	[Button("Position")]
	public virtual void Initialize()
	{
		SetNode(_startNode);
		SetTrack(_startNode?.Track);

		transform.position = CurrentNode.transform.position;
	}

	public virtual void Move(float moveFactor)
	{
		var direction = (CurrentNode.NextNode.transform.position - transform.position).normalized;
		var moveValue = direction * moveFactor;
		var destination = transform.position + moveValue;
		
		if (Vector3.Distance((CurrentNode.NextNode.transform.position - destination).normalized, direction) > 0.1f || Vector3.Distance(CurrentNode.NextNode.transform.position, destination) <= 0.01f)
		{
			SetNode(CurrentNode.NextNode);

			var lastNode = CurrentNode == CurrentNode.Track.Nodes[CurrentNode.Track.Nodes.Length - 1];
			var firstNode = CurrentNode == CurrentNode.Track.Nodes[0];

			if(lastNode && CurrentNode.Track.EnemyTrack && !CurrentNode.Track.ConnectedLoop && CurrentNode.NextNode != null)
			{
				transform.position = CurrentNode.NextNode.transform.position; // is this needed?

				SetNode(CurrentNode.NextNode);

				Activate(); // This isn't gonna work for looped tracks
			}

			destination = CurrentNode.transform.position;

			if (lastNode)
			{
				SetTrack(this is Player ? null : CurrentTrack?.NextTrack);
			}
			else if (firstNode)
			{
				SetTrack(CurrentNode.Track);
			}
		}

		transform.position = destination;
	}

	protected virtual void SetTrack(Track track)
	{
		CurrentTrack = track;

		Level = CurrentTrack?.Level;
	}

	protected virtual void SetNode(Node node)
	{
		CurrentNode = node;
	}

	public virtual void Activate()
	{
		
	}

	public virtual void Deactivate()
	{
		
	}
}

using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Track : MonoBehaviour
{
	[SerializeField] private bool _enemyTrack = false;
	[SerializeField] private bool _loop = false;
	[SerializeField] private bool _connectedLoop = false;
	[SerializeField] private Track _overrideNextTrack = null;

	public Track NextTrack { get; set; }
	public Track PreviousTrack { get; set; }

	public bool ConnectedLoop => _connectedLoop;
	public bool EnemyTrack => _enemyTrack;

	public Level Level
	{
		get
		{
			if (_level == null)
			{
				_level = GetComponentsInParent<Level>(true)[0];
			}

			return _level;
		}
	}

	public LineRenderer LineRenderer
	{
		get
		{
			if(_lineRenderer == null)
			{
				_lineRenderer = GetComponentsInChildren<LineRenderer>(true)[0];
			}

			return _lineRenderer;
		}
	}

	public Node[] Nodes
	{
		// TODO : fix the fact that it won't get new nodes

		get
		{
			if (_nodes == null || _nodes.Length == 0)
			{
				_nodes = GetComponentsInChildren<Node>(true);
			}

			return _nodes;
		}
	}

	private LineRenderer _lineRenderer;
	private Node[] _nodes;
	private Level _level;
	
	private void Start()
	{
		if (_enemyTrack)
		{
			Initialize(null, null);
		}
	}

	public void Initialize(Track nextTrack, Track previousTrack)
    {
		_nodes = null; // eww

		NextTrack = _overrideNextTrack ? _overrideNextTrack : nextTrack;
		PreviousTrack = previousTrack;

		for (var i = 0; i < Nodes.Length; i++)
		{
			Nodes[i].Initialize(
				i < Nodes.Length - 1
					? Nodes[i + 1]
					: _loop
						? Nodes[0]
						: NextTrack?.Nodes[0]);
		}
		
		Trace(NextTrack);
	}
	
	public void Trace(Track nextTrack)
	{
		_nodes = null; // eww

		LineRenderer.positionCount = Nodes.Length;

		var positions = Nodes.Select(x => x.transform.localPosition).ToList();

		for (var i = 0; i < Nodes.Length; i++)
		{
			Nodes[i].gameObject.name = $"Node{i}";
			Nodes[i].GetComponent<SpriteRenderer>().color = LineRenderer.startColor;
		}

		NextTrack = _overrideNextTrack ? _overrideNextTrack : nextTrack;

		if (NextTrack != null)
		{
			positions.Add(transform.InverseTransformPoint(NextTrack.Nodes[0].transform.position));

			LineRenderer.positionCount++;
		}

		LineRenderer.SetPositions(positions.ToArray());
		LineRenderer.loop = _connectedLoop;
	}

	[Button]
	private void Trace()
	{
		Trace(null);
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		for (var i = 0; i < Nodes.Length; i++)
		{
			var displayedName = string.Concat(Nodes[i].gameObject.name.Where(c => c >= 'A' && c <= 'Z' || c >= '0' && c <= '9'));

			var style = new GUIStyle();
			style.normal.textColor = new Color(0f, 1 - (i * (1f / (Nodes.Length - 1))), 1f);

			Handles.Label(Nodes[i].transform.position + new Vector3(0.05f, -0.05f, 0f), displayedName, style);
		}
	}
#endif
}

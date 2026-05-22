using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Node : MonoBehaviour
{
	[SerializeField] private Node _gatedBy = null;
	[SerializeField] private bool _forcedGate = false;

	public Node GatedBy => _gatedBy;
	public bool ForcedGate => _forcedGate;

	public Node NextNode { get; set; }
	public bool ReachedByPlayer { get; set; }

	public Track Track
	{
		get
		{
			if(_track == null)
			{
				_track = GetComponentsInParent<Track>(true)[0];
			}

			return _track;
		}
	}
	

	private Track _track;

	public void Initialize(Node nextNode)
	{
		NextNode = nextNode;
	}
}

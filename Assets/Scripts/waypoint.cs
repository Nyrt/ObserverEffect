using UnityEngine;
using System.Collections;

public class waypoint : MonoBehaviour {

	void OnDrawGizmos()
	{
		if (!Application.isPlaying) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere (transform.position, 1f);
		}
	}
}

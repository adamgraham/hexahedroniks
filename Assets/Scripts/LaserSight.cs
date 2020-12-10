using UnityEngine;
using System.Collections;

[RequireComponent( typeof( LineRenderer ) )]
public class LaserSight : MonoBehaviour 
{
	public Vector3 offset;
	public float distance = 100.0f;

	private LineRenderer _lineRenderer;

	private void Start()
	{
		_lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	private void LateUpdate()
	{
		Vector3 startPos = transform.position + offset;
		Vector3 targetPos = startPos + (transform.forward * distance);

		_lineRenderer.SetPosition( 0, startPos );
		_lineRenderer.SetPosition( 1, targetPos );
	}

}

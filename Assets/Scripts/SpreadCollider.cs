using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Collider ) )]
public class SpreadCollider : MonoBehaviour 
{
	public bool destroyOnTrigger = true;

	[HideInInspector]
	public Cube source;
	[HideInInspector]
	public Cube.CubeColor color;
	[HideInInspector]
	public Material mat;

	private void OnTriggerEnter( Collider other )
	{
		if ( destroyOnTrigger )
			Destroy( gameObject, Mathf.Epsilon );
	}

}

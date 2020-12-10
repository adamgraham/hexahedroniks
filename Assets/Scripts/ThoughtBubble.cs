using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Renderer ) )]
public class ThoughtBubble : MonoBehaviour 
{
	private void OnCollisionEnter( Collision collision )
	{
		Destroy( gameObject, Mathf.Epsilon );
	}

	public void SetColor( Cube.CubeColor color )
	{
		SetColor( Cube.GetColor( color ) );
	}

	public void SetColor( Color color )
	{
		gameObject.GetComponent<Renderer>().material.color = color;
	}

}

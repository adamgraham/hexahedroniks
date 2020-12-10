using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour 
{
	public float speed = 1.0f;

	private void FixedUpdate()
	{
		transform.position += transform.forward * speed;
	}

}

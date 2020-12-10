using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Collider ) )]
public class RubikFaceCollider : MonoBehaviour 
{
	private Cube.CubeColor _color;

	private int _amountCubesChecked;
	private int _amountCubesToCheck;

	private bool _sameColor;
	private bool _checking;

	private Collider _collider;
	private Vector3 _localPos;

	private void Awake()
	{
		_collider = gameObject.GetComponent<Collider>();
		_collider.enabled = false;

		_localPos = transform.localPosition;
		transform.localPosition = Vector3.zero;
	}

	private void OnTriggerEnter( Collider other )
	{
		CubeFace face = other.gameObject.GetComponent<CubeFace>();

		if ( face != null )
		{
			_amountCubesChecked++;

			if ( _color == Cube.CubeColor.Unknown ) // 1st cube check
			{
				_color = face.color;
			}
			else if ( face.color != _color ) // not the same colors
			{
				_sameColor = false;
				FinishChecking();
			}
			
			if ( _amountCubesChecked == _amountCubesToCheck ) // all cubes checked
				FinishChecking();
		}
	}

	public void CheckIfSameColor( int amountCubes = 9 )
	{
		if ( !_checking )
		{
			_color = Cube.CubeColor.Unknown;
			_amountCubesChecked = 0;
			_amountCubesToCheck = amountCubes;
			_sameColor = true;
			_checking = true;

			gameObject.SetActive( true );
			transform.localPosition = _localPos;

			_collider.enabled = false;
			_collider.enabled = true;
		}
	}

	public Cube.CubeColor GetColor()
	{
		return _color;
	}

	public bool IsSameColor()
	{
		return _sameColor && _amountCubesChecked == _amountCubesToCheck;
	}

	public bool IsSameColor( Cube.CubeColor color )
	{
		return IsSameColor() && _color == color;
	}

	public bool IsChecking()
	{
		return _checking;
	}

	public bool IsFinishedChecking()
	{
		return !_checking;
	}

	public void FinishChecking()
	{
		if ( gameObject.activeSelf )
		{
			_collider.enabled = false;

			transform.localPosition = Vector3.zero;
			gameObject.SetActive( false );
		}

		_checking = false;
	}

}

using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Cube : MonoBehaviour 
{
	static public Material blackMat;
	static public Material blueMat;
	static public Material greenMat;
	static public Material orangeMat;
	static public Material redMat;
	static public Material whiteMat;
	static public Material yellowMat;

	public enum CubeAxis { X_POS, X_NEG, Y_POS, Y_NEG, Z_POS, Z_NEG }
	public enum CubeColor { Black, Blue, Green, Orange, Red, White, Yellow, Unknown };
	public enum CubeType { Cardinal, Center, Corner, Core, Invalid };

	public Transform edges;
	public Transform faces;
	public CubeAxis[] facesOuterAxis;

	internal Rubik _rubik;

	internal CubeFace[] _faces;
	internal Renderer[] _edges;

	internal Vector3 _defaultPosition;
	internal bool _transitioning;

	internal CubeType _type;

	internal void Init( Rubik rubik )
	{
		_rubik = rubik;

		InitFaces();

		_edges = edges.GetComponentsInChildren<Renderer>();
		_defaultPosition = transform.localPosition;
		_type = CalculateCubeType( this );
	}

	private void InitFaces()
	{
		Renderer[] mFaces = faces.transform.GetComponentsInChildren<Renderer>();
		int len = mFaces.Length;

		_faces = new CubeFace[len];

		for ( int i = 0; i < len; i++ )
		{
			CubeFace face = mFaces[i].gameObject.AddComponent<CubeFace>();

			face.cube = this;
			face.color = CubeColor.White;
			face.renderer = face.gameObject.GetComponent<Renderer>();

			_faces[i] = face;
		}
	}

	public void Transition()
	{
		if ( !_transitioning )
		{
			_transitioning = true;

			float distance = 4.0f;
			float durationMove = 3.0f;
			float durationRotate = 1.0f;

			_defaultPosition = transform.localPosition;

			transform.DOLocalMove( transform.localPosition + (GetNormal() * distance), durationMove ).
				SetEase( Ease.OutBack );
			transform.DOLocalRotate( transform.localEulerAngles + (GetRandomCardinalAxis() * 90.0f), durationRotate ).
				SetDelay( durationMove ).SetEase( Ease.Linear );
			transform.DOLocalMove( _defaultPosition, durationMove ).
				SetDelay( durationMove + durationRotate ).SetEase( Ease.InBack ).OnComplete( OnTransitionComplete );
		}
	}

	private void OnTransitionComplete()
	{
		_transitioning = false;
	}

	public Vector3 GetNormal( int faceIndex = -1 )
	{
		if ( faceIndex <= -1 || faceIndex >= facesOuterAxis.Length )
		{
			if ( facesOuterAxis.Length > 0 )
				faceIndex = Random.Range( 0, facesOuterAxis.Length );
			else
				faceIndex = -1;
		}

		if ( faceIndex != -1 )
			return GetNormal( facesOuterAxis[faceIndex] );

		return Vector3.zero;
	}

	public Vector3 GetNormal( CubeAxis axis )
	{
		switch ( axis )
		{
			case CubeAxis.X_POS:
				return Vector3.right;

			case CubeAxis.X_NEG:
				return Vector3.left;

			case CubeAxis.Y_POS:
				return Vector3.up;

			case CubeAxis.Y_NEG:
				return Vector3.down;

			case CubeAxis.Z_POS:
				return Vector3.forward;

			case CubeAxis.Z_NEG:
				return Vector3.back;
		}

		return Vector3.zero;
	}

	public Vector3 GetRandomCardinalAxis()
	{
		Vector3 axis = Vector3.zero;

		int randomAxis = Random.Range( 0, 3 );

		if ( randomAxis == 0 )
			axis.x = 1.0f;
		else if ( randomAxis == 1 )
			axis.y = 1.0f;
		else if ( randomAxis == 2 )
			axis.z = 1.0f;

		return axis;
	}

	public void ChangeColor( CubeColor color, int faceIndex = -1 )
	{
		if ( _faces != null )
		{
			Material mat = GetMaterial( color );

			if ( faceIndex <= -1 || faceIndex >= _faces.Length )
			{
				int len = _faces.Length;
				for ( int i = 0; i < len; i++ )
					_faces[i].SetMat( mat, color );
			}
			else
			{
				_faces[faceIndex].SetMat( mat, color );
			}
		}
	}

	public void ChangeEdgeColor( Color color, float duration = 0.0f )
	{
		if ( _edges != null )
		{
			int len = _edges.Length;
			for ( int i = 0; i < len; i++ )
				_edges[i].material.DOColor( color, duration );
		}
	}

	static public Material GetMaterial( CubeColor color )
	{
		switch ( color )
		{
			case CubeColor.Black:
				return blackMat;

			case CubeColor.Blue:
				return blueMat;

			case CubeColor.Green:
				return greenMat;

			case CubeColor.Orange:
				return orangeMat;

			case CubeColor.Red:
				return redMat;

			case CubeColor.White:
				return whiteMat;

			case CubeColor.Yellow:
				return yellowMat;
		}

		return whiteMat;
	}

	static public Color GetColor( CubeColor color )
	{
		switch ( color )
		{
			case CubeColor.Black:
				return whiteMat.color * 0.05f;

			case CubeColor.Blue:
				return blueMat.color * 0.95f;

			case CubeColor.Green:
				return greenMat.color * 0.95f;

			case CubeColor.Orange:
				return orangeMat.color * 0.95f;

			case CubeColor.Red:
				return redMat.color* 0.95f;

			case CubeColor.White:
				return whiteMat.color * 0.95f;

			case CubeColor.Yellow:
				return yellowMat.color * 0.95f;
		}

		return whiteMat.color;
	}

	static public CubeColor GetRandomColor()
	{
		int random = Random.Range( 0, 6 );

		switch ( random )
		{
			case 0:
				return CubeColor.Blue;

			case 1:
				return CubeColor.Green;

			case 2:
				return CubeColor.Orange;

			case 3:
				return CubeColor.Red;

			case 4:
				return CubeColor.White;

			case 5:
				return CubeColor.Yellow;
		}

		return CubeColor.White;
	}

	static public CubeType CalculateCubeType( Cube cube )
	{
		string name = cube.name.ToLower();

		if ( name.Contains( "corner" ) )
			return CubeType.Corner;
		else if ( name.Contains( "center" ) )
			return CubeType.Center;
		else if ( name.Contains( "cardinal" ) )
			return CubeType.Cardinal;

		return CubeType.Invalid;
	}

}

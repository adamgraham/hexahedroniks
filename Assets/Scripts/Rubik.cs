using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Rubik : MonoBehaviour 
{
	[Header( "Rubik / Cubes" )]

	public GameObject core;
	private Cube.CubeColor _coreColor;

	public RubikFaceCollider[] faceColliders;

	private Cube[] _cubes;
	private Renderer[] _coreRenderers;

	internal List<Vector3> cardinalPositions;
	internal List<Vector3> centerPositions;
	internal List<Vector3> cornerPositions;

	[Header( "Materials" )]

	public Material blackMat;
	public Material blueMat;
	public Material greenMat;
	public Material orangeMat;
	public Material redMat;
	public Material whiteMat;
	public Material yellowMat;

	private List<Cube.CubeColor> _colorsAvailable;

	[Header( "Look Rotation" )]

	public Camera camera;
	public Transform lookTransform;

	public Vector2 sensitivity = Vector3.one;
	public Vector2 smoothing = Vector3.one;

	public CursorLockMode cursorMode;
	public bool cursorVisible;

	private Vector2 _mouseAbsolute;
	private Vector2 _smoothMouse;

	[Header( "Virtual Reality" )]

	public bool virtualReality;
	public Transform eyeTransform;
	public Vector2 minEyeAngle = new Vector2( 10.0f, 10.0f );
	public Vector2 maxEyeSpeed = new Vector2( 2.5f, 2.5f );

	private Vector3 _oculusPreviousOrientation;

	[Header( "Gameplay" )]

	public ThoughtBubble thoughtBubblePrefab;
	public SpreadCollider spreadColliderPrefab;

	public float rotateIntervalStart = 5.0f;
	public float rotateIntervalStartFluctuation = 1.0f;

	public float rotateIntervalEnd = 1.0f;
	public float rotateIntervalEndFluctuation = 0.25f;
	
	public float startToEndDuration = 60.0f;

	public Renderer[] walls;

	private float _rotateInterval;
	private float _rotateFluctuation;

	[Header( "UI" )]

	public GameObject[] mainMenu;
	public GameObject[] solvedMenu;
	public GameObject[] gameOverMenu;

	private Stopwatch _stopWatch;

	private bool _started;
	private bool _gameFinsished;
	private bool _loading;

	[Header( "Audio" )]

	public AudioClip shootFX;
	public AudioClip goodSFX;
	public AudioClip badSFX;
	public AudioClip coreHitSFX;

	private void Start()
	{
		_cubes = transform.GetComponentsInChildren<Cube>();
		_coreRenderers = core.GetComponentsInChildren<Renderer>();
		_stopWatch = gameObject.GetComponent<Stopwatch>();

		_started = false;
		_gameFinsished = false;
		_loading = false;

		CubeFace.spreadColliderPrefab = spreadColliderPrefab.gameObject;
		CubeFace.edgeSize = 0.1f;

		if ( lookTransform == null )
			lookTransform = transform;

		if ( virtualReality )
			_oculusPreviousOrientation = Vector3.zero;

		CreateColorsList();
		ColorCubes();
		CalculateCubePositions();
		StartRandomization();
		ChangeCoreColor( Cube.CubeColor.Black );
	}

	private void FixedUpdate()
	{
		// Look Rotation

		if ( !virtualReality )
		{
			Cursor.lockState = cursorMode;
			Cursor.visible = cursorVisible;

			Vector2 mouseDelta = new Vector2( Input.GetAxisRaw( "Mouse X" ), Input.GetAxisRaw( "Mouse Y" ) );
			
			mouseDelta = Vector2.Scale( mouseDelta, Vector2.Scale( sensitivity, smoothing ) );

			_smoothMouse.x = Mathf.Lerp( _smoothMouse.x, mouseDelta.x, 1.0f / smoothing.x );
			_smoothMouse.y = Mathf.Lerp( _smoothMouse.y, mouseDelta.y, 1.0f / smoothing.y );

			_mouseAbsolute += _smoothMouse;

			Quaternion xRotation = Quaternion.AngleAxis( -_mouseAbsolute.y, Vector3.right );
			lookTransform.localRotation = xRotation;

			Quaternion yRotation = Quaternion.AngleAxis( _mouseAbsolute.x, lookTransform.InverseTransformDirection( Vector3.up ) );
			lookTransform.localRotation *= yRotation;
		}
		else
		{
			Vector3 lookDelta = _oculusPreviousOrientation - eyeTransform.localEulerAngles;

			lookDelta.x += 180.0f;
			lookDelta.y += 180.0f;

			if ( lookDelta.x < 0.0f ) lookDelta.x = -180.0f - lookDelta.x;
			else lookDelta.x = 180.0f - lookDelta.x;

			if ( lookDelta.y < 0.0f ) lookDelta.y = -180.0f - lookDelta.y;
			else lookDelta.y = 180.0f - lookDelta.y;

			if ( Mathf.Abs( lookDelta.x ) < minEyeAngle.x ) lookDelta.x *= 0.2f;
			else lookDelta.x *= Mathf.Abs(lookDelta.x) / (maxEyeSpeed.x - -maxEyeSpeed.x);
			//else lookDelta.x = Interpolation.QuadInOut( (lookDelta.x + minEyeDelta.x) / (maxEyeDelta.x - minEyeDelta.x), 1.0f, -maxEyeDelta.x, maxEyeDelta.x );

			if ( Mathf.Abs( lookDelta.y ) < minEyeAngle.y ) lookDelta.y *= 0.2f;
			else lookDelta.y *= Mathf.Abs( lookDelta.y ) / (maxEyeSpeed.y - -maxEyeSpeed.y);
			//else lookDelta.y = Interpolation.QuadInOut( (lookDelta.y + minEyeDelta.y) / (maxEyeDelta.y - minEyeDelta.y), 1.0f, -maxEyeDelta.y, maxEyeDelta.y );

			lookDelta.x = Mathf.Clamp( lookDelta.x, -maxEyeSpeed.x, maxEyeSpeed.x );
			lookDelta.y = Mathf.Clamp( lookDelta.y, -maxEyeSpeed.y, maxEyeSpeed.y );

			Vector2 mouseDelta = new Vector2( lookDelta.y, lookDelta.x );

			mouseDelta = Vector2.Scale( mouseDelta, Vector2.Scale( sensitivity, smoothing ) );

			_smoothMouse.x = Mathf.Lerp( _smoothMouse.x, mouseDelta.x, 1.0f / smoothing.x );
			_smoothMouse.y = Mathf.Lerp( _smoothMouse.y, mouseDelta.y, 1.0f / smoothing.y );

			_mouseAbsolute += _smoothMouse;

			Quaternion xRotation = Quaternion.AngleAxis( -_mouseAbsolute.y, Vector3.right );
			lookTransform.localRotation = xRotation;

			Quaternion yRotation = Quaternion.AngleAxis( _mouseAbsolute.x, lookTransform.InverseTransformDirection( Vector3.up ) );
			lookTransform.localRotation *= yRotation;

			//_oculusPreviousOrientation = eyeTransform.localEulerAngles;
		}

		// Actions

		if ( _started )
		{
			if ( !_gameFinsished )
			{
				if ( Input.anyKeyDown )
				{
					ThoughtBubble thoughtBubble = (Instantiate( thoughtBubblePrefab.gameObject ) as GameObject).GetComponent<ThoughtBubble>();
					thoughtBubble.SetColor( _coreColor );
					thoughtBubble.transform.position = camera.transform.position + (camera.transform.forward * 2.0f);
					thoughtBubble.transform.forward = camera.transform.forward;

					if ( shootFX != null )
						AudioSource.PlayClipAtPoint( shootFX, transform.position, 0.25f );
				}
			}
		}
		else
		{
			if ( !_gameFinsished )
			{
				if ( Input.anyKeyDown )
					StartGame();
			}
			else if ( !_loading )
			{
				if ( Input.anyKeyDown )
				{
					Application.LoadLevel( "Game" );
					_loading = true;
					return;
				}
			}
		}
	}

	private void StartGame()
	{
		if ( !_started && !_gameFinsished )
		{
			if ( _stopWatch != null )
				_stopWatch.StartTime();

			if ( mainMenu != null )
			{
				int len = mainMenu.Length;
				for ( int i = 0; i < len; i++ )
					mainMenu[i].GetComponent<Text>().DOFade( 0.0f, 0.75f );
			}

			_started = true;
		}
	}

	private void EndGame()
	{
		_started = false;
	}

	private void Solved()
	{
		Debug.Log( "SOLVED!" );

		if ( _stopWatch != null )
		{
			_stopWatch.StopTime();
			Debug.Log( _stopWatch.ToString() );
		}

		if ( solvedMenu != null )
		{
			int len = solvedMenu.Length;
			for ( int i = 0; i < len; i++ )
			{
				solvedMenu[i].SetActive( true );

				Text solvedText = solvedMenu[i].GetComponent<Text>();
				if ( solvedText != null )
				{
					solvedText.color = new Color( solvedText.color.r, solvedText.color.g, solvedText.color.b, 0.0f );
					solvedText.DOFade( 1.0f, 0.75f ).OnComplete( EndGame );
				}
			}
		}

		_gameFinsished = true;
	}

	private void GameOver()
	{
		Debug.Log( "GAME OVER" );

		if ( _stopWatch != null )
		{
			_stopWatch.StopTime();
			Debug.Log( _stopWatch.ToString() );
		}

		if ( gameOverMenu != null )
		{
			int len = gameOverMenu.Length;
			for ( int i = 0; i < len; i++ )
			{
				gameOverMenu[i].SetActive( true );

				Text gameOverText = gameOverMenu[i].GetComponent<Text>();
				if ( gameOverText != null )
				{
					gameOverText.color = new Color( gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0.0f );
					gameOverText.DOFade( 1.0f, 0.75f ).OnComplete( EndGame );
				}
			}
		}

		_gameFinsished = true;
	}

	private void CreateColorsList()
	{
		Cube.blackMat = blackMat;
		Cube.blueMat = blueMat;
		Cube.greenMat = greenMat;
		Cube.orangeMat = orangeMat;
		Cube.redMat = redMat;
		Cube.whiteMat = whiteMat;
		Cube.yellowMat = yellowMat;

		int amountCubes = 26; // core cube isn't counted
		int amountFaces = amountCubes * 6;
		int colorIndex = 0;
		int count = 0;

		Cube.CubeColor[] colors = new Cube.CubeColor[6];

		colors[0] = Cube.CubeColor.Blue;
		colors[1] = Cube.CubeColor.Green;
		colors[2] = Cube.CubeColor.Orange;
		colors[3] = Cube.CubeColor.Red;
		colors[4] = Cube.CubeColor.White;
		colors[5] = Cube.CubeColor.Yellow;

		_colorsAvailable = new List<Cube.CubeColor>();

		for ( int i = 0; i < amountFaces; i++ )
		{
			_colorsAvailable.Add( colors[colorIndex] );

			if ( ++count == amountCubes )
			{
				colorIndex++;
				count = 0;
			}
		}
	}

	private void ColorCubes()
	{
		Cube.CubeColor[] colors = new Cube.CubeColor[6];

		colors[0] = Cube.CubeColor.Blue;
		colors[1] = Cube.CubeColor.Green;
		colors[2] = Cube.CubeColor.Orange;
		colors[3] = Cube.CubeColor.Red;
		colors[4] = Cube.CubeColor.White;
		colors[5] = Cube.CubeColor.Yellow;

		int len = _cubes.Length;
		int centerColorIndex = 0;

		for ( int i = 0; i < len; i++ )
		{
			Cube cube = _cubes[i];

			cube.Init( this );

			if ( !cube.name.ToLower().Contains( "center" ) )
			{
				for ( int j = 0; j < 6; j++ )
					cube.ChangeColor( GetRandomAvailableColor(), j );
			}
			else
			{
				Cube.CubeColor centerColor = colors[centerColorIndex++];
				cube.ChangeColor( centerColor );

				_colorsAvailable.Remove( centerColor );
				_colorsAvailable.Remove( centerColor );
				_colorsAvailable.Remove( centerColor );
				_colorsAvailable.Remove( centerColor );
				_colorsAvailable.Remove( centerColor );
				_colorsAvailable.Remove( centerColor );
			}
		}
	}

	private Cube.CubeColor GetRandomAvailableColor()
	{
		int index = Random.Range( 0, _colorsAvailable.Count );
		Cube.CubeColor color = _colorsAvailable[index];
		_colorsAvailable.RemoveAt( index );
		return color;
	}

	private void CalculateCubePositions()
	{
		cornerPositions = new List<Vector3>();
		centerPositions = new List<Vector3>();
		cardinalPositions = new List<Vector3>();

		int len = _cubes.Length;
		for ( int i = 0; i < len; i++ )
		{
			Cube cube = _cubes[i];
			Cube.CubeType type = Cube.CalculateCubeType( cube );

			if ( type == Cube.CubeType.Corner )
				cornerPositions.Add( cube.transform.localPosition );
			else if ( type == Cube.CubeType.Center )
				centerPositions.Add( cube.transform.localPosition );
			else if ( type == Cube.CubeType.Cardinal )
				cardinalPositions.Add( cube.transform.localPosition );
		}
	}

	private void StartRandomization()
	{
		_rotateInterval = rotateIntervalStart;
		_rotateFluctuation = rotateIntervalStartFluctuation;

		DOTween.To( () => this.rotateInterval, x => this.rotateInterval = x, rotateIntervalEnd, startToEndDuration );
		DOTween.To( () => this.rotateFluctuation, x => this.rotateFluctuation = x, rotateIntervalEndFluctuation, startToEndDuration );

		Invoke( "TransitionRandomCube", _rotateInterval );
	}

	private void TransitionRandomCube()
	{
		_cubes[Random.Range( 0, _cubes.Length )].Transition();

		CancelInvoke( "TransitionRandomCube" );
		Invoke( "TransitionRandomCube", _rotateInterval );
	}

	public void CalculateIfSolved()
	{
		if ( _started )
		{
			if ( faceColliders != null )
			{
				int len = faceColliders.Length;
				if ( len > 0 )
				{
					StopAllCoroutines();
					StartCoroutine( CheckIfSolved() );

					for ( int i = 0; i < len; i++ )
					{
						faceColliders[i].FinishChecking();
						faceColliders[i].CheckIfSameColor();
					}
				}
			}
		}
	}

	private IEnumerator CheckIfSolved()
	{
		bool solved = false;
		bool checkSolved = false;

		int len = faceColliders.Length;
		int checkedCount = 0;

		while ( !checkSolved )
		{
			for ( int i = 0; i < len; i++ )
			{
				RubikFaceCollider face = faceColliders[i];

				if ( face.IsFinishedChecking() )
					checkedCount++;
			}

			if ( checkedCount == len )
				checkSolved = true;

			yield return null;
		}

		Cube.CubeColor color = faceColliders[0].GetColor();
		solved = true;

		for ( int i = 0; i < len; i++ )
		{
			RubikFaceCollider face = faceColliders[i];

			if ( !face.IsSameColor( color ) )
			{
				solved = false;
				break;
			}
		}

		if ( solved && color == Cube.CubeColor.Black )
		{
			solved = false;
			GameOver();
		}

		if ( solved )
			Solved();
	}

	public void ChangeCoreColor( Cube.CubeColor color )
	{
		Color mColor = Cube.GetColor( color );

		_coreColor = color;

		if ( _coreRenderers != null )
		{
			int len = _coreRenderers.Length;
			for ( int i = 0; i < len; i++ )
				_coreRenderers[i].material.DOColor( mColor, 1.0f );
		}

		if ( walls != null )
		{
			int len = walls.Length;
			for ( int i = 0; i < len; i++ )
				walls[i].material.DOColor( mColor, 1.0f );
		}

		if ( _cubes != null )
		{
			int len = _cubes.Length;
			for ( int i = 0; i < len; i++ )
				_cubes[i].ChangeEdgeColor( mColor, 1.0f );
		}
	}

	public Cube.CubeColor GetCoreColor()
	{
		return _coreColor;
	}

	public float rotateInterval
	{
		get { return _rotateInterval; }
		set { _rotateInterval = value; }
	}

	public float rotateFluctuation
	{
		get { return _rotateFluctuation; }
		set { _rotateFluctuation = value; }
	}

}

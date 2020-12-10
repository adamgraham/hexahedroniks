using UnityEngine;
using System.Collections;
using DG.Tweening;

[RequireComponent( typeof( Renderer ) )]
public class CubeFace : MonoBehaviour 
{
	internal Cube cube;
	internal Cube.CubeColor color;
	internal Renderer renderer;
	internal Material mat;

	static internal GameObject spreadColliderPrefab;
	static internal float edgeSize;

	static public bool canSpreadFromCenter = true;

	private void OnCollisionEnter( Collision collision )
	{
		if ( cube != null )
		{
			if ( !cube._transitioning )
			{
				if ( collision.collider.gameObject.GetComponent<ThoughtBubble>() != null )
				{
					Cube.CubeColor coreColor = cube._rubik.GetCoreColor();

					if ( cube._type == Cube.CubeType.Center || coreColor == Cube.CubeColor.Black )
					{
						if ( color != Cube.CubeColor.Black )
						{
							if ( canSpreadFromCenter && color == coreColor )
							{
								Spread( coreColor );
								PlayGoodSFX();
							}
							else
							{
								cube._rubik.ChangeCoreColor( color );
								PlayCoreHitSFX();
							}
						}
						else
						{
							Spread( Cube.CubeColor.Black );
							PlayBadSFX();
						}
					}
					else
					{
						if ( color == coreColor )
						{
							if ( color != Cube.CubeColor.Black )
							{
								Spread( coreColor );
								PlayGoodSFX();
							}
							else
							{
								Spread( Cube.CubeColor.Black );
								PlayBadSFX();
							}
						}
						else
						{
							Spread( Cube.CubeColor.Black );
							PlayBadSFX();
						}
					}
				}
			}
		}
	}

	private void OnTriggerEnter( Collider other )
	{
		SpreadCollider spread = other.gameObject.GetComponent<SpreadCollider>();
		if ( spread != null )
		{
			if ( cube._rubik.GetCoreColor() != Cube.CubeColor.Black )
			{
				if ( color == Cube.CubeColor.Black )
				{
					if ( cube._faces != null )
					{
						int len = cube._faces.Length;
						for ( int i = 0; i < len; i++ )
						{
							CubeFace face = cube._faces[i];
							if ( face != this && face.color == Cube.CubeColor.Black )
								cube.ChangeColor( Cube.GetRandomColor(), i );
						}
					}
				}
			}

			if ( canSpreadFromCenter )
				SetMat( spread.mat, spread.color );
			else if ( cube._type != Cube.CubeType.Center )
				SetMat( spread.mat, spread.color );
		}
	}

	internal void SetMat( Material _mat, Cube.CubeColor _color )
	{
		renderer.DOKill();
		renderer.material.DOColor( _mat.color, 0.5f ).
			SetId( renderer ).OnComplete( OnSetMatComplete );

		mat = _mat;
		color = _color;

		//cube._rubik.CalculateIfSolved();
	}

	private void OnSetMatComplete()
	{
		renderer.material = mat;
	}

	private void Spread( Cube.CubeColor color )
	{
		if ( color == Cube.CubeColor.Black )
		{
			cube.ChangeColor( color );
			cube._rubik.ChangeCoreColor( Cube.CubeColor.Black );
		}

		SpreadCollider collider1 = (Instantiate( spreadColliderPrefab.gameObject, transform.position, transform.rotation ) as GameObject).GetComponent<SpreadCollider>();
		SpreadCollider collider2 = (Instantiate( spreadColliderPrefab.gameObject, transform.position, transform.rotation ) as GameObject).GetComponent<SpreadCollider>();

		collider1.source = cube;
		collider1.color = color;
		collider1.mat = mat;

		collider2.source = cube;
		collider2.color = color;
		collider2.mat = mat;

		float nonEdgeSize = (1.0f - (edgeSize * 2.0f)) * 2.0f;

		if ( transform.localScale.x == edgeSize )
		{
			collider1.transform.localScale = new Vector3( collider1.transform.localScale.x, nonEdgeSize, collider1.transform.localScale.z );
			collider2.transform.localScale = new Vector3( collider2.transform.localScale.x, collider2.transform.localScale.y, nonEdgeSize );
		}
		else if ( transform.localScale.y == edgeSize )
		{
			collider1.transform.localScale = new Vector3( nonEdgeSize, collider1.transform.localScale.y, collider1.transform.localScale.z );
			collider2.transform.localScale = new Vector3( collider2.transform.localScale.x, collider2.transform.localScale.y, nonEdgeSize );
		}
		else if ( transform.localScale.z == edgeSize )
		{
			collider1.transform.localScale = new Vector3( nonEdgeSize, collider1.transform.localScale.y, collider1.transform.localScale.z );
			collider2.transform.localScale = new Vector3( collider2.transform.localScale.x, nonEdgeSize, collider2.transform.localScale.z );
		}
	}

	private void PlayGoodSFX()
	{
		if ( cube._rubik.goodSFX != null )
			AudioSource.PlayClipAtPoint( cube._rubik.goodSFX, transform.position, 0.5f );
	}

	private void PlayBadSFX()
	{
		if ( cube._rubik.badSFX != null )
			AudioSource.PlayClipAtPoint( cube._rubik.badSFX, transform.position, 0.75f );
	}

	private void PlayCoreHitSFX()
	{
		if ( cube._rubik.coreHitSFX != null )
			AudioSource.PlayClipAtPoint( cube._rubik.coreHitSFX, transform.position, 0.5f );
	}

}

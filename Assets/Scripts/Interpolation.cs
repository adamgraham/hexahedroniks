using UnityEngine;
using System.Collections;

public class Interpolation  
{
	static public float QuadInOut( float t, float d, float a, float b ) 
	{
		t /= (d * 0.5f);

		if ( t < 1.0 ) 
			return ((b-a)*0.5f)*t*t + a;

		t -= 1.0f;

		return -((b-a)*0.5f) * (t*(t-2.0f) - 1.0f) + a;
	}

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Stopwatch : MonoBehaviour 
{
	public Text text;

	private float _time;
	private bool _running;

	public override string ToString()
	{
		return "Stopwatch: " + ToHHMMSS( (int)_time );
	}

	private void Update()
	{
		if ( _running )
		{
			_time += Time.deltaTime;
			UpdateText();
		}
	}

	public void StartTime()
	{
		_running = true;
	}

	public void StopTime()
	{
		_running = false;
	}

	public void ResetTime()
	{
		_time = 0.0f;
	}

	private void UpdateText()
	{
		if ( text != null )
			text.text = ToHHMMSS( (int)_time );
	}

	static public string ToHHMMSS( int seconds )
	{
		int minutes = (int)(SecondsToMinutes( (float)seconds ) % 3600);
		int hours = (int)(SecondsToHours( (float)seconds ) % 86400);

		string strSeconds = ((seconds % 60) < 10) ? "0" + (seconds % 60).ToString() : (seconds % 60).ToString();
		string strMinutes = (minutes < 10) ? "0" + minutes.ToString() : minutes.ToString();
		string strHours = (hours < 10) ? "0" + hours.ToString() : hours.ToString();

		return strHours + ":" + strMinutes + ":" + strSeconds;
	}

	static public float SecondsToHours( float seconds )
	{
		return MinutesToHours( SecondsToMinutes( seconds ) );
	}

	static public float SecondsToMinutes( float seconds )
	{
		return seconds / 60.0f;
	}

	static public float MinutesToHours( float minutes )
	{
		return minutes / 60.0f;
	}

}

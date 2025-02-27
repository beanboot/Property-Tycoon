using Godot;
using System;
using System.Threading;
using Range = Godot.Range;

public partial class Dice : AnimatedSprite2D
{
	private int _currentVal;
	private double _elapsedTime;
	private double _duration = 2.0f;
	private bool _isRolling;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_isRolling = false;
		_currentVal = 0;
		SetFrame(_currentVal);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_isRolling)
		{
			_elapsedTime += delta;
			if (_elapsedTime >= _duration)
			{
				stop_rolling();
			}
		}
	}

	public void roll(uint targetVal)
	{
		_currentVal = (int)targetVal - 1;
		_isRolling = true;
		_elapsedTime = 0.0f;
		_duration = 2.0f;
		Play();
	}

	private void stop_rolling()
	{
		_isRolling = false;
		Stop();
		Frame = _currentVal;
	}
	
	
}

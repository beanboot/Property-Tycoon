using Godot;
using System;
using System.Numerics;

public partial class Player : Node2D
{
	// CurrentPos is unique to each player and holds the current position of the player
	private int _currentPos;
	private int _balance; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_currentPos = 0;
		_balance = 1500;
	}

	// Takes a vector2 as a parameter (p1) and sets the player's spacial position to that of p1
	public void player_movement(Godot.Vector2 p1)
	{
		Position = p1;
		
	}

	// Iterates the currentPos variable by 1, if it surpasses 40 it goes back to 0
	public void iterate_pos()
	{
		_currentPos += 1;
		if (_currentPos > 39) {
			_currentPos = 0;
		};
	}

	// Returns the player's current position
	public int get_pos()
	{
		return _currentPos;
	}

	public int get_balance()
	{
		return _balance;
	}
}

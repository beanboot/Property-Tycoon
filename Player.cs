using Godot;
using System;
using System.Numerics;

public partial class Player : Node2D
{
	// CurrentPos is unique to each player and holds the current position of the player
	private int currentPos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentPos = 0;
	}

	// Takes a vector2 as a parameter (p1) and sets the player's spacial position to that of p1
	public void player_movement(Godot.Vector2 p1)
	{
		this.Position = p1;
		
	}

	// Iterates the currentPos variable by 1, if it surpasses 40 it goes back to 0
	public void iterate_pos()
	{
		currentPos += 1;
		if (currentPos > 39) {
			currentPos = 0;
		};
	}

	// Returns the player's current position
	public int get_pos()
	{
		return currentPos;
	}
}

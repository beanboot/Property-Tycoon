using Godot;
using System;
using System.Numerics;

public partial class Player : Node2D
{
	private int currentPos;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentPos = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void player_movement(Godot.Vector2 p1)
	{
		this.Position = p1;
		
	}

	public void iterate_pos()
	{
		currentPos += 1;
		if (currentPos > 7) {
			currentPos = 0;
		};
	}

	public int get_pos()
	{
		return currentPos;
	}
}

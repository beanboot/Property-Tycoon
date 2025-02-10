using Godot;
using System;
using System.Formats.Asn1;

public partial class Board : Node2D
{
	private uint[] diceRoll;
	private Sprite2D[] boardSpaces;
	private bool canPressButton = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boardSpaces = new Sprite2D[]
		{
			GetNode<Sprite2D>("BoardSpace1"),
			GetNode<Sprite2D>("BoardSpace2"),
			GetNode<Sprite2D>("BoardSpace3"),
			GetNode<Sprite2D>("BoardSpace4"),
			GetNode<Sprite2D>("BoardSpace5"),
			GetNode<Sprite2D>("BoardSpace6"),
			GetNode<Sprite2D>("BoardSpace7"),
			GetNode<Sprite2D>("BoardSpace8")
		};
		var player = GetNode<Player>("Player");
		player.player_movement(boardSpaces[0].Position);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private async void _on_button_pressed()
	{
		if (canPressButton) {
			canPressButton = false;
			var textBox = GetNode<RichTextLabel>("Button/DiceOutput");
			var player = GetNode<Player>("Player");

			diceRoll = new uint[] {GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			textBox.Text = Convert.ToString(diceRoll[0]) + ", " + Convert.ToString(diceRoll[1]);

			int targetMoveValue = Convert.ToInt16(diceRoll[0] + diceRoll[1]);
		
			for (int i = 0; i < targetMoveValue; i++) {
				player.player_movement(boardSpaces[(player.get_pos() + 1) % 8].Position);
				player.iterate_pos();
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			}
			canPressButton = true;
		}

	}
}

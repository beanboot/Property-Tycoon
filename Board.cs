using Godot;
using System;
using System.Formats.Asn1;

public partial class Board : Node2D
{
	private uint[] diceRoll;
	private Sprite2D[] boardSpaces;
	private Player[] players;
	private bool canPressButton = true;
	private int numOfPlayers = 2;
	private Player currentPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Fills boardSpaces array with every space on the board
		boardSpaces = new Sprite2D[40];
		int i;
		for (i = 0; i < 40; i++) {
			boardSpaces[i] = GetNode<Sprite2D>("BoardSpaces/BoardSpace" + (i+1));
		};

		var playerScene = GD.Load<PackedScene>("res://player.tscn");
		var playerInstance = playerScene.Instantiate();
		players = new Player[numOfPlayers];
		for (i = 0; i < numOfPlayers; i++) {
			playerInstance.Name = ("Player" + (i+1));
			AddChild(playerInstance);
			players[i] = GetNode<Player>("Player" + (i+1));
		}
		
		currentPlayer = players[0];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var playerTextBox = GetNode<RichTextLabel>("CurrentPlayerText");
		playerTextBox.Text = ("It is currently: ") + currentPlayer.Name + ("'s turn"); 
	}

	// Called when the Roll Dice button is pressed
	private async void _on_button_pressed()
	{
		// If statement will only run if the player is not currently moving
		if (canPressButton) {
			canPressButton = false;
			var textBox = GetNode<RichTextLabel>("Button/DiceOutput");
			var player = GetNode<Player>("Player");

			// Rolls two d6s and stores each roll in the diceRoll array, prints result to textbox
			diceRoll = new uint[] {GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			textBox.Text = Convert.ToString(diceRoll[0]) + ", " + Convert.ToString(diceRoll[1]);

			// targetMoveValue combines each dice roll into one integer
			int targetMoveValue = Convert.ToInt16(diceRoll[0] + diceRoll[1]);
		
			// Iterates the player through the boardSpaces array targetMoveValue times (with a delay)
			for (int i = 0; i < targetMoveValue; i++) {
				currentPlayer.player_movement(boardSpaces[(currentPlayer.get_pos() + 1) % 40].Position);
				currentPlayer.iterate_pos();
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

				/* Potential dynamic delay code:
				double delay = (0.6 / (targetMoveValue - i)) + 0.1;
				await ToSignal(GetTree().CreateTimer(delay), "timeout"); */ 
				
			};

			canPressButton = true;
		}

	}
}

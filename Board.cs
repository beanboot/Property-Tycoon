using Godot;
using System;


public partial class Board : Node2D
{
	private uint[] _diceRoll;
	private Sprite2D[] _boardSpaces;
	private Player[] _players;
	private bool _canPressButton = true;
	private int _numOfPlayers = 2;
	private int _currentPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		initialise_board();

		initialise_players();
		
		_currentPlayer = 0;

		display_current_player_text();
	}

	// Iterates the currentPlayer variable by 1 unless it excedes the number of players
	public void change_player()
	{
		if (_currentPlayer >= (_numOfPlayers - 1)) {
			_currentPlayer = 0;
		}

		else {
			_currentPlayer += 1;
		}

		display_current_player_text();
	}

	// Updates the text displaying what player's turn it is
	public void display_current_player_text()
	{
		var playerTextBox = GetNode<RichTextLabel>("CurrentPlayerText");
		playerTextBox.Text = ("It is currently: ") + _players[_currentPlayer].Name + ("'s turn");
	}

	// Called within _Ready(), fills the array players[] by instantiating the player scene depending on numOfPlayers
	public void initialise_players() 
	{
		int i;
		var playerScene = GD.Load<PackedScene>("res://player.tscn");
		_players = new Player[_numOfPlayers];
		for (i = 0; i < _numOfPlayers; i++) {
			var playerInstance = playerScene.Instantiate();
			playerInstance.Name = ("Player" + (i+1));
			AddChild(playerInstance);
			_players[i] = GetNode<Player>("Player" + (i+1));
			_players[i].player_movement(_boardSpaces[0].Position);
		};
	}

	// Called within _Ready(), fills the array boardSpaces[] with every space on the board
	public void initialise_board() 
	{
		int i;
		_boardSpaces = new Sprite2D[40];
		for (i = 0; i < 40; i++) {
			_boardSpaces[i] = GetNode<Sprite2D>("BoardSpaces/BoardSpace" + (i+1));
		};
	}

	// Rolls two d6s and stores each roll in the diceRoll array, prints result to textbox
	public void dice_roll()
	{
			var textBox = GetNode<RichTextLabel>("Button/DiceOutput");

			_diceRoll = new []{GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			textBox.Text = Convert.ToString(_diceRoll[0]) + ", " + Convert.ToString(_diceRoll[1]);
			var dice1 = GetNode<Dice>("Button/Dice1");
			dice1.roll(_diceRoll[0]);
			var dice2 = GetNode<Dice>("Button/Dice2");
			dice2.roll(_diceRoll[1]);
	}

	public async void move_current_player()
	{
			// targetMoveValue combines each dice roll into one integer
			int targetMoveValue = Convert.ToInt16(_diceRoll[0] + _diceRoll[1]);
		
			// Iterates the current player through the boardSpaces array targetMoveValue times (with a delay)
			for (int i = 0; i < targetMoveValue; i++) {
				_players[_currentPlayer].player_movement(_boardSpaces[(_players[_currentPlayer].get_pos() + 1) % 40].Position);
				_players[_currentPlayer].iterate_pos();
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

				/* Potential dynamic delay code:
				double delay = (0.6 / (targetMoveValue - i)) + 0.1;
				await ToSignal(GetTree().CreateTimer(delay), "timeout"); */ 
			};

			change_player();
	}

	// Called when the Roll Dice button is pressed
	private async void _on_button_pressed()
	{
		// If statement will only run if a player is not currently moving
		if (_canPressButton) {
			_canPressButton = false;

			dice_roll();
			
			move_current_player();

			_canPressButton = true;
		}

	}
}

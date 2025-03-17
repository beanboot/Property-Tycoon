namespace PropTycoon
{
using Godot;
using System;

public partial class Board : Node2D
{
	//Global Variables
	private uint[] _diceRoll;
	private Sprite2D[] _boardSpaces;
	private Player[] _players;
	private bool _canPressButton = true;
	private int _numOfPlayers = 2;
	private int _currentPlayerIndex;
	private Space[] _boardSpaceData;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		initialise_board();

		initialise_players();
		
		_currentPlayerIndex = 0;

		display_current_player_text();

		initialise_board_data();
	}

	public void initialise_board_data()
	{
		_boardSpaceData = new Space[40];
		using (var boardData = FileAccess.Open("res://data/BoardData.csv", FileAccess.ModeFlags.Read))
		{
			bool firstLine = true;
			int i = 0;
			int j;
			while (!boardData.EofReached())
			{
				string line = boardData.GetLine();
				if (firstLine) {
					firstLine = false;
					continue;
				}
				
				string[] values = line.Split(",");
				if(values[4].Equals("Yes")) {
					int[] rent = new int[6];
					for (j = 0; j < 6; j++) {
						if (values[j + 6].Equals("")) {
							rent[j] = 0;
						}
						else {
							rent[j] = int.Parse(values[j + 6]);
						}
					}
					_boardSpaceData[i] = new PropertySpace(int.Parse(values[0]), values[1], values[2], int.Parse(values[5]), rent);
				}
				else if(values[4].Equals("No")) {
					_boardSpaceData[i] = new Space(int.Parse(values[0]), values[1], values[2]);
				}
				
				i += 1;
			}
		}
	}

	public void display_board_info() {
		var debugTextBox = GetNode<RichTextLabel>("CurrentBoardInfo");
		debugTextBox.Text = "Player 1 Position: " + _players[0].get_pos() + "\n" 
		+ "Player 1 Current Space: " + _boardSpaceData[_players[0].get_pos()].get_name() + "\n\n" 
		+ "Player 2 Position: " + _players[1].get_pos() + "\n" 
		+ "Player 1 Current Space: " + _boardSpaceData[_players[1].get_pos()].get_name();
	}

	// Iterates the currentPlayer variable by 1 unless it excedes the number of players
	public void change_player()
	{
		if (_currentPlayerIndex >= (_numOfPlayers - 1)) {
			_currentPlayerIndex = 0;
		}

		else {
			_currentPlayerIndex += 1;
		}

		display_current_player_text();
	}

	// Updates the text displaying what player's turn it is
	public void display_current_player_text()
	{
		var playerTextBox = GetNode<RichTextLabel>("CurrentPlayerDebug");
		playerTextBox.Text = ("It is currently: ") + _players[_currentPlayerIndex].Name + ("'s turn");
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
			_players[i].player_movement((_boardSpaces[0].Position) + (GetNode<Node2D>("BoardSpaces").Position));
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

	// Rolls two d6s and stores each roll in the diceRoll array, displays results via 2 AnimatedSprite2Ds
	public async void dice_roll()
	{
			_diceRoll = new []{GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			var dice1 = GetNode<Dice>("Button/Dice1");
			dice1.roll(_diceRoll[0]);
			var dice2 = GetNode<Dice>("Button/Dice2");
			dice2.roll(_diceRoll[1]);
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
			move_current_player();
	}
	
	//Moves the player based on the number they rolled in dice_roll
	public async void move_current_player()
	{
			// targetMoveValue combines each dice roll into one integer
			int targetMoveValue = Convert.ToInt16(_diceRoll[0] + _diceRoll[1]);
		
			// Iterates the current player through the boardSpaces array targetMoveValue times (with a delay)
			for (int i = 0; i < targetMoveValue; i++) {
				_players[_currentPlayerIndex].player_movement((_boardSpaces[(_players[_currentPlayerIndex].get_pos() + 1) % 40].Position) + (GetNode<Node2D>("BoardSpaces").Position));
				_players[_currentPlayerIndex].iterate_pos();
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

				/* Potential dynamic delay code:
				double delay = (0.6 / (targetMoveValue - i)) + 0.1;
				await ToSignal(GetTree().CreateTimer(delay), "timeout"); */ 
			};

			if (_diceRoll[0] == _diceRoll[1]) {
				// placeholder for double roll counter (go to jail)
			} else {
				change_player();
			}
	
			_canPressButton = true;
	}

	// Called when the Roll Dice button is pressed
	private void _on_button_pressed()
	{
		// If statement will only run if a player is not currently moving
		if (_canPressButton) {
			_canPressButton = false;

			dice_roll();
		}

	}

	public override void _Process(double delta) 
	{
		display_board_info();
	}
}


}

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
	private int _numOfPlayers = 5;
	private int _currentPlayerIndex;
	private BoardData _boardData;
	private Deck _deck;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_deck = new Deck();
		_boardData = new BoardData();

		initialise_board();
		
		initialise_players();
		
		_currentPlayerIndex = 0;
		
		display_current_player_text();
		
	}

	public void display_board_info() {
		var debugTextBox = GetNode<RichTextLabel>("CurrentBoardInfo");
		string displayBoardInfo = "";
		for (int i = 0; i < _numOfPlayers; i++)
		{
			int playerPos = _players[i].get_pos();
			displayBoardInfo += "Player " + (i+1) + " is on " + _boardData.get_space(playerPos).get_name() + "\n";
			
		}
		debugTextBox.Text = displayBoardInfo;
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
		display_board_info();
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
			_players[i].set_name("Player"+(i+1));
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
			//determines the type of board space and loads the respective texture
			SpaceType type = _boardData.get_space(i).get_type();
			Console.WriteLine(_boardData.get_space(i));
			switch (type)
			{
				case SpaceType.PL:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/potluckSpace.png");
					break;
				case SpaceType.OK:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/opportunityknocksSpace.png");
					break;
				case SpaceType.FINE:
					if (i == 4)
					{
						_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/taxSpace.png");
					}else if (i == 38)
					{
						_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/supertaxSpace.png");
					}
					break;
				case SpaceType.STATION:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/stationSpace.png");
					break;
				case SpaceType.UTIL:
					if (i == 12)
					{
						_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/electriccompanySpace.png");
					}
					else if (i == 28)
					{
						_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/waterworksSpace.png");
					}
					break;
				case SpaceType.BROWN:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/brownPSpace.png");
					break;
				case SpaceType.BLUE:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/bluePSpace.png");
					break;
				case SpaceType.PURPLE:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/purplePSpace.png");
					break;
				case SpaceType.ORANGE:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/orangePSpace.png");
					break;
				case SpaceType.RED:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/redPSpace.png");
					break;
				case SpaceType.YELLOW:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/yellowPSpace.png");
					break;
				case SpaceType.GREEN:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/greenPSpace.png");
					break;
				case SpaceType.DEEPBLUE:
					_boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/Property Spaces/deepbluePSpace.png");
					break;
				
			}
			//sets the scale for each space that isn't on a corner
			if((i+1) % 10 != 1)
			{
				//sets scale and adds label to space
				_boardSpaces[i].Scale = new Vector2((float)0.157, (float)0.17);
				add_label_to_space(_boardSpaces[i], i);
			}
		};
	}

	private void add_label_to_space(Sprite2D space, int pos)
	{
		//creates a label, adds the name and cost/description and then sets the position
		Label name = new Label();
		name.Text = _boardData.get_space(pos).get_name();
		Vector2 position = space.GetTexture().GetSize();
		name.Position.Clamp(position.X, position.Y);
		Console.WriteLine("Space " + (pos + 1) + " " + position); ;
		
		name.Position = position;
		name.Scale = new Vector2(4, 4);
		space.AddChild(name);
		
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
			Player currentPlayer = _players[_currentPlayerIndex];
			_boardData.get_space(currentPlayer.get_pos()).land(currentPlayer);

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
	}
}


}

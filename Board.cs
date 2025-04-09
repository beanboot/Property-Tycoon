using System.Collections.Generic;

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
	private Jail jail;
	private FreeParking freeParking;
	private BoardData _boardData;
	private Deck _deck;
	private Bank bank;
	private int goValue = 200;
	private bool purchaseable;
	private bool doubleRoll = false;
	private int doubleRollCounter = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_deck = new Deck();
		_boardData = new BoardData();
		bank = new Bank(_boardData.get_property_list(), 50000);

		initialise_board();
		
		initialise_players();
		
		_currentPlayerIndex = 0;
		
		display_current_player_text();
		
	}

	public void display_board_info() 
	{
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
		Player currentPlayer = _players[_currentPlayerIndex];
		//if the player is in prison they can either use their Get out of jail free card or remain in prison until daysInJail reaches 2
		//leaving prison uses your turn so no matter what the player is changed after a turn concerning prison sentence
			
		if (jail.is_in_jail(currentPlayer))
		{
			if (currentPlayer.getOutJail)
			{
				jail.release_from_jail(currentPlayer);
			}
			else
			{
				if (currentPlayer.daysInJail >= 2)
				{
					jail.release_from_jail(currentPlayer);
					currentPlayer.daysInJail = 0;
				}
				currentPlayer.daysInJail++;
			}
			change_player();
		}
		display_current_player_text();
	}

	// Updates the text displaying what player's turn it is
	public void display_current_player_text()
	{
		var playerTextBox = GetNode<RichTextLabel>("CurrentPlayerDebug");
		playerTextBox.Text = "It is currently: " + _players[_currentPlayerIndex].Name + "'s turn";
		
	}

	public void display_player_balances()
	{
		var balanceTextBox = GetNode<RichTextLabel>("PlayerBalances");
		string displayBalances = "Bank balance: £" + bank.get_bank_balance() + "\n\n";
		
		for (int i = 0; i < _numOfPlayers; i++)
		{
			int playerBalance = _players[i].get_balance();
			string playerName = _players[i].get_name();
			displayBalances += playerName + "'s balance is: £" + playerBalance + "\n";
			
		}

		balanceTextBox.Text = displayBalances;
	}

		public void display_player_properties()
	{
		var propertiesTextBox = GetNode<RichTextLabel>("OwnedPropertiesDisplay");
		string propertiesText = "";
		
		for (int i = 0; i < _numOfPlayers; i++)
		{
			string playerName = _players[i].get_name();
			LinkedList<Property> properties = _players[i].get_properties();
			propertiesText += "\n" + playerName + " owns:" + "\n";

			var current = properties.First;
			for (int j = 0; j < properties.Count; j++)
			{
				propertiesText += current.Value.get_name() + "\n";
				current = current.Next;
			}
			
			propertiesTextBox.Text = propertiesText;
		}

		propertiesTextBox.Text = propertiesText;
	}

	// Called within _Ready(), fills the array players[] by instantiating the player scene depending on numOfPlayers
	public void initialise_players() 
	{
		int i;
		var playerScene = GD.Load<PackedScene>("res://player.tscn");
		_players = new Player[_numOfPlayers];
		for (i = 0; i < _numOfPlayers; i++)
		{
			var playerInstance = playerScene.Instantiate();
			playerInstance.Name = "Player" + (i+1);
			AddChild(playerInstance);
			_players[i] = GetNode<Player>("Player" + (i+1));
			_players[i].set_name("Player"+(i+1));
			_players[i].player_movement(_boardSpaces[0].Position + GetNode<Node2D>("BoardSpaces").Position);
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
					} else if (i == 38)
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
			
			if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
			|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE
			|| type == SpaceType.STATION || type == SpaceType.UTIL)
			{
				add_cost_label_to_space(_boardSpaces[i], i);
			}
			
			//sets the scale for each space that isn't on a corner
			if ((i+1) % 10 != 1)
			{
				//sets scale and adds label to space
				_boardSpaces[i].Scale = new Vector2((float)0.157, (float)0.17);
				add_name_label_to_space(_boardSpaces[i], i);
			}
			
			jail = (Jail) _boardData.get_space(10);
			freeParking = (FreeParking) _boardData.get_space(20);
		};
	}

	private void add_name_label_to_space(Sprite2D space, int pos)
	{
		//creates a label, adds the name and then sets the position
		Label name = new Label();
		Vector2 size = new Vector2(200, 50);
		name.Text = _boardData.get_space(pos).get_name();
		name.Scale = new Vector2(4, 4);
		name.Size = size;
		name.Position = -(size * name.Scale) / 2 + new Vector2(0, 500);
		name.HorizontalAlignment = HorizontalAlignment.Center;
		name.VerticalAlignment = VerticalAlignment.Center;

		space.AddChild(name);
	}
	
	private void add_cost_label_to_space(Sprite2D space, int pos)
	{
		//creates a label, adds the cost and then sets the position
		Label cost = new Label();
		Vector2 size = new Vector2(200, 50);
		PropertySpace propertySpace = (PropertySpace) _boardData.get_space(pos);
		int costValue = propertySpace.get_property().get_cost();
		cost.Text = "£" + costValue.ToString();
		cost.Modulate = new Color(0, 0, 0);
		cost.Scale = new Vector2(4, 4);
		cost.Size = size;
		cost.Position = -(size * cost.Scale) / 2 + new Vector2(0, 350);
		cost.HorizontalAlignment = HorizontalAlignment.Center;
		cost.VerticalAlignment = VerticalAlignment.Center;

		space.AddChild(cost);
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

			if (_diceRoll[0] == _diceRoll[1]) 
			{
				doubleRoll = true;
				doubleRollCounter++;
			} else
			{
				doubleRoll = false;
				doubleRollCounter = 0;
			}

			// targetMoveValue combines each dice roll into one integer
			int targetMoveValue = Convert.ToInt16(_diceRoll[0] + _diceRoll[1]);
			move_current_player(targetMoveValue, true);
	}
	
	// Moves the player equal times to the targetMoveValue parameter, canCollect is used to determine whether the player collects £200 when passing go
	public async void move_current_player(int targetMoveValue, bool canCollect)
	{
		Player currentPlayer = _players[_currentPlayerIndex];

		if (doubleRollCounter >= 3)
		{
			send_to_jail(currentPlayer);
			doubleRollCounter = 0;
			change_player();
			_canPressButton = true;
			return;
		}

		// Iterates the current player through the boardSpaces array targetMoveValue times (with a delay)
		for (int i = 0; i < targetMoveValue; i++) {
			_players[_currentPlayerIndex].player_movement(_boardSpaces[(_players[_currentPlayerIndex].get_pos() + 1) % 40].Position + GetNode<Node2D>("BoardSpaces").Position);
			// if the player moves past go, this method will return true and we will give the player £200 from the bank
			if (_players[_currentPlayerIndex].iterate_pos() && canCollect)
			{
				if (bank.take_from_bank(goValue))
				{
					_players[_currentPlayerIndex].increase_balance(goValue);
				} else
				{
					break;
				}
			}
			await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
		};

		SpaceType type = _boardData.get_space(currentPlayer.get_pos()).land(currentPlayer);

		if (type == SpaceType.PL | type == SpaceType.OK)
		{
			play_card(currentPlayer, _deck.draw(type), type);
		} 
		else if (type == SpaceType.GTJ)
		{
			send_to_jail(currentPlayer);
			_canPressButton = true;
			return;
		} 
		else if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
		|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE
		|| type == SpaceType.STATION || type == SpaceType.UTIL)
		{
			PropertySpace currentSpace = (PropertySpace) _boardData.get_space(currentPlayer.get_pos());
			Property property = currentSpace.get_property();

			if (bank.does_bank_contain(property) && currentPlayer.hasPassedGo)
			{
				purchaseable = true;
			}
		}

		if (!purchaseable)
		{
			if (!doubleRoll) 
			{
				change_player();
			} 

			_canPressButton = true;
		}
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

	private void _on_button_2_pressed()
	{
		send_to_jail(_players[_currentPlayerIndex]);
	}

	public void _on_purchase_button_pressed()
	{
		Player currentPlayer = _players[_currentPlayerIndex];
		PropertySpace currentSpace = (PropertySpace) _boardData.get_space(currentPlayer.get_pos());
		Property property = currentSpace.get_property();

		if (bank.purchase_property(property))
		{
			currentPlayer.decrease_balance(property.get_cost());
			currentPlayer.add_to_properties(property);
			var purchaseTextBox = GetNode<RichTextLabel>("PurchaseDisplay");
			purchaseTextBox.Text = currentPlayer.get_name() + " has purchased " + property.get_name() + " for £" + property.get_cost();
		}

		if (!doubleRoll) 
		{
			change_player();
		} 

		purchaseable = false;
		_canPressButton = true;
	}

	public void _on_auction_button_pressed()
	{

	}

	public void play_card(Player player, Card card, SpaceType spaceType)
	{
		CardType cardType = card.get_cardType();
		int cardParam = card.get_cardParameter();
		string cardDescription = card.get_description();
		var cardTextBox = GetNode<RichTextLabel>("CardDisplay");

		string text = player.get_name();
		if (spaceType == SpaceType.OK)
		{
			text += " drew an opportunity knocks card:";
		} else
		{
			text += " drew a pot luck card:";
		}
		text += "\n(" + cardType + ")\n" + cardDescription;
		cardTextBox.Text = text;

		//finds which cardType is being played and then applies the card's parameter value
		switch (cardType)
		{
			case CardType.GTJ:
				send_to_jail(player);
				break;
			case CardType.FINE:
				player.decrease_balance(cardParam);
				freeParking.collect_fine(cardParam);
				break;
			case CardType.GOOJF:
				player.getOutJail = true;
				break;
			case CardType.PAYALL:
				for (int i = 0; i < _players.Length; i++)
				{
					if (i != _currentPlayerIndex)
					{
						player.decrease_balance(cardParam);
						_players[i].increase_balance(cardParam);
					}
				}
				break;
			case CardType.COLLECT:
				player.increase_balance(cardParam);
				break;
			case CardType.PAYBANK:
				player.decrease_balance(cardParam);
				bank.add_to_bank(cardParam);
				break;
			case CardType.FINEOROK:
				// PLAYER HAS TO CHOOSE A FINE OR AN OK CARD
				break;
			case CardType.COLLECTALL:
				for (int i = 0; i < _players.Length; i++)
				{
					if (i != _currentPlayerIndex)
					{
						player.increase_balance(cardParam);
						_players[i].decrease_balance(cardParam);
					}
				}
				break;
			case CardType.PAYREPAIRS:
				LinkedList<Property> properties = player.get_properties();
				int totalCost = 0;
				foreach (Property property in properties)
				{
					if (property.get_num_houses() < 5)
					{
						totalCost += cardParam * property.get_num_houses();
					}else if(property.get_num_houses() == 5){
						if(cardParam == 25){
							totalCost += 100;
						}if(cardParam == 40){
							totalCost += 115;
						}
					}
				}
				player.decrease_balance(totalCost);
				break;
			case CardType.MOVESPACESB:
				
				break;
			case CardType.MOVELOCATIONB:
				
				break;
			case CardType.MOVELOCATIONF:
				
				break;
			default:
				Console.WriteLine("Unknown card type");
				break;
			
		}
	}

	public void send_to_jail(Player player)
	{
		_players[_currentPlayerIndex].set_pos(10);
		_players[_currentPlayerIndex].player_movement(_boardSpaces[_players[_currentPlayerIndex].get_pos() % 40].Position + GetNode<Node2D>("BoardSpaces").Position + new Vector2(20, -20));
		jail.send_to_jail(player);
		change_player();
	}

	public override void _Process(double delta) 
	{
		display_player_balances();
		display_board_info();
		display_player_properties();

		if (!purchaseable)
		{
			GetNode<Button>("PurchaseButton").Hide();
			GetNode<Button>("AuctionButton").Hide();
		} else
		{
			GetNode<Button>("PurchaseButton").Show();
			GetNode<Button>("AuctionButton").Show();
		}
	}
}
}

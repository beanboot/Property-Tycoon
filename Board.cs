using System.Collections.Generic;

namespace PropTycoon {

using Godot;
using System;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;


public partial class Board : Node2D
{
	// initialise global variables
	private bool displayInfo;
	private uint[] diceRoll;
	private Sprite2D[] boardSpaces;
	private Player[] players;
	private bool canPressButton = true;
	private int numOfPlayers;
	private int currentPlayerIndex;
	private Jail jail;
	private FreeParking freeParking;
	private BoardData boardData;
	private Deck deck;
	private Bank bank;
	private int goValue = 200;
	private bool purchaseable;
	private bool doubleRoll = false;
	private int doubleRollCounter = 0;
	private string purchaseLogString = "Purchase Log:";
	private Dictionary<SpaceType, int> colourSetSizes = new()
	{
		{SpaceType.BROWN, 2},
		{SpaceType.BLUE, 3},
		{SpaceType.PURPLE, 3},
		{SpaceType.ORANGE, 3},
		{SpaceType.RED, 3},
		{SpaceType.YELLOW, 3},
		{SpaceType.GREEN, 3},
		{SpaceType.DEEPBLUE, 2}
	};
	private bool doneButtonPressed = false;
	private Card currentCard;
	private GameData gameData;
	private Label[] labelArray = new Label[40];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// passes in information from main menu scene
		gameData = (GameData)GetNode<Node>("/root/GameData");
		numOfPlayers = gameData.numPlayers + gameData.numBots;

		// creates card deck
		deck = new Deck();

		// initialises board data
		boardData = new BoardData();

		// creates the bank
		bank = new Bank(boardData.get_property_list(), 50000);

		initialise_board();
		
		initialise_players();
		
		// index starts at player 1
		currentPlayerIndex = 0;
		
		display_current_player_text();

		show_houses();
	}

	// displays the board space each player is currently on
	public void display_board_info() 
	{
		var debugTextBox = GetNode<RichTextLabel>("CurrentBoardInfo");
		string displayBoardInfo = "";

		for (int i = 0; i < numOfPlayers; i++)
		{
			int playerPos = players[i].get_pos();
			displayBoardInfo += players[i].get_name() + " is on " + boardData.get_space(playerPos).get_name() + "\n";
		}

		debugTextBox.Text = displayBoardInfo;
	}

	// iterates the currentPlayer variable by 1 unless it excedes the number of players
	public void change_player()
	{
		if (currentPlayerIndex >= (numOfPlayers - 1)) {
			currentPlayerIndex = 0;
		}

		else {
			currentPlayerIndex += 1;
		}

		Player currentPlayer = players[currentPlayerIndex];

		// if the player is in prison they can either use their 'get out of jail free' card or remain in prison until daysInJail reaches 2
		// leaving prison uses your turn so no matter what the player is changed after a turn concerning prison sentence
		if (jail.is_in_jail(currentPlayer))
		{
			if (currentPlayer.getOutJail)
			{
				jail.release_from_jail(currentPlayer);
			}
			else
			{
				if (currentPlayer.daysInJail >= 2) // released on third turn
				{
					jail.release_from_jail(currentPlayer);
					currentPlayer.daysInJail = 0;
				}

				currentPlayer.daysInJail++;
				change_player();
			}
		}

		// if the player is bankrupt skip their turn
		if (currentPlayer.isBankrupt)
		{
			change_player();
		}

		display_current_player_text();
	}

	// updates the text displaying what player's turn it is
	public void display_current_player_text()
	{
		var playerTextBox = GetNode<RichTextLabel>("CurrentPlayerDebug");
		playerTextBox.Text = "It is currently: " + players[currentPlayerIndex].Name + "'s turn";
		
	}

	// displays the balance of each player
	public void display_player_balances()
	{
		var balanceTextBox = GetNode<RichTextLabel>("PlayerBalances");
		string displayBalances = "Bank balance: £" + bank.get_bank_balance() + "\n\n";
		
		for (int i = 0; i < numOfPlayers; i++)
		{
			int playerBalance = players[i].get_balance();
			string playerName = players[i].get_name();
			displayBalances += playerName + "'s balance is: £" + playerBalance + "\n";
			
		}

		balanceTextBox.Text = displayBalances;
	}

	// displays what properties each player currently owns
	public void display_player_properties()
	{
		var propertiesTextBox = GetNode<RichTextLabel>("OwnedPropertiesDisplay");
		string propertiesText = "";
		
		for (int i = 0; i < numOfPlayers; i++) // for each player, gets a list of their properties and loops through it to add their names to the text box
		{
			string playerName = players[i].get_name();
			LinkedList<Property> properties = players[i].get_properties();
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

	// called within _Ready(), fills the array players[] by instantiating the player scene depending on numOfPlayers
	public void initialise_players() 
	{
		int i;
		var playerScene = GD.Load<PackedScene>("res://player.tscn");
		players = new Player[numOfPlayers];
		Texture2D[] playerSprites = new Texture2D[6];

		// fills playerSprites[] with each sprite in the files
		playerSprites[0] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.boot.png");
		playerSprites[1] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.cat.png");
		playerSprites[2] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.ship.png");
		playerSprites[3] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.iron.png");
		playerSprites[4] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.mobilephone.png");
		playerSprites[5] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.hatstand.png");

		for (i = 0; i < gameData.numPlayers; i++) // loops through the number of players and assigns them a name and a sprite, then puts them on go
		{
			var playerInstance = playerScene.Instantiate();
			playerInstance.Name = "Player" + (i+1);
			AddChild(playerInstance);

			players[i] = GetNode<Player>("Player" + (i+1));
			var playerSprite = GetNode<Sprite2D>("Player" + (i+1) + "/Sprite2D");
			playerSprite.Texture = playerSprites[i];

			players[i].set_name("Player "+(i+1));
			players[i].player_movement(boardSpaces[0].Position + GetNode<Node2D>("BoardSpaces").Position);
		};

		for (i = gameData.numPlayers; i < numOfPlayers; i++) // does the same but for bots (unfinished ai code)
		{
			var playerInstance = playerScene.Instantiate();
			playerInstance.Name = "Player" + (i+1);
			AddChild(playerInstance);

			players[i] = GetNode<Player>("Player" + (i+1));
			var playerSprite = GetNode<Sprite2D>("Player" + (i+1) + "/Sprite2D");
			playerSprite.Texture = playerSprites[i];

			int j = i - gameData.numPlayers;
			players[i].set_name("Bot "+(j+1));
			players[i].player_movement(boardSpaces[0].Position + GetNode<Node2D>("BoardSpaces").Position);
			players[i].isBot = true;
		};

		displayInfo = true;
	}

	// called within _Ready(), fills the array boardSpaces[] with every space on the board
	public void initialise_board() 
	{
		int i;
		boardSpaces = new Sprite2D[40];

		for (i = 0; i < 40; i++) 
		{
			boardSpaces[i] = GetNode<Sprite2D>("BoardSpaces/BoardSpace" + (i+1));
			SpaceType type = boardData.get_space(i).get_type();

			// loads the relevant texture depending on the board space's type
			switch (type)
			{
				case SpaceType.PL:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/potluckSpace.png");
					break;
				case SpaceType.OK:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/opportunityknocksSpace.png");
					break;
				case SpaceType.FINE:
					if (i == 4)
					{
						boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/taxSpace.png");
					} else if (i == 38)
					{
						boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/supertaxSpace.png");
					}
					break;
				case SpaceType.STATION:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/stationSpace.png");
					break;
				case SpaceType.UTIL:
					if (i == 12)
					{
						boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/electriccompanySpace.png");
					}
					else if (i == 28)
					{
						boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/waterworksSpace.png");
					}
					break;
				case SpaceType.BROWN:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/brownPSpace.png");
					break;
				case SpaceType.BLUE:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/bluePSpace.png");
					break;
				case SpaceType.PURPLE:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/purplePSpace.png");
					break;
				case SpaceType.ORANGE:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/orangePSpace.png");
					break;
				case SpaceType.RED:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/redPSpace.png");
					break;
				case SpaceType.YELLOW:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/yellowPSpace.png");
					break;
				case SpaceType.GREEN:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/greenPSpace.png");
					break;
				case SpaceType.DEEPBLUE:
					boardSpaces[i].Texture = (Texture2D)GD.Load("res://BoardSprites/PropertySpaces/deepbluePSpace.png");
					break;
			}
			
			// adds a cost label if the space has a cost
			if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
			|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE
			|| type == SpaceType.STATION || type == SpaceType.UTIL)
			{
				add_cost_label_to_space(boardSpaces[i], i);
			}
			
			//sets the scale for each space that isn't on a corner
			if ((i+1) % 10 != 1)
			{
				//sets scale and adds label to space
				boardSpaces[i].Scale = new Vector2((float)0.157, (float)0.17);

				// assigns name labels at the correct positions depending on space type
				if (!(type == SpaceType.OK) && !(type == SpaceType.PL))
				{
					if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
					|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
					{
						add_name_label_to_space(boardSpaces[i], i, -265);
					} else
					{
						add_name_label_to_space(boardSpaces[i], i, -320);
					}
					
				}
				
			}
			
			jail = (Jail) boardData.get_space(10);
			freeParking = (FreeParking) boardData.get_space(20);
		};
	}

	// creates a name label and inserts it at the correct position
	private void add_name_label_to_space(Sprite2D space, int pos, int offset)
	{
		Label name = new Label();

		name.Text = boardData.get_space(pos).get_name();

		Vector2 size = new Vector2(100, 50);
		name.CustomMinimumSize = size;
		name.AutowrapMode = TextServer.AutowrapMode.Word;

		name.Modulate = new Color(0, 0, 0);
		name.Size = size;
		name.Scale = new Vector2(4, 4);
		name.Position = -(size * name.Scale) / 2 + new Vector2(0, offset);
		name.HorizontalAlignment = HorizontalAlignment.Center;
		name.VerticalAlignment = VerticalAlignment.Center;

		space.AddChild(name);
	}
	
	// creates a cost label and inserts it at the correct position
	private void add_cost_label_to_space(Sprite2D space, int pos)
	{
		Label cost = new Label();

		Vector2 size = new Vector2(200, 50);

		PropertySpace propertySpace = (PropertySpace) boardData.get_space(pos);
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

	// rolls two d6s and stores each roll in the diceRoll array, calls the dice's roll method to display the animation
	public async void dice_roll()
	{
			diceRoll = new []{GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			var dice1 = GetNode<Dice>("DiceButton/Dice1");
			dice1.roll(diceRoll[0]);
			var dice2 = GetNode<Dice>("DiceButton/Dice2");
			dice2.roll(diceRoll[1]);

			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

			if (diceRoll[0] == diceRoll[1]) 
			{
				doubleRoll = true;
				doubleRollCounter++;
			} else
			{
				doubleRoll = false;
				doubleRollCounter = 0;
			}

			// targetMoveValue combines each dice roll into one integer
			int targetMoveValue = Convert.ToInt16(diceRoll[0] + diceRoll[1]);

			handle_current_player(targetMoveValue, true, true);
	}
	
	// handles the essential logic on the players turn, this includes: double rolls, forwards and backwards movement, and performing the respective action
	public async void handle_current_player(int targetMoveValue, bool canCollect, bool forward)
	{
		Player currentPlayer = players[currentPlayerIndex];
		var collectTextBox = GetNode<RichTextLabel>("CollectDisplay");
		var rentTextBox = GetNode<RichTextLabel>("RentDisplay");

		if (doubleRollCounter >= 3) // if player has rolled a double 3 times, send them to jail
		{
			send_to_jail(currentPlayer);
			doubleRollCounter = 0;
			change_player();
			canPressButton = true;
			return;
		}

		if (forward) // checks to see if player is moving forwards or backwards
		{
			// iterates the current player through the boardSpaces array targetMoveValue times (with a delay)
			for (int i = 0; i < targetMoveValue; i++)
			{
				currentPlayer.player_movement(boardSpaces[(currentPlayer.get_pos() + 1) % 40].Position + GetNode<Node2D>("BoardSpaces").Position);
				
				// if the player moves past go, this method will return true and we will give the player £200 from the bank (only if can collect is true)
				if (currentPlayer.iterate_pos() && canCollect)
				{
					if (bank.take_from_bank(goValue))
					{
						currentPlayer.increase_balance(goValue);
						collectTextBox.Text = currentPlayer.get_name() + " collected £" + goValue;
						clear_text_after_delay(collectTextBox, 2000);
					}
				}

				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			};
		} else // go backwards
		{
			for(int i = 0; i < targetMoveValue; i++){
				players[currentPlayerIndex].iterate_pos_backwards();
				players[currentPlayerIndex].player_movement(boardSpaces[players[currentPlayerIndex].get_pos() % 40].Position + GetNode<Node2D>("BoardSpaces").Position);
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			}
		}

		SpaceType type = boardData.get_space(currentPlayer.get_pos()).land(currentPlayer);

		if (type == SpaceType.PL | type == SpaceType.OK) // if its a pot luck or opportunity knocks, call the play_card method
		{
			play_card(currentPlayer, deck.draw(type), type);
			return;
		} 
		else if (type == SpaceType.GTJ) // go to jail
		{
			send_to_jail(currentPlayer);
			change_player();
			canPressButton = true;
			return;
		} 
		else if (type == SpaceType.FINE) // (SpaceType.FINE should've been called TAX) handles the taxxing logic 
		{
			int tax = 0;

			if (currentPlayer.get_pos() == 4)
			{
				tax = 200;
			} else
			{
				tax = 100;
			}

			// if player can't afford the tax, call the selling method
			if (currentPlayer.decrease_balance(tax))
			{
				bank.add_to_bank(tax);
				rentTextBox.Text = currentPlayer.get_name() + " paid £" + tax + " in taxes";
				clear_text_after_delay(rentTextBox, 5000);
			} else
				{
					await selling(tax, currentPlayer);

					if (!currentPlayer.isBankrupt) {
						bank.add_to_bank(tax);
						rentTextBox.Text = currentPlayer.get_name() + " paid £" + tax + " in taxes";
						clear_text_after_delay(rentTextBox, 5000);
					}
				}
		}

		// if space type is a purchaseable property 
		else if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
		|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE
		|| type == SpaceType.STATION || type == SpaceType.UTIL)
		{
			PropertySpace currentSpace = (PropertySpace) boardData.get_space(currentPlayer.get_pos());
			Property property = currentSpace.get_property();
	
			if (bank.does_bank_contain(property)) // bank owns property
			{
				if(currentPlayer.hasPassedGo)
				{
					purchaseable = true;
				}
			} else if ((type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
			|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE) && currentPlayer != property.get_owner()) // a player owns property
			{
				int rentDue = property.get_rent();

				if (property.get_owner().does_player_have_colour_set(type) && property.get_num_houses() == 0)
				{
					rentDue = rentDue * 2;
				}

				// if player can't afford the property rent, call the selling method
				if (currentPlayer.decrease_balance(rentDue))
				{
					property.get_owner().increase_balance(rentDue);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
				} else
				{
					await selling(rentDue, currentPlayer);

					if (currentPlayer.isBankrupt) {
						property.get_owner().increase_balance(rentDue);
						bank.take_from_bank(rentDue);
						rentTextBox.Text = "The bank paid £" + rentDue + " in rent to " + property.get_owner().get_name() + " because " + currentPlayer.get_name() + " went bankrupt";
						clear_text_after_delay(rentTextBox, 5000);
					} else 
					{
						property.get_owner().increase_balance(rentDue);
						rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
						clear_text_after_delay(rentTextBox, 5000);
					}
				}
			} else if (type == SpaceType.UTIL && currentPlayer != property.get_owner())
			{
				int ownedUtils = 0;
				int rentDue = Convert.ToInt16(diceRoll[0] + diceRoll[1]);

				foreach (Property p in property.get_owner().get_properties())
				{
					if (p.get_type() == SpaceType.UTIL)
					{
						ownedUtils++;
					}
				}

				if (ownedUtils == 2) // works out how much the rent is based of the number of owned utils
				{
					rentDue = rentDue * 10;
				} else if (ownedUtils == 1) {
					rentDue = rentDue * 4;
				}

				// if player can't afford the utility rent, call the selling method
				if (currentPlayer.decrease_balance(rentDue))
				{
					property.get_owner().increase_balance(rentDue);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
				} else
				{
					await selling(rentDue, currentPlayer);

					if (currentPlayer.isBankrupt) {
						property.get_owner().increase_balance(rentDue);
						bank.take_from_bank(rentDue);
						rentTextBox.Text = "The bank paid £" + rentDue + " in rent to " + property.get_owner().get_name() + " because " + currentPlayer.get_name() + " went bankrupt";
						clear_text_after_delay(rentTextBox, 5000);
					} else 
					{
						property.get_owner().increase_balance(rentDue);
						rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
						clear_text_after_delay(rentTextBox, 5000);
					}
				}
				
			} else if (type == SpaceType.STATION && currentPlayer != property.get_owner())
			{
				int ownedStations = 0;

				foreach (Property p in property.get_owner().get_properties())
				{
					if (p.get_type() == SpaceType.STATION)
					{
						ownedStations++;
					}
				}

				int[] rentDue = property.get_rent_array();

				// if player can't afford the station rent, call the selling method
				if (currentPlayer.decrease_balance(rentDue[ownedStations - 1]))
				{
					property.get_owner().increase_balance(rentDue[ownedStations - 1]);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue[ownedStations - 1] + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
				} else
				{
					await selling(rentDue[ownedStations - 1], currentPlayer);

					if (currentPlayer.isBankrupt) {
						property.get_owner().increase_balance(rentDue[ownedStations - 1]);
						bank.take_from_bank(rentDue[ownedStations - 1]);
						rentTextBox.Text = "The bank paid £" + rentDue[ownedStations - 1] + " in rent to " + property.get_owner().get_name() + " because " + currentPlayer.get_name() + " went bankrupt";
						clear_text_after_delay(rentTextBox, 5000);
					} else 
					{
						property.get_owner().increase_balance(rentDue[ownedStations - 1]);
						rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue[ownedStations - 1] + " in rent to " + property.get_owner().get_name();
						clear_text_after_delay(rentTextBox, 5000);
					}
				}
			}
		}

		// asks if the player would like to buy houses if they have a colour set and the space they are on isn't purchasable
		if (currentPlayer.does_player_have_colour_set() && !purchaseable)
		{
			purchase_houses_confirmation();
			return;
		}

		// if the space the player is on isn't purchasable, change the player unless they rolled a double
		if (!purchaseable)
		{
			if (!doubleRoll) 
			{
				change_player();
			}

			// dice button can now be pressed
			canPressButton = true;
		}
	}

	// waits 2 seconds to clear the go collection text box
	private async Task clear_text_after_delay(RichTextLabel textBox, int delay)
	{
		await Task.Delay(delay);
		textBox.Text = "";
	}

	// called when the roll dice button is pressed
	private void _on_dice_button_pressed()
	{
		// If statement will only run if a player is not currently moving
		if (canPressButton) {
			canPressButton = false;
			dice_roll();
		}

	}

	// asks if the player would like to buy any houses if they own any colour sets
	private async void purchase_houses_confirmation()
	{
		GetNode<RichTextLabel>("PurchaseHousesQuery").Show();
		var yesButton = GetNode<Button>("PurchaseHousesQuery/Button");
		var noButton = GetNode<Button>("PurchaseHousesQuery/Button2");

		bool? result = null;

		void on_yes_pressed() {
			result = true;
		}

		void on_no_pressed() {
			result = false;
		}

		yesButton.Pressed += on_yes_pressed;
		noButton.Pressed += on_no_pressed;

		while (result == null)
		{
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
		}

		if (result == true)
		{
			GetNode<RichTextLabel>("PurchaseHousesQuery").Hide();
			purchase_houses();
		} else 
		{
			GetNode<RichTextLabel>("PurchaseHousesQuery").Hide();

			if (!doubleRoll) 
			{
				change_player();
			}

			canPressButton = true;
		}
	}

	// handles the logic of buying houses
	private async void purchase_houses()
	{
		GetNode<Control>("PurchaseHousesMenu").Show();
		var ownedProperties = GetNode<RichTextLabel>("PurchaseHousesMenu/OwnedProperties");
		var inputBox = GetNode<LineEdit>("PurchaseHousesMenu/LineEdit");
		var enterButton = GetNode<Button>("PurchaseHousesMenu/EnterButton");
		var quitButton = GetNode<Button>("PurchaseHousesMenu/QuitButton");

		inputBox.Text = "";

		string validPropertiesText = "You can buy houses for:";
		LinkedList<Property> validProperties = new LinkedList<Property>();
		foreach (Property p in players[currentPlayerIndex].get_properties()) // for each property a player owns, selects only the ones that can have houses bought for them
		{
			if (players[currentPlayerIndex].does_player_have_colour_set(p.get_type()) && p.get_num_houses() < 5)
			{
				validPropertiesText += "\n" + p.get_name();
				validProperties.AddLast(p);
			}
		}

		ownedProperties.Text = validPropertiesText;

		bool? endTurn = null;

		void on_enter_pressed() {
			endTurn = false;
		}

		void on_quit_pressed() {
			endTurn = true;
		}

		enterButton.Pressed += on_enter_pressed;
		quitButton.Pressed += on_quit_pressed;

		while(endTurn == null)
		{
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
		}

		if (endTurn == false)
		{
			bool validInput = false;
			Property selectedProperty = null;

			foreach (Property p in validProperties)
			{
				if (inputBox.Text.ToLower() == p.get_name().ToLower()) // if inputted name is equal to the property name
				{
					validInput = true;
					selectedProperty = p;
				}
			}

			if (validInput)
			{
				GetNode<Control>("PurchaseHousesMenu").Hide();

				var purchaseConfirmationTextBox = GetNode<RichTextLabel>("PurchaseHousesConfirmation");

				string houseOrHotel = "";
				if (selectedProperty.get_num_houses() == 4)
				{
					houseOrHotel = "hotel";
				} else
				{
					houseOrHotel = "house";
				}

				int houseCost = 0;
				SpaceType type = selectedProperty.get_type();
				if (type == SpaceType.BROWN || type == SpaceType.BLUE) // works out the cost of a house / hotel
				{
					houseCost = 50;
				} else if (type == SpaceType.PURPLE || type == SpaceType.ORANGE)
				{
					houseCost = 100;
				} else if (type == SpaceType.RED || type == SpaceType.YELLOW)
				{
					houseCost = 150;
				} else if (type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
				{
					houseCost = 200;
				}

				purchaseConfirmationTextBox.Show();
				purchaseConfirmationTextBox.Text = "Are you sure you want to buy a " + houseOrHotel + " at " + selectedProperty.get_name() + " for £" + houseCost;
				
				var yesButton = GetNode<Button>("PurchaseHousesConfirmation/Button");
				var noButton = GetNode<Button>("PurchaseHousesConfirmation/Button2");

				bool? result = null;

				void on_yes_pressed() {
					result = true;
				}

				void on_no_pressed() {
					result = false;
				}

				yesButton.Pressed += on_yes_pressed;
				noButton.Pressed += on_no_pressed;

				while (result == null)
				{
					await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
				}

				if (result == true)
				{
					if (players[currentPlayerIndex].decrease_balance(houseCost))
					{
						selectedProperty.add_house();
						purchaseConfirmationTextBox.Hide();

						if (selectedProperty.get_num_houses() == 5)
						{
							labelArray[selectedProperty.get_position() - 1].Text = "1 Hotel";
						} else
						{
							labelArray[selectedProperty.get_position() - 1].Text = selectedProperty.get_num_houses() + " House/s";
						}
						
						purchase_houses();
					} else {
						purchaseConfirmationTextBox.Hide();
						purchase_houses();
					}
				} else // if player doesn't want to buy that house, go back
				{
					purchaseConfirmationTextBox.Hide();
					purchase_houses();
				}

			} else // if input is invalid, go back
			{
				purchase_houses();
			}
		} else // end turn
		{
			GetNode<Control>("PurchaseHousesMenu").Hide();

			if (!doubleRoll) 
			{
				change_player();
			}

			canPressButton = true;
		}
	}

	// handles the logic of selling properties, takes as parameters the player selling and the debt they owe
	private async Task selling(int debt, Player player)
	{
		GetNode<Control>("SellingMenu").Show();
		var sellableProperties = GetNode<RichTextLabel>("SellingMenu/SellableProperties");
		var inputBox = GetNode<LineEdit>("SellingMenu/LineEdit");
		var enterButton = GetNode<Button>("SellingMenu/EnterButton");
		var stopButton = GetNode<Button>("SellingMenu/StopButton");
		var bankruptButton = GetNode<Button>("SellingMenu/BankruptButton");

		stopButton.Hide();

		if (player.get_balance() > debt)
		{
			stopButton.Show();
		}

		inputBox.Text = "";

		string validPropertiesText = "Insufficient funds " + player.get_name() + "! You can sell: \n";
		LinkedList<Property> validProperties = new LinkedList<Property>();
		foreach (Property p in player.get_properties())
		{
			validPropertiesText += "\n" + p.get_name();
			validProperties.AddLast(p);
		}

		sellableProperties.Text = validPropertiesText;

		string? buttonPressed = null;

		void on_enter_pressed() {
			buttonPressed = "enter";
		}

		void on_stop_pressed() {
			buttonPressed = "stop";
		}

		void on_bankrupt_pressed() {
			buttonPressed = "bankrupt";
		}

		enterButton.Pressed += on_enter_pressed;
		stopButton.Pressed += on_stop_pressed;
		bankruptButton.Pressed += on_bankrupt_pressed;

		while (buttonPressed == null)
		{
			await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
		}

		if (buttonPressed == "enter")
		{
			bool validInput = false;
			Property selectedProperty = null;

			foreach (Property p in validProperties)
			{
				if (inputBox.Text.ToLower() == p.get_name().ToLower())
				{
					validInput = true;
					selectedProperty = p;
				}
			}

			if (validInput) // if input is valid
			{
				GetNode<Control>("SellingMenu").Hide();

				var sellingConfirmationTextBox = GetNode<RichTextLabel>("SellingConfirmation");

				int sellPrice = 0;
				bool sellingProperty = false;

				if (selectedProperty.get_num_houses() > 0)
				{
					string houseOrHotel = "";
					if (selectedProperty.get_num_houses() == 4)
					{
						houseOrHotel = "hotel";
					} else
					{
						houseOrHotel = "house";
					}

					int houseCost = 0;
					SpaceType type = selectedProperty.get_type();
					if (type == SpaceType.BROWN || type == SpaceType.BLUE) // works out the cost of a house / hotel
					{
						houseCost = 50;
					} else if (type == SpaceType.PURPLE || type == SpaceType.ORANGE)
					{
						houseCost = 100;
					} else if (type == SpaceType.RED || type == SpaceType.YELLOW)
					{
						houseCost = 150;
					} else if (type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
					{
						houseCost = 200;
					}

					sellingConfirmationTextBox.Text = "Are you sure you want to sell a " + houseOrHotel + " at " + selectedProperty.get_name() + " for £" + houseCost;

					sellPrice = houseCost;
					sellingProperty = false;
				} else
				{
					sellingConfirmationTextBox.Text = "Are you sure you want to sell " + selectedProperty.get_name() + " for £" + selectedProperty.get_cost();
					sellPrice = selectedProperty.get_cost();
					sellingProperty = true;
				}

				sellingConfirmationTextBox.Show();

				var yesButton = GetNode<Button>("SellingConfirmation/Button");
				var noButton = GetNode<Button>("SellingConfirmation/Button2");

				bool? result = null;

				void on_yes_pressed() {
					result = true;
				}

				void on_no_pressed() {
					result = false;
				}

				yesButton.Pressed += on_yes_pressed;
				noButton.Pressed += on_no_pressed;

				while (result == null)
				{
					await ToSignal(GetTree().CreateTimer(0.01f), "timeout");
				}

				if (result == true) // if they confirm the sell
				{
					if (sellingProperty) // if they are selling a property, remove it from the player and give it to the bank
					{
						if (bank.take_from_bank(sellPrice))
						{
							player.remove_from_properties(selectedProperty);
							bank.add_to_properties(selectedProperty);
							player.increase_balance(sellPrice);

							sellingConfirmationTextBox.Hide();

							await selling(debt, player);
						} else
						{
							sellingConfirmationTextBox.Hide();
							bankrupt(player, validProperties);
						}
					} else // if they are selling a house, remove a house from the property
					{
						if (bank.take_from_bank(sellPrice))
						{
							selectedProperty.remove_house();
							player.increase_balance(sellPrice);

							sellingConfirmationTextBox.Hide();

							await selling(debt, player);
						} else
						{
							sellingConfirmationTextBox.Hide();
							bankrupt(player, validProperties);
						}
					}
				} else // go back
				{
					sellingConfirmationTextBox.Hide();

					await selling(debt, player);
				}
			} else // input invalid, go back
			{
				await selling(debt, player);
			}
		} else if (buttonPressed == "stop") // stops selling
		{
			GetNode<Control>("SellingMenu").Hide();
			return;
		} else if (buttonPressed == "bankrupt") // player goes bankrupt, call method
		{
			GetNode<Control>("SellingMenu").Hide();
			bankrupt(player, validProperties);
			return;
		}
	}

	// handles player bankrupcy logic
	private void bankrupt(Player player, LinkedList<Property> properties)
	{
		foreach (Property p in properties)
		{
			while(p.get_num_houses() > 0) // while a player's property has houses, remove them and add that amount to the bank
			{
				int houseCost = 0;
				SpaceType type = p.get_type();
				if (type == SpaceType.BROWN || type == SpaceType.BLUE)
				{
					houseCost = 50;
				} else if (type == SpaceType.PURPLE || type == SpaceType.ORANGE)
				{
					houseCost = 100;
				} else if (type == SpaceType.RED || type == SpaceType.YELLOW)
				{
					houseCost = 150;
				} else if (type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
				{
					houseCost = 200;
				}

				p.remove_house();
				bank.add_to_bank(houseCost);
			}

			p.set_owner(null);
			bank.add_to_properties(p);

			player.isBankrupt = true; // player is now bankrupt
			player.Hide();
 		}
	}

	// when the purchase button is pressed, handle player purchasing logic
	private async void _on_purchase_button_pressed()
	{
		Player currentPlayer = players[currentPlayerIndex];
		PropertySpace currentSpace = (PropertySpace) boardData.get_space(currentPlayer.get_pos());
		Property property = currentSpace.get_property();
		SpaceType type = property.get_type();

		if (bank.remove_from_properties(property)) // returns true if property has been removed from bank
		{
			// returns true if they can afford it
			if (currentPlayer.decrease_balance(property.get_cost()))
			{
				property.set_owner(currentPlayer);
				currentPlayer.add_to_properties(property);
				bank.add_to_bank(property.get_cost());

				if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
				|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
				{
					update_owned_colour_sets(currentPlayer, type);
				}
				
				var purchaseTextBox = GetNode<RichTextLabel>("PurchaseDisplay");
				purchaseLogString += "\n" + currentPlayer.get_name() + " has purchased " + property.get_name() + " for £" + property.get_cost();
				purchaseTextBox.Text = purchaseLogString;
			} else // if they can't afford property call selling()
			 {
				purchaseable = false;

				await selling(property.get_cost(), currentPlayer);

				if (currentPlayer.isBankrupt)
				{
					bank.add_to_properties(property);
				} else
				{
					currentPlayer.decrease_balance(property.get_cost());
					property.set_owner(currentPlayer);
					currentPlayer.add_to_properties(property);
					bank.add_to_bank(property.get_cost());

					if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
					|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
					{
						update_owned_colour_sets(currentPlayer, type);
					}
					
					var purchaseTextBox = GetNode<RichTextLabel>("PurchaseDisplay");
					purchaseLogString += "\n" + currentPlayer.get_name() + " has purchased " + property.get_name() + " for £" + property.get_cost();
					purchaseTextBox.Text = purchaseLogString;
				}
			}
		}

		// asks if player wants to buy a house
		if (currentPlayer.does_player_have_colour_set())
		{
			purchase_houses_confirmation();
			purchaseable = false;
			return;
		}

		if (!doubleRoll) 
		{
			change_player();
		} 

		purchaseable = false;
		canPressButton = true;
	}

	// if auction button is pressed, handle auctioning logic
	private async void _on_auction_button_pressed() 
	{
		Player currentPlayer = players[currentPlayerIndex];
		PropertySpace currentSpace = (PropertySpace) boardData.get_space(currentPlayer.get_pos());
		Property property = currentSpace.get_property();
		SpaceType type = property.get_type();
		var auctionTextBox = GetNode<RichTextLabel>("AuctionDisplay");
		var inputBox = GetNode<LineEdit>("AuctionDisplay/LineEdit");
		var doneButton = GetNode<Button>("AuctionDisplay/DoneButton");
		int[] playerAuctions = new int[numOfPlayers];
	
		
		auctionTextBox.Text = "";
		auctionTextBox.Show();

		purchaseable = false;

		for (int i = 0; i < numOfPlayers; i++) // asks each player for their auction price
		{
			if (!jail.is_in_jail(players[i]) || !players[i].hasPassedGo)
			{
				int userInput;
				bool validInput = false;

				while (!validInput)
				{
					inputBox.Text = "";
					auctionTextBox.Text = players[i].get_name() + ": Please type your desired auction \n price:";

					await ToSignal(doneButton, "pressed");

					if (int.TryParse(inputBox.Text, out userInput))
					{
						if (!(userInput > players[i].get_balance()) && userInput >= 0)
						{
							validInput = true;
							playerAuctions[i] = userInput;
						}
					}
				}
			}
		}

		auctionTextBox.Hide();

		int highestBid = -1;
		int highestBidderIndex = -1;

		for (int i = 0; i < playerAuctions.Length; i++) // finds the highest bidder
		{
			if (playerAuctions[i] > highestBid)
			{
				highestBid = playerAuctions[i];
				highestBidderIndex = i;
			}
		}

		if (bank.remove_from_properties(property)) // returns true if property has been removed from bank
		{
			if (players[highestBidderIndex].decrease_balance(highestBid))
			{
				property.set_owner(players[highestBidderIndex]);
				players[highestBidderIndex].add_to_properties(property);
				bank.add_to_bank(highestBid);

				if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
				|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
				{
					update_owned_colour_sets(players[highestBidderIndex], type);
				}
				
				var purchaseTextBox = GetNode<RichTextLabel>("PurchaseDisplay");
				purchaseLogString += "\n" + players[highestBidderIndex].get_name() + " has purchased " + property.get_name() + " for £" + highestBid;
				purchaseTextBox.Text = purchaseLogString;
			} else {
				bank.add_to_properties(property); // adds the property back if the player doesn't have enough to buy property
				return;
			}
		}

		if (currentPlayer.does_player_have_colour_set())
		{
			purchase_houses_confirmation();
			return;
		}

		if (!doubleRoll) 
		{
			change_player();
		} 

		canPressButton = true;
	}

	// checks if the player owns any colour sets and upadtes the respective player's colour set booleans
	private void update_owned_colour_sets(Player player, SpaceType type)
	{
		LinkedList<Property> playerOwnedProperties = player.get_properties();

		int ownedCount = 0;

		foreach (Property p in playerOwnedProperties)
		{
			if (p.get_type() == type)
			{
				ownedCount++;
			}
		}

		int requiredCount = colourSetSizes[type];

		if (ownedCount == requiredCount) // uses the colourSetSizes dictionary to determine if the player has a colour set 
		{
			switch(type) 
			{
				case SpaceType.BROWN:
					player.hasBrownSet = true;
					break;
				case SpaceType.BLUE:
					player.hasBlueSet = true;
					break;
				case SpaceType.PURPLE:
					player.hasPurpleSet = true;
					break;
				case SpaceType.ORANGE:
					player.hasOrangeSet = true;
					break;
				case SpaceType.RED:
					player.hasRedSet = true;
					break;
				case SpaceType.YELLOW:
					player.hasYellowSet = true;
					break;
				case SpaceType.GREEN:
					player.hasGreenSet = true;
					break;
				case SpaceType.DEEPBLUE:
					player.hasDeepBlueSet = true;
					break;
			}
		}
	}

	// displays the card on screen
	private void play_card(Player player, Card card, SpaceType spaceType)
	{
		// updates global variable currentCard to the card drawn
		currentCard = card;
		CardType cardType = card.get_cardType();
		string cardDescription = card.get_description();

		// shows the correct sprite depending on the type of card
		if (spaceType == SpaceType.OK)
		{
			GetNode<Sprite2D>("Card/PotLuck").Hide();
			GetNode<Sprite2D>("Card/OpportunityKnocks").Show();
		} else
		{
			GetNode<Sprite2D>("Card/OpportunityKnocks").Hide();
			GetNode<Sprite2D>("Card/PotLuck").Show();
		}

		GetNode<RichTextLabel>("Card/Description").Text = cardDescription;

		// show the card node
		GetNode<Node2D>("Card").Show();
		canPressButton = false;

		// shows the accept card button unless FINEOROK cardType passed
		var button = GetNode<Button>("Card/AcceptCard");

		if (cardType != CardType.FINEOROK) 
		{
			button.Show();
		} else
		{
			button.Hide();
			GetNode<HBoxContainer>("Card/FineOrOpportunity").Show();
		}
		
	}

	// handles card operation based on cardType and cardParam
	public void handle_card(int cardParam, CardType cardType, Player player)
	{
		switch (cardType)
		{
			case CardType.GTJ:
				send_to_jail(player);
				change_player();
				break;
			case CardType.FINE:
				player.decrease_balance(cardParam);
				freeParking.collect_fine(cardParam);
				break;
			case CardType.GOOJF:
				player.getOutJail = true;
				break;
			case CardType.PAYALL:
				for (int i = 0; i < players.Length; i++)
				{
					if (i != currentPlayerIndex)
					{
						player.decrease_balance(cardParam);
						players[i].increase_balance(cardParam);
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

			case CardType.COLLECTALL:
				for (int i = 0; i < players.Length; i++) // collects the required ammount of all players
				{
					if (i != currentPlayerIndex)
					{
						player.increase_balance(cardParam);
						players[i].decrease_balance(cardParam);
					}
				}
				break;
			case CardType.PAYREPAIRS:
				LinkedList<Property> properties = player.get_properties();
				int totalCost = 0;
				foreach (Property property in properties)
				{
					if (property.get_num_houses() < 5) // owrks out the required amount in repairs
					{
						totalCost += cardParam * property.get_num_houses();
					} else if (property.get_num_houses() == 5)
					{
						if (cardParam == 25) {
							totalCost += 100;
						} 
						if(cardParam == 40) {
							totalCost += 115;
						}
					}
				}

				player.decrease_balance(totalCost);
				break;
			case CardType.MOVESPACESB:
				GetNode<Node2D>("Card").Hide();
				handle_current_player(cardParam, false, false);
				return;
			case CardType.MOVELOCATIONB:
				if (cardParam < player.get_pos()) // moves player backwards
				{
					GetNode<Node2D>("Card").Hide();
					handle_current_player(player.get_pos() - cardParam, false, false);
					return;
				}
				else
				{
					GetNode<Node2D>("Card").Hide();
					handle_current_player(40 - cardParam + player.get_pos(), false, false);
					return;
				}
			case CardType.MOVELOCATIONF:
				if(cardParam > player.get_pos()) // moves player forwards
				{
					GetNode<Node2D>("Card").Hide();
					handle_current_player(cardParam - player.get_pos(), true, true);
					return;
				}
				else
				{
					GetNode<Node2D>("Card").Hide();
					handle_current_player(40 - player.get_pos() + cardParam, true, true);
					return;
				}
			default:
				Console.WriteLine("Unknown card type");
				break;
		}

		GetNode<Node2D>("Card").Hide();

		if (players[currentPlayerIndex].does_player_have_colour_set())
		{
			purchase_houses_confirmation();
			return;
		}

		if (!doubleRoll) 
		{
			change_player();
		}

		canPressButton = true;
	}

	// sends player to jail
	public void send_to_jail(Player player)
	{
		// sets players position to the index of Jail and then updates the Jail class
		players[currentPlayerIndex].set_pos(10);
		players[currentPlayerIndex].player_movement(boardSpaces[players[currentPlayerIndex].get_pos() % 40].Position + GetNode<Node2D>("BoardSpaces").Position + new Vector2(20, -20));
		jail.send_to_jail(player);
	}
	
	// adds labels to all properties to display their houses
	public void show_houses()
	{
		for(int i = 0; i < 40; i++)
		{
			SpaceType type = boardData.get_space(i).get_type();

			if(type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
			|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
			{
				PropertySpace propertySpace = (PropertySpace)boardData.get_space(i);
				Property property = propertySpace.get_property();

				int numHouses = property.get_num_houses();

				Label housesLabel = new Label();

				Vector2 size = new Vector2(200, 50);

				housesLabel.Text = numHouses.ToString() + " House/s";
				housesLabel.Scale = new Vector2(4, 4);
				housesLabel.Size = size;
				housesLabel.Position = -(size * housesLabel.Scale) / 2 + new Vector2(0, 525);
				housesLabel.HorizontalAlignment = HorizontalAlignment.Center;
				housesLabel.VerticalAlignment = VerticalAlignment.Center;

				labelArray[i] = housesLabel;

				boardSpaces[i].AddChild(housesLabel);
			}
		}
	}

	public void _on_draw_opportunity_card_pressed()
	{
		// called when the DrawOpportunityCard button is pressed
		// draws an opportunity knocks card when the potluck card with type FINEOROK is called
		play_card(players[currentPlayerIndex], deck.draw(SpaceType.OK), SpaceType.OK);
		GetNode<HBoxContainer>("Card/FineOrOpportunity").Hide();
	}
	public void _on_accept_card_pressed()
	{
		// calls handle_card with the apropriate parameters once AcceptCard is pressed
		CardType cardType = currentCard.get_cardType();
		int cardParam = currentCard.get_cardParameter();
		Player currentPlayer = players[currentPlayerIndex];
		handle_card(cardParam, cardType, currentPlayer);
	}

	public void _on_take_fine_pressed()
	{
		// called when TakeFine button is pressed
		// takes £10 when the potluck card with type FINEOROK is called
		freeParking.collect_fine(10);
		players[currentPlayerIndex].decrease_balance(10);
		GetNode<HBoxContainer>("Card/FineOrOpportunity").Hide();
		GetNode<Node2D>("Card").Hide();
	}

	// checks to see if a player has won the game
	private void check_for_winners()
	{
		int bankruptPlayers = 0;

		foreach (Player p in players)
		{
			if (p.isBankrupt)
			{
				bankruptPlayers++;
			}
		}

		if (bankruptPlayers == players.Length - 1)
		{
			Player winner = null;

			foreach (Player p in players)
			{
				if (!p.isBankrupt)
				{
					winner = p;
				}
			}

			var winScreen = GetNode<Control>("WinScreen");
			var winText = GetNode<RichTextLabel>("WinScreen/WinText");

			winScreen.Show();
			winText.Text = winner.get_name().ToUpper() + " WINS!";
		}
	}

	// quits to main menu
	private void _on_quit_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://mainMenu.tscn");
	}

	// runs every frame
	public override void _Process(double delta) 
	{
		//constantly displays the player balances, board info and player property lists, once displayInfo is set to true
		if(displayInfo)
		{
		display_player_balances();
		display_board_info();
		display_player_properties();
		}

		//checks if the property the current player is on is purchasable at all times, if so displays the purchasing options
		if (!purchaseable)
		{
			GetNode<Button>("PurchaseButton").Hide();
			GetNode<Button>("AuctionButton").Hide();
		} else
		{
			GetNode<Button>("PurchaseButton").Show();
			GetNode<Button>("AuctionButton").Show();
		}

		// checks for winners every frame
		check_for_winners();
	}
}
}

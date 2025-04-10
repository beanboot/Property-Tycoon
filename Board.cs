using System.Collections.Generic;

namespace PropTycoon {

using Godot;
using System;
using System.Threading.Tasks;


public partial class Board : Node2D
{
	//Global Variables
	private uint[] diceRoll;
	private Sprite2D[] boardSpaces;
	private Player[] players;
	private bool canPressButton = true;
	private int numOfPlayers = 6;
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		deck = new Deck();
		boardData = new BoardData();
		bank = new Bank(boardData.get_property_list(), 50000);

		initialise_board();
		
		initialise_players();
		
		currentPlayerIndex = 0;
		
		display_current_player_text();
		
	}

	public void display_board_info() 
	{
		var debugTextBox = GetNode<RichTextLabel>("CurrentBoardInfo");
		string displayBoardInfo = "";
		for (int i = 0; i < numOfPlayers; i++)
		{
			int playerPos = players[i].get_pos();
			displayBoardInfo += "Player " + (i+1) + " is on " + boardData.get_space(playerPos).get_name() + "\n";
			
		}
		debugTextBox.Text = displayBoardInfo;
	}

	// Iterates the currentPlayer variable by 1 unless it excedes the number of players
	public void change_player()
	{
		if (currentPlayerIndex >= (numOfPlayers - 1)) {
			currentPlayerIndex = 0;
		}

		else {
			currentPlayerIndex += 1;
		}
		Player currentPlayer = players[currentPlayerIndex];
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
		playerTextBox.Text = "It is currently: " + players[currentPlayerIndex].Name + "'s turn";
		
	}

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

		public void display_player_properties()
	{
		var propertiesTextBox = GetNode<RichTextLabel>("OwnedPropertiesDisplay");
		string propertiesText = "";
		
		for (int i = 0; i < numOfPlayers; i++)
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

	// Called within _Ready(), fills the array players[] by instantiating the player scene depending on numOfPlayers
	public void initialise_players() 
	{
		int i;
		var playerScene = GD.Load<PackedScene>("res://player.tscn");
		players = new Player[numOfPlayers];
		Texture2D[] playerSprites = new Texture2D[6];

		playerSprites[0] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.boot.png");
		playerSprites[1] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.cat.png");
		playerSprites[2] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.ship.png");
		playerSprites[3] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.iron.png");
		playerSprites[4] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.mobilephone.png");
		playerSprites[5] = (Texture2D)GD.Load("res://BoardSprites/PlayerSprites/pt.hatstand.png");

		for (i = 0; i < numOfPlayers; i++)
		{
			var playerInstance = playerScene.Instantiate();
			playerInstance.Name = "Player" + (i+1);
			AddChild(playerInstance);

			players[i] = GetNode<Player>("Player" + (i+1));
			var playerSprite = GetNode<Sprite2D>("Player" + (i+1) + "/Sprite2D");
			playerSprite.Texture = playerSprites[i];

			players[i].set_name("Player"+(i+1));
			players[i].player_movement(boardSpaces[0].Position + GetNode<Node2D>("BoardSpaces").Position);
		};
	}

	// Called within _Ready(), fills the array boardSpaces[] with every space on the board
	public void initialise_board() 
	{
		int i;
		boardSpaces = new Sprite2D[40];
		for (i = 0; i < 40; i++) {
			boardSpaces[i] = GetNode<Sprite2D>("BoardSpaces/BoardSpace" + (i+1));
			//determines the type of board space and loads the respective texture
			SpaceType type = boardData.get_space(i).get_type();
			Console.WriteLine(boardData.get_space(i));
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

	private void add_name_label_to_space(Sprite2D space, int pos, int offset)
	{
		//creates a label, adds the name and then sets the position
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
	
	private void add_cost_label_to_space(Sprite2D space, int pos)
	{
		//creates a label, adds the cost and then sets the position
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

	// Rolls two d6s and stores each roll in the diceRoll array, displays results via 2 AnimatedSprite2Ds
	public async void dice_roll()
	{
			diceRoll = new []{GD.Randi() % 6 + 1, GD.Randi() % 6 + 1};
			var dice1 = GetNode<Dice>("Button/Dice1");
			dice1.roll(diceRoll[0]);
			var dice2 = GetNode<Dice>("Button/Dice2");
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
	
	// Moves the player equal times to the targetMoveValue parameter, canCollect is used to determine whether the player collects £200 when passing go
	public async void handle_current_player(int targetMoveValue, bool canCollect, bool forward)
	{
		Player currentPlayer = players[currentPlayerIndex];
		var collectTextBox = GetNode<RichTextLabel>("CollectDisplay");
		var rentTextBox = GetNode<RichTextLabel>("RentDisplay");

		if (doubleRollCounter >= 3)
		{
			send_to_jail(currentPlayer);
			doubleRollCounter = 0;
			change_player();
			canPressButton = true;
			return;
		}

		if(forward)
		{
			// Iterates the current player through the boardSpaces array targetMoveValue times (with a delay)
			for (int i = 0; i < targetMoveValue; i++) {
			currentPlayer.player_movement(boardSpaces[(currentPlayer.get_pos() + 1) % 40].Position + GetNode<Node2D>("BoardSpaces").Position);
			// if the player moves past go, this method will return true and we will give the player £200 from the bank
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
		} else
		{
			for(int i = 0; i < targetMoveValue; i++){
				players[currentPlayerIndex].iterate_pos_backwards();
				players[currentPlayerIndex].player_movement(boardSpaces[players[currentPlayerIndex].get_pos() % 40].Position + GetNode<Node2D>("BoardSpaces").Position);
				await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
			}
		}

		SpaceType type = boardData.get_space(currentPlayer.get_pos()).land(currentPlayer);

		if (type == SpaceType.PL | type == SpaceType.OK)
		{
			play_card(currentPlayer, deck.draw(type), type);
		} 
		else if (type == SpaceType.GTJ)
		{
			send_to_jail(currentPlayer);
			change_player();
			canPressButton = true;
			return;
		} 
		else if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
		|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE
		|| type == SpaceType.STATION || type == SpaceType.UTIL)
		{
			PropertySpace currentSpace = (PropertySpace) boardData.get_space(currentPlayer.get_pos());
			Property property = currentSpace.get_property();
	
			if (bank.does_bank_contain(property)) // bank owns property
			{
				if(currentPlayer.hasPassedGo){
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

				if (currentPlayer.decrease_balance(rentDue))
				{
					property.get_owner().increase_balance(rentDue);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
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

				if (ownedUtils == 2)
				{
					rentDue = rentDue * 10;
				} else if (ownedUtils == 1) {
					rentDue = rentDue * 4;
				}

				if (currentPlayer.decrease_balance(rentDue))
				{
					property.get_owner().increase_balance(rentDue);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
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

				if (currentPlayer.decrease_balance(rentDue[ownedStations - 1]))
				{
					property.get_owner().increase_balance(rentDue[ownedStations - 1]);
					rentTextBox.Text = currentPlayer.get_name() + " paid £" + rentDue[ownedStations - 1] + " in rent to " + property.get_owner().get_name();
					clear_text_after_delay(rentTextBox, 5000);
				}
			}
		}

		if (!purchaseable)
		{
			if (!doubleRoll) 
			{
				change_player();
			} 

			canPressButton = true;
		}
	}

	// waits 2 seconds to clear the go collection text box
	private async Task clear_text_after_delay(RichTextLabel textBox, int delay)
	{
		await Task.Delay(delay);
		textBox.Text = "";
	}

	// Called when the Roll Dice button is pressed
	private void _on_button_pressed()
	{
		// If statement will only run if a player is not currently moving
		if (canPressButton) {
			canPressButton = false;
			dice_roll();
		}

	}

	private void _on_button_2_pressed()
	{
		send_to_jail(players[currentPlayerIndex]);
		change_player();
	}

	private void _on_purchase_button_pressed()
	{
		Player currentPlayer = players[currentPlayerIndex];
		PropertySpace currentSpace = (PropertySpace) boardData.get_space(currentPlayer.get_pos());
		Property property = currentSpace.get_property();
		SpaceType type = property.get_type();

		if (bank.remove_from_properties(property, property.get_cost())) // returns true if property has been removed from bank
		{
			if (currentPlayer.decrease_balance(property.get_cost()))
			{
				property.set_owner(currentPlayer);
				currentPlayer.add_to_properties(property);

				if (type == SpaceType.BROWN || type == SpaceType.BLUE || type == SpaceType.PURPLE || type == SpaceType.ORANGE
				|| type == SpaceType.RED || type == SpaceType.YELLOW || type == SpaceType.GREEN || type == SpaceType.DEEPBLUE)
				{
					update_owned_colour_sets(currentPlayer, type);
				}
				
				var purchaseTextBox = GetNode<RichTextLabel>("PurchaseDisplay");
				purchaseLogString += "\n" + currentPlayer.get_name() + " has purchased " + property.get_name() + " for £" + property.get_cost();
				purchaseTextBox.Text = purchaseLogString;
			} else {
				bank.add_to_properties(property); // adds the property back if the player doesn't have enough to buy property
				return;
			}
		}

		if (!doubleRoll) 
		{
			change_player();
		} 

		purchaseable = false;
		canPressButton = true;
	}

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

		for (int i = 0; i < numOfPlayers; i++)
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

		auctionTextBox.Hide();

		int highestBid = -1;
		int highestBidderIndex = -1;

		for (int i = 0; i < playerAuctions.Length; i++)
		{
			if (playerAuctions[i] > highestBid)
			{
				highestBid = playerAuctions[i];
				highestBidderIndex = i;
			}
		}

		if (bank.remove_from_properties(property, highestBid)) // returns true if property has been removed from bank
		{
			if (players[highestBidderIndex].decrease_balance(highestBid))
			{
				property.set_owner(players[highestBidderIndex]);
				players[highestBidderIndex].add_to_properties(property);

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

		if (!doubleRoll) 
		{
			change_player();
		} 

		canPressButton = true;
	}

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

		if (ownedCount == requiredCount)
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

	private void play_card(Player player, Card card, SpaceType spaceType)
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
			case CardType.FINEOROK:
				canPressButton = false;
				GetNode<HBoxContainer>("FineOrOpportunity").Show();
				break;
			case CardType.COLLECTALL:
				for (int i = 0; i < players.Length; i++)
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
					if (property.get_num_houses() < 5)
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
				handle_current_player(cardParam, false, false);
				break;
			case CardType.MOVELOCATIONB:
				if(cardParam < player.get_pos())
				{
					handle_current_player(player.get_pos() - cardParam, false, false);
				}
				else
				{
					handle_current_player(40 - cardParam + player.get_pos(), false, false);
				}
				break;
			case CardType.MOVELOCATIONF:
				if(cardParam > player.get_pos())
				{
					handle_current_player(cardParam - player.get_pos(), true, true);
				}
				else
				{
					handle_current_player(40 - player.get_pos() + cardParam, true, true);
				}
				break;
			default:
				Console.WriteLine("Unknown card type");
				break;
			
		}
	}

	public void send_to_jail(Player player)
	{
		players[currentPlayerIndex].set_pos(10);
		players[currentPlayerIndex].player_movement(boardSpaces[players[currentPlayerIndex].get_pos() % 40].Position + GetNode<Node2D>("BoardSpaces").Position + new Vector2(20, -20));
		jail.send_to_jail(player);
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

	public void _on_draw_card_button_debug_pressed() {
		play_card(players[currentPlayerIndex], deck.draw(SpaceType.PL), SpaceType.PL);
	}
	public void _on_draw_opportunity_card_pressed() {
		play_card(players[currentPlayerIndex], deck.draw(SpaceType.OK), SpaceType.OK);
		GetNode<HBoxContainer>("FineOrOpportunity").Hide();
		canPressButton = true;
	}
	public void _on_take_fine_pressed() {
		freeParking.collect_fine(10);
		players[currentPlayerIndex].decrease_balance(10);
		GetNode<HBoxContainer>("FineOrOpportunity").Hide();
		canPressButton = true;
	}

	public void _on_move_player_spaces_debug_pressed() {
		var inputBox = GetNode<LineEdit>("MovePlayerSpaces(DEBUG)/LineEdit");
		int spaces = int.Parse(inputBox.Text);
		handle_current_player(spaces, true, true);
	}
}
}

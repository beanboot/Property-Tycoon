using Godot;
using System;

public partial class MainMenu : Node
{
	private int numPlayers = 2;
	private int numBots = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
			
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Label>("Players/Label").Text = "Number of players: " + numPlayers.ToString();
		GetNode<Label>("Bots/Label").Text = "Number of bots " + numBots.ToString();
	}

	public void _on_start_button_pressed()
	{
		GameData gameData = (GameData) GetNode("/root/GameData");
		gameData.numPlayers = numPlayers;
		gameData.numBots = numBots;
		GetTree().ChangeSceneToFile("res://board.tscn");
	}

	public void _on_subtract_player_pressed(){
		if(numPlayers > 0)
		{
			numPlayers--;
		}
	}

	public void _on_add_player_pressed(){
		if(numBots + numPlayers < 6){
			numPlayers++;
		}
	}

	public void _on_subtract_bots_pressed(){
		if(numBots > 0)
		{
			numBots--;
		}
	}

	public void _on_add_bots_pressed(){
		if(numBots + numPlayers < 6){
			numBots++;
		}
	}
}

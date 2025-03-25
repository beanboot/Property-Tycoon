using System;
using Godot;
public class Card{
    private string description;
    private CardType cardType;
    private string cardParameter;
    public Card(string description, CardType cardType, string cardParameter){
        
    }
    public void play(){
        Console.WriteLine(cardType.ToString(), cardParameter);
    }
}
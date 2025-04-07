using System;
using Godot;
public class Card{
    private string description;
    private CardType cardType;
    private int cardParameter;
    public Card(string description, CardType cardType, int cardParameter){
        this.description = description;
        this.cardType = cardType;
        this.cardParameter = cardParameter;
    }

    public string get_description() { return description; }

    public CardType get_cardType() { return cardType; }

    public int get_cardParameter() { return cardParameter; }
}
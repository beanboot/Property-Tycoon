using Godot;
using System.Collections.Generic;
using System;


public class Deck{
	private Queue<Card> plDeck;
	private Queue<Card> okDeck;
	private Card[] plCards;
	private Card[] okCards;

	public Deck(){
		plCards = new Card[14];
		okCards = new Card[19];
		plDeck = new Queue<Card>();
		okDeck = new Queue<Card>();
			using (var cardData = FileAccess.Open("res://data/CardData.csv", FileAccess.ModeFlags.Read))
		{
			bool firstLine = true;
			int plIndex = 0;
			int okIndex = 0;
			while (cardData.GetPosition() < cardData.GetLength() - 1)
			{
				string line = cardData.GetLine();
				//first line is ignored since it's just column names
				if (firstLine)
				{
					firstLine = false;
					continue;
				}
				string[] values = line.Split(",");
				string description = values[0];
				CardType cardActionType = Enum.Parse<CardType>(values[1]);
				string cardActionParameter = values[2];
				SpaceType cardType = Enum.Parse<SpaceType>(values[3]);

				switch(cardType){
					case(SpaceType.PL):
						plCards[plIndex] = new Card(description, cardActionType, cardActionParameter);
						plIndex++;
					break;
					case(SpaceType.OK):
						okCards[okIndex] = new Card(description, cardActionType, cardActionParameter);
						okIndex++;
					break;
				}
			
			}
		}
	}
		public Card draw(SpaceType type){
		Queue<Card> deck;
		Card[] cards;
		switch(type){
			case(SpaceType.PL):
			deck = plDeck;
			cards = plCards;
			break;
			default:
			deck = okDeck;
			cards= okCards;
			break;
		}
		if(deck.Count == 0){
			shuffle(cards);
				foreach(Card card in cards){
					deck.Enqueue(card);
				}
				return deck.Dequeue();
		}
		return deck.Dequeue();
		
	}
	private Card[] shuffle(Card[] cards){
		Random rand = new Random();
		for(int i = cards.Length - 1; i > 0; i--){
			int swap = rand.Next(0, (i + 1));
			Card temp = cards[i];
			cards[i] = cards[swap];
			cards[swap] = temp;
		}
		return cards;
	}


}

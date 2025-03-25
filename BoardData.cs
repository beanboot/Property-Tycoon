namespace PropTycoon;
using Godot;
using System;

public class BoardData
{
	//_boardSpaceData an array that will hold the Board class' information
	private Space[] _boardSpaceData;

	public BoardData()
	{
		//instantiates _boardSpaceData array for use in the Board class
		_boardSpaceData = new Space[40];
		//uses the BoardData.csv file 
		using (var boardData = FileAccess.Open("res://data/BoardData.csv", FileAccess.ModeFlags.Read))
		{
			bool firstLine = true;
			int i = 0;
			int j;
			while (boardData.GetPosition() < boardData.GetLength() - 1)
			{
				string line = boardData.GetLine();
				//first line is ignored since it's just column names
				if (firstLine)
				{
					firstLine = false;
					continue;
				}
				
				//csv is split by the commas and saved into an array of strings
				string[] values = line.Split(",");
				string canPurchase = values[4];
				SpaceType spaceType = Enum.Parse<SpaceType>(values[2]);
				//switch case instantiates the correct type of Space child class depending on if it can be purchased
				switch (canPurchase)
				{
					case "Yes":
						int[] rent = new int[6];
						for (j = 0; j < 6; j++)
						{
							if (values[j + 6].Equals(""))
							{
								rent[j] = 0;
							}
							else
							{
								rent[j] = int.Parse(values[j + 6]);
							}
						}
						Property property = new Property(values[1], values[2], int.Parse(values[5]), rent);
						_boardSpaceData[i] = new PropertySpace(int.Parse(values[0]), property);
						break;
					case "No":
						if(spaceType == SpaceType.OK || spaceType == SpaceType.PL)
						{
							_boardSpaceData[i] = new CardSpace(int.Parse(values[0]), values[2]);
						}
						else if(spaceType == SpaceType.JAIL)
						{
							_boardSpaceData[i] = new Jail(int.Parse(values[0]));
						}
						else
						{
							_boardSpaceData[i] = new ActionSpace(int.Parse(values[0]), values[1], spaceType, values[3]);
						}
						
						break;
				}

				i += 1;
			}
		}
	}
	
	/**
	 * @param index - the index of the space you're retrieving data for
	 *
	 * @return _boardSpaceData[index] the Space object that was requested
	 */
	public Space get_space(int index)
	{
		return _boardSpaceData[index];
	}
}

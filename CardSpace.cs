using System;

public class CardSpace: Space{
    public CardSpace(int position, string type){
        this.position = position;
        this.type = Enum.Parse<SpaceType>(type);
    }
}

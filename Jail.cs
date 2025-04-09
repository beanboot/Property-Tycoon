using System.Collections.Generic;

public class Jail:Space{
    private LinkedList<Player> jailed;
    public Jail(int position)
    {
        this.position = position;
        name = "Jail";
        type = SpaceType.JAIL;
        jailed = new LinkedList<Player>();

    }

    public void send_to_jail(Player player)
    {
        jailed.AddFirst(player);
    }

    public void release_from_jail(Player player)
    {
        jailed.Remove(player);
    }

    public bool is_in_jail(Player player)
    {
        return jailed.Contains(player);
    }
}
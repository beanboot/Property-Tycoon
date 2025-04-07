public class Jail:Space{
    private Player[] justVisiting;
    private Player[] jailed;
    public Jail(int position)
    {
        this.position = position;
        name = "Jail";
        type = SpaceType.JAIL;
        justVisiting = new Player[6];
        jailed = new Player[6];

    }
}
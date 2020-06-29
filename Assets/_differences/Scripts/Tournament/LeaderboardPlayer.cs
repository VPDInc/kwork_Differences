public class LeaderboardPlayer {
    public string DisplayName;
    public string Id;
    public int Score;
    public bool IsFriend;
    public bool IsMe;
    public string Facebook;

    public override string ToString() {
        return $"{Id}: {DisplayName}, Score: {Score}";
    }
}
public struct LeaderboardPlayer {
    public string DisplayName;
    public string Id;
    public int Score;
    public bool IsFriend;
    public bool IsMe;

    public override string ToString() {
        return $"{Id}: {DisplayName}, Score: {Score}";
    }
}
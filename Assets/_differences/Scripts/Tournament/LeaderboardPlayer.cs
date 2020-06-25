public struct LeaderboardPlayer {
    public string DisplayName;
    public string Id;
    public int Score;
    public string AvatarPath;
    public bool IsFriend;

    public override string ToString() {
        return $"{Id}: {DisplayName}, {AvatarPath}. Score: {Score}";
    }
}
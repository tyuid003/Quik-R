namespace QuickReply;

public sealed class BalloonData
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string BackgroundHex { get; set; } = "#FFFFFF";
    public double Opacity { get; set; } = 0.8;
    public int Order { get; set; }
}

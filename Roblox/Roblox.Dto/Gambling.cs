namespace Roblox.Dto.Gambling;
public enum GamblingStatus
{
    Won = 0,
    Lost = 1,
    UserNotFound = 2,
    InsufficientBalance = 3,
    InvalidAmount = 4,
    UnknownError = 5,
}

public class GamblingResponse
{
    public string message { get; set; }
    public string? submessage { get; set; }
    public int status { get; set; }
}

namespace AgriPod.Infrastructure.Otp;

public sealed class TwilioVerifyOptions
{
    public const string SectionName = "TwilioVerify";

    public string AccountSid { get; init; } = "";
    public string AuthToken { get; init; } = "";
    public string VerifyServiceSid { get; init; } = "";
}

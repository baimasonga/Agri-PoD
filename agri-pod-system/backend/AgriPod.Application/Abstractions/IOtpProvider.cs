using AgriPod.Shared;

namespace AgriPod.Application.Abstractions;

public interface IOtpProvider
{
    Task<OtpResult> SendAsync(string recipientPhone, string channel, CancellationToken cancellationToken);
    Task<OtpResult> VerifyAsync(string recipientPhone, string code, CancellationToken cancellationToken);
}

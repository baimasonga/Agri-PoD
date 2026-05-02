using AgriPod.Application.Abstractions;
using AgriPod.Shared;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace AgriPod.Infrastructure.Otp;

public sealed class TwilioVerifyOtpProvider(IOptions<TwilioVerifyOptions> options) : IOtpProvider
{
    private readonly TwilioVerifyOptions _options = options.Value;

    public async Task<OtpResult> SendAsync(string recipientPhone, string channel, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        var verification = await VerificationResource.CreateAsync(
            to: recipientPhone,
            channel: channel,
            pathServiceSid: _options.VerifyServiceSid);

        return new OtpResult(true, verification.Status);
    }

    public async Task<OtpResult> VerifyAsync(string recipientPhone, string code, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        var check = await VerificationCheckResource.CreateAsync(
            to: recipientPhone,
            code: code,
            pathServiceSid: _options.VerifyServiceSid);

        return new OtpResult(check.Status.Equals("approved", StringComparison.OrdinalIgnoreCase), check.Status);
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.AccountSid) ||
            string.IsNullOrWhiteSpace(_options.AuthToken) ||
            string.IsNullOrWhiteSpace(_options.VerifyServiceSid))
        {
            throw new InvalidOperationException("Twilio Verify settings are not configured.");
        }
    }
}

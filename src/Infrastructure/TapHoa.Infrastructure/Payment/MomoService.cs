using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TapHoa.Application.Payment;

namespace TapHoa.Infrastructure.Payment;

public class MomoService(
    IHttpClientFactory httpClientFactory,
    IOptions<MomoOptions> options) : IMomoService
{
    private readonly MomoOptions _options = options.Value;

    public async Task<MomoPaymentResponse> CreatePaymentAsync(decimal amount, string orderRef)
    {
        var requestId = Guid.NewGuid().ToString();
        var orderId = $"{orderRef}_{DateTime.UtcNow.Ticks}";
        var orderInfo = $"Thanh toan don hang {orderRef}";

        var rawSignature = $"accessKey={_options.AccessKey}&amount={amount}&extraData=&ipnUrl={_options.IpnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_options.PartnerCode}&redirectUrl={_options.ReturnUrl}&requestId={requestId}&requestType=captureWallet";
        var signature = HmacSha256(_options.SecretKey, rawSignature);

        var requestBody = new
        {
            partnerCode = _options.PartnerCode,
            partnerName = "TapHoa",
            storeId = "TapHoa",
            requestId,
            amount,
            orderId,
            orderInfo,
            redirectUrl = _options.ReturnUrl,
            ipnUrl = _options.IpnUrl,
            requestType = "captureWallet",
            extraData = "",
            signature,
            lang = "vi"
        };

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("https://test-payment.momo.vn/v2/gateway/api/create", requestBody);
        var result = await response.Content.ReadFromJsonAsync<MomoGatewayResponse>();

        if (result is null)
            throw new InvalidOperationException("MoMo payment request failed - empty response");

        return new MomoPaymentResponse(
            result.PayUrl ?? string.Empty,
            result.OrderId ?? orderId,
            requestId,
            result.ResultCode,
            result.Message ?? string.Empty);
    }

    public async Task<bool> RefundAsync(string orderRef, decimal amount, long transId)
    {
        var requestId = Guid.NewGuid().ToString();
        var orderId = $"{orderRef}_refund_{DateTime.UtcNow.Ticks}";
        var rawSignature = $"accessKey={_options.AccessKey}&amount={amount}&description=Hoan tien don hang&orderId={orderId}&partnerCode={_options.PartnerCode}&requestId={requestId}&transId={transId}";
        var signature = HmacSha256(_options.SecretKey, rawSignature);

        var requestBody = new
        {
            partnerCode = _options.PartnerCode,
            orderId,
            requestId,
            amount,
            transId,
            lang = "vi",
            description = "Hoan tien don hang",
            signature,
        };

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("https://test-payment.momo.vn/v2/gateway/api/refund", requestBody);
        var result = await response.Content.ReadFromJsonAsync<MomoGatewayResponse>();
        return result?.ResultCode == 0;
    }

    public bool VerifyIpn(Dictionary<string, string> parameters, string signature)
    {
        var rawSignature = $"accessKey={_options.AccessKey}&amount={parameters["amount"]}&extraData={parameters["extraData"]}&ipnUrl={_options.IpnUrl}&orderId={parameters["orderId"]}&orderInfo={parameters["orderInfo"]}&partnerCode={_options.PartnerCode}&redirectUrl={_options.ReturnUrl}&requestId={parameters["requestId"]}&requestType={parameters["requestType"]}";
        var computedSignature = HmacSha256(_options.SecretKey, rawSignature);
        return string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string HmacSha256(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLower();
    }

    private record MomoGatewayResponse(
        string? PayUrl,
        string? OrderId,
        string? RequestId,
        int ResultCode,
        string? Message,
        string? Signature);
}

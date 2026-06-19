using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Options;
using TapHoa.Application.Payment;

namespace TapHoa.Infrastructure.Payment;

public class VnpayService(IOptions<VnpayOptions> options) : IVnpayService
{
    private readonly VnpayOptions _options = options.Value;

    public string CreatePaymentUrl(decimal amount, string orderRef, string ipAddress)
    {
        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _options.TmnCode },
            { "vnp_Amount", ((long)(amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", $"Thanh toan don hang {orderRef}" },
            { "vnp_OrderType", "other" },
            { "vnp_ReturnUrl", _options.ReturnUrl },
            { "vnp_TxnRef", orderRef }
        };

        var hashData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
        var secureHash = HmacSha512(_options.HashSecret, hashData);

        return $"{_options.Url}?{hashData}&vnp_SecureHash={secureHash}";
    }

    public bool VerifyIpn(Dictionary<string, string> parameters, string secureHash)
    {
        var sortedParams = new SortedDictionary<string, string>(
            parameters.Where(p => p.Key != "vnp_SecureHash" && !string.IsNullOrEmpty(p.Value))
                .ToDictionary(p => p.Key, p => p.Value));

        var hashData = string.Join("&", sortedParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
        var computedHash = HmacSha512(_options.HashSecret, hashData);

        return string.Equals(computedHash, secureHash, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> RefundAsync(decimal amount, string orderRef, string transactionNo, string transactionDate)
    {
        var requestId = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var createDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var ipAddr = _options.ServerIpAddress;

        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_RequestId", requestId },
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "refund" },
            { "vnp_TmnCode", _options.TmnCode },
            { "vnp_TxnRef", orderRef },
            { "vnp_Amount", ((long)(amount * 100)).ToString() },
            { "vnp_OrderInfo", $"Hoan tien don hang {orderRef}" },
            { "vnp_TransactionNo", transactionNo },
            { "vnp_TransactionDate", transactionDate },
            { "vnp_CreateBy", "TapHoa" },
            { "vnp_CreateDate", createDate },
            { "vnp_IpAddr", ipAddr },
        };

        var hashData = string.Join("&", vnpParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
        var secureHash = HmacSha512(_options.HashSecret, hashData);
        vnpParams["vnp_SecureHash"] = secureHash;

        using var client = new HttpClient();
        var formData = new FormUrlEncodedContent(vnpParams);
        var response = await client.PostAsync($"{_options.Url.TrimEnd('/')}/refund", formData);
        var responseBody = await response.Content.ReadAsStringAsync();

        return responseBody.Contains("\"vnp_ResponseCode\":\"00\"");
    }

    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToUpper();
    }
}

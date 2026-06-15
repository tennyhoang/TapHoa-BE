namespace TapHoa.Application.Payment;

public interface IMomoService
{
    Task<MomoPaymentResponse> CreatePaymentAsync(decimal amount, string orderRef);
    bool VerifyIpn(Dictionary<string, string> parameters, string signature);
    Task<bool> RefundAsync(string orderRef, decimal amount, long transId);
}

public record MomoPaymentResponse(string PayUrl, string OrderId, string RequestId, int ErrorCode, string Message);

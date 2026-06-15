namespace TapHoa.Application.Payment;

public interface IVnpayService
{
    string CreatePaymentUrl(decimal amount, string orderRef, string ipAddress);
    bool VerifyIpn(Dictionary<string, string> parameters, string secureHash);
    Task<bool> RefundAsync(decimal amount, string orderRef, string transactionNo, string transactionDate);
}

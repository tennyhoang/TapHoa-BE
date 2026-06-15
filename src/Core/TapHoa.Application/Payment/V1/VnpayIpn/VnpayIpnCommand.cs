namespace TapHoa.Application.Payment.V1.VnpayIpn;

public record VnpayIpnCommand(Dictionary<string, string> Parameters) : IRequest<bool>;

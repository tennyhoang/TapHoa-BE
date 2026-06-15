namespace TapHoa.Application.Payment.V1.MomoIpn;

public record MomoIpnCommand(Dictionary<string, string> Parameters) : IRequest<bool>;

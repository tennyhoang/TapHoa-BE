using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1.SetDeliveryPhoto;

public record SetDeliveryPhotoCommand(Guid OrderId, string PhotoUrl) : IRequest<Result<bool>>;

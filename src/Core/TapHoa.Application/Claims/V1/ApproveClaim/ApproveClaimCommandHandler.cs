using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Claims.V1.CreateClaim;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Claims.V1.ApproveClaim;

public class ApproveClaimCommandHandler(IRepository<OrderClaim> claimRepo)
    : IRequestHandler<ApproveClaimCommand, Result<ClaimResponse>>
{
    public async Task<Result<ClaimResponse>> Handle(ApproveClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await claimRepo.Query()
            .Include(c => c.Order)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);

        if (claim is null)
            return Result<ClaimResponse>.Fail("Không tìm thấy khiếu nại.", "CLAIM_NOT_FOUND");

        if (claim.Status != ClaimStatus.Pending)
            return Result<ClaimResponse>.Fail(
                $"Khiếu nại đã được xử lý (trạng thái: '{claim.Status}').", "CLAIM_ALREADY_RESOLVED");

        try
        {
            claim.Order.Refund();
        }
        catch (OrderDomainException ex)
        {
            return Result<ClaimResponse>.Fail(ex.Message, "REFUND_FAILED");
        }

        claim.Status = ClaimStatus.Approved;
        await claimRepo.SaveChangesAsync();

        return Result<ClaimResponse>.Ok(
            CreateClaimCommandHandler.MapToResponse(claim, claim.Customer.FullName));
    }
}

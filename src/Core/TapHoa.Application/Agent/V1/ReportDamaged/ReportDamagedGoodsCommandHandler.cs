using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Agent.V1.ReportDamaged;

public class ReportDamagedGoodsCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<OrderDamagedReport> reportRepo)
    : IRequestHandler<ReportDamagedGoodsCommand, Result<DamagedReportResponse>>
{
    public async Task<Result<DamagedReportResponse>> Handle(
        ReportDamagedGoodsCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<DamagedReportResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        if (order.HubId != request.AgentHubId)
            return Result<DamagedReportResponse>.Fail(
                "Bạn không có quyền thao tác đơn hàng của Hub khác.", "HUB_FORBIDDEN");

        if (order.Status is not (OrderStatus.InHub_ReadyForPickup or OrderStatus.Completed))
            return Result<DamagedReportResponse>.Fail(
                "Chỉ có thể báo cáo hàng lỗi khi đơn hàng đã tại Hub (trạng thái: InHub_ReadyForPickup hoặc Completed).",
                "INVALID_ORDER_STATUS");

        var orderItem = order.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (orderItem is null)
            return Result<DamagedReportResponse>.Fail(
                "Sản phẩm này không có trong đơn hàng.", "PRODUCT_NOT_IN_ORDER");

        if (request.DamagedQuantity > orderItem.Quantity)
            return Result<DamagedReportResponse>.Fail(
                $"Số lượng hàng lỗi ({request.DamagedQuantity}) không thể lớn hơn số lượng đã đặt ({orderItem.Quantity}).",
                "QUANTITY_EXCEEDED");

        var report = OrderDamagedReport.Create(
            orderId:         request.OrderId,
            productId:       request.ProductId,
            damagedQuantity: request.DamagedQuantity,
            unitPrice:       orderItem.UnitPrice,
            reason:          request.Reason,
            agentId:         request.AgentId);

        await reportRepo.AddAsync(report);
        await reportRepo.SaveChangesAsync();

        return Result<DamagedReportResponse>.Ok(new DamagedReportResponse(
            Id:              report.Id,
            OrderId:         report.OrderId,
            ProductId:       report.ProductId,
            ProductName:     orderItem.Product?.Name ?? string.Empty,
            DamagedQuantity: report.DamagedQuantity,
            UnitPrice:       orderItem.UnitPrice,
            RefundAmount:    report.RefundAmount,
            Reason:          report.Reason,
            Status:          report.Status.ToString(),
            CreatedAt:       report.CreatedAt
        ));
    }
}

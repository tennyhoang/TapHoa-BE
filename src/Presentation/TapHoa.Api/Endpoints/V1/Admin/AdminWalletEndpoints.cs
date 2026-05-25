using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminWalletEndpoints
{
    public static void MapAdminWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/wallet")
            .WithTags("Admin - Wallet")
            .RequireAuthorization("Admin");

        // List withdraw requests (default: pending)
        group.MapGet("/withdraw-requests", async (AppDbContext db, string? status) =>
        {
            var query = db.WithdrawRequests
                .Include(w => w.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<WithdrawRequestStatus>(status, true, out var parsedStatus))
                query = query.Where(w => w.Status == parsedStatus);
            else
                query = query.Where(w => w.Status == WithdrawRequestStatus.Pending);

            var items = await query
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new
                {
                    w.Id,
                    w.Amount,
                    w.BankName,
                    w.AccountNumber,
                    w.HolderName,
                    w.Status,
                    w.CreatedAt,
                    w.ProcessedAt,
                    w.AdminNote,
                    UserName  = w.User.FullName,
                    UserEmail = w.User.Email,
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        // Mark completed (admin already transferred the money)
        group.MapPatch("/withdraw-requests/{id:guid}/complete", async (
            Guid id, AppDbContext db, AdminNoteRequest? body) =>
        {
            var req = await db.WithdrawRequests.FindAsync(id);
            if (req is null) return Results.NotFound();
            if (req.Status != WithdrawRequestStatus.Pending)
                return Results.BadRequest(new { message = "Yêu cầu này đã được xử lý." });

            req.Status      = WithdrawRequestStatus.Completed;
            req.ProcessedAt = DateTime.UtcNow;
            req.AdminNote   = body?.Note;
            await db.SaveChangesAsync();

            return Results.Ok(new { req.Id, req.Status });
        });

        // Reject — refund wallet balance back
        group.MapPatch("/withdraw-requests/{id:guid}/reject", async (
            Guid id, AppDbContext db, AdminNoteRequest? body) =>
        {
            var req = await db.WithdrawRequests
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (req is null) return Results.NotFound();
            if (req.Status != WithdrawRequestStatus.Pending)
                return Results.BadRequest(new { message = "Yêu cầu này đã được xử lý." });

            req.User.CreditWallet(req.Amount); // refund
            req.Status      = WithdrawRequestStatus.Rejected;
            req.ProcessedAt = DateTime.UtcNow;
            req.AdminNote   = body?.Note;

            db.WalletTransactions.Add(new WalletTransaction
            {
                UserId      = req.UserId,
                Amount      = req.Amount,
                Type        = WalletTransactionType.Credit,
                Description = $"Hoàn tiền từ yêu cầu rút bị từ chối",
            });

            await db.SaveChangesAsync();

            return Results.Ok(new { req.Id, req.Status });
        });
    }
}

public record AdminNoteRequest(string? Note);

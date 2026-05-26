namespace TapHoa.Application.Contracts;

public record ModerationResult(bool IsToxic, string Sentiment);

public interface IReviewModerationService
{
    Task<ModerationResult> ModerateAsync(string content, CancellationToken cancellationToken = default);
}

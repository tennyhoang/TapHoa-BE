using System.Text.Json.Serialization;

namespace TapHoa.Application.Orders.V1.ConfirmPayment;

public record SepayWebhookPayload(
    [property: JsonPropertyName("id")]              long   Id,
    [property: JsonPropertyName("gateway")]         string Gateway,
    [property: JsonPropertyName("transactionDate")] string TransactionDate,
    [property: JsonPropertyName("accountNumber")]   string AccountNumber,
    [property: JsonPropertyName("content")]         string Content,
    [property: JsonPropertyName("transferType")]    string TransferType,
    [property: JsonPropertyName("transferAmount")]  decimal TransferAmount,
    [property: JsonPropertyName("referenceCode")]   string ReferenceCode
);

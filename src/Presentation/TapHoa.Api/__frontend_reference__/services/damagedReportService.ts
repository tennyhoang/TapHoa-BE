// Đặt file này tại: services/damagedReportService.ts  (trong repo frontend)

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5084";

export interface ReportDamagedRequest {
  orderId: string;
  productId: string;
  damagedQuantity: number;
  reason: string;
}

export interface DamagedReportResponse {
  id: string;
  orderId: string;
  productId: string;
  productName: string;
  damagedQuantity: number;
  unitPrice: number;
  refundAmount: number;
  reason: string;
  status: "Pending" | "Approved";
  createdAt: string;
}

export async function reportDamagedGoods(
  token: string,
  body: ReportDamagedRequest
): Promise<DamagedReportResponse> {
  const res = await fetch(`${API_BASE}/api/v1/agent/report-damaged`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: "Lỗi không xác định" }));
    throw new Error(err.error ?? `HTTP ${res.status}`);
  }

  return res.json();
}

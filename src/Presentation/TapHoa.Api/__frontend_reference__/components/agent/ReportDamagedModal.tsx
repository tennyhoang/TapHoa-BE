// Đặt file này tại: components/agent/ReportDamagedModal.tsx  (trong repo frontend)
"use client";

import { useState } from "react";
import { reportDamagedGoods } from "@/services/damagedReportService";
import toast from "react-hot-toast";

const DAMAGE_REASONS = [
  "Dập nát",
  "Héo / Hỏng",
  "Hết hạn sử dụng",
  "Bị ướt / Ngấm nước",
  "Thiếu hàng trong kiện",
  "Khác",
] as const;

interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  thumbnailUrl?: string;
}

interface Props {
  orderId: string;
  orderItems: OrderItem[];
  token: string;
  onClose: () => void;
  onSuccess?: () => void;
}

export default function ReportDamagedModal({
  orderId,
  orderItems,
  token,
  onClose,
  onSuccess,
}: Props) {
  const [selectedProductId, setSelectedProductId] = useState(
    orderItems[0]?.productId ?? ""
  );
  const [damagedQuantity, setDamagedQuantity] = useState(1);
  const [reason, setReason] = useState<string>(DAMAGE_REASONS[0]);
  const [customReason, setCustomReason] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const selectedItem = orderItems.find((i) => i.productId === selectedProductId);
  const maxQty = selectedItem?.quantity ?? 1;

  const estimatedRefund =
    (selectedItem?.unitPrice ?? 0) * damagedQuantity;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const finalReason = reason === "Khác" ? customReason.trim() : reason;
    if (!finalReason) {
      toast.error("Vui lòng nhập lý do hàng lỗi.");
      return;
    }

    try {
      setSubmitting(true);
      await reportDamagedGoods(token, {
        orderId,
        productId: selectedProductId,
        damagedQuantity,
        reason: finalReason,
      });

      toast.success(
        `Đã ghi nhận báo cáo hàng lỗi. Hoàn tiền ước tính: ${formatCurrency(estimatedRefund)}`
      );
      onSuccess?.();
      onClose();
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Gửi báo cáo thất bại.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-md rounded-2xl bg-white shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-lg font-semibold text-gray-800">
            Báo cáo hàng lỗi
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600"
            aria-label="Đóng"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 px-6 py-5">
          {/* Chọn sản phẩm */}
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Sản phẩm bị lỗi
            </label>
            <select
              value={selectedProductId}
              onChange={(e) => {
                setSelectedProductId(e.target.value);
                setDamagedQuantity(1);
              }}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
              required
            >
              {orderItems.map((item) => (
                <option key={item.productId} value={item.productId}>
                  {item.productName} (đặt {item.quantity})
                </option>
              ))}
            </select>
          </div>

          {/* Số lượng hỏng */}
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Số lượng bị lỗi{" "}
              <span className="text-gray-400">(tối đa {maxQty})</span>
            </label>
            <input
              type="number"
              min={1}
              max={maxQty}
              value={damagedQuantity}
              onChange={(e) =>
                setDamagedQuantity(
                  Math.min(maxQty, Math.max(1, Number(e.target.value)))
                )
              }
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
              required
            />
          </div>

          {/* Lý do */}
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-700">
              Lý do
            </label>
            <select
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
            >
              {DAMAGE_REASONS.map((r) => (
                <option key={r} value={r}>
                  {r}
                </option>
              ))}
            </select>
            {reason === "Khác" && (
              <textarea
                value={customReason}
                onChange={(e) => setCustomReason(e.target.value)}
                placeholder="Mô tả chi tiết lý do..."
                maxLength={1000}
                rows={3}
                className="mt-2 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
                required
              />
            )}
          </div>

          {/* Hoàn tiền ước tính */}
          {selectedItem && (
            <div className="rounded-lg bg-red-50 px-4 py-3 text-sm">
              <span className="text-gray-600">Hoàn tiền ước tính: </span>
              <span className="font-semibold text-red-600">
                {formatCurrency(estimatedRefund)}
              </span>
              <span className="ml-1 text-gray-400">
                ({damagedQuantity} × {formatCurrency(selectedItem.unitPrice)})
              </span>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 rounded-lg bg-red-500 px-4 py-2 text-sm font-medium text-white hover:bg-red-600 disabled:opacity-60"
            >
              {submitting ? "Đang gửi..." : "Gửi báo cáo"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(amount);
}

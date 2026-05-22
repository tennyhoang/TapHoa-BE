// Đặt file này tại: components/agent/AgentOrderCard.tsx  (trong repo frontend)
// Đây là ví dụ tích hợp nút "Báo cáo hàng lỗi" vào card đơn hàng Agent.
// Tích hợp tương tự vào trang /agent hiện có bằng cách import ReportDamagedModal.

"use client";

import { useState } from "react";
import ReportDamagedModal from "./ReportDamagedModal";

interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  thumbnailUrl?: string;
}

interface Order {
  id: string;
  status: string;
  totalAmount: number;
  items: OrderItem[];
  hub: { name: string; address: string };
  createdAt: string;
}

interface Props {
  order: Order;
  token: string;
  onArrived?: (orderId: string) => void;
  onPickedUp?: (orderId: string) => void;
  onReportSuccess?: () => void;
}

export default function AgentOrderCard({
  order,
  token,
  onArrived,
  onPickedUp,
  onReportSuccess,
}: Props) {
  const [showDamagedModal, setShowDamagedModal] = useState(false);

  const canReportDamage =
    order.status === "ArrivedAtHub" || order.status === "Delivered";

  return (
    <div className="rounded-xl border bg-white p-5 shadow-sm">
      {/* ... header, items, v.v. của card hiện tại ... */}

      <div className="mt-4 flex flex-wrap gap-2">
        {order.status === "Shipping" && (
          <button
            onClick={() => onArrived?.(order.id)}
            className="rounded-lg bg-blue-500 px-4 py-2 text-sm font-medium text-white hover:bg-blue-600"
          >
            Đã nhận hàng
          </button>
        )}

        {order.status === "ArrivedAtHub" && (
          <button
            onClick={() => onPickedUp?.(order.id)}
            className="rounded-lg bg-green-500 px-4 py-2 text-sm font-medium text-white hover:bg-green-600"
          >
            Khách đã lấy
          </button>
        )}

        {/* Nút báo cáo hàng lỗi — hiện khi hàng đã đến Hub hoặc đã giao */}
        {canReportDamage && (
          <button
            onClick={() => setShowDamagedModal(true)}
            className="rounded-lg border border-red-300 bg-red-50 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-100"
          >
            Báo cáo hàng lỗi
          </button>
        )}
      </div>

      {showDamagedModal && (
        <ReportDamagedModal
          orderId={order.id}
          orderItems={order.items}
          token={token}
          onClose={() => setShowDamagedModal(false)}
          onSuccess={onReportSuccess}
        />
      )}
    </div>
  );
}

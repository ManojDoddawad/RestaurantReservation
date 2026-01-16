import { useEffect, useState } from "react";
import { tablesApi } from "../../api/tables.api";
import type { TableDto, TableScheduleResponse } from "../../models/table";

export default function TableSchedule() {
  const [tables, setTables] = useState<TableDto[]>([]);
  const [tableId, setTableId] = useState<number | null>(null);
  const [date, setDate] = useState("");
  const [schedule, setSchedule] = useState<TableScheduleResponse | null>(null);
  const [loading, setLoading] = useState(false);

  // 🔹 Load tables
  useEffect(() => {
    const loadTables = async () => {
      const data = await tablesApi.getAll();
      setTables(data);
    };

    loadTables();
  }, []);

  // 🔹 Load schedule
  const loadSchedule = async () => {
    if (!tableId || !date) {
      alert("Select table and date");
      return;
    }

    setLoading(true);
    try {
      const data = await tablesApi.getSchedule(tableId, date);
      setSchedule(data);
    } catch {
      alert("Failed to load table schedule");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Table Schedule</h1>

      {/* Filters */}
      <div className="flex gap-4">
        <select
          className="border p-2"
          value={tableId ?? ""}
          onChange={(e) => setTableId(+e.target.value)}
        >
          <option value="">Select Table</option>
          {Array.isArray(tables) &&
            tables.map((t) => (
              <option key={t.tableId} value={t.tableId}>
                {t.tableNumber}
              </option>
            ))}
        </select>

        <input
          type="date"
          className="border p-2"
          value={date}
          onChange={(e) => setDate(e.target.value)}
        />

        <button
          onClick={loadSchedule}
          disabled={loading}
          className="bg-black text-white px-4 py-2"
        >
          {loading ? "Loading..." : "View Schedule"}
        </button>
      </div>

      {/* Schedule */}
      {schedule && (
        <div className="border rounded p-4 space-y-2">
          <h2 className="text-xl font-semibold">
            Table {schedule.tableNumber} — {schedule.date}
          </h2>

          {schedule.schedule.length === 0 && (
            <p className="text-green-600">No reservations for this day</p>
          )}

          {schedule.schedule.map((slot, i) => (
            <div key={i} className="border p-2 rounded flex justify-between">
              <span>
                {slot.startTime} - {slot.endTime}
              </span>
              <span>
                {slot.customerName} ({slot.partySize})
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

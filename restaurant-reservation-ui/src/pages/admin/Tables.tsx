import { useEffect, useState } from "react";
import { tablesApi } from "../../api/tables.api";
import type { TableDto, CreateTableDto } from "../../models/table";

export default function Tables() {
  const [tables, setTables] = useState<TableDto[]>([]);
  const [form, setForm] = useState<CreateTableDto>({
    tableNumber: "",
    capacity: 2,
  });

  const loadTables = async () => {
    const data = await tablesApi.getAll();
    setTables(data);
  };

  useEffect(() => {
    loadTables();
  }, []);

  const createTable = async () => {
    await tablesApi.create(form);
    setForm({ tableNumber: "", capacity: 2 });
    loadTables();
  };

  const deleteTable = async (id: number) => {
    if (!confirm("Delete this table?")) return;

    try {
      await tablesApi.delete(id);
      loadTables();
    } catch (err: any) {
      alert(err?.message || "Cannot delete table with active reservations.");
    }
  };

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Tables Management</h1>

      {/* Create Table */}
      <div className="border p-4 rounded space-y-2">
        <h2 className="font-semibold">Add Table</h2>

        <input
          className="border p-2 w-full"
          placeholder="Table Number"
          value={form.tableNumber}
          onChange={(e) => setForm({ ...form, tableNumber: e.target.value })}
        />

        <input
          type="number"
          className="border p-2 w-full"
          value={form.capacity}
          onChange={(e) => setForm({ ...form, capacity: +e.target.value })}
        />

        <button onClick={createTable} className="bg-black text-white px-4 py-2">
          Create
        </button>
      </div>

      {/* Tables List */}
      <table className="w-full border">
        <thead className="bg-gray-200">
          <tr>
            <th className="border p-2">Table</th>
            <th className="border p-2">Capacity</th>
            <th className="border p-2">Status</th>
            <th className="border p-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          {tables.map((t) => (
            <tr key={t.tableId}>
              <td className="border p-2">{t.tableNumber}</td>
              <td className="border p-2">{t.capacity}</td>
              <td className="border p-2">
                {t.isActive ? "Active" : "Inactive"}
              </td>
              <td className="border p-2">
                <button
                  onClick={() => deleteTable(t.tableId)}
                  className="text-red-600"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

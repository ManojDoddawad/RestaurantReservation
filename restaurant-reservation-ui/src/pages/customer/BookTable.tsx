import { useState } from "react";
import { reservationsApi } from "../../api/reservations.api";
import { useAuth } from "../../auth/AuthContext";
import type {
  AvailabilityResponse,
  ReservationConfirmation,
} from "../../models/reservation";

export default function BookTable() {
  const { user } = useAuth();

  const [date, setDate] = useState("");
  const [partySize, setPartySize] = useState(2);
  const [availability, setAvailability] = useState<AvailabilityResponse | null>(
    null
  );
  const [loading, setLoading] = useState(false);
  const [confirmation, setConfirmation] =
    useState<ReservationConfirmation | null>(null);

  const checkAvailability = async () => {
    if (!date) {
      alert("Please select date & time");
      return;
    }

    setLoading(true);
    try {
      const res = await reservationsApi.checkAvailability({
        date,
        partySize,
        duration: 120,
      });

      setAvailability(res.data);
    } catch {
      alert("Failed to check availability");
    } finally {
      setLoading(false);
    }
  };

  const createReservation = async () => {
    if (!user) return;

    try {
      const res = await reservationsApi.createReservation({
        customerId: user.userId,
        reservationDate: date,
        partySize,
        duration: 120,
      });

      setConfirmation(res.data);
    } catch {
      alert("Failed to create reservation");
    }
  };

  return (
    <div className="max-w-xl mx-auto p-6 space-y-6">
      <h1 className="text-3xl font-bold">Book a Table</h1>

      {/* Inputs */}
      <div className="space-y-3">
        <input
          type="datetime-local"
          className="border p-2 w-full"
          value={date}
          onChange={(e) => setDate(e.target.value)}
        />

        <input
          type="number"
          min={1}
          className="border p-2 w-full"
          value={partySize}
          onChange={(e) => setPartySize(+e.target.value)}
        />

        <button
          onClick={checkAvailability}
          disabled={loading}
          className="bg-black text-white px-4 py-2 w-full"
        >
          {loading ? "Checking..." : "Check Availability"}
        </button>
      </div>

      {/* Availability Result */}
      {availability && (
        <div className="space-y-2">
          <h2 className="text-xl font-semibold">Available Slots</h2>

          {availability.availableSlots.length === 0 && (
            <p className="text-red-600">No tables available</p>
          )}

          {availability.availableSlots.map((slot, i) => (
            <div key={i} className="flex justify-between border p-2 rounded">
              <span>{slot.time}</span>
              <span>{slot.availableTables} tables</span>
            </div>
          ))}

          {availability.availableSlots.length > 0 && (
            <button
              onClick={createReservation}
              className="bg-green-600 text-white px-4 py-2 w-full mt-4"
            >
              Confirm Reservation
            </button>
          )}
        </div>
      )}

      {/* Confirmation */}
      {confirmation && (
        <div className="border border-green-600 p-4 rounded bg-green-50">
          <h2 className="text-xl font-bold text-green-700">
            Reservation Confirmed 🎉
          </h2>
          <p>Table: {confirmation.tableNumber}</p>
          <p>Confirmation Code: {confirmation.confirmationCode}</p>
        </div>
      )}
    </div>
  );
}

export interface AvailabilityCheckDto {
  date: string;        // ISO string
  partySize: number;
  duration: number;
}

export interface AvailableTableSlot {
  time: string;
  availableTables: number;
}

export interface AvailabilityResponse {
  date: string;
  availableSlots: AvailableTableSlot[];
}

export interface CreateReservationDto {
  customerId: number;
  reservationDate: string;
  partySize: number;
  duration: number;
  specialRequests?: string;
}

export interface ReservationConfirmation {
  reservationId: number;
  confirmationCode: string;
  tableNumber: string;
  message: string;
}

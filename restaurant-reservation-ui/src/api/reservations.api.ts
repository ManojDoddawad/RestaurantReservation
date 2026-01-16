import api from "./axios";
import type {
    AvailabilityCheckDto,
    CreateReservationDto,
    ReservationConfirmation
} from "../models/reservation";

export const reservationsApi = {
  checkAvailability: (params: AvailabilityCheckDto) =>
    api.get("/Reservations/availability", { params }),

  createReservation: (data: CreateReservationDto) =>
    api.post<ReservationConfirmation>("/Reservations", data),
};

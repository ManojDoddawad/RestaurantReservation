export interface TableDto {
  tableId: number;
  tableNumber: string;
  capacity: number;
  location?: string;
  isActive: boolean;
}

export interface CreateTableDto {
  tableNumber: string;
  capacity: number;
  location?: string;
  minCapacity?: number;
  maxCapacity?: number;
}
export interface TableScheduleSlot {
  startTime: string;
  endTime: string;
  reservationId: number;
  customerName: string;
  partySize: number;
}

export interface TableScheduleResponse {
  tableNumber: string;
  date: string;
  schedule: TableScheduleSlot[];
}

export interface UpdateTableDto extends CreateTableDto {}

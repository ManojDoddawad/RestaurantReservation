import api from "./axios";
import type { TableDto, TableScheduleResponse } from "../models/table";

export const tablesApi = {
  getAll: async (): Promise<TableDto[]> => {
    const res = await api.get("/Tables");

    /*
      res shape:
      {
        success: true,
        data: {
          data: TableDto[],
          pageNumber,
          pageSize,
          ...
        }
      }
    */

    return res.data.data; // 🔥 THIS IS THE ARRAY
  },

  create: async (data: any): Promise<void> => {
    await api.post("/Tables", data);
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/Tables/${id}`);
  },

  getSchedule: async (
    tableId: number,
    date: string
  ): Promise<TableScheduleResponse> => {
    const res = await api.get(`/Tables/${tableId}/schedule`, {
      params: { date },
    });

    return res.data;
  },
};

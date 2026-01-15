// src/services/api.js
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7050'; // Update with your API URL

// Create axios instance
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to include auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const auth = {
  login: (credentials) => api.post('/auth/login', credentials),
  register: (userData) => api.post('/auth/register', userData),
};

export const customers = {
  getAll: (params = {}) => api.get('/customers', { params }),
  getById: (id) => api.get(`/customers/${id}`),
  create: (customerData) => api.post('/customers', customerData),
  update: (id, customerData) => api.put(`/customers/${id}`, customerData),
  delete: (id) => api.delete(`/customers/${id}`),
};

export const reservations = {
  getAll: (params = {}) => api.get('/reservations', { params }),
  getById: (id) => api.get(`/reservations/${id}`),
  create: (reservationData) => api.post('/reservations', reservationData),
  update: (id, reservationData) => api.put(`/reservations/${id}`, reservationData),
  delete: (id) => api.delete(`/reservations/${id}`),
  getAvailability: (params) => api.get('/reservations/availability', { params }),
  confirm: (id) => api.post(`/reservations/${id}/confirm`),
  cancel: (id) => api.delete(`/reservations/${id}`),
};

export const tables = {
  getAll: (params = {}) => api.get('/tables', { params }),
  getById: (id) => api.get(`/tables/${id}`),
  create: (tableData) => api.post('/tables', tableData),
  update: (id, tableData) => api.put(`/tables/${id}`, tableData),
  delete: (id) => api.delete(`/tables/${id}`),
};

export const menu = {
  getCategories: () => api.get('/menu/categories'),
  getItems: (params = {}) => api.get('/menu/items', { params }),
  getItemById: (id) => api.get(`/menu/items/${id}`),
};

export const reports = {
  dailySummary: (date) => api.get('/reports/daily-summary', { params: { date } }),
  customerHistory: (customerId) => api.get(`/reports/customer-history/${customerId}`),
};

export default api;

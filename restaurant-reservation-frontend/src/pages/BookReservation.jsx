// src/pages/BookReservation.jsx
import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { reservations, tables } from '../services/api';
import Card from '../components/Card';

const BookReservation = () => {
  const { user } = useAuth();
  const [formData, setFormData] = useState({
    reservationDate: '',
    partySize: 2,
    duration: 120,
    specialRequests: '',
  });
  const [tablesData, setTablesData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchTables = async () => {
      try {
        const response = await tables.getAll();
        setTablesData(response.data.data);
      } catch (err) {
        setError('Failed to load tables');
      } finally {
        setLoading(false);
      }
    };
    fetchTables();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const response = await reservations.create({
        ...formData,
        customerId: user.customerId,
      });
      alert(`Reservation created with ID: ${response.data.reservationId}`);
    } catch (err) {
      setError('Failed to create reservation');
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="text-red-500">{error}</div>;

  return (
    <div className="max-w-md mx-auto">
      <Card title="Book a Reservation">
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Date and Time</label>
            <input
              type="datetime-local"
              value={formData.reservationDate}
              onChange={(e) => setFormData({ ...formData, reservationDate: e.target.value })}
              className="w-full p-2 border rounded"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Party Size</label>
            <input
              type="number"
              min="1"
              max="12"
              value={formData.partySize}
              onChange={(e) => setFormData({ ...formData, partySize: e.target.value })}
              className="w-full p-2 border rounded"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Duration (minutes)</label>
            <input
              type="number"
              value={formData.duration}
              onChange={(e) => setFormData({ ...formData, duration: e.target.value })}
              className="w-full p-2 border rounded"
              required
            />
          </div>
          <div className="mb-4">
            <label className="block text-gray-700 mb-2">Special Requests</label>
            <textarea
              value={formData.specialRequests}
              onChange={(e) => setFormData({ ...formData, specialRequests: e.target.value })}
              className="w-full p-2 border rounded"
            />
          </div>
          <button type="submit" className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600">
            Book Table
          </button>
        </form>
      </Card>
    </div>
  );
};

export default BookReservation;

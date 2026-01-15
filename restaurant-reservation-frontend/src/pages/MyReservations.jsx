// src/pages/MyReservations.jsx
import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { reservations } from '../services/api';
import Card from '../components/Card';

const MyReservations = () => {
  const { user } = useAuth();
  const [reservationsData, setReservationsData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!user?.customerId) return;

    const fetchReservations = async () => {
      try {
        const response = await reservations.getAll({ customerId: user.customerId });
        setReservationsData(response.data.data);
      } catch (err) {
        setError('Failed to load reservations');
      } finally {
        setLoading(false);
      }
    };
    fetchReservations();
  }, [user]);

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="text-red-500">{error}</div>;

  return (
    <div className="space-y-4">
      {reservationsData.map((reservation) => (
        <Card key={reservation.reservationId} className="hover:shadow-lg transition-shadow">
          <div className="flex justify-between items-center">
            <div>
              <h3 className="text-lg font-semibold">Reservation #{reservation.reservationId}</h3>
              <p className="text-gray-600">
                Date: {new Date(reservation.reservationDate).toLocaleString()}
              </p>
              <p className="text-gray-600">Party Size: {reservation.partySize}</p>
              <p className="text-gray-600">Status: {reservation.status}</p>
            </div>
            <div>
              <p className="text-gray-600">Table: {reservation.table.tableNumber}</p>
              <p className="text-gray-600">Confirmation Code: {reservation.confirmationCode}</p>
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
};

export default MyReservations;

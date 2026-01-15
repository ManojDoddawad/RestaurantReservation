// src/pages/Menu.jsx
import { useState, useEffect } from 'react';
import { menu } from '../services/api';
import Card from '../components/Card';

const Menu = () => {
  const [menuItems, setMenuItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchMenu = async () => {
      try {
        const response = await menu.getItems();
        setMenuItems(response.data.data);
      } catch (err) {
        setError('Failed to load menu');
      } finally {
        setLoading(false);
      }
    };
    fetchMenu();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div className="text-red-500">{error}</div>;

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {menuItems.map((item) => (
        <Card key={item.menuItemId} className="hover:shadow-lg transition-shadow">
          <h3 className="text-lg font-semibold">{item.name}</h3>
          <p className="text-gray-600 mb-2">{item.description}</p>
          <p className="text-gray-800 font-medium">${item.price}</p>
          {item.dietaryTags && (
            <div className="mt-2">
              {item.dietaryTags.map((tag, index) => (
                <span key={index} className="inline-block bg-gray-200 rounded-full px-3 py-1 text-sm mr-2">
                  {tag}
                </span>
              ))}
            </div>
          )}
        </Card>
      ))}
    </div>
  );
};

export default Menu;

// src/pages/Home.jsx
import Card from '../components/Card';

const Home = () => {
  return (
    
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <Card title="Welcome to Our Restaurant">
        <p className="text-gray-600">Make a reservation and enjoy our delicious food!</p>
      </Card>
      <Card title="Featured Dishes">
        <p className="text-gray-600">Check out our seasonal specials.</p>
      </Card>
      <Card title="Book a Table">
        <p className="text-gray-600">Reserve your table for a memorable dining experience.</p>
      </Card>
      <h1 className="text-4xl font-bold text-red-600">
  Hello Tailwind
</h1>
    </div>
  );
};

export default Home;

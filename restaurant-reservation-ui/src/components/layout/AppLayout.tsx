import Navbar from "./Navbar";

export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <main className="pt-6">{children}</main>
    </div>
  );
}

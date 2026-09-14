import { Outlet } from 'react-router';
import { Card } from '../ui/Card';
import { Footer } from './Footer';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

export function AppLayout() {
  return (
    <div className="flex h-screen bg-navy">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header />
        <div className="flex flex-1 flex-col overflow-hidden rounded-tl-2xl bg-gray-bg">
          <main className="flex-1 overflow-y-auto p-8">
            <Card>
              <Outlet />
            </Card>
          </main>
          <Footer />
        </div>
      </div>
    </div>
  );
}

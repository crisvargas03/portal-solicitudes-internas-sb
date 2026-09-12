import { Outlet } from 'react-router';
import { ContentCard } from '../common/ContentCard';
import { Header } from './Header';
import { Sidebar } from './Sidebar';

export function AppLayout() {
  return (
    <div className="flex h-screen bg-gray-bg">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header />
        <main className="flex-1 overflow-y-auto p-8">
          <ContentCard>
            <Outlet />
          </ContentCard>
        </main>
      </div>
    </div>
  );
}

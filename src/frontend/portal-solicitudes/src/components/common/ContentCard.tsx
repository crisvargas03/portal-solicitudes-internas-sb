import type { PropsWithChildren } from 'react';

export function ContentCard({ children }: PropsWithChildren) {
  return <div className="rounded-xl bg-white p-6 shadow-sm sm:p-8">{children}</div>;
}

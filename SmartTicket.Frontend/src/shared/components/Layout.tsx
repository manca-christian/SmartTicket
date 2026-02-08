import type { ReactNode } from 'react'
import Navbar from './Navbar'
import ToastHost from './ToastHost'

type LayoutProps = {
  children: ReactNode
}

const Layout = ({ children }: LayoutProps) => {
  return (
    <div className="appShell">
      <ToastHost />
      <Navbar />
      <main className="container" style={{ flex: 1 }}>
        {children}
      </main>
    </div>
  )
}

export default Layout

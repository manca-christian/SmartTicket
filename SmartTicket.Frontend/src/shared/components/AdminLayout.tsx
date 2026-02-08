import { Link, Outlet, useNavigate } from 'react-router-dom'
import useAuth from '../../auth/useAuth'

type AdminLayoutProps = {
  title?: string
}

const AdminLayout = ({ title = 'Control Room' }: AdminLayoutProps) => {
  const navigate = useNavigate()
  const { logout } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-slate-100">
      <div className="flex min-h-screen">
        <aside className="hidden w-72 flex-col gap-6 border-r border-slate-800/80 bg-slate-950/70 px-6 py-6 lg:flex">
          <Link to="/admin/dashboard" className="text-lg font-semibold text-slate-100">
            Admin Console
          </Link>
          <nav className="grid gap-2 text-sm">
            <Link to="/admin/dashboard" className="rounded-lg px-3 py-2 text-slate-200 transition hover:bg-slate-800/60">
              Dashboard
            </Link>
            <Link to="/admin/tickets" className="rounded-lg px-3 py-2 text-slate-200 transition hover:bg-slate-800/60">
              Ticket
            </Link>
            <Link to="/admin/users" className="rounded-lg px-3 py-2 text-slate-200 transition hover:bg-slate-800/60">
              Utenti
            </Link>
          </nav>
          <button
            type="button"
            className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70"
            onClick={handleLogout}
          >
            Logout
          </button>
        </aside>

        <div className="flex-1">
          <header className="flex items-center justify-between border-b border-slate-800/80 bg-slate-950/60 px-6 py-4">
            <div>
              <p className="text-xs uppercase tracking-[0.2em] text-rose-400">Admin Area</p>
              <h1 className="text-lg font-semibold text-slate-100">{title}</h1>
            </div>
            <div className="flex items-center gap-3">
              <Link
                to="/app/tickets"
                className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm text-slate-200 transition hover:border-slate-400/70"
              >
                User App
              </Link>
              <button
                type="button"
                className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm text-slate-200 transition hover:border-slate-400/70"
                onClick={handleLogout}
              >
                Logout
              </button>
            </div>
          </header>
          <main className="mx-auto max-w-6xl px-6 py-8">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}

export default AdminLayout

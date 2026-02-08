import { Link, useNavigate } from 'react-router-dom'
import useAuth from '../../auth/useAuth'

const Navbar = () => {
  const navigate = useNavigate()
  const { isAuthenticated, logout, isAdmin } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className="navbar">
      <nav className="container navbar__inner">
        <Link to="/app/tickets" className="navbar__logo">
          SmartTicket
        </Link>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {isAuthenticated && isAdmin ? (
            <Link to="/admin/dashboard" className="button">
              Admin
            </Link>
          ) : null}
          {isAuthenticated ? (
            <button type="button" onClick={handleLogout} className="button">
              Logout
            </button>
          ) : (
            <Link to="/login" className="button">
              Login
            </Link>
          )}
        </div>
      </nav>
    </header>
  )
}

export default Navbar

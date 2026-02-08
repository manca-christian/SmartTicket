import type { ReactNode } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import useAuth from '../../auth/useAuth'

type RequireAdminProps = {
  children?: ReactNode
}

const RequireAdmin = ({ children }: RequireAdminProps) => {
  const { isAuthenticated, isAdmin } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (!isAdmin) {
    return <Navigate to="/app/tickets" replace />
  }

  return children ? <>{children}</> : <Outlet />
}

export default RequireAdmin

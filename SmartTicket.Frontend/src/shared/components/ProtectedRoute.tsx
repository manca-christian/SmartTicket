import type { ReactNode } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import useAuth from '../../auth/useAuth'

type ProtectedRouteProps = {
  children?: ReactNode
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return children ? <>{children}</> : <Outlet />
}

export default ProtectedRoute

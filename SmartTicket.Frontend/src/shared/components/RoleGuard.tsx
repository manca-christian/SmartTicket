import type { ReactNode } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import useAuth from '../../auth/useAuth'

type RoleGuardProps = {
  allowedRoles: string[]
  children?: ReactNode
}

const RoleGuard = ({ allowedRoles, children }: RoleGuardProps) => {
  const { isAuthenticated, role } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (!role || !allowedRoles.includes(role)) {
    return <Navigate to="/app/tickets" replace />
  }

  return children ? <>{children}</> : <Outlet />
}

export default RoleGuard

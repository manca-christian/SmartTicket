import { Navigate, Route, Routes } from 'react-router-dom'
import LoginPage from '../pages/LoginPage'
import AdminPage from '../pages/AdminPage'
import AdminUsersPage from '../pages/AdminUsersPage'
import NotFoundPage from '../pages/NotFoundPage'
import ProfilePage from '../pages/ProfilePage'
import TicketsPage from '../features/tickets/pages/TicketsPage'
import TicketDetailPage from '../features/tickets/pages/TicketDetailPage'
import useAuth from '../auth/useAuth'
import RequireAdmin from '../shared/components/RequireAdmin'
import RequireAuth from '../shared/components/RequireAuth'
import AppLayout from '../shared/components/AppLayout'
import AdminLayout from '../shared/components/AdminLayout'

const HomeRedirect = () => {
	const { isAuthenticated, isAdmin } = useAuth()
	const destination = isAuthenticated && isAdmin ? '/admin/dashboard' : '/app/tickets'
	return <Navigate to={isAuthenticated ? destination : '/login'} replace />
}

const AppRouter = () => {
	return (
		<Routes>
			<Route path="/" element={<HomeRedirect />} />
			<Route path="/login" element={<LoginPage />} />
			<Route
				path="/app"
				element={
					<RequireAuth>
						<AppLayout />
					</RequireAuth>
				}
			>
				<Route index element={<Navigate to="/app/tickets" replace />} />
				<Route path="tickets" element={<TicketsPage />} />
				<Route path="tickets/:id" element={<TicketDetailPage />} />
				<Route path="profile" element={<ProfilePage />} />
			</Route>
			<Route
				path="/admin"
				element={
					<RequireAdmin>
						<AdminLayout />
					</RequireAdmin>
				}
			>
				<Route index element={<Navigate to="/admin/dashboard" replace />} />
				<Route path="dashboard" element={<AdminPage />} />
				<Route path="tickets" element={<TicketsPage />} />
				<Route path="tickets/:id" element={<TicketDetailPage />} />
				<Route path="users" element={<AdminUsersPage />} />
			</Route>
			<Route path="*" element={<NotFoundPage />} />
		</Routes>
	)
}

export default AppRouter

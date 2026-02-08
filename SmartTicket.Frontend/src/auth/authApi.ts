import api from '../api/client'
import { AUTH_LOGIN, AUTH_LOGOUT, AUTH_ME, AUTH_REFRESH } from '../api/endpoints'
import type { MeDto, Role } from './types'
import { getState, logout as clearAuth, setAccessToken } from './authStore'

type LoginResponse = {
	accessToken: string
}

type LoginResult = {
	userId: string | null
	role: Role | null
}

const login = async (email: string, password: string) => {
	const response = await api.post<LoginResponse>(AUTH_LOGIN, { email, password })
	const token = response.data.accessToken
	setAccessToken(token)
	const snapshot = getState()
	return { userId: snapshot.userId, role: snapshot.role }
}

const refresh = async () => {
	const response = await api.post<LoginResponse>(AUTH_REFRESH)
	const token = response.data.accessToken
	setAccessToken(token)
	return token
}

const me = async () => {
	const response = await api.get<MeDto>(AUTH_ME)
	return response.data
}

const logout = async () => {
	try {
		await api.post(AUTH_LOGOUT)
	} catch {
		// ignore
	} finally {
		clearAuth()
	}
}

const authApi = {
	login,
	refresh,
	me,
	logout,
}

export default authApi
export { login, refresh, me, logout }

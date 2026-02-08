import type { Role } from './types'

type AuthState = {
	accessToken: string | null
	userId: string | null
	role: Role | null
}

type AuthListener = () => void

const listeners = new Set<AuthListener>()

const state: AuthState = {
	accessToken: null,
	userId: null,
	role: null,
}

const base64UrlDecode = (value: string) => {
	const base64 = value.replace(/-/g, '+').replace(/_/g, '/').padEnd(Math.ceil(value.length / 4) * 4, '=')
	return atob(base64)
}

const decodeJwtPayload = (token: string) => {
	const parts = token.split('.')
	if (parts.length < 2) {
		return null
	}
	try {
		const payload = base64UrlDecode(parts[1])
		return JSON.parse(payload) as Record<string, unknown>
	} catch {
		return null
	}
}

const getClaimValue = (payload: Record<string, unknown> | null, keys: string[]) => {
	if (!payload) {
		return null
	}
	for (const key of keys) {
		const value = payload[key]
		if (typeof value === 'string') {
			return value
		}
		if (Array.isArray(value) && typeof value[0] === 'string') {
			return value[0]
		}
	}
	return null
}

const applyTokenClaims = (token: string | null) => {
	if (!token) {
		state.userId = null
		state.role = null
		return
	}
	const payload = decodeJwtPayload(token)
	const roleValue = getClaimValue(payload, ['role', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
	const userIdValue = getClaimValue(payload, [
		'sub',
		'nameid',
		'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
	])
	state.userId = userIdValue
	state.role = roleValue === 'Admin' || roleValue === 'User' ? roleValue : null
}

const notify = () => {
	listeners.forEach((listener) => listener())
}

export const getState = () => {
	return state
}

export const setAccessToken = (token: string | null) => {
	state.accessToken = token
	applyTokenClaims(token)
	notify()
}

export const logout = () => {
	state.accessToken = null
	state.userId = null
	state.role = null
	notify()
}

export const subscribe = (listener: AuthListener) => {
	listeners.add(listener)
	return () => listeners.delete(listener)
}

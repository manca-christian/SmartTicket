import { useEffect, useMemo, useState } from 'react'
import type { Role } from './types'
import { getState, logout, setAccessToken, subscribe } from './authStore'

export type UseAuthResult = {
	accessToken: string | null
	userId: string | null
	role: Role | null
	isAuthenticated: boolean
	isAdmin: boolean
	setAccessToken: (token: string | null) => void
	logout: () => void
}

const useAuth = (): UseAuthResult => {
	const [snapshot, setSnapshot] = useState(() => getState())

	useEffect(() => {
		const unsubscribe = subscribe(() => setSnapshot({ ...getState() }))
		return () => unsubscribe()
	}, [])

	return useMemo(
		() => ({
			accessToken: snapshot.accessToken,
			userId: snapshot.userId,
			role: snapshot.role,
			isAuthenticated: Boolean(snapshot.accessToken),
			isAdmin: snapshot.role === 'Admin',
			setAccessToken,
			logout,
		}),
		[snapshot],
	)
}

export default useAuth

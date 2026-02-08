import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { AUTH_REFRESH } from './endpoints'
import { getState, logout, setAccessToken } from '../auth/authStore'

type RetryableRequestConfig = InternalAxiosRequestConfig & {
	_retry?: boolean
}

const api: AxiosInstance = axios.create({
	baseURL: import.meta.env.VITE_API_BASE_URL,
	withCredentials: true,
})

let refreshPromise: Promise<string | null> | null = null

const refreshAccessToken = async () => {
	if (!refreshPromise) {
		refreshPromise = axios
			.post(
				`${import.meta.env.VITE_API_BASE_URL}${AUTH_REFRESH}`,
				null,
				{ withCredentials: true },
			)
			.then((response) => {
				const token = response?.data?.accessToken ?? null
				if (token) {
					setAccessToken(token)
				}
				return token
			})
			.catch(() => {
				logout()
				window.location.assign('/login')
				return null
			})
			.finally(() => {
				refreshPromise = null
			})
	}

	return refreshPromise
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
	const { accessToken } = getState()
	if (accessToken) {
		config.headers = config.headers ?? {}
		config.headers.Authorization = `Bearer ${accessToken}`
	}
	return config
})

api.interceptors.response.use(
	(response) => response,
	async (error: AxiosError) => {
		const status = error.response?.status
		const originalConfig = error.config as RetryableRequestConfig | undefined

		if (status !== 401 || !originalConfig) {
			return Promise.reject(error)
		}

		if (originalConfig._retry) {
			return Promise.reject(error)
		}

		if (originalConfig.url?.includes(AUTH_REFRESH)) {
			logout()
			window.location.assign('/login')
			return Promise.reject(error)
		}

		originalConfig._retry = true

		const newToken = await refreshAccessToken()
		if (!newToken) {
			return Promise.reject(error)
		}

		originalConfig.headers = originalConfig.headers ?? {}
		originalConfig.headers.Authorization = `Bearer ${newToken}`

		return api.request(originalConfig)
	},
)

export default api

import { useMemo, useRef, useState } from 'react'
import type { CSSProperties } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'
import authApi from '../auth/authApi'
import useToast from '../shared/hooks/useToast'
import { haptic, pulse, shake } from '../shared/utils/feedback'

type FieldErrors = {
	email?: string
	password?: string
}

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const particles = [
	{ x: '8%', y: '12%', size: '6px', delay: '0s', duration: '6.5s' },
	{ x: '22%', y: '28%', size: '4px', delay: '1s', duration: '7.8s' },
	{ x: '35%', y: '16%', size: '5px', delay: '0.4s', duration: '6.2s' },
	{ x: '48%', y: '30%', size: '3px', delay: '1.6s', duration: '8.4s' },
	{ x: '62%', y: '18%', size: '6px', delay: '0.8s', duration: '7.1s' },
	{ x: '75%', y: '26%', size: '4px', delay: '1.2s', duration: '8s' },
	{ x: '88%', y: '12%', size: '5px', delay: '0.2s', duration: '6.8s' },
	{ x: '12%', y: '58%', size: '5px', delay: '1.4s', duration: '9s' },
	{ x: '28%', y: '70%', size: '3px', delay: '0.6s', duration: '7.4s' },
	{ x: '40%', y: '62%', size: '6px', delay: '1.1s', duration: '8.6s' },
	{ x: '54%', y: '74%', size: '4px', delay: '0.3s', duration: '6.9s' },
	{ x: '68%', y: '60%', size: '5px', delay: '1.7s', duration: '9.2s' },
	{ x: '82%', y: '72%', size: '3px', delay: '0.9s', duration: '7.7s' },
	{ x: '92%', y: '54%', size: '6px', delay: '1.3s', duration: '8.3s' },
	{ x: '18%', y: '86%', size: '4px', delay: '0.5s', duration: '7.2s' },
	{ x: '76%', y: '86%', size: '5px', delay: '1.5s', duration: '9.4s' },
]

const NeonParticles = () => {
	return (
		<div className="neonParticles" aria-hidden="true">
			{particles.map((particle, index) => (
				<span
					key={`particle-${index}`}
					className="neonParticle"
					style={
						{
							'--x': particle.x,
							'--y': particle.y,
							'--size': particle.size,
							'--delay': particle.delay,
							'--duration': particle.duration,
						} as CSSProperties
					}
				/>
			))}
		</div>
	)
}

const LoginPage = () => {
	const navigate = useNavigate()
	const toast = useToast()
	const [email, setEmail] = useState('')
	const [password, setPassword] = useState('')
	const [showPassword, setShowPassword] = useState(false)
	const [touched, setTouched] = useState({ email: false, password: false })
	const [errors, setErrors] = useState<FieldErrors>({})
	const [isSubmitting, setIsSubmitting] = useState(false)
	const cardRef = useRef<HTMLElement | null>(null)
	const passwordInputRef = useRef<HTMLInputElement | null>(null)

	const normalizedEmail = useMemo(() => email.trim(), [email])

	const validate = (nextEmail: string, nextPassword: string): FieldErrors => {
		const nextErrors: FieldErrors = {}
		const trimmedEmail = nextEmail.trim()

		if (!trimmedEmail) {
			nextErrors.email = 'Inserisci la tua email.'
		} else if (!emailRegex.test(trimmedEmail)) {
			nextErrors.email = 'Email non valida. Usa un formato tipo nome@azienda.it.'
		}

		if (!nextPassword) {
			nextErrors.password = 'Inserisci la password.'
		} else if (nextPassword.length < 6) {
			nextErrors.password = 'La password deve avere almeno 6 caratteri.'
		}

		return nextErrors
	}

	const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
		event.preventDefault()
		const nextErrors = validate(email, password)
		setErrors(nextErrors)
		setTouched({ email: true, password: true })
		if (Object.keys(nextErrors).length > 0) {
			shake(cardRef.current)
			haptic('medium')
			toast.error('Controlla i campi evidenziati.')
			return
		}

		setIsSubmitting(true)
		try {
			const authState = await authApi.login(normalizedEmail, password)
			const roleLabel = authState.role === 'Admin' ? 'Admin' : 'User'
			toast.success(`Benvenuto ${normalizedEmail}`, `Ruolo: ${roleLabel}`)
			pulse(cardRef.current)
			haptic('light')
			const destination = authState.role === 'Admin' ? '/admin/dashboard' : '/app/tickets'
			navigate(destination, { replace: true })
		} catch (err) {
			if (axios.isAxiosError(err)) {
				const status = err.response?.status
				if (status === 401 || status === 403) {
					toast.error('Email o password errate')
					shake(cardRef.current)
					haptic('medium')
					setPassword('')
					passwordInputRef.current?.focus()
					return
				}
			}
			toast.error('Errore di connessione. Riprova.')
		} finally {
			setIsSubmitting(false)
		}
	}

	const showEmailError = touched.email && Boolean(errors.email)
	const showPasswordError = touched.password && Boolean(errors.password)

	return (
		<div className="loginShell">
			<div className="loginBg" aria-hidden="true">
				<NeonParticles />
			</div>
			<div className="glowOrb" aria-hidden="true" />
			<section className="neonCard" ref={cardRef}>
				<div className="scanlines" aria-hidden="true" />
				<header className="cardHeader">
					<p className="neonTitle">SMARTTICKET // ACCESS</p>
					<span className="typingText">Ticket tracking • Priorita smart • Aggiornamenti live</span>
				</header>
				<form className="neonForm" onSubmit={handleSubmit} noValidate>
					{isSubmitting ? <div className="formOverlay" aria-hidden="true" /> : null}
					<label className="field">
						<span className="label">Email</span>
						<input
							className={`neonInput${showEmailError ? ' errorInput' : ''}`}
							type="email"
							autoFocus
							value={email}
							onChange={(event) => setEmail(event.target.value)}
							onBlur={() => {
								setTouched((prev) => ({ ...prev, email: true }))
								setErrors((prev) => ({ ...prev, ...validate(email, password) }))
							}}
							autoComplete="email"
							inputMode="email"
							placeholder="nome@azienda.it"
						/>
						{showEmailError ? (
							<span className="errorText">{errors.email}</span>
						) : (
							<span className="helper">Useremo questa mail per riconoscerti.</span>
						)}
					</label>

					<label className="field">
						<span className="label">Password</span>
						<div className="passwordField">
							<input
								className={`neonInput${showPasswordError ? ' errorInput' : ''}`}
								type={showPassword ? 'text' : 'password'}
								ref={passwordInputRef}
								value={password}
								onChange={(event) => setPassword(event.target.value)}
								onBlur={() => {
									setTouched((prev) => ({ ...prev, password: true }))
									setErrors((prev) => ({ ...prev, ...validate(email, password) }))
								}}
								autoComplete="current-password"
								placeholder="••••••••"
							/>
							<button
								type="button"
								className="toggleBtn"
								onClick={() => setShowPassword((prev) => !prev)}
							>
								{showPassword ? 'Nascondi' : 'Mostra'}
							</button>
						</div>
						{showPasswordError ? (
							<span className="errorText">{errors.password}</span>
						) : (
							<span className="helper">Minimo 6 caratteri.</span>
						)}
					</label>

					<button className="neonBtn" type="submit" disabled={isSubmitting}>
						{isSubmitting ? 'Connessione...' : 'Entra'}
					</button>
				</form>
			</section>
		</div>
	)
}

export default LoginPage

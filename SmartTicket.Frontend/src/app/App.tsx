import AppProviders from './providers'
import AppRouter from './router'
import ToastHost from '../shared/components/ToastHost'

const App = () => {
	return (
		<AppProviders>
			<ToastHost />
			<AppRouter />
		</AppProviders>
	)
}

export default App

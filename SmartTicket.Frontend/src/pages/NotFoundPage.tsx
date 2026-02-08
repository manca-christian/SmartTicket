import { Link } from 'react-router-dom'

const NotFoundPage = () => {
  return (
    <div style={{ maxWidth: 520, margin: '48px auto', padding: '0 16px' }}>
      <h1>Pagina non trovata</h1>
      <p>La pagina richiesta non esiste.</p>
      <Link to="/app/tickets">Vai ai ticket</Link>
    </div>
  )
}

export default NotFoundPage

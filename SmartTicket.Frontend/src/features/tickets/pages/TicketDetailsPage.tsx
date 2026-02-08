import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { useTicketDetails, useTicketMutations } from '../hooks'
import type { TicketPriority } from '../types'
import { ticketPriorityLabels } from '../types'
import { formatDateTime } from '../../../shared/utils/date'
import { useTicketComments, useCreateComment } from '../../comments/hooks'
import { useTicketHistory } from '../../history/hooks'
import CommentsList from '../../comments/components/CommentsList'
import CommentComposer from '../../comments/components/CommentComposer'
import HistoryTimeline from '../../history/components/HistoryTimeline'
import AttachmentGrid from '../../../shared/components/AttachmentGrid'

const TicketDetailsPage = () => {
  const { id } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
  const basePath = location.pathname.startsWith('/admin') ? '/admin' : '/app'
  const { data, isLoading, error } = useTicketDetails(id)
  const { updateTicket, closeTicket, setPriority, setDueDate, clearDueDate } = useTicketMutations()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priorityValue, setPriorityValue] = useState<TicketPriority>('Medium')
  const [dueAtValue, setDueAtValue] = useState('')
  const [commentsPage, setCommentsPage] = useState(1)
  const [historyPage, setHistoryPage] = useState(1)
  const commentsQuery = useTicketComments(id, { page: commentsPage, pageSize: 5 })
  const historyQuery = useTicketHistory(id, { page: historyPage, pageSize: 5 })
  const createComment = useCreateComment(id)

  useEffect(() => {
    if (data?.data) {
      setTitle(data.data.title)
      setDescription(data.data.description)
      setPriorityValue(data.data.priority)
      setDueAtValue(data.data.dueAt ? data.data.dueAt.slice(0, 16) : '')
    }
  }, [data?.data])

  if (!id) {
    return <p>Ticket non valido.</p>
  }

  if (isLoading) {
    return <p>Caricamento…</p>
  }

  if (error || !data?.data) {
    return <p>Impossibile caricare il ticket.</p>
  }

  const handleUpdate = () => {
    updateTicket.mutate({ id, dto: { title, description }, etag: data.etag })
  }

  const handleClose = () => {
    closeTicket.mutate({ id, etag: data.etag })
      navigate(`${basePath}/tickets`)
  }

  const handlePriority = () => {
    setPriority.mutate({ id, dto: { priority: priorityValue }, etag: data.etag })
  }

  const handleDueDate = () => {
    if (!dueAtValue) {
      return
    }
    setDueDate.mutate({ id, dto: { dueAt: new Date(dueAtValue).toISOString() }, etag: data.etag })
  }

  const handleClearDueDate = () => {
    clearDueDate.mutate({ id, etag: data.etag })
    setDueAtValue('')
  }

  const handleCreateComment = async (text: string, attachments: string[]) => {
    if (!data?.etag) {
      window.alert('ETag non disponibile. Ricarica il ticket.')
      return
    }
    await createComment.mutateAsync({ dto: { text, attachments }, etag: data.etag })
  }

  return (
    <div style={{ display: 'grid', gap: 16, maxWidth: 680 }}>
      <div>
        <h1>{data.data.title}</h1>
        <p>Stato: {data.data.status}</p>
      </div>

      <div style={{ display: 'grid', gap: 12 }}>
        <label style={{ display: 'grid', gap: 6 }}>
          Titolo
          <input value={title} onChange={(event) => setTitle(event.target.value)} />
        </label>
        <label style={{ display: 'grid', gap: 6 }}>
          Descrizione
          <textarea value={description} onChange={(event) => setDescription(event.target.value)} rows={4} />
        </label>
        <button type="button" onClick={handleUpdate} disabled={updateTicket.isPending}>
          Aggiorna
        </button>
      </div>

      <div style={{ display: 'grid', gap: 12 }}>
        <label style={{ display: 'grid', gap: 6 }}>
          Priorità
          <select value={priorityValue} onChange={(event) => setPriorityValue(event.target.value as TicketPriority)}>
            <option value="VeryLow">{ticketPriorityLabels.VeryLow}</option>
            <option value="Low">{ticketPriorityLabels.Low}</option>
            <option value="Medium">{ticketPriorityLabels.Medium}</option>
            <option value="High">{ticketPriorityLabels.High}</option>
            <option value="VeryHigh">{ticketPriorityLabels.VeryHigh}</option>
          </select>
        </label>
        <button type="button" onClick={handlePriority} disabled={setPriority.isPending}>
          Salva priorità
        </button>
      </div>

      <div style={{ display: 'grid', gap: 12 }}>
        <label style={{ display: 'grid', gap: 6 }}>
          Due date
          <input
            type="datetime-local"
            value={dueAtValue}
            onChange={(event) => setDueAtValue(event.target.value)}
          />
        </label>
        <div style={{ display: 'flex', gap: 8 }}>
          <button type="button" onClick={handleDueDate} disabled={setDueDate.isPending}>
            Salva due date
          </button>
          <button type="button" onClick={handleClearDueDate} disabled={clearDueDate.isPending}>
            Rimuovi due date
          </button>
        </div>
      </div>

      <div>
        <p>Assegnato: {data.data.assigneeUserId ?? '—'}</p>
        <p>Creato: {formatDateTime(data.data.createdAt)}</p>
        <p>Aggiornato: {formatDateTime(data.data.updatedAt)}</p>
        {data.data.attachments && data.data.attachments.length > 0 ? (
          <div style={{ marginTop: 8 }}>
            <AttachmentGrid urls={data.data.attachments} />
          </div>
        ) : null}
      </div>

      <section style={{ display: 'grid', gap: 12 }}>
        <h2>Commenti</h2>
        <CommentComposer onSubmit={handleCreateComment} isSubmitting={createComment.isPending} />
        {commentsQuery.isLoading ? <p>Caricamento commenti…</p> : null}
        {commentsQuery.error ? <p>Errore nel caricamento commenti.</p> : null}
        {commentsQuery.data ? <CommentsList items={commentsQuery.data.items} /> : null}
        <div style={{ display: 'flex', gap: 8 }}>
          <button
            type="button"
            onClick={() => setCommentsPage((prev) => Math.max(1, prev - 1))}
            disabled={commentsPage <= 1}
          >
            Prev
          </button>
          <span>Pagina {commentsPage}</span>
          <button
            type="button"
            onClick={() => setCommentsPage((prev) => prev + 1)}
            disabled={(commentsQuery.data?.items.length ?? 0) < 5}
          >
            Next
          </button>
        </div>
      </section>

      <section style={{ display: 'grid', gap: 12 }}>
        <h2>History</h2>
        {historyQuery.isLoading ? <p>Caricamento history…</p> : null}
        {historyQuery.error ? <p>Errore nel caricamento history.</p> : null}
        {historyQuery.data ? <HistoryTimeline items={historyQuery.data.items} /> : null}
        <div style={{ display: 'flex', gap: 8 }}>
          <button
            type="button"
            onClick={() => setHistoryPage((prev) => Math.max(1, prev - 1))}
            disabled={historyPage <= 1}
          >
            Prev
          </button>
          <span>Pagina {historyPage}</span>
          <button
            type="button"
            onClick={() => setHistoryPage((prev) => prev + 1)}
            disabled={(historyQuery.data?.items.length ?? 0) < 5}
          >
            Next
          </button>
        </div>
      </section>

      {import.meta.env.DEV ? <p>ETag: {data.etag ?? '—'}</p> : null}

      <button type="button" onClick={handleClose} disabled={closeTicket.isPending}>
        Chiudi ticket
      </button>
    </div>
  )
}

export default TicketDetailsPage

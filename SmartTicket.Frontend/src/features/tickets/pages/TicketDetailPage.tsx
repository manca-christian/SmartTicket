import { useEffect, useMemo, useRef, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import axios from 'axios'
import { useTicketDetails, useTicketMutations } from '../hooks'
import Badge from '../components/Badge'
import type { TicketPriority, TicketStatus } from '../types'
import { ticketPriorityLabels } from '../types'
import { formatDateTime } from '../../../shared/utils/date'
import { useTicketHistory } from '../../history/hooks'
import TicketComments from '../../comments/components/TicketComments'
import HistoryTimeline from '../../history/components/HistoryTimeline'
import useAuth from '../../../auth/useAuth'
import useToast from '../../../shared/hooks/useToast'
import Input from '../../../shared/components/Input'
import AttachmentGrid from '../../../shared/components/AttachmentGrid'

type TabKey = 'details' | 'notes' | 'history'

const getStatusTone = (status: TicketStatus) => {
  switch (status) {
    case 'Open':
      return 'success'
    case 'InProgress':
      return 'warn'
    case 'Closed':
      return 'danger'
    default:
      return 'warn'
  }
}

const getPriorityBadgeClass = (priority?: TicketPriority | null) => {
  switch (priority) {
    case 'VeryLow':
      return 'bg-slate-500/20 text-slate-100 border-slate-400/40'
    case 'Low':
      return 'bg-emerald-500/20 text-emerald-100 border-emerald-400/50'
    case 'Medium':
      return 'bg-amber-500/25 text-amber-100 border-amber-300/50'
    case 'High':
      return 'bg-orange-500/25 text-orange-100 border-orange-300/60'
    case 'VeryHigh':
      return 'bg-rose-500/25 text-rose-100 border-rose-300/60'
    default:
      return 'bg-slate-500/10 text-slate-400 border-slate-600/40'
  }
}

const getPriorityLabel = (priority?: TicketPriority | null) => {
  if (!priority) {
    return '—'
  }
  return ticketPriorityLabels[priority]
}

const SkeletonLine = ({ width }: { width: string }) => (
  <div
    style={{
      height: 12,
      borderRadius: 999,
      background: 'rgba(148, 163, 184, 0.2)',
      width,
    }}
  />
)

const DetailSkeleton = () => {
  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="card" style={{ display: 'grid', gap: 12 }}>
        <SkeletonLine width="40%" />
        <SkeletonLine width="55%" />
        <div style={{ display: 'flex', gap: 8 }}>
          <SkeletonLine width="90px" />
          <SkeletonLine width="90px" />
        </div>
      </div>
      <div className="card" style={{ display: 'grid', gap: 10 }}>
        <SkeletonLine width="30%" />
        <SkeletonLine width="80%" />
        <SkeletonLine width="50%" />
      </div>
      <div className="card" style={{ display: 'grid', gap: 10 }}>
        <SkeletonLine width="35%" />
        <SkeletonLine width="60%" />
      </div>
    </div>
  )
}

const TicketDetailPage = () => {
  const { id } = useParams()
  const navigate = useNavigate()
  const location = useLocation()
    const basePath = location.pathname.startsWith('/admin') ? '/admin' : '/app'
  const [activeTab, setActiveTab] = useState<TabKey>('details')
  const [etag, setEtag] = useState<string | null>(null)
  const [priorityValue, setPriorityValue] = useState<TicketPriority>('Medium')
  const [assigneeValue, setAssigneeValue] = useState('')
  const [optimisticStatus, setOptimisticStatus] = useState<TicketStatus | null>(null)
  const [optimisticPriority, setOptimisticPriority] = useState<TicketPriority | null>(null)
  const redirectOnceRef = useRef(false)
  const historyToastOnceRef = useRef(false)

  const toast = useToast()
  const { userId, isAdmin } = useAuth()
  const { data, isLoading, error, refetch } = useTicketDetails(id)
  const { closeTicket, setPriority, assignTicket } = useTicketMutations()

  const historyQuery = useTicketHistory(id, { page: 1, pageSize: 10 })
  const historyItems = historyQuery.data?.items ?? []
  const isHistoryEmpty = !historyQuery.isLoading && !historyQuery.error && historyItems.length === 0

  useEffect(() => {
    setEtag(data?.etag ?? null)
  }, [data?.etag])

  useEffect(() => {
    if (data?.data) {
      setPriorityValue(data.data.priority)
      setAssigneeValue(data.data.assigneeUserId ?? '')
      setOptimisticStatus(null)
      setOptimisticPriority(null)
    }
  }, [data?.data])

  const ticket = data?.data
  const displayStatus = optimisticStatus ?? ticket?.status
  const displayPriority = optimisticPriority ?? ticket?.priority
  const isClosed = displayStatus === 'Closed'

  const isOwner = Boolean(ticket && userId && ticket.createdByUserId === userId)
  const canClose = (isAdmin || isOwner) && !isClosed
  const canChangePriority = (isAdmin || isOwner) && !isClosed
  const canAssign = isAdmin && !isClosed

  const errorStatus = useMemo(() => {
    if (!error || !axios.isAxiosError(error)) {
      return undefined
    }
    return error.response?.status
  }, [error])

  const errorData = useMemo(() => {
    if (!error || !axios.isAxiosError(error)) {
      return undefined
    }
    return error.response?.data
  }, [error])

  useEffect(() => {
    if (!error) {
      return
    }
    console.log('TicketDetail error', {
      id,
      status: errorStatus,
      data: errorData,
    })
  }, [error, errorData, errorStatus, id])

  useEffect(() => {
    if (errorStatus !== 401 || redirectOnceRef.current) {
      return
    }
    redirectOnceRef.current = true
    navigate('/login', { replace: true })
  }, [errorStatus, navigate])

  const isForbidden = errorStatus === 403
  const isNotFound = errorStatus === 404
  const isUnauthorized = errorStatus === 401

  const historyStatus = useMemo(() => {
    if (!historyQuery.error || !axios.isAxiosError(historyQuery.error)) {
      return undefined
    }
    return historyQuery.error.response?.status
  }, [historyQuery.error])

  useEffect(() => {
    if (historyStatus !== 500 || historyToastOnceRef.current) {
      return
    }
    historyToastOnceRef.current = true
    toast.error('Errore nel caricare la cronologia')
  }, [historyStatus, toast])

  if (!id) {
    return (
      <section className="stateCard">
        <div className="stateCard__title">Rotta incompleta</div>
        <div className="stateCard__body">Il link non contiene un ticket valido. Torna alla plancia.</div>
        <div className="stateCard__actions">
          <button type="button" className="button" onClick={() => navigate(`${basePath}/tickets`)}>
            Torna ai ticket
          </button>
        </div>
      </section>
    )
  }

  const handleClose = async () => {
    if (!ticket || !etag) {
      toast.error('ETag non disponibile. Ricarica il ticket.')
      return
    }
    const previousStatus = displayStatus
    setOptimisticStatus('Closed')
    try {
      await closeTicket.mutateAsync({ id: ticket.id, etag })
    } catch (err) {
      setOptimisticStatus(previousStatus ?? null)
      if (axios.isAxiosError(err) && err.response?.status === 412) {
        toast.error('Ticket aggiornato, ricarica')
        return
      }
      toast.error('Impossibile chiudere il ticket')
    }
  }

  const handlePriorityChange = async () => {
    if (!ticket || !etag) {
      toast.error('ETag non disponibile. Ricarica il ticket.')
      return
    }
    const previousPriority = displayPriority
    setOptimisticPriority(priorityValue)
    try {
      await setPriority.mutateAsync({ id: ticket.id, dto: { priority: priorityValue }, etag })
    } catch (err) {
      setOptimisticPriority(previousPriority ?? null)
      if (axios.isAxiosError(err) && err.response?.status === 412) {
        toast.error('Ticket aggiornato, ricarica')
        return
      }
      toast.error('Impossibile aggiornare la priorita')
    }
  }

  const handleAssign = async () => {
    if (!ticket || !etag) {
      toast.error('ETag non disponibile. Ricarica il ticket.')
      return
    }
    const trimmed = assigneeValue.trim()
    if (!trimmed) {
      return
    }
    try {
      await assignTicket.mutateAsync({ id: ticket.id, dto: { assigneeUserId: trimmed }, etag })
      toast.success('Assegnazione aggiornata')
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 412) {
        toast.error('Ticket aggiornato, ricarica')
        return
      }
      toast.error('Impossibile assegnare il ticket')
    }
  }

  if (isLoading) {
    return <DetailSkeleton />
  }

  if (isForbidden) {
    return (
      <section className="stateCard">
        <div className="stateCard__title">Accesso negato</div>
        <div className="stateCard__body">Non hai i permessi per vedere questo ticket.</div>
        <div className="stateCard__actions">
          <button type="button" className="button" onClick={() => navigate(`${basePath}/tickets`)}>
            Torna ai ticket
          </button>
        </div>
      </section>
    )
  }

  if (isNotFound) {
    return (
      <section className="stateCard">
        <div className="stateCard__title">Ticket non trovato</div>
        <div className="stateCard__body">Il ticket richiesto non esiste o e stato rimosso.</div>
        <div className="stateCard__actions">
          <button type="button" className="button" onClick={() => navigate(`${basePath}/tickets`)}>
            Torna ai ticket
          </button>
        </div>
      </section>
    )
  }

  if (!ticket && !error) {
    return <DetailSkeleton />
  }

  if (error && !isUnauthorized) {
    return (
      <section className="stateCard stateCard--error">
        <div className="stateCard__title">Interferenza sul canale</div>
        <div className="stateCard__body">Non siamo riusciti a caricare il ticket. Riprova o torna alla lista.</div>
        <div className="stateCard__actions">
          <button type="button" className="button" onClick={() => refetch()}>
            Riprova
          </button>
          <button type="button" className="button" onClick={() => navigate(`${basePath}/tickets`)}>
            Torna ai ticket
          </button>
        </div>
      </section>
    )
  }

  if (!ticket) {
    return <DetailSkeleton />
  }

  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <header className="card" style={{ display: 'grid', gap: 12 }}>
        <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 12 }}>
          <h1 style={{ margin: 0 }}>{ticket.title}</h1>
          {displayStatus ? <Badge label={displayStatus} tone={getStatusTone(displayStatus)} /> : null}
          <span
            className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${getPriorityBadgeClass(
              displayPriority,
            )}`}
          >
            {getPriorityLabel(displayPriority)}
          </span>
        </div>
        <p className="muted" style={{ margin: 0 }}>
          ID: {ticket.id}
        </p>
      </header>

      <nav className="card" style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        <button
          type="button"
          className="button"
          onClick={() => setActiveTab('details')}
          disabled={activeTab === 'details'}
        >
          Dettaglio
        </button>
        <button
          type="button"
          className="button"
          onClick={() => setActiveTab('notes')}
          disabled={activeTab === 'notes'}
        >
          Note
        </button>
        <button
          type="button"
          className="button"
          onClick={() => setActiveTab('history')}
          disabled={activeTab === 'history'}
        >
          History
        </button>
      </nav>

      {activeTab === 'details' ? (
        <section className="card" style={{ display: 'grid', gap: 12 }}>
          <div style={{ display: 'grid', gap: 6 }}>
            <span className="muted">Descrizione</span>
            <p style={{ margin: 0 }}>{ticket.description || '—'}</p>
          </div>
          {ticket.attachments && ticket.attachments.length > 0 ? (
            <div style={{ display: 'grid', gap: 6 }}>
              <span className="muted">Allegati</span>
              <AttachmentGrid urls={ticket.attachments} />
            </div>
          ) : null}
          <div style={{ display: 'grid', gap: 6 }}>
            <span className="muted">Creato il</span>
            <p style={{ margin: 0 }}>{formatDateTime(ticket.createdAt)}</p>
          </div>
          <div style={{ display: 'grid', gap: 6 }}>
            <span className="muted">Creato da</span>
            <p style={{ margin: 0 }}>{ticket.createdByUserId}</p>
          </div>
          <div style={{ display: 'grid', gap: 6 }}>
            <span className="muted">Assegnato a</span>
            <p style={{ margin: 0 }}>{ticket.assigneeUserId ?? '—'}</p>
          </div>

          {(canClose || canChangePriority || canAssign) ? (
            <div style={{ display: 'grid', gap: 12, borderTop: '1px solid rgba(148, 163, 184, 0.2)', paddingTop: 12 }}>
              {canClose ? (
                <div style={{ display: 'flex', alignItems: 'center', gap: 12, flexWrap: 'wrap' }}>
                  <button
                    type="button"
                    className="button"
                    onClick={handleClose}
                    disabled={!canClose || closeTicket.isPending}
                  >
                    {closeTicket.isPending ? 'Chiusura…' : 'Close ticket'}
                  </button>
                  {isClosed ? <span className="muted">Ticket chiuso</span> : null}
                </div>
              ) : null}

              {canChangePriority ? (
                <div style={{ display: 'grid', gap: 8 }}>
                  <span className="muted">Priorita</span>
                  <div style={{ display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
                    <select
                      className="select"
                      value={priorityValue}
                      onChange={(event) => setPriorityValue(event.target.value as TicketPriority)}
                      disabled={!canChangePriority}
                    >
                      <option value="VeryLow">{ticketPriorityLabels.VeryLow}</option>
                      <option value="Low">{ticketPriorityLabels.Low}</option>
                      <option value="Medium">{ticketPriorityLabels.Medium}</option>
                      <option value="High">{ticketPriorityLabels.High}</option>
                      <option value="VeryHigh">{ticketPriorityLabels.VeryHigh}</option>
                    </select>
                    <button
                      type="button"
                      className="button"
                      onClick={handlePriorityChange}
                      disabled={!canChangePriority || setPriority.isPending}
                    >
                      {setPriority.isPending ? 'Salvataggio…' : 'Change priority'}
                    </button>
                  </div>
                </div>
              ) : null}

              {canAssign ? (
                <div style={{ display: 'grid', gap: 8 }}>
                  <span className="muted">Assegna utente</span>
                  <div style={{ display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' }}>
                    <div style={{ minWidth: 240 }}>
                      <Input
                        value={assigneeValue}
                        onChange={(event) => setAssigneeValue(event.target.value)}
                        placeholder="User ID"
                        disabled={!canAssign}
                      />
                    </div>
                    <button
                      type="button"
                      className="button"
                      onClick={handleAssign}
                      disabled={!canAssign || assignTicket.isPending || !assigneeValue.trim()}
                    >
                      {assignTicket.isPending ? 'Assegnazione…' : 'Assign user'}
                    </button>
                  </div>
                </div>
              ) : null}
            </div>
          ) : null}
        </section>
      ) : null}

      {activeTab === 'notes' ? (
        <section className="card" style={{ display: 'grid', gap: 12 }}>
          <h2 style={{ margin: 0 }}>Note</h2>
          <TicketComments ticketId={ticket.id} etag={etag ?? ''} />
        </section>
      ) : null}

      {activeTab === 'history' && historyStatus !== 404 ? (
        <section className="card" style={{ display: 'grid', gap: 12 }}>
          <h2 style={{ margin: 0 }}>History</h2>
          {historyQuery.isLoading ? <p>Allineamento cronologia…</p> : null}
          {historyStatus === 403 ? (
            <div className="stateCard" style={{ padding: 12 }}>
              <div className="stateCard__title">Cronologia non disponibile</div>
              <div className="stateCard__body">Non hai i permessi per vedere la cronologia.</div>
            </div>
          ) : null}
          {historyQuery.error && historyStatus !== 403 ? (
            <div className="stateCard stateCard--error">
              <div className="stateCard__title">Cronologia non disponibile</div>
              <div className="stateCard__body">Il feed eventi non e raggiungibile in questo momento.</div>
              <div className="stateCard__actions">
                <button type="button" className="button" onClick={() => historyQuery.refetch()}>
                  Riprova
                </button>
              </div>
            </div>
          ) : null}
          {isHistoryEmpty ? (
            <div className="stateCard">
              <div className="stateCard__title">Nessun evento registrato</div>
              <div className="stateCard__body">La timeline e pronta. Prova a ricaricare tra poco.</div>
              <div className="stateCard__actions">
                <button type="button" className="button" onClick={() => historyQuery.refetch()}>
                  Aggiorna feed
                </button>
              </div>
            </div>
          ) : null}
          {!historyQuery.isLoading && !historyQuery.error && !isHistoryEmpty ? (
            <HistoryTimeline items={historyItems} />
          ) : null}
        </section>
      ) : null}
    </div>
  )
}

export default TicketDetailPage

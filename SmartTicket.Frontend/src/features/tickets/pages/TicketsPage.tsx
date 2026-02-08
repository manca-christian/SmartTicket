import { useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import useAuth from '../../../auth/useAuth'
import TicketsTable from '../components/TicketsTable'
import TicketFilters from '../components/TicketFilters'
import TicketFormModal from '../components/TicketFormModal'
import { useAllTickets, useMyTickets, useTicketMutations } from '../hooks'
import type { TicketPriority, TicketStatus } from '../types'
import { getErrorMessage } from '../../../api/errors'
import useToast from '../../../shared/hooks/useToast'

type TabKey = 'mine' | 'all'

const TicketsPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { userId, isAdmin } = useAuth()
  const toast = useToast()
  const [tab, setTab] = useState<TabKey>('mine')
  const [assignedOnly, setAssignedOnly] = useState(false)
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [query, setQuery] = useState({
    page: 1,
    pageSize: 10,
    search: undefined as string | undefined,
    status: undefined as TicketStatus | undefined,
    priority: undefined as TicketPriority | undefined,
  })

  const apiQuery = useMemo(
    () => ({
      ...query,
      assigneeUserId: assignedOnly ? userId : undefined,
    }),
    [query, assignedOnly, userId],
  )

  const mineQuery = useMyTickets(apiQuery)
  const allQuery = useAllTickets(apiQuery)
  const { createTicket } = useTicketMutations()

  const data = tab === 'all' && isAdmin ? allQuery.data : mineQuery.data
  const isLoading = tab === 'all' && isAdmin ? allQuery.isLoading : mineQuery.isLoading
  const error = tab === 'all' && isAdmin ? allQuery.error : mineQuery.error
  const errorMessage = error ? getErrorMessage(error) : null
  const refetch = tab === 'all' && isAdmin ? allQuery.refetch : mineQuery.refetch

  const items = data?.items ?? []
  const showTable = !isLoading && !errorMessage
  const isEmpty = showTable && items.length === 0

  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / (query.pageSize || 1)))

  const basePath = location.pathname.startsWith('/admin') ? '/admin' : '/app'

  const handleSelect = (id: string) => {
    navigate(`${basePath}/tickets/${id}`)
  }

  const handleCreateTicket = async (values: {
    title: string
    description: string
    priority: TicketPriority
    attachments?: string[]
  }) => {
    try {
      const created = await createTicket.mutateAsync(values)
      setIsFormOpen(false)
      toast.success('Ticket creato')
      navigate(`${basePath}/tickets/${created.id}`)
    } catch (err) {
      toast.error(getErrorMessage(err))
    }
  }

  const handleResetFilters = () => {
    setAssignedOnly(false)
    setQuery({
      page: 1,
      pageSize: 10,
      search: undefined,
      status: undefined,
      priority: undefined,
    })
  }

  const cardClass = 'rounded-2xl bg-slate-900/40 p-5 ring-1 ring-white/10 backdrop-blur'

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 text-slate-100">
      <div className="mx-auto max-w-6xl px-6 py-10">
        <div className="flex items-start justify-between gap-6">
          <div>
            <h1 className="text-2xl font-semibold">Ticket</h1>
            <p className="mt-2 text-sm text-slate-400">Gestisci richieste, assegnazioni e priorita.</p>
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              className="rounded-xl border border-sky-400/40 bg-sky-500/20 px-4 py-2 text-sm font-semibold text-sky-100 transition hover:border-sky-300 hover:bg-sky-500/30 focus:outline-none focus:ring-2 focus:ring-sky-400/40"
              onClick={() => {
                setIsFormOpen(true)
              }}
            >
              Nuovo ticket
            </button>
            <button
              type="button"
              className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-4 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/30"
              onClick={handleResetFilters}
            >
              Reset filtri
            </button>
          </div>
        </div>

        <div className="mt-10 grid grid-cols-1 gap-6 lg:grid-cols-4">
          <aside className="lg:col-span-1">
            <TicketFilters
              search={query.search ?? ''}
              onSearchChange={(value) => setQuery((prev) => ({ ...prev, search: value || undefined, page: 1 }))}
              status={query.status}
              onStatusChange={(value) =>
                setQuery((prev) => ({ ...prev, status: value && value !== 'all' ? value : undefined, page: 1 }))
              }
              priority={query.priority}
              onPriorityChange={(value) =>
                setQuery((prev) => ({ ...prev, priority: value && value !== 'all' ? value : undefined, page: 1 }))
              }
              pageSize={query.pageSize ?? 10}
              onPageSizeChange={(value) => setQuery((prev) => ({ ...prev, pageSize: value, page: 1 }))}
              assignedOnly={assignedOnly}
              onAssignedOnlyChange={(value) => {
                setAssignedOnly(value)
                setQuery((prev) => ({ ...prev, page: 1 }))
              }}
              onReset={handleResetFilters}
            />
          </aside>

          <main className="lg:col-span-3">
            <div className={`${cardClass} grid gap-5`}>
              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  className={`rounded-xl border px-4 py-2 text-sm font-semibold transition focus:outline-none focus:ring-2 focus:ring-sky-400/40 ${
                    tab === 'mine'
                      ? 'border-sky-400/50 bg-sky-500/20 text-sky-100'
                      : 'border-slate-700/60 bg-slate-900/60 text-slate-300 hover:border-slate-400/70'
                  }`}
                  onClick={() => setTab('mine')}
                  disabled={tab === 'mine'}
                >
                  Mine
                </button>
                {isAdmin ? (
                  <button
                    type="button"
                    className={`rounded-xl border px-4 py-2 text-sm font-semibold transition focus:outline-none focus:ring-2 focus:ring-sky-400/40 ${
                      tab === 'all'
                        ? 'border-sky-400/50 bg-sky-500/20 text-sky-100'
                        : 'border-slate-700/60 bg-slate-900/60 text-slate-300 hover:border-slate-400/70'
                    }`}
                    onClick={() => setTab('all')}
                    disabled={tab === 'all'}
                  >
                    All
                  </button>
                ) : null}
              </div>

              {isLoading ? (
                <div className={cardClass}>
                  <div className="text-lg font-semibold text-slate-100">Allineamento segnali…</div>
                  <div className="mt-1 text-sm text-slate-400">
                    Stiamo sincronizzando i ticket. Tra poco sei operativo.
                  </div>
                </div>
              ) : null}

              {errorMessage ? (
                <div className={`${cardClass} ring-rose-400/20`}>
                  <div className="text-lg font-semibold text-rose-100">Connessione instabile</div>
                  <div className="mt-1 text-sm text-rose-200/80">
                    Non siamo riusciti a recuperare i ticket. Riprova ora.
                  </div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="rounded-xl border border-rose-300/50 bg-rose-500/20 px-3 py-2 text-sm font-semibold text-rose-100 transition hover:bg-rose-500/30 focus:outline-none focus:ring-2 focus:ring-rose-400/40"
                      onClick={() => refetch()}
                    >
                      Riprova
                    </button>
                    <button
                      type="button"
                      className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/30"
                      onClick={handleResetFilters}
                    >
                      Reset filtri
                    </button>
                  </div>
                </div>
              ) : null}

              {isEmpty ? (
                <div className={cardClass}>
                  <div className="text-lg font-semibold text-slate-100">Nessun ticket in vista</div>
                  <div className="mt-1 text-sm text-slate-400">Crea una nuova richiesta oppure amplia i filtri.</div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="rounded-xl border border-sky-400/40 bg-sky-500/20 px-3 py-2 text-sm font-semibold text-sky-100 transition hover:border-sky-300 hover:bg-sky-500/30 focus:outline-none focus:ring-2 focus:ring-sky-400/40"
                      onClick={() => setIsFormOpen(true)}
                    >
                      Nuovo ticket
                    </button>
                    <button
                      type="button"
                      className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/30"
                      onClick={handleResetFilters}
                    >
                      Reset filtri
                    </button>
                  </div>
                </div>
              ) : null}

              {showTable && !isEmpty ? (
                <TicketsTable
                  items={items}
                  onSelect={handleSelect}
                  className="rounded-2xl bg-slate-950/40 p-4 ring-1 ring-white/5"
                />
              ) : null}

              <div className="flex flex-wrap items-center justify-center gap-3 text-sm text-slate-300">
                <button
                  type="button"
                  className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/30"
                  onClick={() => setQuery((prev) => ({ ...prev, page: Math.max(1, (prev.page ?? 1) - 1) }))}
                  disabled={(query.page ?? 1) <= 1}
                >
                  Prev
                </button>
                <span>
                  Pagina {query.page} di {totalPages}
                </span>
                <button
                  type="button"
                  className="rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/30"
                  onClick={() => setQuery((prev) => ({ ...prev, page: Math.min(totalPages, (prev.page ?? 1) + 1) }))}
                  disabled={(query.page ?? 1) >= totalPages}
                >
                  Next
                </button>
              </div>
            </div>
          </main>
        </div>

        <TicketFormModal
          isOpen={isFormOpen}
          isSubmitting={createTicket.isPending}
          onClose={() => {
            setIsFormOpen(false)
          }}
          onSubmit={handleCreateTicket}
        />
      </div>
    </div>
  )
}

export default TicketsPage

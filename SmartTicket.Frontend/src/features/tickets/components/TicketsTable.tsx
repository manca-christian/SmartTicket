import type { TicketListItemDto } from '../types'
import { ticketPriorityLabels } from '../types'
import { formatDateTime, formatRelativeTime } from '../../../shared/utils/date'

type TicketsTableProps = {
  items: TicketListItemDto[]
  onSelect: (id: string) => void
  className?: string
}

const TicketsTable = ({ items, onSelect, className }: TicketsTableProps) => {
  const getStatusTone = (status: TicketListItemDto['status']) => {
    if (status === 'Closed') {
      return 'bg-emerald-500/20 text-emerald-300 border-emerald-400/40'
    }
    if (status === 'InProgress') {
      return 'bg-amber-500/20 text-amber-200 border-amber-400/40'
    }
    return 'bg-rose-500/20 text-rose-200 border-rose-400/40'
  }

  const getPriorityTone = (priority?: TicketListItemDto['priority'] | null) => {
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

  const getPriorityLabel = (priority?: TicketListItemDto['priority'] | null) => {
    if (!priority) {
      return '—'
    }
    return ticketPriorityLabels[priority]
  }

  return (
    <div className={className}>
      <div className="overflow-x-auto">
        <table className="w-full border-separate border-spacing-y-2 text-sm">
        <thead>
          <tr>
            <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">Title</th>
            <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">Status</th>
            <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">Priority</th>
            <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">Created</th>
            <th className="px-3 py-2 text-left text-xs font-semibold uppercase tracking-wide text-slate-400">Assigned</th>
          </tr>
        </thead>
        <tbody>
          {items.map((ticket) => (
            <tr
              key={ticket.id}
              onClick={() => onSelect(ticket.id)}
              className="group cursor-pointer transition hover:-translate-y-0.5"
            >
              <td className="rounded-l-xl bg-slate-950/40 px-3 py-3 shadow-[0_10px_24px_rgba(2,6,23,0.4)] transition group-hover:bg-slate-900/70">
                <div className="font-semibold text-slate-100">{ticket.title}</div>
                <div className="text-xs text-slate-400">
                  {ticket.id}
                </div>
              </td>
              <td className="bg-slate-950/40 px-3 py-3 transition group-hover:bg-slate-900/70">
                <span
                  className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${getStatusTone(
                    ticket.status,
                  )}`}
                >
                  {ticket.status}
                </span>
              </td>
              <td className="bg-slate-950/40 px-3 py-3 transition group-hover:bg-slate-900/70">
                <span
                  className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${getPriorityTone(
                    ticket.priority,
                  )}`}
                >
                  {getPriorityLabel(ticket.priority)}
                </span>
              </td>
              <td className="bg-slate-950/40 px-3 py-3 text-slate-200 transition group-hover:bg-slate-900/70">
                <span title={formatDateTime(ticket.createdAt)}>
                  {formatRelativeTime(ticket.createdAt)}
                </span>
              </td>
              <td className="rounded-r-xl bg-slate-950/40 px-3 py-3 text-slate-200 transition group-hover:bg-slate-900/70">
                {ticket.assigneeUserId ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
        </table>
      </div>
    </div>
  )
}

export default TicketsTable

export type TicketPriority = 'VeryLow' | 'Low' | 'Medium' | 'High' | 'VeryHigh'

export const ticketPriorityLabels: Record<TicketPriority, string> = {
  VeryLow: 'Molto bassa',
  Low: 'Bassa',
  Medium: 'Media',
  High: 'Alta',
  VeryHigh: 'Molto alta',
}

export type TicketStatus = 'Open' | 'InProgress' | 'Closed'

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export type TicketListItemDto = {
  id: string
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  createdAt: string
  updatedAt: string
  createdByUserId: string
  assigneeUserId: string | null
  dueAt: string | null
  attachments?: string[]
}

export type TicketDetailsDto = {
  id: string
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  createdAt: string
  updatedAt: string
  createdByUserId: string
  assigneeUserId: string | null
  dueAt: string | null
  attachments?: string[]
}

export type CreateTicketDto = {
  title: string
  description: string
  priority: TicketPriority
  attachments?: string[]
}

export type UpdateTicketDto = {
  title: string
  description: string
}

export type AssignTicketDto = {
  assigneeUserId: string
}

export type UpdateTicketPriorityDto = {
  priority: TicketPriority
}

export type UpdateTicketDueDateDto = {
  dueAt: string
}

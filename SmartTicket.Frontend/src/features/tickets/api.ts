import api from '../../api/client'
import { TICKETS } from '../../api/endpoints'
import { getEtagFromResponse, withIfMatch } from '../../api/etag'
import type {
  AssignTicketDto,
  CreateTicketDto,
  PagedResult,
  TicketDetailsDto,
  TicketListItemDto,
  TicketPriority,
  TicketStatus,
  UpdateTicketDueDateDto,
  UpdateTicketDto,
  UpdateTicketPriorityDto,
} from './types'

type TicketQuery = {
  page?: number
  pageSize?: number
  search?: string
  status?: TicketStatus
  priority?: TicketPriority
  assigneeUserId?: string
  createdByUserId?: string
}

const getMine = async (query?: TicketQuery) => {
  const response = await api.get<PagedResult<TicketListItemDto>>(`${TICKETS}/mine`, {
    params: query,
  })
  return response.data
}

const getAll = async (query?: TicketQuery) => {
  const response = await api.get<PagedResult<TicketListItemDto>>(TICKETS, {
    params: query,
  })
  return response.data
}

const create = async (dto: CreateTicketDto) => {
  const response = await api.post<TicketDetailsDto>(TICKETS, dto)
  return response.data
}

const getById = async (id: string) => {
  const url = `${TICKETS}/${id}`
  if (import.meta.env.DEV) {
    console.debug('GET ticket', url)
  }
  const response = await api.get<TicketDetailsDto>(url)
  return {
    data: response.data,
    etag: getEtagFromResponse(response),
  }
}

const update = async (id: string, dto: UpdateTicketDto, etag: string | null) => {
  const response = await api.put<TicketDetailsDto>(`${TICKETS}/${id}`, dto, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const close = async (id: string, etag: string | null) => {
  const response = await api.put<TicketDetailsDto>(`${TICKETS}/${id}/close`, null, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const assign = async (id: string, dto: AssignTicketDto, etag: string | null) => {
  const response = await api.put<TicketDetailsDto>(`${TICKETS}/${id}/assign`, dto, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const setPriority = async (id: string, dto: UpdateTicketPriorityDto, etag: string | null) => {
  const response = await api.put<TicketDetailsDto>(`${TICKETS}/${id}/priority`, dto, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const setDueDate = async (id: string, dto: UpdateTicketDueDateDto, etag: string | null) => {
  const response = await api.put<TicketDetailsDto>(`${TICKETS}/${id}/due-date`, dto, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const clearDueDate = async (id: string, etag: string | null) => {
  const response = await api.delete<TicketDetailsDto>(`${TICKETS}/${id}/due-date`, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const ticketsApi = {
  getMine,
  getAll,
  create,
  getById,
  update,
  close,
  assign,
  setPriority,
  setDueDate,
  clearDueDate,
}

export default ticketsApi
export {
  getMine,
  getAll,
  create,
  getById,
  update,
  close,
  assign,
  setPriority,
  setDueDate,
  clearDueDate,
}

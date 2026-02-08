import { useMemo } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { AxiosError } from 'axios'
import ticketsApi from './api'
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
import useAuth from '../../auth/useAuth'

export type TicketQuery = {
  page?: number
  pageSize?: number
  search?: string
  status?: TicketStatus
  priority?: TicketPriority
  assigneeUserId?: string
  createdByUserId?: string
}

type TicketDetailsResult = {
  data: TicketDetailsDto
  etag: string | null
}

const useMyTickets = (query: TicketQuery) => {
  return useQuery<PagedResult<TicketListItemDto>>({
    queryKey: ['tickets', 'mine', query],
    queryFn: () => ticketsApi.getMine(query),
  })
}

const useAllTickets = (query: TicketQuery) => {
  const { isAdmin } = useAuth()

  return useQuery<PagedResult<TicketListItemDto>>({
    queryKey: ['tickets', 'all', query],
    queryFn: () => ticketsApi.getAll(query),
    enabled: isAdmin,
  })
}

const useTicketDetails = (id?: string) => {
  return useQuery<TicketDetailsResult>({
    queryKey: ['tickets', 'details', id],
    queryFn: () => ticketsApi.getById(id ?? ''),
    enabled: Boolean(id),
    retry: false,
  })
}

const useTicketMutations = () => {
  const queryClient = useQueryClient()

  const invalidateLists = () =>
    queryClient.invalidateQueries({ queryKey: ['tickets'] })

  const handleConcurrencyError = (error: AxiosError, id: string) => {
    const status = error.response?.status
    if (status === 412) {
      window.alert('Il ticket è stato modificato da un altro utente. Ricarico i dati.')
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', id] })
      return true
    }

    if (status === 428) {
      window.alert('Ricarica ticket e riprova')
      return true
    }

    return false
  }

  const updateTicket = useMutation({
    mutationFn: ({ id, dto, etag }: { id: string; dto: UpdateTicketDto; etag: string | null }) =>
      ticketsApi.update(id, dto, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  const closeTicket = useMutation({
    mutationFn: ({ id, etag }: { id: string; etag: string | null }) => ticketsApi.close(id, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  const assignTicket = useMutation({
    mutationFn: ({ id, dto, etag }: { id: string; dto: AssignTicketDto; etag: string | null }) =>
      ticketsApi.assign(id, dto, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  const createTicket = useMutation({
    mutationFn: (dto: CreateTicketDto) => ticketsApi.create(dto),
    onSuccess: () => {
      invalidateLists()
    },
  })

  const setPriority = useMutation({
    mutationFn: ({ id, dto, etag }: { id: string; dto: UpdateTicketPriorityDto; etag: string | null }) =>
      ticketsApi.setPriority(id, dto, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  const setDueDate = useMutation({
    mutationFn: ({ id, dto, etag }: { id: string; dto: UpdateTicketDueDateDto; etag: string | null }) =>
      ticketsApi.setDueDate(id, dto, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  const clearDueDate = useMutation({
    mutationFn: ({ id, etag }: { id: string; etag: string | null }) => ticketsApi.clearDueDate(id, etag),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tickets', 'details', variables.id] })
      invalidateLists()
    },
    onError: (error, variables) => {
      handleConcurrencyError(error as AxiosError, variables.id)
    },
  })

  return useMemo(
    () => ({
      createTicket,
      updateTicket,
      closeTicket,
      assignTicket,
      setPriority,
      setDueDate,
      clearDueDate,
    }),
    [createTicket, updateTicket, closeTicket, assignTicket, setPriority, setDueDate, clearDueDate],
  )
}

export { useMyTickets, useAllTickets, useTicketDetails, useTicketMutations }

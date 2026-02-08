import { useQuery } from '@tanstack/react-query'
import historyApi from './api'
import type { HistoryQuery } from './types'

const useTicketHistory = (ticketId?: string, query?: HistoryQuery) => {
  return useQuery({
    queryKey: ['tickets', ticketId, 'history', query],
    queryFn: () => historyApi.getHistory(ticketId ?? '', query),
    enabled: Boolean(ticketId),
    retry: false,
    refetchOnWindowFocus: false,
  })
}

export { useTicketHistory }

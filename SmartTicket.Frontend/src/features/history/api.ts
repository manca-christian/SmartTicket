import api from '../../api/client'
import type { PagedResult } from '../tickets/types'
import type { HistoryEntryDto, HistoryQuery } from './types'

const getHistory = async (ticketId: string, query?: HistoryQuery) => {
  const response = await api.get<PagedResult<HistoryEntryDto>>(`/api/tickets/${ticketId}/history`, {
    params: query,
  })
  return response.data
}

const historyApi = {
  getHistory,
}

export default historyApi
export { getHistory }

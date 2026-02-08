export type HistoryEntryDto = {
  id: string
  action: string
  createdAt: string
  actorUserId: string
  details?: string
}

export type HistoryQuery = {
  page?: number
  pageSize?: number
}

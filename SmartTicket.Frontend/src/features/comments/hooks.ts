import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { AxiosError } from 'axios'
import commentsApi from './api'
import type { CommentsQuery, CreateCommentDto } from './types'

const useTicketComments = (ticketId?: string, query?: CommentsQuery) => {
  return useQuery({
    queryKey: ['tickets', ticketId, 'comments', query],
    queryFn: () => commentsApi.getComments(ticketId ?? '', query),
    enabled: Boolean(ticketId),
  })
}

const useCreateComment = (ticketId?: string) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ dto, etag }: { dto: CreateCommentDto; etag: string | null }) =>
      commentsApi.createComment(ticketId ?? '', dto, etag),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tickets', ticketId, 'comments'] })
    },
    onError: (error) => {
      const status = (error as AxiosError).response?.status
      if (status === 412) {
        window.alert('Il ticket è stato aggiornato. Ricarico i dati.')
        queryClient.invalidateQueries({ queryKey: ['tickets', 'details', ticketId] })
      }
    },
  })
}

export { useTicketComments, useCreateComment }

import api from '../../api/client'
import { withIfMatch } from '../../api/etag'
import type { PagedResult } from '../tickets/types'
import type { CommentDto, CommentsQuery, CreateCommentDto } from './types'

const getComments = async (ticketId: string, query?: CommentsQuery) => {
  const response = await api.get<PagedResult<CommentDto>>(`/api/tickets/${ticketId}/comments`, {
    params: query,
  })
  return response.data
}

const createComment = async (ticketId: string, dto: CreateCommentDto, etag: string | null) => {
  const response = await api.post<CommentDto>(`/api/tickets/${ticketId}/comments`, dto, {
    headers: withIfMatch(undefined, etag),
  })
  return response.data
}

const commentsApi = {
  getComments,
  createComment,
}

export default commentsApi
export { getComments, createComment }

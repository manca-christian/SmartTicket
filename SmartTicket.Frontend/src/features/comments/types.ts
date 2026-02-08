export type CommentDto = {
  id: string
  text: string
  createdAt: string
  createdByUserId: string
  attachments?: string[]
}

export type CreateCommentDto = {
  text: string
  attachments?: string[]
}

export type CommentsQuery = {
  page?: number
  pageSize?: number
}

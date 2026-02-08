import type { CommentDto } from '../types'
import AttachmentGrid from '../../../shared/components/AttachmentGrid'

type CommentsListProps = {
  items: CommentDto[]
}

const CommentsList = ({ items }: CommentsListProps) => {
  if (!items.length) {
    return <p>Nessun commento.</p>
  }

  return (
    <ul style={{ display: 'grid', gap: 12, paddingLeft: 0, listStyle: 'none' }}>
      {items.map((comment) => (
        <li key={comment.id} style={{ border: '1px solid #e5e7eb', borderRadius: 6, padding: 12 }}>
          <p style={{ margin: 0 }}>{comment.text}</p>
          {comment.attachments && comment.attachments.length > 0 ? (
            <div style={{ marginTop: 8 }}>
              <AttachmentGrid urls={comment.attachments} />
            </div>
          ) : null}
          <div style={{ fontSize: 12, color: '#6b7280', marginTop: 6 }}>
            <span>{comment.createdByUserId}</span> · <span>{comment.createdAt}</span>
          </div>
        </li>
      ))}
    </ul>
  )
}

export default CommentsList

import { useMemo, useRef, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { AxiosError } from 'axios'
import { useTicketComments } from '../hooks'
import commentsApi from '../api'
import useToast from '../../../shared/hooks/useToast'
import Textarea from '../../../shared/components/Textarea'
import AttachmentGrid from '../../../shared/components/AttachmentGrid'
import useImageAttachments from '../../../shared/hooks/useImageAttachments'

type TicketCommentsProps = {
  ticketId: string
  etag: string
}

const MAX_LENGTH = 4000
const PAGE_SIZE = 8

const TicketComments = ({ ticketId, etag }: TicketCommentsProps) => {
  const toast = useToast()
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [text, setText] = useState('')
  const textareaRef = useRef<HTMLTextAreaElement | null>(null)
  const fileInputRef = useRef<HTMLInputElement | null>(null)

  const {
    items: attachmentItems,
    attachments,
    remainingSlots,
    isUploading,
    addFiles,
    removeAttachment,
    resetAttachments,
  } = useImageAttachments({ maxItems: 5 })

  const commentsQuery = useTicketComments(ticketId, { page, pageSize: PAGE_SIZE })

  const createComment = useMutation({
    mutationFn: (payload: { text: string; attachments?: string[] }) =>
      commentsApi.createComment(ticketId, payload, etag || null),
    onSuccess: () => {
      setText('')
      resetAttachments()
      toast.success('Nota aggiunta')
      queryClient.invalidateQueries({ queryKey: ['tickets', ticketId, 'comments'] })
    },
    onError: (error) => {
      const status = (error as AxiosError).response?.status
      if (status === 412) {
        toast.error('Ticket aggiornato, ricarica')
        queryClient.invalidateQueries({ queryKey: ['tickets', 'details', ticketId] })
        return
      }
      toast.error('Impossibile aggiungere la nota')
    },
  })

  const items = commentsQuery.data?.items ?? []
  const totalCount = commentsQuery.data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  const isDisabled = useMemo(() => {
    return createComment.isPending || isUploading || !text.trim() || text.trim().length > MAX_LENGTH || !etag
  }, [createComment.isPending, isUploading, text, etag])

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const trimmed = text.trim()
    if (!trimmed || trimmed.length > MAX_LENGTH) {
      return
    }
    if (!etag) {
      toast.error('ETag non disponibile. Ricarica il ticket.')
      return
    }
    if (isUploading) {
      toast.error('Attendi il completamento degli upload')
      return
    }
    await createComment.mutateAsync({ text: trimmed, attachments })
  }

  const handleFileInputChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) {
      return
    }
    await addFiles(event.target.files)
    event.target.value = ''
  }

  return (
    <div className="ticketNotes">
      <form onSubmit={handleSubmit} className="ticketNotes__form">
        <label className="ticketNotes__label">
          Nota
          <Textarea
            className="ticketNotes__textarea"
            ref={textareaRef}
            rows={4}
            maxLength={MAX_LENGTH}
            value={text}
            onChange={(event) => setText(event.target.value)}
            placeholder="Scrivi una nota"
            disabled={createComment.isPending}
          />
        </label>
        <div className="grid gap-2">
          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              className="rounded-xl border border-slate-700/60 bg-slate-900/70 px-3 py-2 text-sm text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/40 disabled:cursor-not-allowed disabled:opacity-60"
              onClick={() => fileInputRef.current?.click()}
              disabled={remainingSlots === 0 || isUploading}
            >
              Aggiungi screenshot
            </button>
            <span className="text-xs text-slate-400">{attachmentItems.length}/5</span>
            {isUploading ? <span className="text-xs text-slate-400">Upload in corso…</span> : null}
          </div>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/*"
            multiple
            hidden
            onChange={handleFileInputChange}
          />
          {attachmentItems.length > 0 ? (
            <div className="grid grid-cols-3 gap-2 sm:grid-cols-5">
              {attachmentItems.map((item) => (
                <div
                  key={item.id}
                  className="relative overflow-hidden rounded-xl border border-slate-700/60 bg-slate-950/40"
                >
                  <img
                    src={item.url ?? item.previewUrl}
                    alt={item.name}
                    className="h-20 w-full object-cover"
                  />
                  {item.status === 'uploading' ? (
                    <div className="absolute inset-0 flex items-center justify-center bg-slate-950/70 text-xs text-slate-200">
                      {item.progress}%
                    </div>
                  ) : null}
                  {item.status === 'error' ? (
                    <div className="absolute inset-0 flex items-center justify-center bg-rose-500/20 text-xs text-rose-100">
                      Errore
                    </div>
                  ) : null}
                  <button
                    type="button"
                    className="absolute right-1 top-1 rounded-full bg-slate-900/80 px-2 py-0.5 text-xs text-slate-100"
                    onClick={() => removeAttachment(item.id)}
                  >
                    ✕
                  </button>
                </div>
              ))}
            </div>
          ) : null}
        </div>
        <div className="ticketNotes__footer">
          <span className="ticketNotes__counter">
            {text.length}/{MAX_LENGTH}
          </span>
          <button type="submit" className="button" disabled={isDisabled}>
            {createComment.isPending ? 'Invio…' : 'Aggiungi nota'}
          </button>
        </div>
      </form>

      {commentsQuery.isLoading ? <p>Allineamento note…</p> : null}
      {commentsQuery.error ? (
        <div className="stateCard stateCard--error">
          <div className="stateCard__title">Note non disponibili</div>
          <div className="stateCard__body">Il canale note e instabile. Riprova tra un attimo.</div>
          <div className="stateCard__actions">
            <button type="button" className="button" onClick={() => commentsQuery.refetch()}>
              Riprova
            </button>
          </div>
        </div>
      ) : null}

      {!commentsQuery.isLoading && !commentsQuery.error ? (
        <div className="ticketNotes__list" role="list">
          {items.length === 0 ? (
            <div className="stateCard">
              <div className="stateCard__title">Nessuna nota ancora</div>
              <div className="stateCard__body">Lascia un promemoria per chi entra in scena.</div>
              <div className="stateCard__actions">
                <button type="button" className="button" onClick={() => textareaRef.current?.focus()}>
                  Scrivi la prima nota
                </button>
              </div>
            </div>
          ) : null}
          {items.map((comment) => (
            <article key={comment.id} className="card ticketNotes__item" role="listitem">
              <div className="ticketNotes__meta">
                <span className="ticketNotes__author">{comment.createdByUserId}</span>
                <span className="ticketNotes__timestamp">{comment.createdAt}</span>
              </div>
              <p className="ticketNotes__text">{comment.text}</p>
              {comment.attachments && comment.attachments.length > 0 ? (
                <AttachmentGrid urls={comment.attachments} className="mt-3" />
              ) : null}
            </article>
          ))}
        </div>
      ) : null}

      <div className="ticketNotes__pagination">
        <button type="button" className="button" onClick={() => setPage((prev) => Math.max(1, prev - 1))} disabled={page <= 1}>
          Prev
        </button>
        <span className="muted">Pagina {page} di {totalPages}</span>
        <button
          type="button"
          className="button"
          onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
          disabled={page >= totalPages}
        >
          Next
        </button>
      </div>
    </div>
  )
}

export default TicketComments

import { useRef, useState } from 'react'
import useImageAttachments from '../../../shared/hooks/useImageAttachments'

type CommentComposerProps = {
  onSubmit: (text: string, attachments: string[]) => Promise<void> | void
  isSubmitting?: boolean
}

const CommentComposer = ({ onSubmit, isSubmitting }: CommentComposerProps) => {
  const [text, setText] = useState('')
  const [error, setError] = useState<string | null>(null)
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

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!text.trim()) {
      setError('Inserisci un commento.')
      return
    }
    setError(null)
    if (isUploading) {
      setError('Attendi il completamento degli upload.')
      return
    }
    await onSubmit(text.trim(), attachments)
    setText('')
    resetAttachments()
  }

  const handleFileInputChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) {
      return
    }
    await addFiles(event.target.files)
    event.target.value = ''
  }

  return (
    <form onSubmit={handleSubmit} style={{ display: 'grid', gap: 8 }}>
      <label style={{ display: 'grid', gap: 6 }}>
        Commento
        <textarea
          rows={3}
          value={text}
          onChange={(event) => setText(event.target.value)}
          placeholder="Scrivi un commento"
        />
      </label>
      {error ? <p style={{ color: '#dc2626', margin: 0 }}>{error}</p> : null}
      <div style={{ display: 'grid', gap: 8 }}>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, alignItems: 'center' }}>
          <button type="button" onClick={() => fileInputRef.current?.click()} disabled={remainingSlots === 0 || isUploading}>
            Aggiungi screenshot
          </button>
          <span style={{ fontSize: 12, color: '#6b7280' }}>{attachmentItems.length}/5</span>
          {isUploading ? <span style={{ fontSize: 12, color: '#6b7280' }}>Upload in corso…</span> : null}
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
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, minmax(0, 1fr))', gap: 8 }}>
            {attachmentItems.map((item) => (
              <div key={item.id} style={{ position: 'relative', border: '1px solid #e5e7eb', borderRadius: 8, overflow: 'hidden' }}>
                <img src={item.url ?? item.previewUrl} alt={item.name} style={{ width: '100%', height: 72, objectFit: 'cover' }} />
                {item.status === 'uploading' ? (
                  <div style={{ position: 'absolute', inset: 0, background: 'rgba(15, 23, 42, 0.75)', color: '#e2e8f0', fontSize: 12, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    {item.progress}%
                  </div>
                ) : null}
                {item.status === 'error' ? (
                  <div style={{ position: 'absolute', inset: 0, background: 'rgba(248, 113, 113, 0.2)', color: '#fca5a5', fontSize: 12, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    Errore
                  </div>
                ) : null}
                <button
                  type="button"
                  style={{ position: 'absolute', top: 4, right: 4, background: 'rgba(15, 23, 42, 0.85)', color: '#e2e8f0', borderRadius: 999, padding: '2px 6px', fontSize: 12 }}
                  onClick={() => removeAttachment(item.id)}
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        ) : null}
      </div>
      <button type="submit" disabled={isSubmitting || isUploading}>
        {isSubmitting ? 'Invio…' : 'Invia commento'}
      </button>
    </form>
  )
}

export default CommentComposer

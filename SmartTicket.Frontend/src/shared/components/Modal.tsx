import { useId } from 'react'
import type { CSSProperties, ReactNode, MouseEvent } from 'react'
import Button from './Button'
import Card from './Card'

type ModalProps = {
  isOpen: boolean
  title: string
  children: ReactNode
  actions?: ReactNode
  onClose?: () => void
  onOverlayClick?: (event: MouseEvent<HTMLDivElement>) => void
  closeDisabled?: boolean
}

const Modal = ({
  isOpen,
  title,
  children,
  actions,
  onClose,
  onOverlayClick,
  closeDisabled = false,
}: ModalProps) => {
  const titleId = useId()

  if (!isOpen) {
    return null
  }

  const overlayStyles: CSSProperties = {
    position: 'fixed',
    inset: 0,
    background: 'rgba(15, 23, 42, 0.5)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 16,
    zIndex: 50,
  }

  return (
    <div role="dialog" aria-modal="true" aria-labelledby={titleId} style={overlayStyles} onClick={onOverlayClick}>
      <Card style={{ width: '100%', maxWidth: 560, padding: 24 }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12 }}>
          <h2 id={titleId} style={{ margin: 0 }}>
            {title}
          </h2>
          {onClose ? (
            <Button variant="secondary" size="sm" type="button" onClick={onClose} disabled={closeDisabled}>
              Chiudi
            </Button>
          ) : null}
        </div>
        <div style={{ marginTop: 16, display: 'grid', gap: 12 }}>{children}</div>
        {actions ? (
          <div style={{ marginTop: 20, display: 'flex', justifyContent: 'flex-end', gap: 12 }}>{actions}</div>
        ) : null}
      </Card>
    </div>
  )
}

export default Modal

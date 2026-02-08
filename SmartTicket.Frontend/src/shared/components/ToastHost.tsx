import useToast from '../hooks/useToast'

const ToastHost = () => {
  const { toasts, dismiss } = useToast()

  if (!toasts.length) {
    return null
  }

  return (
    <div className="toastHost toastHostBottomRight" aria-live="polite">
      {toasts.map((toast) => {
        const typeClass =
          toast.type === 'success' ? 'toastSuccess' : toast.type === 'error' ? 'toastError' : 'toastInfo'

        return (
          <div
            key={toast.id}
            className={`toast ${typeClass}${toast.isClosing ? ' toast--closing' : ''}`}
            role="status"
          >
          <div className="toast__body">
            {toast.title ? <div className="toast__title">{toast.title}</div> : null}
            <div className="toast__message">{toast.message}</div>
          </div>
          <button type="button" className="toast__close" onClick={() => dismiss(toast.id)} aria-label="Chiudi">
            x
          </button>
          <div className="toastProgress" aria-hidden="true" />
          </div>
        )
      })}
    </div>
  )
}

export default ToastHost

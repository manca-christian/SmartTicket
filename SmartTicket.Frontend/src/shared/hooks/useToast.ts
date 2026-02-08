import { useMemo, useSyncExternalStore } from 'react'

type ToastType = 'success' | 'error' | 'info'

type Toast = {
  id: string
  type: ToastType
  title?: string
  message: string
  createdAt: number
  isClosing?: boolean
}

const TOAST_DURATION_MS = 3500
const TOAST_EXIT_MS = 220

let toastState: Toast[] = []
const listeners = new Set<() => void>()
const autoDismissTimers = new Map<string, number>()
const exitTimers = new Map<string, number>()

const notify = () => {
  listeners.forEach((listener) => listener())
}

const getSnapshot = () => toastState

const subscribe = (listener: () => void) => {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}

const cleanupTimer = (id: string) => {
  const autoTimer = autoDismissTimers.get(id)
  if (autoTimer) {
    window.clearTimeout(autoTimer)
    autoDismissTimers.delete(id)
  }
  const exitTimer = exitTimers.get(id)
  if (exitTimer) {
    window.clearTimeout(exitTimer)
    exitTimers.delete(id)
  }
}

const removeToast = (id: string) => {
  toastState = toastState.filter((toast) => toast.id !== id)
  cleanupTimer(id)
  notify()
}

const beginDismiss = (id: string) => {
  const toast = toastState.find((item) => item.id === id)
  if (!toast || toast.isClosing) {
    return
  }
  toastState = toastState.map((item) => (item.id === id ? { ...item, isClosing: true } : item))
  notify()
  const exitTimer = window.setTimeout(() => removeToast(id), TOAST_EXIT_MS)
  exitTimers.set(id, exitTimer)
}

const addToast = (type: ToastType, message: string, title?: string) => {
  const id = `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`
  const createdAt = Date.now()
  toastState = [...toastState, { id, type, title, message, createdAt }]
  notify()
  const autoTimer = window.setTimeout(() => beginDismiss(id), TOAST_DURATION_MS)
  autoDismissTimers.set(id, autoTimer)
}

const success = (message: string, title?: string) => addToast('success', message, title)
const error = (message: string, title?: string) => addToast('error', message, title)
const info = (message: string, title?: string) => addToast('info', message, title)
const dismiss = (id: string) => beginDismiss(id)

const toast = {
  success,
  error,
  info,
  dismiss,
}

const useToast = () => {
  const toasts = useSyncExternalStore(subscribe, getSnapshot, getSnapshot)

  return useMemo(
    () => ({
      toasts,
      success,
      error,
      info,
      dismiss,
    }),
    [toasts],
  )
}

export type { Toast }
export { toast }
export default useToast

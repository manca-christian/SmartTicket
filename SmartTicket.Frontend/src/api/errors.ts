import axios from 'axios'

type ProblemDetails = {
  status: number
  title: string
  detail: string
  errorCode?: string
}

export const isProblemDetails = (value: unknown): value is ProblemDetails => {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Record<string, unknown>

  if (typeof candidate.status !== 'number') {
    return false
  }

  if (typeof candidate.title !== 'string') {
    return false
  }

  if (typeof candidate.detail !== 'string') {
    return false
  }

  if ('errorCode' in candidate && typeof candidate.errorCode !== 'string') {
    return false
  }

  return true
}

export const getErrorMessage = (err: unknown): string => {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data
    if (isProblemDetails(data)) {
      return data.detail || data.title
    }
  }

  if (err instanceof Error) {
    return err.message
  }

  return 'Si è verificato un errore imprevisto.'
}

const pad2 = (value: number) => String(value).padStart(2, '0')

const toDate = (value: string | number | Date) => {
  const parsed = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return null
  }
  return parsed
}

const formatDateTime = (value: string | number | Date) => {
  const date = toDate(value)
  if (!date) {
    return '—'
  }
  const day = pad2(date.getDate())
  const month = pad2(date.getMonth() + 1)
  const year = date.getFullYear()
  const hours = pad2(date.getHours())
  const minutes = pad2(date.getMinutes())
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

const formatRelativeTime = (value: string | number | Date, nowInput?: Date) => {
  const date = toDate(value)
  if (!date) {
    return '—'
  }
  const now = nowInput ?? new Date()
  const diffMs = now.getTime() - date.getTime()
  const future = diffMs < 0
  const diffSeconds = Math.floor(Math.abs(diffMs) / 1000)

  if (diffSeconds < 60) {
    return future ? 'tra poco' : 'ora'
  }

  const diffMinutes = Math.floor(diffSeconds / 60)
  if (diffMinutes < 60) {
    return future ? `tra ${diffMinutes} min` : `${diffMinutes} min fa`
  }

  const diffHours = Math.floor(diffMinutes / 60)
  if (diffHours < 24) {
    const label = diffHours === 1 ? 'ora' : 'ore'
    return future ? `tra ${diffHours} ${label}` : `${diffHours} ${label} fa`
  }

  const diffDays = Math.floor(diffHours / 24)
  if (diffDays === 1) {
    return future ? 'domani' : 'ieri'
  }
  if (diffDays < 30) {
    return future ? `tra ${diffDays} giorni` : `${diffDays} giorni fa`
  }

  const diffMonths = Math.floor(diffDays / 30)
  if (diffMonths < 12) {
    const label = diffMonths === 1 ? 'mese' : 'mesi'
    return future ? `tra ${diffMonths} ${label}` : `${diffMonths} ${label} fa`
  }

  const diffYears = Math.floor(diffMonths / 12)
  const label = diffYears === 1 ? 'anno' : 'anni'
  return future ? `tra ${diffYears} ${label}` : `${diffYears} ${label} fa`
}

export { formatDateTime, formatRelativeTime }

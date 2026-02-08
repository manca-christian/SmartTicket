type HeaderValue = string | string[] | number | boolean | null | undefined

export const getEtagFromResponse = (res: { headers?: unknown } | null | undefined): string | null => {
  if (!res?.headers) {
    return null
  }

  const headerAccessor = res.headers as { get?: (key: string) => string | null }
  if (typeof headerAccessor.get === 'function') {
    return headerAccessor.get('etag')
  }

  if (typeof res.headers !== 'object') {
    return null
  }

  const recordHeaders = res.headers as Record<string, HeaderValue>
  const headerKey = Object.keys(recordHeaders).find((key) => key.toLowerCase() === 'etag')
  if (!headerKey) {
    return null
  }

  const value = recordHeaders[headerKey]
  if (Array.isArray(value)) {
    return value[0] ?? null
  }

  if (typeof value === 'string') {
    return value
  }

  if (typeof value === 'number' || typeof value === 'boolean') {
    return String(value)
  }

  return null
}

export const withIfMatch = (headers: Record<string, string> | undefined, etag: string | null) => {
  const base: Record<string, string> = { ...(headers ?? {}) }
  if (!etag) {
    return base
  }

  return {
    ...base,
    'If-Match': etag,
  }
}

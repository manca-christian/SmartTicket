import api from './client'

export type UploadImagesResult = {
  urls: string[]
}

type UploadResponse = UploadImagesResult | { url: string } | string[] | string | { urls?: string[] }

const normalizeUploadResponse = (data: UploadResponse): string[] => {
  if (Array.isArray(data)) {
    return data.filter((value): value is string => typeof value === 'string')
  }
  if (typeof data === 'string') {
    return [data]
  }
  if (data && typeof data === 'object') {
    if ('urls' in data && Array.isArray(data.urls)) {
      return data.urls.filter((value): value is string => typeof value === 'string')
    }
    if ('url' in data && typeof data.url === 'string') {
      return [data.url]
    }
  }
  return []
}

const uploadImages = async (files: File[], onProgress?: (progress: number) => void) => {
  const formData = new FormData()
  files.forEach((file) => {
    formData.append('files', file)
  })

  const response = await api.post<UploadResponse>('/api/uploads', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
    onUploadProgress: (event) => {
      if (!onProgress || !event.total) {
        return
      }
      const progress = Math.round((event.loaded / event.total) * 100)
      onProgress(progress)
    },
  })

  return {
    urls: normalizeUploadResponse(response.data),
  }
}

export { uploadImages }

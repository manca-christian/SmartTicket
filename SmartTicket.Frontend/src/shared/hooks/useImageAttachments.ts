import { useMemo, useState } from 'react'
import useToast from './useToast'
import { uploadImages } from '../../api/uploads'

type AttachmentItem = {
  id: string
  previewUrl: string
  url?: string
  status: 'uploading' | 'ready' | 'error'
  progress: number
  name: string
}

type UseImageAttachmentsOptions = {
  maxItems?: number
}

const createId = () => {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID()
  }
  return `${Date.now()}-${Math.random()}`
}

const revokePreview = (previewUrl: string) => {
  if (previewUrl.startsWith('blob:')) {
    URL.revokeObjectURL(previewUrl)
  }
}

const useImageAttachments = (options?: UseImageAttachmentsOptions) => {
  const toast = useToast()
  const maxItems = options?.maxItems ?? 5
  const [items, setItems] = useState<AttachmentItem[]>([])

  const remainingSlots = Math.max(0, maxItems - items.length)

  const attachments = useMemo(
    () => items.filter((item) => item.status === 'ready' && item.url).map((item) => item.url as string),
    [items],
  )

  const isUploading = items.some((item) => item.status === 'uploading')

  const addFiles = async (files: FileList | File[]) => {
    const list = Array.from(files).filter((file) => file.type.startsWith('image/'))
    if (list.length === 0 || remainingSlots === 0) {
      return
    }

    const accepted = list.slice(0, remainingSlots)
    const newItems = accepted.map((file) => ({
      id: createId(),
      previewUrl: URL.createObjectURL(file),
      status: 'uploading' as const,
      progress: 0,
      name: file.name,
    }))

    const itemIds = newItems.map((item) => item.id)

    setItems((prev) => [...prev, ...newItems])

    try {
      const { urls } = await uploadImages(accepted, (progress) => {
        setItems((prev) =>
          prev.map((item) => (itemIds.includes(item.id) ? { ...item, progress } : item)),
        )
      })

      setItems((prev) =>
        prev.map((item) => {
          const itemIndex = itemIds.indexOf(item.id)
          if (itemIndex === -1) {
            return item
          }
          const url = urls[itemIndex]
          if (!url) {
            return { ...item, status: 'error', progress: 0 }
          }
          return { ...item, url, status: 'ready', progress: 100 }
        }),
      )

      if (urls.length < accepted.length) {
        toast.error('Caricamento parziale degli screenshot')
      }
    } catch (error) {
      setItems((prev) =>
        prev.map((item) => (itemIds.includes(item.id) ? { ...item, status: 'error', progress: 0 } : item)),
      )
      toast.error('Upload screenshot fallito')
    }
  }

  const removeAttachment = (id: string) => {
    setItems((prev) => {
      const target = prev.find((item) => item.id === id)
      if (target) {
        revokePreview(target.previewUrl)
      }
      return prev.filter((item) => item.id !== id)
    })
  }

  const resetAttachments = () => {
    setItems((prev) => {
      prev.forEach((item) => revokePreview(item.previewUrl))
      return []
    })
  }

  return {
    items,
    attachments,
    remainingSlots,
    isUploading,
    addFiles,
    removeAttachment,
    resetAttachments,
  }
}

export type { AttachmentItem }
export default useImageAttachments

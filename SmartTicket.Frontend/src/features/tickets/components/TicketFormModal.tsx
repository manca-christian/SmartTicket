import { Fragment, useEffect, useMemo, useRef, useState } from 'react'
import { Dialog, Listbox, Transition } from '@headlessui/react'
import type { TicketPriority } from '../types'
import { ticketPriorityLabels } from '../types'
import useToast from '../../../shared/hooks/useToast'
import useImageAttachments from '../../../shared/hooks/useImageAttachments'

export type TicketFormValues = {
  title: string
  description: string
  priority: TicketPriority
  attachments?: string[]
}

type TicketFormModalProps = {
  isOpen: boolean
  isSubmitting?: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: TicketFormValues) => void
}

const TicketFormModal = ({
  isOpen,
  isSubmitting = false,
  errorMessage = null,
  onClose,
  onSubmit,
}: TicketFormModalProps) => {
  const toast = useToast()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState<TicketPriority>('Medium')
  const [touched, setTouched] = useState({ title: false, description: false })
  const [submitAttempted, setSubmitAttempted] = useState(false)
  const titleRef = useRef<HTMLInputElement | null>(null)
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

  const trimmedTitle = title.trim()
  const trimmedDescription = description.trim()

  const titleError = useMemo(() => {
    if (!trimmedTitle) {
      return 'Il titolo è obbligatorio.'
    }
    if (trimmedTitle.length < 3) {
      return 'Il titolo deve avere almeno 3 caratteri.'
    }
    if (trimmedTitle.length > 120) {
      return 'Il titolo non può superare 120 caratteri.'
    }
    return null
  }, [trimmedTitle])

  const descriptionError = useMemo(() => {
    if (!trimmedDescription) {
      return 'La descrizione è obbligatoria.'
    }
    if (trimmedDescription.length < 10) {
      return 'La descrizione deve avere almeno 10 caratteri.'
    }
    if (trimmedDescription.length > 4000) {
      return 'La descrizione non può superare 4000 caratteri.'
    }
    return null
  }, [trimmedDescription])

  const showTitleError = (touched.title || submitAttempted) && titleError
  const showDescriptionError = (touched.description || submitAttempted) && descriptionError
  const isInvalid = Boolean(titleError || descriptionError)

  useEffect(() => {
    if (!isOpen) {
      return
    }

    setTitle('')
    setDescription('')
    setPriority('Medium')
    setTouched({ title: false, description: false })
    setSubmitAttempted(false)
    resetAttachments()

    const focusTimer = window.setTimeout(() => {
      titleRef.current?.focus()
    }, 0)

    return () => {
      window.clearTimeout(focusTimer)
    }
  }, [isOpen])

  if (!isOpen) {
    return null
  }

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (isInvalid) {
      setSubmitAttempted(true)
      return
    }
    if (isUploading) {
      toast.error('Attendi il completamento degli upload')
      return
    }
    onSubmit({ title: trimmedTitle, description: trimmedDescription, priority, attachments })
  }

  const handleFileInputChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files) {
      return
    }
    await addFiles(event.target.files)
    event.target.value = ''
  }

  const priorityOptions: Array<{ label: string; value: TicketPriority }> = [
    { label: ticketPriorityLabels.VeryLow, value: 'VeryLow' },
    { label: ticketPriorityLabels.Low, value: 'Low' },
    { label: ticketPriorityLabels.Medium, value: 'Medium' },
    { label: ticketPriorityLabels.High, value: 'High' },
    { label: ticketPriorityLabels.VeryHigh, value: 'VeryHigh' },
  ]

  const selectedPriority = priorityOptions.find((option) => option.value === priority) ?? priorityOptions[2]

  const handleDialogClose = () => {
    if (!isSubmitting) {
      onClose()
    }
  }

  return (
    <Transition appear show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-50" onClose={handleDialogClose} initialFocus={titleRef}>
        <Transition.Child
          as={Fragment}
          enter="transition-opacity duration-200"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="transition-opacity duration-150"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-slate-950/70 backdrop-blur-sm" aria-hidden="true" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4">
            <Transition.Child
              as={Fragment}
              enter="transition duration-200"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="transition duration-150"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Dialog.Panel className="w-full max-w-xl rounded-2xl border border-slate-700/60 bg-slate-900/90 p-6 text-slate-100 shadow-[0_30px_70px_rgba(2,6,23,0.65)] backdrop-blur">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <Dialog.Title className="text-xl font-semibold">Nuovo ticket</Dialog.Title>
                  <button
                    type="button"
                    className="rounded-xl border border-slate-700/60 bg-slate-900/70 px-3 py-1.5 text-sm text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/40"
                    onClick={handleDialogClose}
                    disabled={isSubmitting}
                  >
                    Chiudi
                  </button>
                </div>

                <form id="ticket-create-form" onSubmit={handleSubmit} className="mt-5 grid gap-4">
                  <label className="grid gap-2 text-sm text-slate-200">
                    Titolo
                    <input
                      type="text"
                      required
                      value={title}
                      onChange={(event) => setTitle(event.target.value)}
                      onBlur={() => setTouched((prev) => ({ ...prev, title: true }))}
                      minLength={3}
                      maxLength={120}
                      ref={titleRef}
                      disabled={isSubmitting}
                      className="w-full rounded-xl border border-slate-600/60 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-sky-400/80 focus:ring-2 focus:ring-sky-400/30"
                    />
                  </label>
                  {showTitleError ? <p className="text-sm text-rose-300">{titleError}</p> : null}

                  <label className="grid gap-2 text-sm text-slate-200">
                    Descrizione
                    <textarea
                      required
                      rows={4}
                      value={description}
                      onChange={(event) => setDescription(event.target.value)}
                      onBlur={() => setTouched((prev) => ({ ...prev, description: true }))}
                      minLength={10}
                      maxLength={4000}
                      disabled={isSubmitting}
                      className="w-full rounded-xl border border-slate-600/60 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-sky-400/80 focus:ring-2 focus:ring-sky-400/30"
                    />
                  </label>
                  {showDescriptionError ? <p className="text-sm text-rose-300">{descriptionError}</p> : null}

                  <div className="grid gap-2 text-sm text-slate-200">
                    <span>Priorita</span>
                    <Listbox value={selectedPriority} onChange={(option) => setPriority(option.value)}>
                      <div className="relative">
                        <Listbox.Button className="flex w-full appearance-none items-center justify-between rounded-xl border border-slate-600/60 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 shadow-[0_8px_20px_rgba(2,6,23,0.35)] transition hover:border-slate-400/80 focus:outline-none focus:ring-2 focus:ring-sky-400/30">
                          <span>{selectedPriority.label}</span>
                          <svg viewBox="0 0 20 20" className="h-4 w-4 text-slate-300" aria-hidden="true">
                            <path d="M5 7l5 5 5-5" fill="none" stroke="currentColor" strokeWidth="1.6" />
                          </svg>
                        </Listbox.Button>
                        <Transition
                          as={Fragment}
                          enter="transition ease-out duration-150"
                          enterFrom="opacity-0 scale-95"
                          enterTo="opacity-100 scale-100"
                          leave="transition ease-in duration-100"
                          leaveFrom="opacity-100 scale-100"
                          leaveTo="opacity-0 scale-95"
                        >
                          <Listbox.Options className="absolute z-10 mt-2 w-full rounded-xl border border-slate-700/70 bg-slate-950/95 p-1 text-sm text-slate-100 shadow-[0_20px_40px_rgba(2,6,23,0.6)] outline-none ring-1 ring-slate-700/40 backdrop-blur">
                            {priorityOptions.map((option) => (
                              <Listbox.Option
                                key={option.value}
                                value={option}
                                className={({ active }) =>
                                  `cursor-pointer rounded-lg px-3 py-2 transition ${
                                    active ? 'bg-sky-500/20 text-sky-100' : 'text-slate-200'
                                  }`
                                }
                              >
                                {({ selected }) => (
                                  <div className="flex items-center justify-between">
                                    <span>{option.label}</span>
                                    {selected ? <span className="text-sky-300">✓</span> : null}
                                  </div>
                                )}
                              </Listbox.Option>
                            ))}
                          </Listbox.Options>
                        </Transition>
                      </div>
                    </Listbox>
                  </div>

                  <div className="grid gap-2 text-sm text-slate-200">
                    <span>Screenshot</span>
                    <div className="flex flex-wrap items-center gap-3">
                      <button
                        type="button"
                        className="rounded-xl border border-slate-700/60 bg-slate-900/70 px-3 py-2 text-sm text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/40 disabled:cursor-not-allowed disabled:opacity-60"
                        onClick={() => fileInputRef.current?.click()}
                        disabled={remainingSlots === 0 || isUploading}
                      >
                        Aggiungi screenshot
                      </button>
                      <span className="text-xs text-slate-400">
                        {attachmentItems.length}/5
                      </span>
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

                  {errorMessage ? <p className="text-sm text-rose-300">{errorMessage}</p> : null}
                </form>

                <div className="mt-6 flex flex-wrap justify-end gap-3">
                  <button
                    type="button"
                    className="rounded-xl border border-slate-700/60 bg-slate-900/70 px-4 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-sky-400/40"
                    onClick={handleDialogClose}
                    disabled={isSubmitting}
                  >
                    Annulla
                  </button>
                  <button
                    type="submit"
                    form="ticket-create-form"
                    className="rounded-xl border border-sky-400/40 bg-sky-500/20 px-4 py-2 text-sm font-semibold text-sky-100 transition hover:border-sky-300 hover:bg-sky-500/30 focus:outline-none focus:ring-2 focus:ring-sky-400/40 disabled:cursor-not-allowed disabled:opacity-60"
                    disabled={isSubmitting || isInvalid || isUploading}
                  >
                    {isSubmitting ? 'Creazione…' : 'Crea ticket'}
                  </button>
                </div>
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition>
  )
}

export default TicketFormModal

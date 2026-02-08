import { Listbox, Transition } from '@headlessui/react'
import { Fragment } from 'react'
import type { TicketPriority } from '../types'
import { ticketPriorityLabels } from '../types'

type TicketFiltersProps = {
  search: string
  onSearchChange: (value: string) => void
  status?: string
  onStatusChange: (value: string | undefined) => void
  priority?: TicketPriority
  onPriorityChange: (value: TicketPriority | undefined) => void
  pageSize: number
  onPageSizeChange: (value: number) => void
  assignedOnly: boolean
  onAssignedOnlyChange: (value: boolean) => void
  onReset: () => void
}

type FilterOption<T> = {
  value: T
  label: string
}

type FilterListboxProps<T> = {
  label: string
  value: T
  options: FilterOption<T>[]
  onChange: (value: T) => void
}

const FilterListbox = <T,>({ label, value, options, onChange }: FilterListboxProps<T>) => {
  const selectedLabel = options.find((option) => option.value === value)?.label ?? ''

  return (
    <Listbox value={value} onChange={onChange}>
      <Listbox.Label className="block text-sm text-slate-200">{label}</Listbox.Label>
      <div className="relative mt-2">
        <Listbox.Button className="relative w-full rounded-xl bg-slate-950/50 px-3 py-2 pr-10 text-left text-sm text-slate-100 ring-1 ring-white/10 transition hover:bg-slate-900/60 focus:outline-none focus-visible:ring-2 focus-visible:ring-cyan-400/60">
          <span className="block truncate">{selectedLabel}</span>
          <span className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-slate-400">
            <svg viewBox="0 0 20 20" className="h-4 w-4" fill="currentColor" aria-hidden="true">
              <path
                fillRule="evenodd"
                d="M5.23 7.21a.75.75 0 011.06.02L10 10.94l3.71-3.71a.75.75 0 111.06 1.06l-4.24 4.24a.75.75 0 01-1.06 0L5.21 8.29a.75.75 0 01.02-1.08z"
                clipRule="evenodd"
              />
            </svg>
          </span>
        </Listbox.Button>
        <Transition
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95 -translate-y-1"
          enterTo="opacity-100 scale-100 translate-y-0"
          leave="transition ease-in duration-100"
          leaveFrom="opacity-100 scale-100 translate-y-0"
          leaveTo="opacity-0 scale-95 -translate-y-1"
        >
          <Listbox.Options className="absolute z-10 mt-2 max-h-60 w-full overflow-auto rounded-xl bg-slate-900/70 p-1 text-sm text-slate-100 shadow-[0_18px_40px_rgba(2,6,23,0.55)] ring-1 ring-white/10 backdrop-blur-xl focus:outline-none">
            {options.map((option) => (
              <Listbox.Option
                key={String(option.value)}
                value={option.value}
                className={({ active }) =>
                  `relative cursor-pointer select-none rounded-lg px-3 py-2 transition ${
                    active ? 'bg-cyan-400/15 text-cyan-100' : 'text-slate-100'
                  }`
                }
              >
                {({ selected }) => (
                  <span className={`block truncate ${selected ? 'font-semibold text-cyan-100' : ''}`}>
                    {option.label}
                  </span>
                )}
              </Listbox.Option>
            ))}
          </Listbox.Options>
        </Transition>
      </div>
    </Listbox>
  )
}

const TicketFilters = ({
  search,
  onSearchChange,
  status,
  onStatusChange,
  priority,
  onPriorityChange,
  pageSize,
  onPageSizeChange,
  assignedOnly,
  onAssignedOnlyChange,
  onReset,
}: TicketFiltersProps) => {
  const statusOptions: FilterOption<string | undefined>[] = [
    { value: undefined, label: 'Tutti' },
    { value: 'Open', label: 'Open' },
    { value: 'InProgress', label: 'InProgress' },
    { value: 'Closed', label: 'Closed' },
  ]

  const priorityOptions: FilterOption<TicketPriority | undefined>[] = [
    { value: undefined, label: 'Tutte' },
    { value: 'VeryLow', label: ticketPriorityLabels.VeryLow },
    { value: 'Low', label: ticketPriorityLabels.Low },
    { value: 'Medium', label: ticketPriorityLabels.Medium },
    { value: 'High', label: ticketPriorityLabels.High },
    { value: 'VeryHigh', label: ticketPriorityLabels.VeryHigh },
  ]

  const pageSizeOptions: FilterOption<number>[] = [
    { value: 10, label: '10' },
    { value: 20, label: '20' },
    { value: 50, label: '50' },
  ]

  return (
    <div className="rounded-2xl bg-slate-900/40 p-5 ring-1 ring-white/10 backdrop-blur">
      <div className="grid gap-4">
        <label className="grid gap-2 text-sm text-slate-200">
          Search
          <input
            className="w-full rounded-xl bg-slate-950/40 px-3 py-2 text-sm text-slate-100 ring-1 ring-white/10 focus:outline-none focus:ring-2 focus:ring-cyan-400/40"
            type="search"
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
          />
        </label>

        <div className="grid gap-2">
          <FilterListbox
            label="Status"
            value={status}
            options={statusOptions}
            onChange={onStatusChange}
          />
        </div>

        <div className="grid gap-2">
          <FilterListbox
            label="Priority"
            value={priority}
            options={priorityOptions}
            onChange={onPriorityChange}
          />
        </div>

        <div className="grid gap-2">
          <FilterListbox
            label="Page size"
            value={pageSize}
            options={pageSizeOptions}
            onChange={onPageSizeChange}
          />
        </div>
      </div>

      <label className="mt-5 flex items-center gap-2 text-sm text-slate-200">
        <input
          type="checkbox"
          checked={assignedOnly}
          onChange={(event) => onAssignedOnlyChange(event.target.checked)}
          className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-cyan-400 focus:ring-2 focus:ring-cyan-400/40"
        />
        <span>Solo assegnati a me</span>
      </label>

      <button
        type="button"
        className="mt-4 w-full rounded-xl border border-slate-700/60 bg-slate-900/60 px-3 py-2 text-sm font-semibold text-slate-200 transition hover:border-slate-400/70 focus:outline-none focus:ring-2 focus:ring-cyan-400/40"
        onClick={onReset}
      >
        Reset filtri
      </button>
    </div>
  )
}

export default TicketFilters

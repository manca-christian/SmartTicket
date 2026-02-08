import type { HistoryEntryDto } from '../types'

type HistoryTimelineProps = {
  items: HistoryEntryDto[]
}

type EventTone = 'info' | 'success' | 'warn' | 'danger'

const normalize = (value: string) => value.toLowerCase()

const getEventTone = (action: string): EventTone => {
  const label = normalize(action)
  if (label.includes('close')) {
    return 'danger'
  }
  if (label.includes('assign')) {
    return 'info'
  }
  if (label.includes('priority')) {
    return 'warn'
  }
  if (label.includes('create') || label.includes('open')) {
    return 'success'
  }
  return 'info'
}

const getEventIcon = (action: string) => {
  const label = normalize(action)
  if (label.includes('close')) {
    return 'X'
  }
  if (label.includes('assign')) {
    return 'A'
  }
  if (label.includes('priority')) {
    return 'P'
  }
  if (label.includes('create') || label.includes('open')) {
    return 'C'
  }
  return 'E'
}

const HistoryTimeline = ({ items }: HistoryTimelineProps) => {
  if (!items.length) {
    return <p className="muted">Nessun evento storico.</p>
  }

  return (
    <ul className="historyFeed" role="list">
      {items.map((entry) => {
        const tone = getEventTone(entry.action)
        const icon = getEventIcon(entry.action)
        return (
          <li key={entry.id} className="historyFeed__item" role="listitem">
            <div className={`historyFeed__icon historyFeed__icon--${tone}`} aria-hidden="true">
              {icon}
            </div>
            <div className="historyFeed__content">
              <div className="historyFeed__title">{entry.action}</div>
              {entry.details ? <div className="historyFeed__details">{entry.details}</div> : null}
              <div className="historyFeed__meta">
                <span>{entry.actorUserId}</span>
                <span>·</span>
                <span>{entry.createdAt}</span>
              </div>
            </div>
          </li>
        )
      })}
    </ul>
  )
}

export default HistoryTimeline

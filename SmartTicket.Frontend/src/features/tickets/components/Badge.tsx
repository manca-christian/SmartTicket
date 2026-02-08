type BadgeTone = 'success' | 'warn' | 'danger'

type BadgeProps = {
  label: string
  tone: BadgeTone
}

const Badge = ({ label, tone }: BadgeProps) => {
  return <span className={`badge badge--${tone}`}>{label}</span>
}

export default Badge

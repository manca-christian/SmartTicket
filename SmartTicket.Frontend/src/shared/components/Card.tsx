import type { HTMLAttributes, CSSProperties } from 'react'

type CardProps = HTMLAttributes<HTMLDivElement>

const Card = ({ style, ...props }: CardProps) => {
  const baseStyles: CSSProperties = {
    background: '#ffffff',
    border: '1px solid #e2e8f0',
    borderRadius: 14,
    padding: 20,
    boxShadow: '0 18px 45px rgba(15, 23, 42, 0.08)',
  }

  return <div {...props} style={{ ...baseStyles, ...style }} />
}

export default Card

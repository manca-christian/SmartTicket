import type { ButtonHTMLAttributes, CSSProperties } from 'react'

type ButtonVariant = 'primary' | 'secondary' | 'danger'

type ButtonSize = 'sm' | 'md'

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant
  size?: ButtonSize
}

const variantStyles: Record<ButtonVariant, CSSProperties> = {
  primary: {
    background: '#0f172a',
    color: '#f8fafc',
    border: '1px solid #0f172a',
  },
  secondary: {
    background: '#ffffff',
    color: '#0f172a',
    border: '1px solid #cbd5f5',
  },
  danger: {
    background: '#dc2626',
    color: '#f8fafc',
    border: '1px solid #dc2626',
  },
}

const sizeStyles: Record<ButtonSize, CSSProperties> = {
  sm: {
    padding: '6px 12px',
    fontSize: 14,
  },
  md: {
    padding: '10px 16px',
    fontSize: 15,
  },
}

const Button = ({ variant = 'primary', size = 'md', style, disabled, ...props }: ButtonProps) => {
  const baseStyles: CSSProperties = {
    borderRadius: 10,
    fontWeight: 600,
    lineHeight: 1.2,
    transition: 'transform 0.08s ease, box-shadow 0.2s ease',
    boxShadow: '0 6px 16px rgba(15, 23, 42, 0.08)',
    cursor: disabled ? 'not-allowed' : 'pointer',
    opacity: disabled ? 0.6 : 1,
  }

  return (
    <button
      {...props}
      disabled={disabled}
      style={{
        ...baseStyles,
        ...sizeStyles[size],
        ...variantStyles[variant],
        ...style,
      }}
    />
  )
}

export default Button

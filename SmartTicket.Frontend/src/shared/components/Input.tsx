import { forwardRef } from 'react'
import type { InputHTMLAttributes, CSSProperties } from 'react'

type InputProps = InputHTMLAttributes<HTMLInputElement>

const Input = forwardRef<HTMLInputElement, InputProps>(({ style, disabled, ...props }, ref) => {
  const baseStyles: CSSProperties = {
    width: '100%',
    padding: '10px 12px',
    borderRadius: 10,
    border: '1px solid #d8dee9',
    background: disabled ? '#f1f5f9' : '#ffffff',
    color: '#0f172a',
    fontSize: 15,
  }

  return <input {...props} ref={ref} disabled={disabled} style={{ ...baseStyles, ...style }} />
})

Input.displayName = 'Input'

export default Input

const prefersReducedMotion = () => {
  if (typeof window === 'undefined' || !('matchMedia' in window)) {
    return false
  }
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches
}

const haptic = (type: 'light' | 'medium') => {
  if (prefersReducedMotion()) {
    return
  }
  if (typeof navigator === 'undefined' || typeof navigator.vibrate !== 'function') {
    return
  }
  const duration = type === 'light' ? 12 : 22
  navigator.vibrate(duration)
}

const animateClass = (el: HTMLElement | null, className: string, durationMs: number) => {
  if (!el || prefersReducedMotion()) {
    return
  }
  el.classList.remove(className)
  void el.offsetWidth
  el.classList.add(className)
  window.setTimeout(() => {
    el.classList.remove(className)
  }, durationMs)
}

const shake = (el: HTMLElement | null) => {
  animateClass(el, 'shake', 350)
}

const pulse = (el: HTMLElement | null) => {
  animateClass(el, 'pulseGlow', 450)
}

export { haptic, pulse, shake }

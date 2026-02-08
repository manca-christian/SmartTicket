type AttachmentGridProps = {
  urls?: string[]
  className?: string
}

const AttachmentGrid = ({ urls, className }: AttachmentGridProps) => {
  if (!urls || urls.length === 0) {
    return null
  }

  return (
    <div className={`grid grid-cols-2 gap-2 sm:grid-cols-3 ${className ?? ''}`}>
      {urls.map((url, index) => (
        <a
          key={`${url}-${index}`}
          href={url}
          target="_blank"
          rel="noreferrer"
          className="group relative overflow-hidden rounded-xl border border-slate-700/60 bg-slate-950/50 shadow-[0_10px_24px_rgba(2,6,23,0.35)]"
        >
          <img
            src={url}
            alt={`Attachment ${index + 1}`}
            className="h-24 w-full object-cover transition duration-200 group-hover:scale-[1.03]"
            loading="lazy"
          />
        </a>
      ))}
    </div>
  )
}

export default AttachmentGrid

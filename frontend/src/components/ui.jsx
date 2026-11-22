import { useEffect, useRef } from 'react'
import { Link } from 'react-router-dom'
import { money } from '../api.js'

/** Couverture : image si URL ou chemin local (/images/...), sinon dégradé généré à partir de "#couleur1,#couleur2". */
const isImage = (c) => c.startsWith('http') || c.startsWith('data:') || c.startsWith('/')

export function Cover({ book, size = 'md' }) {
  const cover = book.cover ?? book.bookCover ?? ''
  if (isImage(cover)) {
    return (
      <div className={`cover ${size}`}>
        <img src={cover} alt={book.title ?? book.bookTitle ?? 'Couverture'} loading="lazy" />
        <i className="spine" /><i className="sheen" />
      </div>
    )
  }
  const [a = '#8fa0ff', b = '#5e5ce6'] = cover.split(',')
  return (
    <div className={`cover ${size}`} style={{ background: `linear-gradient(155deg, ${a}, ${b})` }}>
      <span className="cover-title">{book.title ?? book.bookTitle}</span>
      <span className="cover-author">{book.author ?? ''}</span>
      <i className="spine" /><i className="sheen" />
    </div>
  )
}

/** Fond lumineux flouté tiré de la couverture (image ou dégradé). */
export function coverGlow(book) {
  const cover = book?.cover ?? book?.bookCover ?? ''
  if (isImage(cover)) return { backgroundImage: `url("${cover}")` }
  const [a = '#8fa0ff', b = '#5e5ce6'] = cover.split(',')
  return { backgroundImage: `linear-gradient(155deg, ${a}, ${b})` }
}

/** Inclinaison 3D qui suit le curseur (désactivée au tactile et si l'utilisateur réduit les animations). */
export function Tilt({ children }) {
  const ref = useRef(null)
  const onMove = (e) => {
    const el = ref.current
    if (!el || e.pointerType === 'touch' || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const r = el.getBoundingClientRect()
    const x = (e.clientX - r.left) / r.width, y = (e.clientY - r.top) / r.height
    el.classList.add('live')
    el.style.setProperty('--ry', `${(x - 0.5) * 18}deg`)
    el.style.setProperty('--rx', `${(0.5 - y) * 14}deg`)
    el.style.setProperty('--mx', `${x * 100}%`)
    el.style.setProperty('--my', `${y * 100}%`)
  }
  const onLeave = () => {
    const el = ref.current
    if (!el) return
    el.classList.remove('live')
    el.style.setProperty('--ry', '0deg'); el.style.setProperty('--rx', '0deg')
  }
  return <div ref={ref} className="tilt" onPointerMove={onMove} onPointerLeave={onLeave}>{children}</div>
}

export const BrandMark = () => (
  <svg className="brand-mark" viewBox="0 0 32 32" fill="none" aria-hidden="true">
    <defs>
      <linearGradient id="bm" x1="4" y1="3" x2="28" y2="29" gradientUnits="userSpaceOnUse"><stop stopColor="#f6d9a0" /><stop offset="1" stopColor="#c99a4b" /></linearGradient>
    </defs>
    <rect x="1" y="1" width="30" height="30" rx="9" fill="#17171a" stroke="rgba(255,255,255,.14)" />
    <path d="M16 9.5c-2.2-1.5-5-2-8-1.6v13.6c3-.4 5.8.1 8 1.6 2.2-1.5 5-2 8-1.6V7.9c-3-.4-5.8.1-8 1.6Z" fill="url(#bm)" />
    <path d="M16 9.5v13.6" stroke="#17171a" strokeWidth="1.4" />
  </svg>
)

export const SkeletonGrid = ({ n = 10 }) => (
  <div className="grid" aria-busy="true">
    {Array.from({ length: n }, (_, i) => (
      <div key={i} className="book-card"><div className="cover skeleton" /><div className="book-meta"><span className="skeleton" style={{ height: 14, width: '80%', borderRadius: 7 }} /><span className="skeleton" style={{ height: 12, width: '50%', borderRadius: 6, marginTop: 6 }} /></div></div>
    ))}
  </div>
)

export function BookCard({ book }) {
  return (
    <Link to={`/livres/${book.id}`} className="book-card">
      <Cover book={book} />
      <div className="book-meta">
        <strong>{book.title}</strong>
        <span>{book.author}</span>
        <span className={book.isFree ? 'price free' : 'price'}>{book.isFree ? 'Gratuit' : money(book.price)}</span>
      </div>
    </Link>
  )
}

export function Modal({ title, onClose, children, wide }) {
  useEffect(() => {
    const onKey = (e) => e.key === 'Escape' && onClose()
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])
  return (
    <div className="overlay" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div className={`sheet ${wide ? 'wide' : ''}`} role="dialog" aria-modal="true" aria-label={title}>
        <div className="sheet-head">
          <h3>{title}</h3>
          <button className="icon-btn" onClick={onClose} aria-label="Fermer">✕</button>
        </div>
        {children}
      </div>
    </div>
  )
}

export const Spinner = () => <div className="spinner" aria-label="Chargement" />
export const Empty = ({ title, hint, children }) => (
  <div className="empty"><h3>{title}</h3>{hint && <p>{hint}</p>}{children}</div>
)
export const Badge = ({ status }) => {
  const map = {
    Success: ['Réussi', 'green'], Failed: ['Refusé', 'red'], Cancelled: ['Annulé', 'amber'], Pending: ['En attente', 'gray'],
    Published: ['Publié', 'green'], Hidden: ['Masqué', 'amber'], Deleted: ['Supprimé', 'red'],
    Open: ['Ouvert', 'red'], UnderReview: ['En examen', 'amber'], Resolved: ['Résolu', 'green'], Rejected: ['Rejeté', 'gray'],
    Reader: ['Lecteur', 'gray'], Admin: ['Admin', 'blue'], SuperAdmin: ['Super admin', 'purple']
  }
  const [label, color] = map[status] ?? [status, 'gray']
  return <span className={`badge ${color}`}>{label}</span>
}

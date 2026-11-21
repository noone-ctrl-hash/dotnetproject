import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { money } from '../api.js'

/** Couverture : image si URL, sinon dégradé généré à partir de "#couleur1,#couleur2". */
export function Cover({ book, size = 'md' }) {
  const cover = book.cover ?? book.bookCover ?? ''
  if (cover.startsWith('http') || cover.startsWith('data:')) {
    return <div className={`cover ${size}`}><img src={cover} alt="" /></div>
  }
  const [a = '#0a84ff', b = '#5e5ce6'] = cover.split(',')
  return (
    <div className={`cover ${size}`} style={{ background: `linear-gradient(155deg, ${a}, ${b})` }}>
      <span className="cover-title">{book.title ?? book.bookTitle}</span>
      <span className="cover-author">{book.author ?? ''}</span>
    </div>
  )
}

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

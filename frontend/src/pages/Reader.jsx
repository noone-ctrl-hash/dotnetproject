import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api.js'
import { Empty, Spinner } from '../components/ui.jsx'

export default function Reader() {
  const { id } = useParams()
  const [book, setBook] = useState(null)
  const [error, setError] = useState(null)
  const [size, setSize] = useState(19)
  const [serif, setSerif] = useState(false)

  useEffect(() => { api.get(`/books/${id}/read`).then(setBook).catch(setError) }, [id])

  if (error) return (
    <Empty title={error.status === 403 ? 'Ce livre n’est pas encore à vous' : 'Lecture impossible'} hint={error.message}>
      <Link className="btn primary" to={`/livres/${id}`}>Voir la fiche du livre</Link>
    </Empty>
  )
  if (!book) return <Spinner />

  return (
    <div className="reader">
      <div className="reader-bar">
        <Link to={`/livres/${id}`} className="back">‹ {book.title}</Link>
        <div className="row gap">
          <button className="btn ghost sm" onClick={() => setSize((s) => Math.max(15, s - 1))} aria-label="Réduire le texte">A−</button>
          <button className="btn ghost sm" onClick={() => setSize((s) => Math.min(28, s + 1))} aria-label="Agrandir le texte">A+</button>
          <button className={`btn ghost sm ${serif ? 'on' : ''}`} onClick={() => setSerif(!serif)}>Serif</button>
        </div>
      </div>
      <article className="reader-text" style={{ fontSize: size, fontFamily: serif ? 'ui-serif, Georgia, "New York", serif' : undefined }}>
        <h1>{book.title}</h1>
        <p className="lead">{book.author}</p>
        {book.content.split('\n\n').map((para, i) =>
          /^Chapitre \d/.test(para) ? <h2 key={i}>{para}</h2> : <p key={i}>{para}</p>)}
      </article>
    </div>
  )
}

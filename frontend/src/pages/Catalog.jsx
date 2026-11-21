import { useEffect, useState } from 'react'
import { api } from '../api.js'
import { BookCard, Empty, Spinner } from '../components/ui.jsx'

export default function Catalog() {
  const [books, setBooks] = useState(null)
  const [cats, setCats] = useState([])
  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [free, setFree] = useState('')
  const [sort, setSort] = useState('')
  const [error, setError] = useState('')

  useEffect(() => { api.get('/categories').then(setCats).catch(() => {}) }, [])
  useEffect(() => {
    const t = setTimeout(() => {
      api.get(`/books${api.qs({ search, categoryId, free, sort })}`).then(setBooks).catch((e) => setError(e.message))
    }, 220)
    return () => clearTimeout(t)
  }, [search, categoryId, free, sort])

  return (
    <>
      <section className="hero">
        <h1>Lisez ce qui vous ressemble.</h1>
        <p>Des romans, des sciences, de l’histoire et de la technologie. Gratuits ou à l’unité, toujours accessibles depuis votre bibliothèque.</p>
        <div className="search">
          <input type="search" placeholder="Rechercher un titre, un auteur…" value={search} onChange={(e) => setSearch(e.target.value)} aria-label="Rechercher un livre" />
        </div>
      </section>

      <div className="filters">
        <div className="chips" role="tablist" aria-label="Catégories">
          <button className={`chip ${categoryId === '' ? 'on' : ''}`} onClick={() => setCategoryId('')}>Tout</button>
          {cats.map((c) => (
            <button key={c.id} className={`chip ${categoryId === String(c.id) ? 'on' : ''}`} onClick={() => setCategoryId(String(c.id))}>{c.name}</button>
          ))}
        </div>
        <div className="row gap">
          <div className="segmented">
            {[['', 'Tous'], ['true', 'Gratuits'], ['false', 'Payants']].map(([v, l]) => (
              <button key={l} className={free === v ? 'on' : ''} onClick={() => setFree(v)}>{l}</button>
            ))}
          </div>
          <select value={sort} onChange={(e) => setSort(e.target.value)} aria-label="Trier">
            <option value="">Plus récents</option>
            <option value="title">Titre A–Z</option>
            <option value="price">Prix croissant</option>
          </select>
        </div>
      </div>

      {error && <div className="error">{error}</div>}
      {!books ? <Spinner /> : books.length === 0 ? (
        <Empty title="Aucun livre trouvé" hint="Essayez un autre mot-clé ou retirez un filtre." />
      ) : (
        <div className="grid">{books.map((b) => <BookCard key={b.id} book={b} />)}</div>
      )}
    </>
  )
}

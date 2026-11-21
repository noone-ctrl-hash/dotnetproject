import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, dateTimeFr, money } from '../api.js'
import { Badge, BookCard, Empty, Spinner } from '../components/ui.jsx'

export default function MyLibrary() {
  const [books, setBooks] = useState(null)
  const [history, setHistory] = useState(null)
  const [tab, setTab] = useState('books')

  useEffect(() => {
    api.get('/books/library').then(setBooks)
    api.get('/purchases/mine').then(setHistory)
  }, [])

  return (
    <>
      <h1 className="title">Ma bibliothèque</h1>
      <div className="segmented mb">
        <button className={tab === 'books' ? 'on' : ''} onClick={() => setTab('books')}>Livres achetés</button>
        <button className={tab === 'history' ? 'on' : ''} onClick={() => setTab('history')}>Historique des achats</button>
      </div>

      {tab === 'books' && (!books ? <Spinner /> : books.length === 0 ? (
        <Empty title="Aucun livre acheté" hint="Les livres que vous achetez apparaissent ici, prêts à lire.">
          <Link className="btn primary" to="/">Parcourir le catalogue</Link>
        </Empty>
      ) : (
        <div className="grid">{books.map((b) => (
          <div key={b.id} className="stack-sm"><BookCard book={b} /><Link className="btn ghost sm" to={`/lire/${b.id}`}>Lire</Link></div>
        ))}</div>
      ))}

      {tab === 'history' && (!history ? <Spinner /> : history.length === 0 ? (
        <Empty title="Aucun achat pour l’instant" hint="Vos paiements réussis, refusés ou annulés seront listés ici." />
      ) : (
        <div className="table-wrap"><table>
          <thead><tr><th>Date</th><th>Livre</th><th>Montant</th><th>Statut</th><th>Référence</th></tr></thead>
          <tbody>{history.map((p) => (
            <tr key={p.id}>
              <td>{dateTimeFr(p.purchasedAt)}</td>
              <td><Link to={`/livres/${p.bookId}`}>{p.bookTitle}</Link></td>
              <td>{money(p.amount)}</td><td><Badge status={p.status} /></td><td><code>{p.paymentReference}</code></td>
            </tr>))}
          </tbody>
        </table></div>
      ))}
    </>
  )
}

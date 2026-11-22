import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api, dateFr, money } from '../api.js'
import { useAuth } from '../auth.jsx'
import { Cover, Empty, Modal, Spinner, Tilt, coverGlow } from '../components/ui.jsx'
import { useToast } from '../components/Toast.jsx'

/** Séquence : Cliquer sur « Acheter » → Purchase(bookId) → interface de paiement → ProcessPayment(bookId) */
function PaymentModal({ book, onClose, onDone }) {
  const toast = useToast()
  const [step, setStep] = useState('form') // form | processing | result
  const [result, setResult] = useState(null)
  const [f, setF] = useState({ cardHolder: '', cardNumber: '4242 4242 4242 4242', expiry: '12/30', cvc: '123' })

  const pay = async (cancelled = false) => {
    setStep('processing')
    try {
      const r = await api.post(`/purchases/${book.id}/pay`, { ...f, cancelled })
      setResult(r); setStep('result')
      if (r.status === 'Success') onDone()
    } catch (e) { toast(e.message, 'error'); setStep('form') }
  }
  const fmtCard = (v) => v.replace(/\D/g, '').slice(0, 16).replace(/(.{4})/g, '$1 ').trim()

  return (
    <Modal title="Paiement" onClose={onClose}>
      {step === 'result' ? (
        <div className="stack center">
          <div className={`big-icon ${result.status === 'Success' ? 'green' : result.status === 'Cancelled' ? 'amber' : 'red'}`}>
            {result.status === 'Success' ? '✓' : result.status === 'Cancelled' ? '–' : '✕'}
          </div>
          <h3>{result.status === 'Success' ? 'Livre acheté avec succès' : result.status === 'Cancelled' ? 'Achat annulé' : 'Paiement refusé'}</h3>
          <p className="muted">{result.message}{result.reference && <><br />Référence : <code>{result.reference}</code></>}</p>
          {result.status === 'Success'
            ? <Link className="btn primary lg" to={`/lire/${book.id}`}>Lire maintenant</Link>
            : <button className="btn primary lg" onClick={() => setStep('form')}>Réessayer</button>}
          <button className="btn ghost" onClick={onClose}>Fermer</button>
        </div>
      ) : (
        <form className="stack" onSubmit={(e) => { e.preventDefault(); pay(false) }}>
          <div className="summary"><span>{book.title}</span><strong>{money(book.price)}</strong></div>
          <label>Titulaire<input required value={f.cardHolder} onChange={(e) => setF({ ...f, cardHolder: e.target.value })} placeholder="Nom sur la carte" /></label>
          <label>Numéro de carte<input required inputMode="numeric" value={f.cardNumber} onChange={(e) => setF({ ...f, cardNumber: fmtCard(e.target.value) })} /></label>
          <div className="row gap">
            <label className="grow">Expiration<input required value={f.expiry} onChange={(e) => setF({ ...f, expiry: e.target.value })} placeholder="MM/AA" /></label>
            <label className="grow">CVC<input required inputMode="numeric" maxLength={4} value={f.cvc} onChange={(e) => setF({ ...f, cvc: e.target.value })} /></label>
          </div>
          <small className="muted">Paiement simulé : aucune somme réelle n’est débitée. La carte 4000 0000 0000 0002 est refusée.</small>
          <button className="btn primary lg" disabled={step === 'processing'}>{step === 'processing' ? 'Traitement…' : `Payer ${money(book.price)}`}</button>
          <button type="button" className="btn ghost" disabled={step === 'processing'} onClick={() => pay(true)}>Annuler le paiement</button>
        </form>
      )}
    </Modal>
  )
}

export default function BookDetails() {
  const { id } = useParams()
  const { user } = useAuth()
  const nav = useNavigate()
  const toast = useToast()
  const [data, setData] = useState(null)
  const [pubs, setPubs] = useState([])
  const [paying, setPaying] = useState(false)
  const [error, setError] = useState('')

  const load = () => api.get(`/books/${id}`).then(setData).catch((e) => setError(e.message))
  useEffect(() => { setData(null); load(); api.get(`/publications?bookId=${id}`).then(setPubs).catch(() => {}) }, [id, user?.id])

  const buy = async () => {
    if (!user) return nav('/connexion', { state: { from: `/livres/${id}` } })
    try {
      const a = await api.post(`/purchases/${id}/authorize`)
      if (!a.authorized) { toast(a.message, 'error'); load(); return }
      setPaying(true)
    } catch (e) { toast(e.message, 'error') }
  }

  if (error) return <Empty title="Livre introuvable" hint={error}><Link className="btn primary" to="/">Retour au catalogue</Link></Empty>
  if (!data) return <Spinner />
  const { book, hasAccess } = data

  return (
    <div className="details">
      <div className="details-glow" style={coverGlow(book)} />
      <Tilt><Cover book={book} size="lg" /></Tilt>
      <div className="details-body">
        <Link to="/" className="back">‹ Catalogue</Link>
        <h1>{book.title}</h1>
        <p className="lead">{book.author}</p>
        <div className="row gap wrap">
          <span className="tag">{book.categoryName}</span>
          <span className="tag">Publié le {dateFr(book.publishedAt)}</span>
          <span className={`tag ${book.isFree ? 'green' : ''}`}>{book.isFree ? 'Gratuit' : money(book.price)}</span>
        </div>
        <p className="desc">{book.description}</p>
        <div className="row gap wrap">
          {hasAccess
            ? <Link className="btn primary lg" to={`/lire/${book.id}`}>{book.isFree ? 'Lire gratuitement' : 'Lire mon livre'}</Link>
            : book.isFree
              ? <Link className="btn primary lg" to={user ? `/lire/${book.id}` : '/connexion'} state={{ from: `/lire/${book.id}` }}>Lire gratuitement</Link>
              : <button className="btn primary lg" onClick={buy}>Acheter — {money(book.price)}</button>}
          {!user && <span className="muted">Connectez-vous pour lire ou acheter.</span>}
        </div>

        <section className="block">
          <div className="row between"><h2>Avis et réflexions</h2><Link to="/publications" className="link">Voir la communauté</Link></div>
          {pubs.length === 0 ? <p className="muted">Aucune publication sur ce livre pour le moment.</p> :
            pubs.slice(0, 3).map((p) => (
              <article key={p.id} className="mini-pub"><strong>{p.title}</strong><span className="muted">par {p.author} · {dateFr(p.publishedAt)}</span><p>{p.content}</p></article>
            ))}
        </section>
      </div>
      {paying && <PaymentModal book={book} onClose={() => setPaying(false)} onDone={load} />}
    </div>
  )
}

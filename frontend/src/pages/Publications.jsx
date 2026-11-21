import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, dateTimeFr } from '../api.js'
import { useAuth } from '../auth.jsx'
import { useToast } from '../components/Toast.jsx'
import { Empty, Modal, Spinner } from '../components/ui.jsx'

function ReportModal({ target, onClose }) {
  const toast = useToast()
  const [reason, setReason] = useState('')
  const reasons = ['Contenu offensant', 'Publicité indésirable', 'Spoiler non signalé', 'Autre']
  const send = async (e) => {
    e.preventDefault()
    try {
      await api.post(`/${target.kind === 'comment' ? 'comments' : 'publications'}/${target.id}/report`, { reason })
      toast('Signalement envoyé. Merci.'); onClose()
    } catch (ex) { toast(ex.message, 'error') }
  }
  return (
    <Modal title="Signaler ce contenu" onClose={onClose}>
      <form className="stack" onSubmit={send}>
        <div className="chips">{reasons.map((r) => <button type="button" key={r} className={`chip ${reason === r ? 'on' : ''}`} onClick={() => setReason(r)}>{r}</button>)}</div>
        <label>Motif<input required minLength={3} value={reason} onChange={(e) => setReason(e.target.value)} placeholder="Précisez le problème" /></label>
        <button className="btn primary">Envoyer le signalement</button>
      </form>
    </Modal>
  )
}

function CommentNode({ c, tree, depth, user, onReply, onReport, onDelete }) {
  const [replying, setReplying] = useState(false)
  const [text, setText] = useState('')
  const kids = tree[c.id] || []
  const submit = async (e) => {
    e.preventDefault()
    if (await onReply(text, c.id)) { setText(''); setReplying(false) }
  }
  return (
    <div className={`comment ${depth > 0 ? 'nested' : ''}`}>
      <div className="row between">
        <strong>{c.author}</strong>
        <span className="muted small">{dateTimeFr(c.commentedAt)}</span>
      </div>
      <p>{c.content}</p>
      <div className="row gap small-actions">
        {user && depth < 4 && <button className="link" onClick={() => setReplying(!replying)}>Répondre</button>}
        {user && c.userId !== user.id && <button className="link" onClick={() => onReport(c.id)}>Signaler</button>}
        {user && (c.userId === user.id || user.role !== 'Reader') && <button className="link danger" onClick={() => onDelete(c.id)}>Supprimer</button>}
      </div>
      {replying && (
        <form className="row gap" onSubmit={submit}>
          <input className="grow" autoFocus required value={text} onChange={(e) => setText(e.target.value)} placeholder={`Répondre à ${c.author}…`} />
          <button className="btn primary sm">Envoyer</button>
        </form>
      )}
      {kids.map((k) => <CommentNode key={k.id} c={k} tree={tree} depth={depth + 1} user={user} onReply={onReply} onReport={onReport} onDelete={onDelete} />)}
    </div>
  )
}

function Comments({ pubId, user, onCount, onReport }) {
  const toast = useToast()
  const [list, setList] = useState(null)
  const [text, setText] = useState('')
  const load = () => api.get(`/publications/${pubId}/comments`).then((l) => { setList(l); onCount(l.length) })
  useEffect(() => { load() }, [pubId])

  const tree = useMemo(() => {
    const t = {}
    ;(list || []).forEach((c) => { (t[c.parentId ?? 'root'] ||= []).push(c) })
    return t
  }, [list])

  const add = async (content, parentId = null) => {
    try { await api.post(`/publications/${pubId}/comments`, { content, parentId }); await load(); return true }
    catch (e) { toast(e.message, 'error'); return false }
  }
  const del = async (id) => { await api.del(`/comments/${id}`); load() }

  if (!list) return <Spinner />
  return (
    <div className="comments">
      {(tree.root || []).map((c) => <CommentNode key={c.id} c={c} tree={tree} depth={0} user={user} onReply={add} onReport={onReport} onDelete={del} />)}
      {list.length === 0 && <p className="muted">Soyez le premier à commenter.</p>}
      {user ? (
        <form className="row gap" onSubmit={async (e) => { e.preventDefault(); if (await add(text)) setText('') }}>
          <input className="grow" required value={text} onChange={(e) => setText(e.target.value)} placeholder="Ajouter un commentaire…" />
          <button className="btn primary sm">Commenter</button>
        </form>
      ) : <p className="muted"><Link to="/connexion">Connectez-vous</Link> pour commenter.</p>}
    </div>
  )
}

function Editor({ initial, books, onClose, onSaved }) {
  const toast = useToast()
  const [f, setF] = useState(initial || { title: '', content: '', type: 'Reflexion', bookId: '' })
  const save = async (e) => {
    e.preventDefault()
    const body = { title: f.title, content: f.content, type: f.type, bookId: f.bookId ? Number(f.bookId) : null }
    try {
      initial?.id ? await api.put(`/publications/${initial.id}`, body) : await api.post('/publications', body)
      toast(initial?.id ? 'Publication modifiée' : 'Publication publiée'); onSaved()
    } catch (ex) { toast(ex.message, 'error') }
  }
  return (
    <Modal title={initial?.id ? 'Modifier la publication' : 'Nouvelle publication'} onClose={onClose}>
      <form className="stack" onSubmit={save}>
        <div className="segmented">
          {[['Reflexion', 'Réflexion'], ['Avis', 'Avis sur un livre']].map(([v, l]) => (
            <button type="button" key={v} className={f.type === v ? 'on' : ''} onClick={() => setF({ ...f, type: v })}>{l}</button>
          ))}
        </div>
        <label>Titre<input required minLength={3} value={f.title} onChange={(e) => setF({ ...f, title: e.target.value })} /></label>
        {!initial?.id && (
          <label>Livre concerné (facultatif)
            <select value={f.bookId ?? ''} onChange={(e) => setF({ ...f, bookId: e.target.value })}>
              <option value="">Aucun</option>{books.map((b) => <option key={b.id} value={b.id}>{b.title}</option>)}
            </select>
          </label>
        )}
        <label>Votre texte<textarea required minLength={3} rows={6} value={f.content} onChange={(e) => setF({ ...f, content: e.target.value })} /></label>
        <button className="btn primary lg">{initial?.id ? 'Enregistrer' : 'Publier'}</button>
      </form>
    </Modal>
  )
}

export default function Publications() {
  const { user } = useAuth()
  const toast = useToast()
  const [items, setItems] = useState(null)
  const [books, setBooks] = useState([])
  const [type, setType] = useState('')
  const [mine, setMine] = useState(false)
  const [open, setOpen] = useState(null)
  const [counts, setCounts] = useState({})
  const [editor, setEditor] = useState(null)
  const [report, setReport] = useState(null)

  const load = () => api.get(`/publications${api.qs({ type, mine: mine || '' })}`).then((l) => {
    setItems(l); setCounts(Object.fromEntries(l.map((p) => [p.id, p.commentCount])))
  })
  useEffect(() => { load() }, [type, mine, user?.id])
  useEffect(() => { api.get('/books').then(setBooks).catch(() => {}) }, [])

  const remove = async (id) => { await api.del(`/publications/${id}`); toast('Publication supprimée'); load() }

  return (
    <>
      <div className="row between wrap">
        <h1 className="title">Communauté</h1>
        {user ? <button className="btn primary" onClick={() => setEditor({})}>Publier</button> : <Link to="/connexion" className="btn ghost">Se connecter pour publier</Link>}
      </div>
      <div className="row gap wrap mb">
        <div className="segmented">
          {[['', 'Tout'], ['Reflexion', 'Réflexions'], ['Avis', 'Avis']].map(([v, l]) => <button key={l} className={type === v ? 'on' : ''} onClick={() => setType(v)}>{l}</button>)}
        </div>
        {user && <label className="switch"><input type="checkbox" checked={mine} onChange={(e) => setMine(e.target.checked)} />Mes publications</label>}
      </div>

      {!items ? <Spinner /> : items.length === 0 ? (
        <Empty title="Rien à lire ici pour l’instant" hint="Partagez une réflexion ou votre avis sur un livre." />
      ) : (
        <div className="feed">
          {items.map((p) => (
            <article key={p.id} className="card pub">
              <div className="row between wrap">
                <span className={`tag ${p.type === 'Avis' ? 'blue' : ''}`}>{p.type === 'Avis' ? 'Avis' : 'Réflexion'}</span>
                <span className="muted small">{p.author} · {dateTimeFr(p.publishedAt)}</span>
              </div>
              <h3>{p.title}</h3>
              {p.bookId && <Link className="link" to={`/livres/${p.bookId}`}>À propos de « {p.bookTitle} »</Link>}
              <p>{p.content}</p>
              <div className="row gap small-actions">
                <button className="link" onClick={() => setOpen(open === p.id ? null : p.id)}>
                  {open === p.id ? 'Masquer les commentaires' : `Commentaires (${counts[p.id] ?? 0})`}
                </button>
                {user && p.userId === user.id && <button className="link" onClick={() => setEditor(p)}>Modifier</button>}
                {user && (p.userId === user.id || user.role !== 'Reader') && <button className="link danger" onClick={() => remove(p.id)}>Supprimer</button>}
                {user && p.userId !== user.id && <button className="link" onClick={() => setReport({ kind: 'publication', id: p.id })}>Signaler</button>}
              </div>
              {open === p.id && <Comments pubId={p.id} user={user} onCount={(n) => setCounts((c) => ({ ...c, [p.id]: n }))} onReport={(id) => setReport({ kind: 'comment', id })} />}
            </article>
          ))}
        </div>
      )}
      {editor && <Editor initial={editor.id ? editor : null} books={books} onClose={() => setEditor(null)} onSaved={() => { setEditor(null); load() }} />}
      {report && <ReportModal target={report} onClose={() => setReport(null)} />}
    </>
  )
}

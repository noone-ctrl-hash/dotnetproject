import { useEffect, useState } from 'react'
import { api, dateTimeFr } from '../api.js'
import { Badge, Empty, Spinner } from '../components/ui.jsx'
import { useToast } from '../components/Toast.jsx'

function Reports() {
  const toast = useToast()
  const [items, setItems] = useState(null)
  const load = () => api.get('/reports').then(setItems)
  useEffect(() => { load() }, [])
  const examine = async (r) => { await api.put(`/reports/${r.id}/examine`); load() }
  const resolve = async (r, removeContent) => {
    await api.put(`/reports/${r.id}/resolve`, { removeContent, note: removeContent ? 'Contenu masqué' : 'Aucune infraction constatée' })
    toast(removeContent ? 'Contenu masqué, signalement résolu' : 'Signalement rejeté'); load()
  }
  if (!items) return <Spinner />
  if (!items.length) return <Empty title="Aucun signalement" hint="Les contenus signalés par les lecteurs apparaîtront ici." />
  return (
    <div className="feed">{items.map((r) => (
      <div key={r.id} className="card">
        <div className="row between wrap"><div className="row gap"><Badge status={r.status} /><span className="tag">{r.targetType}</span></div><span className="muted small">par {r.reporter} · {dateTimeFr(r.reportedAt)}</span></div>
        <p><strong>Motif :</strong> {r.reason}</p>
        <blockquote>{r.targetContent}</blockquote>
        {r.moderatorNote && <p className="muted small">Note : {r.moderatorNote}</p>}
        {(r.status === 'Open' || r.status === 'UnderReview') && (
          <div className="row gap wrap">
            {r.status === 'Open' && <button className="btn ghost sm" onClick={() => examine(r)}>Examiner</button>}
            <button className="btn danger sm" onClick={() => resolve(r, true)}>Masquer le contenu</button>
            <button className="btn ghost sm" onClick={() => resolve(r, false)}>Rejeter</button>
          </div>)}
      </div>))}
    </div>
  )
}

function Content({ kind }) {
  const toast = useToast()
  const [items, setItems] = useState(null)
  const load = () => api.get(`/admin/${kind}`).then(setItems)
  useEffect(() => { setItems(null); load() }, [kind])
  const setStatus = async (id, status) => { await api.put(`/admin/${kind}/${id}/status`, { status }); toast('Statut mis à jour'); load() }
  if (!items) return <Spinner />
  return (
    <div className="table-wrap"><table>
      <thead><tr><th>{kind === 'publications' ? 'Titre' : 'Commentaire'}</th><th>Auteur</th><th>Date</th><th>Statut</th><th /></tr></thead>
      <tbody>{items.map((i) => (
        <tr key={i.id}>
          <td className="clip">{i.title ?? i.content}</td><td>{i.author}</td><td>{dateTimeFr(i.publishedAt ?? i.commentedAt)}</td><td><Badge status={i.status} /></td>
          <td className="actions">{i.status === 'Published'
            ? <button className="link danger" onClick={() => setStatus(i.id, 'Hidden')}>Masquer</button>
            : <button className="link" onClick={() => setStatus(i.id, 'Published')}>Rétablir</button>}
            <button className="link danger" onClick={() => setStatus(i.id, 'Deleted')}>Supprimer</button></td>
        </tr>))}
      </tbody>
    </table></div>
  )
}

export default function AdminModeration() {
  const [view, setView] = useState('reports')
  return (
    <>
      <div className="segmented mb">
        {[['reports', 'Signalements'], ['publications', 'Publications'], ['comments', 'Commentaires']].map(([k, l]) => <button key={k} className={view === k ? 'on' : ''} onClick={() => setView(k)}>{l}</button>)}
      </div>
      {view === 'reports' ? <Reports /> : <Content kind={view} />}
    </>
  )
}

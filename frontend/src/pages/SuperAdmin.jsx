import { useEffect, useState } from 'react'
import { api, dateFr } from '../api.js'
import { useAuth } from '../auth.jsx'
import { Badge, Modal, Spinner } from '../components/ui.jsx'
import { useToast } from '../components/Toast.jsx'

export default function SuperAdmin() {
  const { user: me } = useAuth()
  const toast = useToast()
  const [users, setUsers] = useState(null)
  const [filter, setFilter] = useState('')
  const [creating, setCreating] = useState(false)
  const [f, setF] = useState({ fullName: '', email: '', password: '' })
  const load = () => api.get(`/users${api.qs({ role: filter })}`).then(setUsers)
  useEffect(() => { setUsers(null); load() }, [filter])

  const setRole = async (u, role) => {
    try { await api.put(`/users/${u.id}/role`, { role }); toast(`Rôle de ${u.fullName} mis à jour`); load() } catch (e) { toast(e.message, 'error') }
  }
  const remove = async (u) => {
    if (!confirm(`Supprimer le compte de ${u.fullName} ?`)) return
    try { await api.del(`/users/${u.id}`); toast('Compte supprimé'); load() } catch (e) { toast(e.message, 'error') }
  }
  const createAdmin = async (e) => {
    e.preventDefault()
    try { await api.post('/users/admins', f); toast('Administrateur créé'); setCreating(false); setF({ fullName: '', email: '', password: '' }); load() }
    catch (ex) { toast(ex.message, 'error') }
  }

  return (
    <>
      <div className="row between wrap"><h1 className="title">Super administration</h1><button className="btn primary" onClick={() => setCreating(true)}>Créer un administrateur</button></div>
      <div className="segmented mb">
        {[['', 'Tous'], ['Reader', 'Lecteurs'], ['Admin', 'Administrateurs'], ['SuperAdmin', 'Super admins']].map(([v, l]) => <button key={l} className={filter === v ? 'on' : ''} onClick={() => setFilter(v)}>{l}</button>)}
      </div>
      {!users ? <Spinner /> : (
        <div className="table-wrap"><table>
          <thead><tr><th>Nom</th><th>E-mail</th><th>Inscription</th><th>Rôle</th><th /></tr></thead>
          <tbody>{users.map((u) => (
            <tr key={u.id}>
              <td><strong>{u.fullName}</strong></td><td>{u.email}</td><td>{dateFr(u.registeredAt)}</td>
              <td>{u.id === me.id ? <Badge status={u.role} /> : (
                <select value={u.role} onChange={(e) => setRole(u, e.target.value)} aria-label={`Rôle de ${u.fullName}`}>
                  <option value="Reader">Lecteur</option><option value="Admin">Administrateur</option><option value="SuperAdmin">Super administrateur</option>
                </select>)}</td>
              <td className="actions">{u.id !== me.id && <button className="link danger" onClick={() => remove(u)}>Supprimer</button>}</td>
            </tr>))}
          </tbody>
        </table></div>
      )}
      <p className="muted small">Un changement de rôle s’applique à la prochaine connexion de la personne concernée.</p>
      {creating && (
        <Modal title="Créer un administrateur" onClose={() => setCreating(false)}>
          <form className="stack" onSubmit={createAdmin}>
            <label>Nom complet<input required autoFocus value={f.fullName} onChange={(e) => setF({ ...f, fullName: e.target.value })} /></label>
            <label>E-mail<input type="email" required value={f.email} onChange={(e) => setF({ ...f, email: e.target.value })} /></label>
            <label>Mot de passe<input type="password" required minLength={6} value={f.password} onChange={(e) => setF({ ...f, password: e.target.value })} /></label>
            <button className="btn primary">Créer le compte</button>
          </form>
        </Modal>
      )}
    </>
  )
}

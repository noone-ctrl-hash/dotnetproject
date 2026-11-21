import { useState } from 'react'
import { useAuth } from '../auth.jsx'
import { useToast } from '../components/Toast.jsx'
import { Badge } from '../components/ui.jsx'
import { dateFr } from '../api.js'

export default function Profile() {
  const { user, updateProfile } = useAuth()
  const toast = useToast()
  const [f, setF] = useState({ fullName: user.fullName, currentPassword: '', newPassword: '' })
  const [err, setErr] = useState('')
  const submit = async (e) => {
    e.preventDefault(); setErr('')
    try {
      await updateProfile({ fullName: f.fullName, currentPassword: f.currentPassword || null, newPassword: f.newPassword || null })
      setF({ ...f, currentPassword: '', newPassword: '' }); toast('Profil mis à jour')
    } catch (ex) { setErr(ex.message) }
  }
  return (
    <div className="narrow">
      <h1 className="title">Mon profil</h1>
      <div className="card row between wrap">
        <div><strong>{user.email}</strong><div className="muted">Inscrit le {dateFr(user.registeredAt)}</div></div>
        <Badge status={user.role} />
      </div>
      <form onSubmit={submit} className="stack card">
        <label>Nom complet<input required minLength={2} value={f.fullName} onChange={(e) => setF({ ...f, fullName: e.target.value })} /></label>
        <label>Mot de passe actuel<input type="password" value={f.currentPassword} onChange={(e) => setF({ ...f, currentPassword: e.target.value })} /></label>
        <label>Nouveau mot de passe<input type="password" minLength={6} value={f.newPassword} onChange={(e) => setF({ ...f, newPassword: e.target.value })} /><small>Laissez vide pour le conserver.</small></label>
        {err && <div className="error">{err}</div>}
        <button className="btn primary">Enregistrer</button>
      </form>
    </div>
  )
}

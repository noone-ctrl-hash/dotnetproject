import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth.jsx'
import { BrandMark } from '../components/ui.jsx'

function AuthShell({ title, subtitle, children, footer }) {
  return (
    <div className="auth">
      <div className="auth-card">
        <BrandMark />
        <h1>{title}</h1>
        <p className="muted">{subtitle}</p>
        {children}
        <p className="muted center">{footer}</p>
      </div>
    </div>
  )
}

export function Login() {
  const { login } = useAuth()
  const nav = useNavigate()
  const loc = useLocation()
  const [f, setF] = useState({ email: '', password: '' })
  const [err, setErr] = useState('')
  const [busy, setBusy] = useState(false)
  const submit = async (e) => {
    e.preventDefault(); setErr(''); setBusy(true)
    try { await login(f.email, f.password); nav(loc.state?.from || '/', { replace: true }) }
    catch (ex) { setErr(ex.message) } finally { setBusy(false) }
  }
  return (
    <AuthShell title="Se connecter" subtitle="Retrouvez votre bibliothèque et vos achats."
      footer={<>Pas encore de compte ? <Link to="/inscription">Créer un compte</Link></>}>
      <form onSubmit={submit} className="stack">
        <label>E-mail<input type="email" required autoFocus value={f.email} onChange={(e) => setF({ ...f, email: e.target.value })} /></label>
        <label>Mot de passe<input type="password" required value={f.password} onChange={(e) => setF({ ...f, password: e.target.value })} /></label>
        {err && <div className="error">{err}</div>}
        <button className="btn primary lg" disabled={busy}>{busy ? 'Connexion…' : 'Se connecter'}</button>
      </form>
      <details className="demo">
        <summary>Comptes de démonstration</summary>
        <ul>
          <li><code>reader@biblio.com</code> / <code>Reader@123</code></li>
          <li><code>admin@biblio.com</code> / <code>Admin@123</code></li>
          <li><code>superadmin@biblio.com</code> / <code>Super@123</code></li>
        </ul>
      </details>
    </AuthShell>
  )
}

export function Register() {
  const { register } = useAuth()
  const nav = useNavigate()
  const [f, setF] = useState({ fullName: '', email: '', password: '' })
  const [err, setErr] = useState('')
  const [busy, setBusy] = useState(false)
  const submit = async (e) => {
    e.preventDefault(); setErr(''); setBusy(true)
    try { await register(f.fullName, f.email, f.password); nav('/', { replace: true }) }
    catch (ex) { setErr(ex.message) } finally { setBusy(false) }
  }
  return (
    <AuthShell title="Créer un compte" subtitle="Gratuit. Achetez des livres, publiez vos avis, commentez."
      footer={<>Déjà inscrit ? <Link to="/connexion">Se connecter</Link></>}>
      <form onSubmit={submit} className="stack">
        <label>Nom complet<input required minLength={2} autoFocus value={f.fullName} onChange={(e) => setF({ ...f, fullName: e.target.value })} /></label>
        <label>E-mail<input type="email" required value={f.email} onChange={(e) => setF({ ...f, email: e.target.value })} /></label>
        <label>Mot de passe<input type="password" required minLength={6} value={f.password} onChange={(e) => setF({ ...f, password: e.target.value })} /><small>6 caractères minimum.</small></label>
        {err && <div className="error">{err}</div>}
        <button className="btn primary lg" disabled={busy}>{busy ? 'Création…' : 'Créer mon compte'}</button>
      </form>
    </AuthShell>
  )
}

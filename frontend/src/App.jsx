import { Navigate, NavLink, Route, Routes, useLocation, Link } from 'react-router-dom'
import { useAuth } from './auth.jsx'
import { BrandMark, Spinner } from './components/ui.jsx'
import Catalog from './pages/Catalog.jsx'
import BookDetails from './pages/BookDetails.jsx'
import Reader from './pages/Reader.jsx'
import MyLibrary from './pages/MyLibrary.jsx'
import Publications from './pages/Publications.jsx'
import { Login, Register } from './pages/AuthPages.jsx'
import Profile from './pages/Profile.jsx'
import Admin from './pages/Admin.jsx'
import SuperAdmin from './pages/SuperAdmin.jsx'

function Navbar() {
  const { user, logout, isStaff, isSuper } = useAuth()
  return (
    <header className="nav">
      <div className="nav-inner">
        <Link to="/" className="brand"><BrandMark />Bibliothèque</Link>
        <nav className="nav-links">
          <NavLink to="/" end>Catalogue</NavLink>
          <NavLink to="/publications">Communauté</NavLink>
          {user && <NavLink to="/bibliotheque">Ma bibliothèque</NavLink>}
          {isStaff && <NavLink to="/admin">Administration</NavLink>}
          {isSuper && <NavLink to="/super-admin">Super admin</NavLink>}
        </nav>
        <div className="nav-user">
          {user ? (
            <>
              <Link to="/profil" className="avatar" title={user.fullName}>{user.fullName.slice(0, 1)}</Link>
              <button className="btn ghost sm" onClick={logout}>Se déconnecter</button>
            </>
          ) : (
            <>
              <Link to="/connexion" className="btn ghost sm">Se connecter</Link>
              <Link to="/inscription" className="btn primary sm">Créer un compte</Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}

function Guard({ children, role }) {
  const { user, ready, isStaff, isSuper } = useAuth()
  const loc = useLocation()
  if (!ready) return <Spinner />
  if (!user) return <Navigate to="/connexion" state={{ from: loc.pathname }} replace />
  if (role === 'staff' && !isStaff) return <Navigate to="/" replace />
  if (role === 'super' && !isSuper) return <Navigate to="/" replace />
  return children
}

export default function App() {
  const { ready } = useAuth()
  if (!ready) return <Spinner />
  return (
    <>
      <Navbar />
      <main className="page">
        <Routes>
          <Route path="/" element={<Catalog />} />
          <Route path="/livres/:id" element={<BookDetails />} />
          <Route path="/lire/:id" element={<Guard><Reader /></Guard>} />
          <Route path="/bibliotheque" element={<Guard><MyLibrary /></Guard>} />
          <Route path="/publications" element={<Publications />} />
          <Route path="/profil" element={<Guard><Profile /></Guard>} />
          <Route path="/admin" element={<Guard role="staff"><Admin /></Guard>} />
          <Route path="/super-admin" element={<Guard role="super"><SuperAdmin /></Guard>} />
          <Route path="/connexion" element={<Login />} />
          <Route path="/inscription" element={<Register />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
      <footer className="footer">Bibliothèque Numérique — lisez, partagez, discutez.</footer>
    </>
  )
}

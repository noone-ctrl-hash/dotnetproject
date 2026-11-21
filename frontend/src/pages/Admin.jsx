import { useState } from 'react'
import AdminDashboard from './AdminDashboard.jsx'
import AdminBooks from './AdminBooks.jsx'
import AdminCategories from './AdminCategories.jsx'
import AdminModeration from './AdminModeration.jsx'

const TABS = [['dash', 'Statistiques'], ['books', 'Livres'], ['cats', 'Catégories'], ['mod', 'Modération']]

export default function Admin() {
  const [tab, setTab] = useState('dash')
  return (
    <>
      <h1 className="title">Administration</h1>
      <div className="segmented mb">{TABS.map(([k, l]) => <button key={k} className={tab === k ? 'on' : ''} onClick={() => setTab(k)}>{l}</button>)}</div>
      {tab === 'dash' && <AdminDashboard />}
      {tab === 'books' && <AdminBooks />}
      {tab === 'cats' && <AdminCategories />}
      {tab === 'mod' && <AdminModeration />}
    </>
  )
}

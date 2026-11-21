import { useEffect, useState } from 'react'
import { api, money } from '../api.js'
import { Badge, Modal, Spinner } from '../components/ui.jsx'
import { useToast } from '../components/Toast.jsx'

const blank = { title: '', author: '', description: '', cover: '#0a84ff,#5e5ce6', file: '', price: 0, isFree: false, categoryId: '', status: 'Published' }

function BookForm({ id, cats, onClose, onSaved }) {
  const toast = useToast()
  const [f, setF] = useState(id ? null : { ...blank, categoryId: cats[0]?.id ?? '' })
  useEffect(() => { if (id) api.get(`/books/${id}/edit`).then(setF) }, [id])
  if (!f) return <Modal title="Livre" onClose={onClose}><Spinner /></Modal>
  const set = (k, v) => setF({ ...f, [k]: v })
  const save = async (e) => {
    e.preventDefault()
    const body = { ...f, price: Number(f.price) || 0, categoryId: Number(f.categoryId) }
    try { id ? await api.put(`/books/${id}`, body) : await api.post('/books', body); toast('Livre enregistré'); onSaved() }
    catch (ex) { toast(ex.message, 'error') }
  }
  return (
    <Modal title={id ? 'Modifier le livre' : 'Ajouter un livre'} onClose={onClose} wide>
      <form className="stack" onSubmit={save}>
        <div className="row gap wrap">
          <label className="grow">Titre<input required value={f.title} onChange={(e) => set('title', e.target.value)} /></label>
          <label className="grow">Auteur<input required value={f.author} onChange={(e) => set('author', e.target.value)} /></label>
        </div>
        <label>Description<textarea rows={3} value={f.description} onChange={(e) => set('description', e.target.value)} /></label>
        <div className="row gap wrap">
          <label className="grow">Catégorie
            <select required value={f.categoryId} onChange={(e) => set('categoryId', e.target.value)}>{cats.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}</select>
          </label>
          <label className="grow">Prix (MAD)<input type="number" min="0" step="1" disabled={f.isFree} value={f.isFree ? 0 : f.price} onChange={(e) => set('price', e.target.value)} /></label>
          <label className="grow">Statut
            <select value={f.status} onChange={(e) => set('status', e.target.value)}><option value="Published">Publié</option><option value="Hidden">Masqué</option></select>
          </label>
        </div>
        <label className="switch"><input type="checkbox" checked={f.isFree} onChange={(e) => set('isFree', e.target.checked)} />Livre gratuit</label>
        <label>Couverture <small>(URL d’image ou deux couleurs « #0a84ff,#5e5ce6 »)</small><input value={f.cover} onChange={(e) => set('cover', e.target.value)} /></label>
        <label>Contenu du livre <small>(séparez les paragraphes par une ligne vide ; « Chapitre N » crée un titre)</small>
          <textarea rows={8} value={f.file} onChange={(e) => set('file', e.target.value)} />
        </label>
        <button className="btn primary lg">Enregistrer</button>
      </form>
    </Modal>
  )
}

export default function AdminBooks() {
  const toast = useToast()
  const [books, setBooks] = useState(null)
  const [cats, setCats] = useState([])
  const [form, setForm] = useState(null)
  const load = () => api.get('/books/admin').then(setBooks)
  useEffect(() => { load(); api.get('/categories').then(setCats) }, [])

  const remove = async (b) => {
    if (!confirm(`Supprimer « ${b.title} » ? S’il a déjà été acheté, il sera simplement masqué.`)) return
    await api.del(`/books/${b.id}`); toast('Livre supprimé'); load()
  }
  if (!books) return <Spinner />
  return (
    <>
      <div className="row between mb"><span className="muted">{books.length} livres</span><button className="btn primary" onClick={() => setForm({})} disabled={!cats.length}>Ajouter un livre</button></div>
      <div className="table-wrap"><table>
        <thead><tr><th>Titre</th><th>Auteur</th><th>Catégorie</th><th>Prix</th><th>Statut</th><th /></tr></thead>
        <tbody>{books.map((b) => (
          <tr key={b.id}>
            <td><strong>{b.title}</strong></td><td>{b.author}</td><td>{b.categoryName}</td>
            <td>{b.isFree ? 'Gratuit' : money(b.price)}</td><td><Badge status={b.status} /></td>
            <td className="actions"><button className="link" onClick={() => setForm({ id: b.id })}>Modifier</button><button className="link danger" onClick={() => remove(b)}>Supprimer</button></td>
          </tr>))}
        </tbody>
      </table></div>
      {form && <BookForm id={form.id} cats={cats} onClose={() => setForm(null)} onSaved={() => { setForm(null); load() }} />}
    </>
  )
}

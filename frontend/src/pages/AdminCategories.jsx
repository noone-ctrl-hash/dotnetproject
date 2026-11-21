import { useEffect, useState } from 'react'
import { api } from '../api.js'
import { Modal, Spinner } from '../components/ui.jsx'
import { useToast } from '../components/Toast.jsx'

export default function AdminCategories() {
  const toast = useToast()
  const [cats, setCats] = useState(null)
  const [form, setForm] = useState(null)
  const load = () => api.get('/categories').then(setCats)
  useEffect(() => { load() }, [])

  const save = async (e) => {
    e.preventDefault()
    try {
      form.id ? await api.put(`/categories/${form.id}`, form) : await api.post('/categories', form)
      toast('Catégorie enregistrée'); setForm(null); load()
    } catch (ex) { toast(ex.message, 'error') }
  }
  const remove = async (c) => {
    try { await api.del(`/categories/${c.id}`); toast('Catégorie supprimée'); load() } catch (ex) { toast(ex.message, 'error') }
  }
  if (!cats) return <Spinner />
  return (
    <>
      <div className="row between mb"><span className="muted">{cats.length} catégories</span><button className="btn primary" onClick={() => setForm({ name: '', description: '' })}>Ajouter une catégorie</button></div>
      <div className="table-wrap"><table>
        <thead><tr><th>Nom</th><th>Description</th><th>Livres</th><th /></tr></thead>
        <tbody>{cats.map((c) => (
          <tr key={c.id}><td><strong>{c.name}</strong></td><td>{c.description}</td><td>{c.bookCount}</td>
            <td className="actions"><button className="link" onClick={() => setForm(c)}>Modifier</button><button className="link danger" onClick={() => remove(c)}>Supprimer</button></td></tr>))}
        </tbody>
      </table></div>
      {form && (
        <Modal title={form.id ? 'Modifier la catégorie' : 'Ajouter une catégorie'} onClose={() => setForm(null)}>
          <form className="stack" onSubmit={save}>
            <label>Nom<input required autoFocus value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} /></label>
            <label>Description<textarea rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></label>
            <button className="btn primary">Enregistrer</button>
          </form>
        </Modal>
      )}
    </>
  )
}

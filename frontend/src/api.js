const TOKEN_KEY = 'biblio_token'

export const getToken = () => localStorage.getItem(TOKEN_KEY)
export const setToken = (t) => (t ? localStorage.setItem(TOKEN_KEY, t) : localStorage.removeItem(TOKEN_KEY))

async function request(method, url, body) {
  const headers = {}
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  let res
  try {
    res = await fetch(`/api${url}`, { method, headers, body: body !== undefined ? JSON.stringify(body) : undefined })
  } catch {
    throw new Error('Serveur injoignable. Vérifiez que l’API est démarrée.')
  }
  if (res.status === 204) return null
  const text = await res.text()
  let data = null
  try { data = text ? JSON.parse(text) : null } catch { data = null }
  if (!res.ok) {
    if (res.status === 401 && token) window.dispatchEvent(new Event('biblio:unauthorized'))
    const firstError = data?.errors ? Object.values(data.errors).flat()[0] : null
    throw Object.assign(new Error(data?.message || firstError || data?.title || `Erreur ${res.status}`), { status: res.status })
  }
  return data
}

const qs = (p = {}) => {
  const s = new URLSearchParams(Object.entries(p).filter(([, v]) => v !== '' && v !== null && v !== undefined)).toString()
  return s ? `?${s}` : ''
}

export const api = {
  get: (u) => request('GET', u),
  post: (u, b = {}) => request('POST', u, b),
  put: (u, b = {}) => request('PUT', u, b),
  del: (u) => request('DELETE', u),
  qs
}

export const money = (n) => new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD', maximumFractionDigits: 0 }).format(n ?? 0)
export const dateFr = (d) => new Date(d).toLocaleDateString('fr-FR', { day: 'numeric', month: 'long', year: 'numeric' })
export const dateTimeFr = (d) => new Date(d).toLocaleString('fr-FR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })

import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { api, getToken, setToken } from './api.js'

const Ctx = createContext(null)
export const useAuth = () => useContext(Ctx)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [ready, setReady] = useState(false)

  const logout = useCallback(() => { setToken(null); setUser(null) }, [])

  useEffect(() => {
    if (!getToken()) { setReady(true); return }
    api.get('/auth/me').then(setUser).catch(() => setToken(null)).finally(() => setReady(true))
  }, [])

  useEffect(() => {
    window.addEventListener('biblio:unauthorized', logout)
    return () => window.removeEventListener('biblio:unauthorized', logout)
  }, [logout])

  const accept = (res) => { setToken(res.token); setUser(res.user); return res.user }
  const login = async (email, password) => accept(await api.post('/auth/login', { email, password }))
  const register = async (fullName, email, password) => accept(await api.post('/auth/register', { fullName, email, password }))
  const updateProfile = async (payload) => accept(await api.put('/auth/profile', payload))

  const isStaff = user?.role === 'Admin' || user?.role === 'SuperAdmin'
  const isSuper = user?.role === 'SuperAdmin'

  return (
    <Ctx.Provider value={{ user, ready, login, register, logout, updateProfile, isStaff, isSuper }}>
      {children}
    </Ctx.Provider>
  )
}

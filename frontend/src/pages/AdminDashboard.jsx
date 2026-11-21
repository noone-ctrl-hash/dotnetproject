import { useEffect, useState } from 'react'
import {
  ArcElement, BarElement, CategoryScale, Chart as ChartJS, Filler, Legend, LinearScale, LineElement, PointElement, Tooltip
} from 'chart.js'
import { Bar, Doughnut, Line } from 'react-chartjs-2'
import { api, money } from '../api.js'
import { Spinner } from '../components/ui.jsx'

ChartJS.register(ArcElement, BarElement, CategoryScale, Filler, Legend, LinearScale, LineElement, PointElement, Tooltip)
ChartJS.defaults.color = '#98989d'
ChartJS.defaults.font.family = '-apple-system, BlinkMacSystemFont, "SF Pro Text", "Inter Variable", Inter, sans-serif'
ChartJS.defaults.borderColor = 'rgba(255,255,255,0.07)'

const PALETTE = ['#0a84ff', '#30d158', '#ff9f0a', '#bf5af2', '#ff375f', '#64d2ff']
const STATUS_LABEL = { Success: 'Réussis', Failed: 'Refusés', Cancelled: 'Annulés', Pending: 'En attente' }
const STATUS_COLOR = { Success: '#30d158', Failed: '#ff453a', Cancelled: '#ff9f0a', Pending: '#8e8e93' }
const tooltip = { backgroundColor: '#2c2c2e', titleColor: '#fff', bodyColor: '#e5e5ea', padding: 10, cornerRadius: 10, displayColors: false }
const base = { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip } }
const grid = { grid: { color: 'rgba(255,255,255,0.06)' }, border: { display: false } }

function Stat({ label, value, hint }) {
  return <div className="stat"><span>{label}</span><strong>{value}</strong>{hint && <small>{hint}</small>}</div>
}
const Panel = ({ title, children, wide }) => <section className={`card panel ${wide ? 'wide' : ''}`}><h3>{title}</h3><div className="chart">{children}</div></section>

export default function AdminDashboard() {
  const [s, setS] = useState(null)
  const [err, setErr] = useState('')
  useEffect(() => { api.get('/admin/stats').then(setS).catch((e) => setErr(e.message)) }, [])
  if (err) return <div className="error">{err}</div>
  if (!s) return <Spinner />
  const t = s.totals

  return (
    <>
      <div className="stats">
        <Stat label="Revenus" value={money(t.revenue)} hint={`${t.sales} ventes réussies`} />
        <Stat label="Utilisateurs" value={t.users} />
        <Stat label="Livres publiés" value={t.books} hint={`${t.freeBooks} gratuits · ${t.paidBooks} payants`} />
        <Stat label="Publications" value={t.publications} hint={`${t.comments} commentaires`} />
        <Stat label="Signalements à traiter" value={t.openReports} />
      </div>

      <div className="charts">
        <Panel title="Revenus des 30 derniers jours" wide>
          <Line options={{ ...base, scales: { x: { ...grid, ticks: { maxTicksLimit: 8 } }, y: { ...grid, beginAtZero: true } } }}
            data={{
              labels: s.revenueByDay.map((d) => d.date.slice(5)),
              datasets: [{ data: s.revenueByDay.map((d) => d.revenue), borderColor: '#0a84ff', backgroundColor: 'rgba(10,132,255,0.15)', fill: true, tension: 0.35, pointRadius: 0, pointHoverRadius: 5, borderWidth: 2.5 }]
            }} />
        </Panel>
        <Panel title="Ventes par catégorie">
          <Doughnut options={{ ...base, cutout: '68%', plugins: { ...base.plugins, legend: { display: true, position: 'bottom', labels: { boxWidth: 10, usePointStyle: true } } } }}
            data={{ labels: s.byCategory.map((c) => c.category), datasets: [{ data: s.byCategory.map((c) => c.sales), backgroundColor: PALETTE, borderColor: '#1c1c1e', borderWidth: 3 }] }} />
        </Panel>
        <Panel title="Livres les plus vendus">
          <Bar options={{ ...base, indexAxis: 'y', scales: { x: { ...grid, beginAtZero: true, ticks: { precision: 0 } }, y: { grid: { display: false }, border: { display: false } } } }}
            data={{ labels: s.topBooks.map((b) => b.title), datasets: [{ data: s.topBooks.map((b) => b.sales), backgroundColor: '#30d158', borderRadius: 8, maxBarThickness: 22 }] }} />
        </Panel>
        <Panel title="Statut des paiements">
          <Doughnut options={{ ...base, cutout: '68%', plugins: { ...base.plugins, legend: { display: true, position: 'bottom', labels: { boxWidth: 10, usePointStyle: true } } } }}
            data={{ labels: s.byStatus.map((x) => STATUS_LABEL[x.status] ?? x.status), datasets: [{ data: s.byStatus.map((x) => x.count), backgroundColor: s.byStatus.map((x) => STATUS_COLOR[x.status] ?? '#8e8e93'), borderColor: '#1c1c1e', borderWidth: 3 }] }} />
        </Panel>
        <Panel title="Nouveaux utilisateurs par mois">
          <Bar options={{ ...base, scales: { x: { grid: { display: false }, border: { display: false } }, y: { ...grid, beginAtZero: true, ticks: { precision: 0 } } } }}
            data={{ labels: s.newUsers.map((m) => m.month), datasets: [{ data: s.newUsers.map((m) => m.count), backgroundColor: '#bf5af2', borderRadius: 8, maxBarThickness: 34 }] }} />
        </Panel>
      </div>
    </>
  )
}

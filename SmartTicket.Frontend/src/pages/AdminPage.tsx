const AdminPage = () => {
  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="pageHeader">
        <div>
          <h1 style={{ margin: 0 }}>Admin</h1>
          <p className="muted" style={{ margin: '6px 0 0' }}>
            Area amministrativa riservata.
          </p>
        </div>
      </div>

      <section className="card">
        <h2 style={{ marginTop: 0 }}>Dashboard</h2>
        <p className="muted" style={{ margin: 0 }}>
          Qui potrai gestire utenti, ruoli e configurazioni.
        </p>
      </section>
    </div>
  )
}

export default AdminPage

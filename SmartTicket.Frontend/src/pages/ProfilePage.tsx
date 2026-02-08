const ProfilePage = () => {
  return (
    <div style={{ display: 'grid', gap: 16 }}>
      <div className="pageHeader">
        <div>
          <h1 style={{ margin: 0 }}>Profilo</h1>
          <p className="muted" style={{ margin: '6px 0 0' }}>
            Impostazioni personali e preferenze.
          </p>
        </div>
      </div>

      <section className="card">
        <p className="muted" style={{ margin: 0 }}>
          In arrivo.
        </p>
      </section>
    </div>
  )
}

export default ProfilePage

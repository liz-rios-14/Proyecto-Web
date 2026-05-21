import Layout from "../components/Layout";

export default function HomePage() {
  return (
    <Layout>
      <h1>Inicio</h1>

      <div className="card">
        <h2>Bienvenido a SalesPoint</h2>
        <p>Sistema web para gestionar clientes, productos, facturas y ventas.</p>
      </div>
    </Layout>
  );
}
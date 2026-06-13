# Despliegue Docker de SalesPoint

Todos los archivos de despliegue están centralizados en esta carpeta.

La forma más sencilla en Windows es ejecutar:

- `iniciar.cmd`: construye e inicia todo.
- `estado.cmd`: muestra los tres servicios.
- `detener.cmd`: detiene todo sin borrar la base.

Desde la raíz del proyecto:

```powershell
Copy-Item docker/.env.example docker/.env
docker compose --env-file docker/.env -f docker/compose.yml up --build -d
```

Direcciones:

- Aplicación completa: `http://localhost:8080`
- Swagger de la API: `http://localhost:5037/swagger`
- SQL Server: `localhost,14330`

Para revisar servicios:

```powershell
docker compose --env-file docker/.env -f docker/compose.yml ps
```

Para detenerlos conservando la base de datos:

```powershell
docker compose --env-file docker/.env -f docker/compose.yml down
```

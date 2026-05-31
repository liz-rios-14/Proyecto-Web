-- Seeder masivo para PostgreSQL
-- Genera 100.000 clientes, 100.000 productos, ventas, detalles y movimientos de stock.
-- Ejecutar después de aplicar migraciones: psql -d salespoint -f Scripts/seed-massive-data.sql

BEGIN;

INSERT INTO customers (first_name, last_name, phone, address, email)
SELECT
    'CLIENTE' || gs,
    'PRUEBA' || gs,
    lpad((900000000 + gs)::text, 10, '0'),
    'DIRECCION PRUEBA ' || gs,
    'cliente' || gs || '@test.local'
FROM generate_series(1, 100000) AS gs
WHERE NOT EXISTS (SELECT 1 FROM customers WHERE email = 'cliente' || gs || '@test.local');

INSERT INTO products (name, price, stock)
SELECT
    'PRODUCTO PRUEBA ' || gs,
    round((1 + random() * 999)::numeric, 2),
    1000
FROM generate_series(1, 100000) AS gs
WHERE NOT EXISTS (SELECT 1 FROM products WHERE name = 'PRODUCTO PRUEBA ' || gs);

INSERT INTO invoices (customer_id, date, invoice_number)
SELECT
    c.id,
    now() - ((gs % 365) || ' days')::interval,
    'FAC-MASS-' || lpad(gs::text, 8, '0')
FROM generate_series(1, 10000) AS gs
JOIN LATERAL (
    SELECT id FROM customers ORDER BY id LIMIT 1 OFFSET ((gs - 1) % 100000)
) c ON true
WHERE NOT EXISTS (SELECT 1 FROM invoices WHERE invoice_number = 'FAC-MASS-' || lpad(gs::text, 8, '0'));

INSERT INTO invoice_details ("InvoiceId", product_id, product_name, price, quantity)
SELECT
    i.id,
    p.id,
    p.name,
    p.price,
    1 + (gs % 5)
FROM generate_series(1, 30000) AS gs
JOIN LATERAL (
    SELECT id FROM invoices WHERE invoice_number LIKE 'FAC-MASS-%' ORDER BY id LIMIT 1 OFFSET ((gs - 1) % 10000)
) i ON true
JOIN LATERAL (
    SELECT id, name, price FROM products ORDER BY id LIMIT 1 OFFSET ((gs - 1) % 100000)
) p ON true
WHERE NOT EXISTS (
    SELECT 1
    FROM invoice_details d
    WHERE d."InvoiceId" = i.id AND d.product_id = p.id
);

UPDATE products p
SET stock = stock - totals.quantity
FROM (
    SELECT product_id, SUM(quantity)::int AS quantity
    FROM invoice_details d
    JOIN invoices i ON i.id = d."InvoiceId"
    WHERE i.invoice_number LIKE 'FAC-MASS-%'
    GROUP BY product_id
) totals
WHERE p.id = totals.product_id;

INSERT INTO stock_movements (product_id, invoice_id, movement_type, quantity, stock_after, created_at, reason)
SELECT
    d.product_id,
    d."InvoiceId",
    'SALE_CONFIRMED',
    d.quantity * -1,
    p.stock,
    now(),
    'VENTA MASIVA DE PRUEBA ' || i.invoice_number
FROM invoice_details d
JOIN invoices i ON i.id = d."InvoiceId"
JOIN products p ON p.id = d.product_id
WHERE i.invoice_number LIKE 'FAC-MASS-%'
  AND NOT EXISTS (
      SELECT 1
      FROM stock_movements sm
      WHERE sm.invoice_id = d."InvoiceId" AND sm.product_id = d.product_id
  );

COMMIT;

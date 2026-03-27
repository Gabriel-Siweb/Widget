# Widget API — MVP fichajes (NestJS + Prisma)

API REST para un MVP de fichajes tipo Sygna: registro secuencial de eventos sin reglas horarias ni validación de duraciones.

## Por qué NestJS (y no solo Express)

NestJS aporta módulos por dominio (`Auth`, `Clocking`), inyección de dependencias, guards JWT y `ValidationPipe` con `class-validator` de forma estándar. Con Express habría que ensamblar manualmente la misma estructura. Para un proyecto académico, NestJS deja la arquitectura más clara y mantenible sin añadir reglas de negocio extra.

## Requisitos

- Node.js 20+
- Docker (para PostgreSQL)

## Puesta en marcha

1. Copia variables de entorno:

   ```bash
   cp .env.example .env
   ```

2. Levanta PostgreSQL:

   ```bash
   docker compose up -d
   ```

3. Instala dependencias (si aún no):

   ```bash
   npm install
   ```

4. Migraciones y cliente Prisma:

   ```bash
   npx prisma migrate dev --name init
   npx prisma generate
   ```

5. Datos demo:

   ```bash
   npx prisma db seed
   ```

6. Arranque en desarrollo:

   ```bash
   npm run start:dev
   ```

La API queda en `http://localhost:3000` (o el `PORT` definido en `.env`).

## Variables de entorno

| Variable | Descripción |
|----------|-------------|
| `DATABASE_URL` | Cadena de conexión PostgreSQL |
| `JWT_SECRET` | Secreto para firmar access tokens |
| `JWT_ACCESS_EXPIRES` | Caducidad del access token (p. ej. `15m`) |
| `JWT_REFRESH_EXPIRES_DAYS` | Días de validez del refresh token |
| `APP_TIMEZONE` | Zona IANA para `dayKey` y horas `HH:mm` (p. ej. `Europe/Madrid`) |
| `PORT` | Puerto HTTP |

## Usuarios seed (contraseña `123456`)

- `juan@example.com` — configuración: comida + 1 descanso; tiene `entry` hoy.
- `maria@example.com` — sin comida + 2 descansos; tiene `entry`, `break1Start`, `break1End` hoy.

## Endpoints principales

| Método | Ruta | Auth |
|--------|------|------|
| POST | `/auth/login` | No |
| POST | `/auth/refresh` | No |
| POST | `/auth/logout` | No |
| GET | `/auth/me` | Bearer access token |
| GET | `/clocking/config` | Bearer |
| GET | `/clocking/today` | Bearer |
| POST | `/clocking/register` | Bearer |

En rutas protegidas envía el header: `Authorization: Bearer <accessToken>`.

## Integración futura (Sygna)

En `src/common/ports/clocking-external.port.ts` hay un puerto vacío para un adaptador externo. El registro local sigue siendo la fuente de verdad del MVP; más adelante se puede notificar a Sygna desde `ClockingService` sin cambiar el contrato HTTP.

## Tests

```bash
npm test
```

Incluye pruebas unitarias de la secuencia de fichajes (`src/clocking/clocking-sequence.spec.ts`).

```bash
npm run test:e2e
```

El e2e mínimo mockea `PrismaService` para poder ejecutarse sin PostgreSQL; el arranque real de la API sigue necesitando la base de datos.

## Prisma

Este proyecto usa **Prisma 5.22** (`prisma` y `@prisma/client` acotados en `package.json`) para conservar `url` en `schema.prisma`, alineado con la documentación habitual del MVP.

## Scripts útiles

- `npm run prisma:migrate` — migraciones de desarrollo
- `npm run prisma:seed` — ejecutar seed
- `npm run test:e2e` — e2e mínimo (401 sin token en `/auth/me`)

-- Migration: create auth-related tables and logs (idempotent)
-- Run in Supabase SQL editor or via psql

BEGIN;

CREATE TABLE IF NOT EXISTS public.perfiles (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid UNIQUE NOT NULL,
  role text NOT NULL CHECK (role IN ('cliente','repartidor','encargado')),
  nombre text,
  telefono text,
  created_at timestamptz DEFAULT now()
);

CREATE INDEX IF NOT EXISTS perfiles_user_id_idx ON public.perfiles(user_id);
CREATE INDEX IF NOT EXISTS perfiles_telefono_idx ON public.perfiles(telefono);

CREATE TABLE IF NOT EXISTS public.direcciones (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  perfil_id uuid NOT NULL REFERENCES public.perfiles(id) ON DELETE CASCADE,
  alias text,
  direccion jsonb,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.metodos_pago (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  perfil_id uuid NOT NULL REFERENCES public.perfiles(id) ON DELETE CASCADE,
  tipo text,
  datos jsonb,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.aceptaciones_login (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL,
  tipo text NOT NULL,
  version text,
  aceptado_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.logs_accesos (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid,
  role text,
  origen text,
  evento text,
  user_agent text,
  ip text,
  meta jsonb,
  created_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.logs_errores (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid,
  role text,
  origen text,
  evento text,
  mensaje text,
  detalle jsonb,
  created_at timestamptz DEFAULT now()
);

COMMIT;

-- NOTE: This migration is conservative and idempotent. It does not touch Supabase Auth users.

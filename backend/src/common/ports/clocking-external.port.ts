/**
 * Puerto para una futura integración con un sistema externo (p. ej. Sygna).
 * El MVP persiste solo en BD local; un adaptador puede implementar esta interfaz
 * para reenviar eventos o sincronizar estado sin cambiar los controladores.
 */
export interface ClockingExternalPort {
  /** Opcional: notificar un fichaje registrado localmente al proveedor externo */
  onClockingRegistered?(payload: {
    userId: string;
    action: string;
    occurredAt: Date;
    dayKey: string;
  }): Promise<void>;
}

export const CLOCKING_EXTERNAL = Symbol('CLOCKING_EXTERNAL');

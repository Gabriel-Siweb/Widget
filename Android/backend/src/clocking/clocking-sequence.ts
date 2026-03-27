import { formatTimeHm } from '../common/time.util';

export const CLOCKING_ACTIONS = [
  'entry',
  'break1Start',
  'break1End',
  'break2Start',
  'break2End',
  'mealStart',
  'mealEnd',
  'exit',
] as const;

export type ClockingAction = (typeof CLOCKING_ACTIONS)[number];

export type ClockingModeConfig = {
  hasMeal: boolean;
  breaks: number;
};

export const STEP_LABELS: Record<ClockingAction, string> = {
  entry: 'Entrada',
  break1Start: 'Descanso 1 — inicio',
  break1End: 'Descanso 1 — fin',
  break2Start: 'Descanso 2 — inicio',
  break2End: 'Descanso 2 — fin',
  mealStart: 'Comida — inicio',
  mealEnd: 'Comida — fin',
  exit: 'Salida',
};

export function assertValidModeConfig(config: ClockingModeConfig): void {
  if (config.breaks < 1 || config.breaks > 2 || !Number.isInteger(config.breaks)) {
    throw new Error('breaks debe ser 1 o 2');
  }
}

export function buildSequence(config: ClockingModeConfig): ClockingAction[] {
  assertValidModeConfig(config);
  const seq: ClockingAction[] = ['entry'];
  if (config.breaks >= 1) {
    seq.push('break1Start', 'break1End');
  }
  if (config.breaks >= 2) {
    seq.push('break2Start', 'break2End');
  }
  if (config.hasMeal) {
    seq.push('mealStart', 'mealEnd');
  }
  seq.push('exit');
  return seq;
}

export type ClockingEventInput = {
  action: ClockingAction;
  occurredAt: Date;
};

export type ClockingComputedState = {
  records: Record<ClockingAction, string | null>;
  nextAction: ClockingAction | null;
  completedSteps: ClockingAction[];
  pendingSteps: ClockingAction[];
  steps: { key: ClockingAction; label: string; done: boolean }[];
};

export function computeClockingState(
  sequence: ClockingAction[],
  events: ClockingEventInput[],
  timeZone: string,
): ClockingComputedState {
  const byAction = new Map<ClockingAction, Date>();
  for (const e of events) {
    if (!byAction.has(e.action)) {
      byAction.set(e.action, e.occurredAt);
    }
  }

  const records = {} as Record<ClockingAction, string | null>;
  for (const action of CLOCKING_ACTIONS) {
    const inSequence = sequence.includes(action);
    const at = byAction.get(action);
    records[action] =
      inSequence && at ? formatTimeHm(at, timeZone) : null;
  }

  let nextAction: ClockingAction | null = null;
  for (const action of sequence) {
    if (!byAction.has(action)) {
      nextAction = action;
      break;
    }
  }

  const completedSteps = sequence.filter((a) => byAction.has(a));
  const pendingSteps = sequence.filter((a) => !byAction.has(a));

  const steps = sequence.map((key) => ({
    key,
    label: STEP_LABELS[key],
    done: byAction.has(key),
  }));

  return {
    records,
    nextAction,
    completedSteps,
    pendingSteps,
    steps,
  };
}

export function isValidNextAction(
  requested: ClockingAction,
  nextAction: ClockingAction | null,
): boolean {
  if (nextAction === null) {
    return false;
  }
  return requested === nextAction;
}

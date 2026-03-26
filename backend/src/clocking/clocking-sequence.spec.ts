import {
  buildSequence,
  computeClockingState,
  isValidNextAction,
} from './clocking-sequence';

describe('buildSequence', () => {
  it('construye secuencia con comida y 1 descanso', () => {
    expect(buildSequence({ hasMeal: true, breaks: 1 })).toEqual([
      'entry',
      'break1Start',
      'break1End',
      'mealStart',
      'mealEnd',
      'exit',
    ]);
  });

  it('construye secuencia sin comida y 2 descansos', () => {
    expect(buildSequence({ hasMeal: false, breaks: 2 })).toEqual([
      'entry',
      'break1Start',
      'break1End',
      'break2Start',
      'break2End',
      'exit',
    ]);
  });

  it('rechaza breaks fuera de 1..2', () => {
    expect(() => buildSequence({ hasMeal: false, breaks: 0 })).toThrow();
    expect(() => buildSequence({ hasMeal: false, breaks: 3 })).toThrow();
  });
});

describe('computeClockingState y nextAction', () => {
  const tz = 'UTC';

  it('calcula nextAction tras entrada', () => {
    const seq = buildSequence({ hasMeal: false, breaks: 2 });
    const t0 = new Date('2025-01-15T09:00:00.000Z');
    const state = computeClockingState(
      seq,
      [{ action: 'entry', occurredAt: t0 }],
      tz,
    );
    expect(state.nextAction).toBe('break1Start');
    expect(state.records.entry).toBe('09:00');
    expect(state.records.break1Start).toBeNull();
  });

  it('rechaza acción fuera de secuencia', () => {
    const seq = buildSequence({ hasMeal: false, breaks: 2 });
    const t0 = new Date('2025-01-15T09:00:00.000Z');
    const state = computeClockingState(
      seq,
      [{ action: 'entry', occurredAt: t0 }],
      tz,
    );
    expect(isValidNextAction('break2Start', state.nextAction)).toBe(false);
    expect(isValidNextAction('break1Start', state.nextAction)).toBe(true);
  });

  it('no valida duración: intervalos largos entre eventos siguen siendo válidos en orden', () => {
    const seq = buildSequence({ hasMeal: true, breaks: 1 });
    const entry = new Date('2025-01-15T08:00:00.000Z');
    const b1s = new Date('2025-01-15T18:00:00.000Z');
    const state = computeClockingState(
      seq,
      [
        { action: 'entry', occurredAt: entry },
        { action: 'break1Start', occurredAt: b1s },
      ],
      tz,
    );
    expect(state.nextAction).toBe('break1End');
    expect(state.records.break1Start).toBe('18:00');
  });

  it('nextAction null cuando el día está completo', () => {
    const seq = buildSequence({ hasMeal: false, breaks: 1 });
    const events = seq.map((action, i) => ({
      action,
      occurredAt: new Date(`2025-01-15T${String(8 + i).padStart(2, '0')}:00:00.000Z`),
    }));
    const state = computeClockingState(seq, events, tz);
    expect(state.nextAction).toBeNull();
    expect(isValidNextAction('entry', state.nextAction)).toBe(false);
  });
});

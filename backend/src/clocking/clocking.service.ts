import {
  BadRequestException,
  Inject,
  Injectable,
  NotFoundException,
  Optional,
} from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { ClockingAction as PrismaClockingAction, User } from '@prisma/client';
import { PrismaClientKnownRequestError } from '@prisma/client/runtime/library';
import {
  CLOCKING_EXTERNAL,
  ClockingExternalPort,
} from '../common/ports/clocking-external.port';
import { dayKeyInTimeZone, formatTimeHm } from '../common/time.util';
import { PrismaService } from '../prisma/prisma.service';
import {
  ClockingAction,
  buildSequence,
  computeClockingState,
  isValidNextAction,
} from './clocking-sequence';

@Injectable()
export class ClockingService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly config: ConfigService,
    @Optional()
    @Inject(CLOCKING_EXTERNAL)
    private readonly external: ClockingExternalPort | undefined,
  ) {}

  private timeZone(): string {
    return this.config.get<string>('APP_TIMEZONE') ?? 'Europe/Madrid';
  }

  async getConfig(userId: string) {
    const cfg = await this.prisma.clockingConfig.findUnique({
      where: { userId },
    });
    if (!cfg) {
      throw new NotFoundException('No hay configuración de fichaje para el usuario');
    }
    return {
      hasMeal: cfg.hasMeal,
      breaks: cfg.breaks,
    };
  }

  async getToday(user: User) {
    const tz = this.timeZone();
    const now = new Date();
    const dayKey = dayKeyInTimeZone(now, tz);

    const cfg = await this.prisma.clockingConfig.findUnique({
      where: { userId: user.id },
    });
    if (!cfg) {
      throw new NotFoundException('No hay configuración de fichaje para el usuario');
    }

    const events = await this.prisma.clockingEvent.findMany({
      where: { userId: user.id, dayKey },
      orderBy: { occurredAt: 'asc' },
    });

    const sequence = buildSequence({
      hasMeal: cfg.hasMeal,
      breaks: cfg.breaks,
    });

    const state = computeClockingState(
      sequence,
      events.map((e) => ({
        action: e.action as ClockingAction,
        occurredAt: e.occurredAt,
      })),
      tz,
    );

    return {
      loggedIn: true,
      employeeName: user.name,
      mode: {
        hasMeal: cfg.hasMeal,
        breaks: cfg.breaks,
      },
      records: state.records,
      nextAction: state.nextAction,
      steps: state.steps,
      completedSteps: state.completedSteps,
      pendingSteps: state.pendingSteps,
    };
  }

  async register(user: User, action: ClockingAction) {
    const tz = this.timeZone();
    const now = new Date();
    const dayKey = dayKeyInTimeZone(now, tz);

    const cfg = await this.prisma.clockingConfig.findUnique({
      where: { userId: user.id },
    });
    if (!cfg) {
      throw new NotFoundException('No hay configuración de fichaje para el usuario');
    }

    const events = await this.prisma.clockingEvent.findMany({
      where: { userId: user.id, dayKey },
      orderBy: { occurredAt: 'asc' },
    });

    const sequence = buildSequence({
      hasMeal: cfg.hasMeal,
      breaks: cfg.breaks,
    });

    const before = computeClockingState(
      sequence,
      events.map((e) => ({
        action: e.action as ClockingAction,
        occurredAt: e.occurredAt,
      })),
      tz,
    );

    if (!isValidNextAction(action, before.nextAction)) {
      throw new BadRequestException({
        message: 'La acción no es la siguiente válida en la secuencia',
        expected: before.nextAction,
      });
    }

    try {
      await this.prisma.clockingEvent.create({
        data: {
          userId: user.id,
          action: action as PrismaClockingAction,
          occurredAt: now,
          dayKey,
        },
      });
    } catch (e) {
      if (e instanceof PrismaClientKnownRequestError && e.code === 'P2002') {
        throw new BadRequestException(
          'Esta acción ya está registrada para hoy',
        );
      }
      throw e;
    }

    await this.external?.onClockingRegistered?.({
      userId: user.id,
      action,
      occurredAt: now,
      dayKey,
    });

    const eventsAfter = await this.prisma.clockingEvent.findMany({
      where: { userId: user.id, dayKey },
      orderBy: { occurredAt: 'asc' },
    });

    const after = computeClockingState(
      sequence,
      eventsAfter.map((e) => ({
        action: e.action as ClockingAction,
        occurredAt: e.occurredAt,
      })),
      tz,
    );

    return {
      success: true as const,
      time: formatTimeHm(now, tz),
      nextAction: after.nextAction,
    };
  }
}

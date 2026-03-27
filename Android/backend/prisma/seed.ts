import { PrismaClient } from '@prisma/client';
import * as bcrypt from 'bcrypt';

const prisma = new PrismaClient();

function dayKeyInTimeZone(date: Date, timeZone: string): string {
  return new Intl.DateTimeFormat('en-CA', {
    timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).format(date);
}

async function main() {
  const tz = process.env.APP_TIMEZONE ?? 'Europe/Madrid';
  const now = new Date();
  const today = dayKeyInTimeZone(now, tz);

  const atOffsetMin = (deltaMin: number) =>
    new Date(now.getTime() + deltaMin * 60_000);

  const passwordHash = await bcrypt.hash('123456', 10);

  await prisma.clockingEvent.deleteMany();
  await prisma.clockingConfig.deleteMany();
  await prisma.refreshToken.deleteMany();
  await prisma.user.deleteMany();

  const userMeal = await prisma.user.create({
    data: {
      name: 'Juan Pérez',
      email: 'juan@example.com',
      passwordHash,
      isActive: true,
      clockingConfig: {
        create: {
          hasMeal: true,
          breaks: 1,
        },
      },
    },
  });

  const userTwoBreaks = await prisma.user.create({
    data: {
      name: 'María López',
      email: 'maria@example.com',
      passwordHash,
      isActive: true,
      clockingConfig: {
        create: {
          hasMeal: false,
          breaks: 2,
        },
      },
    },
  });

  await prisma.clockingEvent.createMany({
    data: [
      {
        userId: userMeal.id,
        action: 'entry',
        occurredAt: atOffsetMin(-180),
        dayKey: today,
      },
      {
        userId: userTwoBreaks.id,
        action: 'entry',
        occurredAt: atOffsetMin(-150),
        dayKey: today,
      },
      {
        userId: userTwoBreaks.id,
        action: 'break1Start',
        occurredAt: atOffsetMin(-120),
        dayKey: today,
      },
      {
        userId: userTwoBreaks.id,
        action: 'break1End',
        occurredAt: atOffsetMin(-105),
        dayKey: today,
      },
    ],
  });

  console.log('Seed OK');
  console.log('Usuarios demo (contraseña: 123456):');
  console.log('- juan@example.com → comida + 1 descanso (entrada registrada hoy)');
  console.log('- maria@example.com → sin comida + 2 descansos (hasta fin descanso 1)');
  console.log(`dayKey de los eventos: ${today} (${tz})`);
}

main()
  .then(() => prisma.$disconnect())
  .catch(async (e) => {
    console.error(e);
    await prisma.$disconnect();
    process.exit(1);
  });

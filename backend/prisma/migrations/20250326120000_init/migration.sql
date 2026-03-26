-- CreateEnum
CREATE TYPE "ClockingAction" AS ENUM (
  'entry',
  'break1Start',
  'break1End',
  'break2Start',
  'break2End',
  'mealStart',
  'mealEnd',
  'exit'
);

-- CreateTable
CREATE TABLE "User" (
    "id" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "email" TEXT NOT NULL,
    "passwordHash" TEXT NOT NULL,
    "isActive" BOOLEAN NOT NULL DEFAULT true,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "User_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "User_email_key" ON "User"("email");

CREATE TABLE "RefreshToken" (
    "id" TEXT NOT NULL,
    "userId" TEXT NOT NULL,
    "tokenHash" TEXT NOT NULL,
    "expiresAt" TIMESTAMP(3) NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "revokedAt" TIMESTAMP(3),

    CONSTRAINT "RefreshToken_pkey" PRIMARY KEY ("id")
);

CREATE INDEX "RefreshToken_userId_idx" ON "RefreshToken"("userId");

ALTER TABLE "RefreshToken" ADD CONSTRAINT "RefreshToken_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE CASCADE ON UPDATE CASCADE;

CREATE TABLE "ClockingConfig" (
    "id" TEXT NOT NULL,
    "userId" TEXT NOT NULL,
    "hasMeal" BOOLEAN NOT NULL,
    "breaks" INTEGER NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL,

    CONSTRAINT "ClockingConfig_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "ClockingConfig_userId_key" ON "ClockingConfig"("userId");

ALTER TABLE "ClockingConfig" ADD CONSTRAINT "ClockingConfig_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE CASCADE ON UPDATE CASCADE;

CREATE TABLE "ClockingEvent" (
    "id" TEXT NOT NULL,
    "userId" TEXT NOT NULL,
    "action" "ClockingAction" NOT NULL,
    "occurredAt" TIMESTAMP(3) NOT NULL,
    "dayKey" TEXT NOT NULL,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "ClockingEvent_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "ClockingEvent_userId_dayKey_action_key" ON "ClockingEvent"("userId", "dayKey", "action");

CREATE INDEX "ClockingEvent_userId_dayKey_idx" ON "ClockingEvent"("userId", "dayKey");

ALTER TABLE "ClockingEvent" ADD CONSTRAINT "ClockingEvent_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE CASCADE ON UPDATE CASCADE;

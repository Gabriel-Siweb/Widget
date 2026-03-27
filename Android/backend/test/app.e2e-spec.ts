import { INestApplication, ValidationPipe } from '@nestjs/common';
import { Test, TestingModule } from '@nestjs/testing';
import request from 'supertest';
import { App } from 'supertest/types';
import { AppModule } from './../src/app.module';
import { PrismaService } from './../src/prisma/prisma.service';

describe('App (e2e)', () => {
  let app: INestApplication<App>;

  const mockPrisma = {
    $connect: jest.fn().mockResolvedValue(undefined),
    $disconnect: jest.fn().mockResolvedValue(undefined),
    user: { findUnique: jest.fn() },
    refreshToken: {
      create: jest.fn(),
      findFirst: jest.fn(),
      updateMany: jest.fn(),
    },
    clockingConfig: { findUnique: jest.fn() },
    clockingEvent: { findMany: jest.fn(), create: jest.fn() },
  };

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    })
      .overrideProvider(PrismaService)
      .useValue(mockPrisma)
      .compile();

    app = moduleFixture.createNestApplication();
    app.useGlobalPipes(
      new ValidationPipe({
        whitelist: true,
        forbidNonWhitelisted: true,
        transform: true,
      }),
    );
    await app.init();
  });

  afterEach(async () => {
    await app.close();
  });

  it('GET /auth/me sin token → 401', () => {
    return request(app.getHttpServer()).get('/auth/me').expect(401);
  });
});

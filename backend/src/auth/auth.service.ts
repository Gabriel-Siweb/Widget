import {
  Injectable,
  UnauthorizedException,
} from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { JwtService } from '@nestjs/jwt';
import * as bcrypt from 'bcrypt';
import { PrismaService } from '../prisma/prisma.service';
import { UsersService } from '../users/users.service';
import { LoginDto } from './dto/login.dto';
import { generateRefreshToken, hashToken } from './token.util';
import { JwtPayload } from './strategies/jwt.strategy';

@Injectable()
export class AuthService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly usersService: UsersService,
    private readonly jwtService: JwtService,
    private readonly config: ConfigService,
  ) {}

  async login(dto: LoginDto) {
    const user = await this.usersService.findByEmail(dto.email);
    if (!user?.isActive) {
      throw new UnauthorizedException('Credenciales inválidas');
    }
    const match = await bcrypt.compare(dto.password, user.passwordHash);
    if (!match) {
      throw new UnauthorizedException('Credenciales inválidas');
    }

    const payload: JwtPayload = { sub: user.id, email: user.email };
    const accessToken = await this.jwtService.signAsync(payload);

    const rawRefresh = generateRefreshToken();
    const tokenHash = hashToken(rawRefresh);
    const expiresAt = this.refreshExpiresAt();

    await this.prisma.refreshToken.create({
      data: {
        userId: user.id,
        tokenHash,
        expiresAt,
      },
    });

    return {
      accessToken,
      refreshToken: rawRefresh,
      user: this.usersService.toPublic(user),
    };
  }

  async refresh(refreshToken: string) {
    const tokenHash = hashToken(refreshToken);
    const row = await this.prisma.refreshToken.findFirst({
      where: {
        tokenHash,
        revokedAt: null,
      },
      include: { user: true },
    });

    if (!row || row.expiresAt.getTime() <= Date.now()) {
      throw new UnauthorizedException('Refresh token inválido o caducado');
    }
    if (!row.user.isActive) {
      throw new UnauthorizedException('Usuario inactivo');
    }

    const payload: JwtPayload = {
      sub: row.user.id,
      email: row.user.email,
    };
    const accessToken = await this.jwtService.signAsync(payload);
    return { accessToken };
  }

  async logout(refreshToken: string) {
    const tokenHash = hashToken(refreshToken);
    await this.prisma.refreshToken.updateMany({
      where: { tokenHash, revokedAt: null },
      data: { revokedAt: new Date() },
    });
    return { success: true as const };
  }

  private refreshExpiresAt(): Date {
    const days = Number(
      this.config.get<string>('JWT_REFRESH_EXPIRES_DAYS') ?? '7',
    );
    const d = new Date();
    d.setDate(d.getDate() + (Number.isFinite(days) ? days : 7));
    return d;
  }
}

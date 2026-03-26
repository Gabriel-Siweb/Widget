import { Body, Controller, Get, Post, Req } from '@nestjs/common';
import { User } from '@prisma/client';
import { Request } from 'express';
import { ClockingAction } from './clocking-sequence';
import { RegisterClockingDto } from './dto/register-clocking.dto';
import { ClockingService } from './clocking.service';

type RequestWithUser = Request & { user: User };

@Controller('clocking')
export class ClockingController {
  constructor(private readonly clockingService: ClockingService) {}

  @Get('config')
  getConfig(@Req() req: RequestWithUser) {
    return this.clockingService.getConfig(req.user.id);
  }

  @Get('today')
  getToday(@Req() req: RequestWithUser) {
    return this.clockingService.getToday(req.user);
  }

  @Post('register')
  register(@Req() req: RequestWithUser, @Body() dto: RegisterClockingDto) {
    return this.clockingService.register(req.user, dto.action as ClockingAction);
  }
}

import { Module } from '@nestjs/common';
import { ClockingController } from './clocking.controller';
import { ClockingService } from './clocking.service';

@Module({
  controllers: [ClockingController],
  providers: [ClockingService],
})
export class ClockingModule {}

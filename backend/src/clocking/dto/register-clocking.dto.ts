import { IsIn, IsString } from 'class-validator';
import { CLOCKING_ACTIONS } from '../clocking-sequence';

const ACTIONS = [...CLOCKING_ACTIONS];

export class RegisterClockingDto {
  @IsString()
  @IsIn(ACTIONS)
  action!: string;
}

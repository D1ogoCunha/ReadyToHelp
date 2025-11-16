import { OccurrenceStatus } from './occurrence-status.enum';
import { OccurrenceType } from './occurrence-type.enum';
import { PriorityLevel } from './priority-level.enum';

export interface OccurrenceDetails {
  id: number;
  title: string;
  description: string;
  type: OccurrenceType;
  status: OccurrenceStatus;
  priority: PriorityLevel;
  latitude: number;
  longitude: number;
  creationDateTime: string;
  endDateTime?: string | null;
  responsibleEntityId?: number | null;
  reportCount: number;
}
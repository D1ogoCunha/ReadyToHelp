import { OccurrenceStatus } from './occurrence-status.enum';
import { OccurrenceType } from './occurrence-type.enum';
import { PriorityLevel } from './priority-level.enum';

/**
 * Detailed information about an occurrence.
 */
export interface OccurrenceDetails {
lastUpdatedDateTime: string|null|undefined;
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
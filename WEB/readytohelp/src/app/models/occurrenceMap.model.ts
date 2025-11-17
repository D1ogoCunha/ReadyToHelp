import { OccurrenceStatus } from "./occurrence-status.enum";
import { OccurrenceType } from "./occurrence-type.enum";
import { PriorityLevel } from "./priority-level.enum";

export interface OccurrenceMap {
  id: number;
  title: string;
  type: OccurrenceType;
  latitude: number;
  longitude: number;
  status: OccurrenceStatus;
  priority: PriorityLevel;
}
import { OccurrenceStatus } from "./occurrence-status.enum";
import { OccurrenceType } from "./occurrence-type.enum";
import { PriorityLevel } from "./priority-level.enum";

/**
 * Represents an occurrence on the map with essential details.
 */
export interface OccurrenceMap {
  id: number;
  title: string;
  type: OccurrenceType;
  latitude: number;
  longitude: number;
  status: OccurrenceStatus;
  priority: PriorityLevel;
}
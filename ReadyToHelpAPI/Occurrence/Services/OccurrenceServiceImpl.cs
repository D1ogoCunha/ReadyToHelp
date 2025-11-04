namespace readytohelpapi.Occurrence.Services;

using System;
using System.Collections.Generic;
using readytohelpapi.Occurrence.Models;
using readytohelpapi.ResponsibleEntity.Services;

/// <summary>
///    Implementation of the occurrence service operations.
/// </summary>
public class OccurrenceServiceImpl : IOccurrenceService
{
    private readonly IOccurrenceRepository occurrenceRepository;
    private readonly IResponsibleEntityService responsibleEntityService;

    /// <summary>
    ///   Initializes a new instance of the <see cref="OccurrenceServiceImpl"/> class.
    /// </summary>
    /// <param name="occurrenceRepository">The occurrence repository instance.</param>
    /// <param name="responsibleEntityService">The responsible entity service instance.</param>
    public OccurrenceServiceImpl(IOccurrenceRepository occurrenceRepository, IResponsibleEntityService responsibleEntityService)
    {
        this.occurrenceRepository = occurrenceRepository;
        this.responsibleEntityService = responsibleEntityService;
    }

    /// <summary>
    /// Creates a new occurrence.
    /// </summary>
    public Occurrence Create(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));
        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException(
                "Occurrence title cannot be null or empty.",
                nameof(occurrence)
            );
        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException(
                "Occurrence description cannot be null or empty.",
                nameof(occurrence)
            );
        if (!Enum.IsDefined(typeof(OccurrenceType), occurrence.Type))
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Invalid occurrence type"
            );
        if (occurrence.ReportCount < 0)
            throw new ArgumentException(
                "ReportCount cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.ReportId.HasValue && occurrence.ReportId < 0)
            throw new ArgumentException(
                "ReportId cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.ResponsibleEntityId < 0)
            throw new ArgumentException(
                "ResponsibleEntityId cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.Location is null)
            throw new ArgumentException(
                "Occurrence location is required.",
                nameof(occurrence)
            );
        if (occurrence.Location.Latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Latitude must be between -90 and 90."
            );
        if (occurrence.Location.Longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Longitude must be between -180 and 180."
            );

        occurrence.CreationDateTime = DateTime.UtcNow;

        if (occurrence.EndDateTime != default && occurrence.EndDateTime <= occurrence.CreationDateTime)
            throw new ArgumentException(
                "EndDateTime must be later than CreationDateTime.",
                nameof(occurrence)
            );

        occurrence.Priority = ComputePriority(occurrence.Type, occurrence.ReportCount);

        if (occurrence.ProximityRadius <= 0)
        {
            occurrence.ProximityRadius = ComputeProximityRadius(
                occurrence.Type,
                occurrence.Priority
            );
        }

        try
        {
            return this.occurrenceRepository.Create(occurrence);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "An error occurred while trying to create an occurrence.",
                e
            );
        }
    }

    /// <inheritdoc />
    public Occurrence CreateAdminOccurrence(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));
        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException(
                "Occurrence title is required.",
                nameof(occurrence)
            );
        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException(
                "Occurrence description is required.",
                nameof(occurrence)
            );
        if (occurrence.Location == null)
            throw new ArgumentException(
                "Occurrence location is required.",
                nameof(occurrence)
            );
        if (!Enum.IsDefined(typeof(OccurrenceType), occurrence.Type))
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Invalid occurrence type"
            );

        var responsibleEntity = responsibleEntityService.FindResponsibleEntity(
            occurrence.Type,
            occurrence.Location.Latitude,
            occurrence.Location.Longitude
        );
        occurrence.ResponsibleEntityId = responsibleEntity?.Id ?? 0;

        occurrence.CreationDateTime = DateTime.UtcNow;
        occurrence.ReportId = null;
        occurrence.ReportCount = 0;
        occurrence.Priority = ComputePriority(occurrence.Type, occurrence.ReportCount);

        if (occurrence.ProximityRadius <= 0)
            occurrence.ProximityRadius = ComputeProximityRadius(occurrence.Type, occurrence.Priority);

        try
        {
            return this.occurrenceRepository.Create(occurrence);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "An error occurred while trying to create an occurrence.",
                e
            );
        }
    }

    /// <inheritdoc />
    public Occurrence Update(Occurrence occurrence)
    {
        if (occurrence == null)
            throw new ArgumentNullException(nameof(occurrence));
        if (occurrence.Id <= 0)
            throw new ArgumentException(
                "Invalid occurrence ID.",
                nameof(occurrence)
            );
        if (string.IsNullOrWhiteSpace(occurrence.Title))
            throw new ArgumentException(
                "Occurrence title cannot be null or empty.",
                nameof(occurrence)
            );
        if (string.IsNullOrWhiteSpace(occurrence.Description))
            throw new ArgumentException(
                "Occurrence description cannot be null or empty.",
                nameof(occurrence)
            );
        if (!Enum.IsDefined(typeof(OccurrenceType), occurrence.Type))
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Invalid occurrence type"
            );
        if (occurrence.ReportCount < 0)
            throw new ArgumentException(
                "ReportCount cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.ReportId.HasValue && occurrence.ReportId < 0)
            throw new ArgumentException(
                "ReportId cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.ResponsibleEntityId < 0)
            throw new ArgumentException(
                "ResponsibleEntityId cannot be negative.",
                nameof(occurrence)
            );
        if (occurrence.EndDateTime != default && occurrence.EndDateTime <= occurrence.CreationDateTime)
            throw new ArgumentException(
                "EndDateTime must be later than CreationDateTime.",
                nameof(occurrence)
            );
        if (occurrence.Location is null)
            throw new ArgumentException(
                "Occurrence location is required.",
                nameof(occurrence)
            );
        if (occurrence.Location.Latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Latitude must be between -90 and 90."
            );
        if (occurrence.Location.Longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(
                nameof(occurrence),
                "Longitude must be between -180 and 180."
            );

        occurrence.Priority = ComputePriority(occurrence.Type, occurrence.ReportCount);
        occurrence.ProximityRadius = ComputeProximityRadius(occurrence.Type, occurrence.Priority);

        try
        {
            var updatedOccurrence = this.occurrenceRepository.Update(occurrence);
            if (updatedOccurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {occurrence.Id} not found.");

            return updatedOccurrence;
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Occurrence with ID {occurrence.Id} not found.");
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid occurrence data.", nameof(occurrence));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "An error occurred while updating the occurrence.",
                e
            );
        }
    }

    /// <inheritdoc />
    public Occurrence Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid occurrence ID.", nameof(id));

        try
        {
            var deletedOccurrence = this.occurrenceRepository.Delete(id);
            if (deletedOccurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {id} not found.");

            return deletedOccurrence;
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Occurrence with ID {id} not found.");
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid occurrence ID.", nameof(id));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "An error occurred while deleting the occurrence.",
                e
            );
        }
    }

    /// <inheritdoc />
    public Occurrence GetOccurrenceById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid occurrence id.");

        var occurrence = this.occurrenceRepository.GetOccurrenceById(id);
        if (occurrence == null)
            throw new KeyNotFoundException($"Occurrence with id {id} not found.");

        return occurrence;
    }

    /// <inheritdoc />
    public List<Occurrence> GetOccurrencesByType(OccurrenceType type)
    {
        return this.occurrenceRepository.GetOccurrencesByType(type);
    }


    /// <inheritdoc />
    public List<Occurrence> GetAllActiveOccurrences(
        int pageNumber,
        int pageSize,
        OccurrenceType? type,
        PriorityLevel? priority,
        int? responsibleEntityId
    )
    {
        if (pageNumber <= 0)
            throw new ArgumentException(
                "Page number must be greater than zero.",
                nameof(pageNumber)
            );
        if (pageSize <= 0 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000.", nameof(pageSize));
        if (type.HasValue && !Enum.IsDefined(typeof(OccurrenceType), type.Value))
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid occurrence type");
        if (priority.HasValue && !Enum.IsDefined(typeof(PriorityLevel), priority.Value))
            throw new ArgumentOutOfRangeException(nameof(priority), "Invalid priority level");
        if (responsibleEntityId.HasValue && responsibleEntityId.Value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(responsibleEntityId),
                "ResponsibleEntityId cannot be negative"
            );

        return occurrenceRepository.GetAllActiveOccurrences(
            pageNumber,
            pageSize,
            type,
            priority,
            responsibleEntityId
        );
    }

    /// <inheritdoc />
    public List<Occurrence> GetAllOccurrences(
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortOrder,
        string filter
    )
    {
        if (string.IsNullOrEmpty(sortBy))
            throw new ArgumentException("Sort field cannot be null or empty.", nameof(sortBy));

        if (sortOrder != "asc" && sortOrder != "desc")
            throw new ArgumentException("Sort order must be 'asc' or 'desc'.", nameof(sortOrder));

        if (pageNumber <= 0)
            throw new ArgumentException(
                "Page number must be greater than zero.",
                nameof(pageNumber)
            );

        if (pageSize <= 0 || pageSize > 1000)
            throw new ArgumentException("Page size must be between 1 and 1000.", nameof(pageSize));

        try
        {
            var occurrences = this.occurrenceRepository.GetAllOccurrences(
                pageNumber,
                pageSize,
                sortBy,
                sortOrder,
                filter
            );
            return occurrences ?? new List<Occurrence>();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "An error occurred while retrieving occurrences.",
                e
            );
        }
    }

    /// <summary>
    ///  Computes the base priority level based on occurrence type.
    /// </summary>
    private static PriorityLevel ComputeBasePriority(OccurrenceType type)
    {
        return type switch
        {
            OccurrenceType.FOREST_FIRE
            or OccurrenceType.URBAN_FIRE
            or OccurrenceType.FLOOD
            or OccurrenceType.LANDSLIDE
            or OccurrenceType.ROAD_ACCIDENT
            or OccurrenceType.CRIME
            or OccurrenceType.DOMESTIC_VIOLENCE
            or OccurrenceType.MEDICAL_EMERGENCY
            or OccurrenceType.WORK_ACCIDENT => PriorityLevel.HIGH,

            OccurrenceType.VEHICLE_BREAKDOWN
            or OccurrenceType.ANIMAL_ON_ROAD
            or OccurrenceType.ROAD_OBSTRUCTION
            or OccurrenceType.TRAFFIC_CONGESTION
            or OccurrenceType.ELECTRICAL_NETWORK
            or OccurrenceType.SANITATION
            or OccurrenceType.PUBLIC_DISTURBANCE
            or OccurrenceType.INJURED_ANIMAL
            or OccurrenceType.POLLUTION => PriorityLevel.MEDIUM,

            OccurrenceType.PUBLIC_LIGHTING
            or OccurrenceType.ROAD_DAMAGE
            or OccurrenceType.TRAFFIC_LIGHT_FAILURE
            or OccurrenceType.LOST_ANIMAL => PriorityLevel.LOW,

            _ => PriorityLevel.LOW,
        };
    }

    /// <summary>
    ///  Computes the priority level based on occurrence type and report count.
    /// </summary>
    private static PriorityLevel ComputePriority(OccurrenceType type, int reportCount)
    {
        var basePriority = ComputeBasePriority(type);

        if (basePriority == PriorityLevel.HIGH)
            return PriorityLevel.HIGH;

        if (basePriority == PriorityLevel.MEDIUM)
            return reportCount >= 5 ? PriorityLevel.HIGH : PriorityLevel.MEDIUM;

        return reportCount >= 7 ? PriorityLevel.MEDIUM : PriorityLevel.LOW;
    }

    /// <summary>
    /// Computes the proximity radius (meters) based on occurrence type and priority.
    /// </summary>
    private static double ComputeProximityRadius(OccurrenceType type, PriorityLevel priority)
    {
        var baseRadius = type switch
        {
            OccurrenceType.FOREST_FIRE => 2500.0,
            OccurrenceType.URBAN_FIRE => 1500.0,
            OccurrenceType.FLOOD => 2000.0,
            OccurrenceType.LANDSLIDE => 500.0,
            OccurrenceType.ROAD_ACCIDENT => 400.0,
            OccurrenceType.VEHICLE_BREAKDOWN => 125.0,
            OccurrenceType.ANIMAL_ON_ROAD => 150.0,
            OccurrenceType.ROAD_OBSTRUCTION => 200.0,
            OccurrenceType.TRAFFIC_CONGESTION => 200.0,
            OccurrenceType.PUBLIC_LIGHTING => 100.0,
            OccurrenceType.SANITATION => 150.0,
            OccurrenceType.ELECTRICAL_NETWORK => 300.0,
            OccurrenceType.ROAD_DAMAGE => 200.0,
            OccurrenceType.TRAFFIC_LIGHT_FAILURE => 100.0,
            OccurrenceType.CRIME => 300.0,
            OccurrenceType.PUBLIC_DISTURBANCE => 300.0,
            OccurrenceType.DOMESTIC_VIOLENCE => 200.0,
            OccurrenceType.LOST_ANIMAL => 250.0,
            OccurrenceType.INJURED_ANIMAL => 300.0,
            OccurrenceType.POLLUTION => 750.0,
            OccurrenceType.MEDICAL_EMERGENCY => 1000.0,
            OccurrenceType.WORK_ACCIDENT => 300.0,
            _ => 300.0,
        };

        var multiplier = priority switch
        {
            PriorityLevel.HIGH => 2.0,
            PriorityLevel.MEDIUM => 1.5,
            _ => 1.0,
        };

        return baseRadius * multiplier;
    }
}

using readytohelpapi.ResponsibleEntity.Models;

namespace readytohelpapi.Occurrence.Models;

/// <summary>
/// Mapping extensions between OccurrenceType and ResponsibleEntityType.
/// </summary>
public static class OccurrenceTypeExtensions
{
    private static readonly Dictionary<OccurrenceType, ResponsibleEntityType> ResponsibleEntityMap = new()
    {
        { OccurrenceType.FOREST_FIRE, ResponsibleEntityType.BOMBEIROS },
        { OccurrenceType.URBAN_FIRE, ResponsibleEntityType.BOMBEIROS },

        { OccurrenceType.FLOOD, ResponsibleEntityType.PROTECAO_CIVIL },
        { OccurrenceType.LANDSLIDE, ResponsibleEntityType.PROTECAO_CIVIL },

        { OccurrenceType.ROAD_ACCIDENT, ResponsibleEntityType.POLICIA },
        { OccurrenceType.ANIMAL_ON_ROAD, ResponsibleEntityType.POLICIA },
        { OccurrenceType.TRAFFIC_CONGESTION, ResponsibleEntityType.POLICIA },
        { OccurrenceType.CRIME, ResponsibleEntityType.POLICIA },
        { OccurrenceType.PUBLIC_DISTURBANCE, ResponsibleEntityType.POLICIA },
        { OccurrenceType.DOMESTIC_VIOLENCE, ResponsibleEntityType.POLICIA },

        { OccurrenceType.PUBLIC_LIGHTING, ResponsibleEntityType.CAMARA_MUNICIPAL },
        { OccurrenceType.SANITATION, ResponsibleEntityType.CAMARA_MUNICIPAL },

        { OccurrenceType.ROAD_DAMAGE, ResponsibleEntityType.INFRAESTRUTURAS },
        { OccurrenceType.ROAD_OBSTRUCTION, ResponsibleEntityType.INFRAESTRUTURAS },
        { OccurrenceType.TRAFFIC_LIGHT_FAILURE, ResponsibleEntityType.INFRAESTRUTURAS },
        { OccurrenceType.ELECTRICAL_NETWORK, ResponsibleEntityType.INFRAESTRUTURAS },
        { OccurrenceType.VEHICLE_BREAKDOWN, ResponsibleEntityType.INFRAESTRUTURAS },

        { OccurrenceType.LOST_ANIMAL, ResponsibleEntityType.SERVICOS_VETERINARIOS },
        { OccurrenceType.INJURED_ANIMAL, ResponsibleEntityType.SERVICOS_VETERINARIOS },

        { OccurrenceType.POLLUTION, ResponsibleEntityType.AMBIENTE },

        { OccurrenceType.MEDICAL_EMERGENCY, ResponsibleEntityType.INEM },

        { OccurrenceType.WORK_ACCIDENT, ResponsibleEntityType.ACT }
    };

    /// <summary>
    /// Returns the responsible entity type for a given occurrence type.
    /// </summary>
    /// <param name="type">The occurrence type.</param>
    public static ResponsibleEntityType GetResponsibleEntityType(this OccurrenceType type)
    {
        if (ResponsibleEntityMap.TryGetValue(type, out var entityType))
            return entityType;

        throw new ArgumentException($"No responsible entity mapped for OccurrenceType: {type}");
    }

    /// <summary>
    /// Returns all occurrence types handled by a given responsible entity type.
    /// </summary>
    /// <param name="entityType">The responsible entity type.</param>
    public static IEnumerable<OccurrenceType> GetHandledOccurrenceTypes(this ResponsibleEntityType entityType)
    {
        return ResponsibleEntityMap
            .Where(kvp => kvp.Value == entityType)
            .Select(kvp => kvp.Key);
    }
}
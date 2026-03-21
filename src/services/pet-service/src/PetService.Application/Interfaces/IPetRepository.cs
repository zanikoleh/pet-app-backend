using PetService.Domain.Aggregates;
using PetService.Domain.Specifications;
using SharedKernel;
using SharedKernel.Infrastructure;

namespace PetService.Application.Handlers;

/// <summary>
/// Repository interface for Pet aggregate.
/// </summary>
public interface IPetRepository : IRepository<Pet, Guid>
{
    // Add custom query methods specific to Pet aggregate if needed
}

// Additional specifications for count queries
public sealed class PetsByOwnerCountSpecification : Specification<Pet>
{
    public PetsByOwnerCountSpecification(Guid ownerId)
    {
        Criteria = p => p.OwnerId == ownerId;
    }
}

public sealed class SearchPetsByNameCountSpecification : Specification<Pet>
{
    public SearchPetsByNameCountSpecification(Guid ownerId, string searchTerm)
    {
        Criteria = p => p.OwnerId == ownerId && p.Name.Contains(searchTerm);
    }
}

public sealed class PetsByOwnerAndTypeCountSpecification : Specification<Pet>
{
    public PetsByOwnerAndTypeCountSpecification(Guid ownerId, string petType)
    {
        Criteria = p => p.OwnerId == ownerId && p.Type.Value == petType.ToLower();
    }
}

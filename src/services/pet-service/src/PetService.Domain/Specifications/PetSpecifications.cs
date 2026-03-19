using PetService.Domain.Aggregates;

namespace PetService.Domain.Specifications;

/// <summary>
/// Specification for querying pets by owner
/// </summary>
public sealed class GetPetsByOwnerSpecification : Specification<Pet, Pet>
{
    public GetPetsByOwnerSpecification(Guid ownerId, int page = 1, int pageSize = 10)
    {
        Criteria = p => p.OwnerId == ownerId;
        OrderByDescending = p => p.CreatedAt;

        var skip = (page - 1) * pageSize;
        ApplyPaging(skip, pageSize);
    }
}

/// <summary>
/// Specification for getting a single pet by ID and owner
/// </summary>
public sealed class GetPetByIdAndOwnerSpecification : Specification<Pet, Pet>
{
    public GetPetByIdAndOwnerSpecification(Guid petId, Guid ownerId)
    {
        Criteria = p => p.Id == petId && p.OwnerId == ownerId;
    }
}

/// <summary>
/// Specification for searching pets by name
/// </summary>
public sealed class SearchPetsByNameSpecification : Specification<Pet, Pet>
{
    public SearchPetsByNameSpecification(Guid ownerId, string searchTerm, int page = 1, int pageSize = 10)
    {
        Criteria = p => p.OwnerId == ownerId && p.Name.Contains(searchTerm);
        OrderByDescending = p => p.CreatedAt;

        var skip = (page - 1) * pageSize;
        ApplyPaging(skip, pageSize);
    }
}

/// <summary>
/// Specification for getting all pets of a specific type owned by a user
/// </summary>
public sealed class GetPetsByOwnerAndTypeSpecification : Specification<Pet, Pet>
{
    public GetPetsByOwnerAndTypeSpecification(Guid ownerId, string petType, int page = 1, int pageSize = 10)
    {
        Criteria = p => p.OwnerId == ownerId && p.Type.Value == petType.ToLower();
        OrderByDescending = p => p.CreatedAt;

        var skip = (page - 1) * pageSize;
        ApplyPaging(skip, pageSize);
    }
}

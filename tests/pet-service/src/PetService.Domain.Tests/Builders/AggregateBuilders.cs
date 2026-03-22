using PetService.Domain.Aggregates;
using PetService.Domain.Entities;
using PetService.Domain.ValueObjects;

namespace PetService.Domain.Tests.Builders;

/// <summary>
/// Builder for creating Pet aggregate test objects with fluent interface
/// </summary>
public class PetAggregateBuilder
{
    private Guid _ownerId = Guid.NewGuid();
    private string _name = "Fluffy";
    private PetType _type = PetType.Cat;
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-2);
    private Breed? _breed;
    private string? _description;

    public PetAggregateBuilder WithOwnerId(Guid ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public PetAggregateBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public PetAggregateBuilder WithType(PetType type)
    {
        _type = type;
        return this;
    }

    public PetAggregateBuilder WithType(string typeName)
    {
        _type = PetType.FromString(typeName);
        return this;
    }

    public PetAggregateBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public PetAggregateBuilder WithBreed(Breed? breed)
    {
        _breed = breed;
        return this;
    }

    public PetAggregateBuilder WithBreed(string? breedName)
    {
        _breed = breedName != null ? Breed.Create(breedName) : null;
        return this;
    }

    public PetAggregateBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public Pet Build()
    {
        return new Pet(_ownerId, _name, _type, _dateOfBirth, _breed, _description);
    }
}

/// <summary>
/// Builder for creating Photo test objects
/// </summary>
public class PhotoBuilder
{
    private Guid _petId = Guid.NewGuid();
    private Guid _photoId = Guid.NewGuid();
    private string _fileName = "photo.jpg";
    private string _fileType = "image/jpeg";
    private long _fileSizeBytes = 1024000;
    private string _url = "https://example.com/photo.jpg";
    private string? _caption;
    private string? _tags;

    public PhotoBuilder WithPetId(Guid petId)
    {
        _petId = petId;
        return this;
    }

    public PhotoBuilder WithPhotoId(Guid photoId)
    {
        _photoId = photoId;
        return this;
    }

    public PhotoBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public PhotoBuilder WithFileType(string fileType)
    {
        _fileType = fileType;
        return this;
    }

    public PhotoBuilder WithFileSizeBytes(long fileSizeBytes)
    {
        _fileSizeBytes = fileSizeBytes;
        return this;
    }

    public PhotoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public PhotoBuilder WithCaption(string? caption)
    {
        _caption = caption;
        return this;
    }

    public PhotoBuilder WithTags(string? tags)
    {
        _tags = tags;
        return this;
    }

    public Photo Build()
    {
        return new Photo(_petId, _photoId, _fileName, _fileType, _fileSizeBytes, _url, _caption, _tags);
    }
}

/// <summary>
/// Builder for creating Document test objects
/// </summary>
public class DocumentBuilder
{
    private Guid _petId = Guid.NewGuid();
    private Guid _documentId = Guid.NewGuid();
    private string _fileName = "document.pdf";
    private string _fileType = "application/pdf";
    private long _fileSizeBytes = 256000;
    private string _url = "https://example.com/document.pdf";
    private string _category = "vaccination";
    private string? _description;

    public DocumentBuilder WithPetId(Guid petId)
    {
        _petId = petId;
        return this;
    }

    public DocumentBuilder WithDocumentId(Guid documentId)
    {
        _documentId = documentId;
        return this;
    }

    public DocumentBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    public DocumentBuilder WithFileType(string fileType)
    {
        _fileType = fileType;
        return this;
    }

    public DocumentBuilder WithFileSizeBytes(long fileSizeBytes)
    {
        _fileSizeBytes = fileSizeBytes;
        return this;
    }

    public DocumentBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public DocumentBuilder WithCategory(string category)
    {
        _category = category;
        return this;
    }

    public DocumentBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public Document Build()
    {
        return new Document(_petId, _documentId, _fileName, _fileType, _fileSizeBytes, _url, _category, _description);
    }
}

using FluentAssertions;
using PetService.Domain.ValueObjects;
using SharedKernel;
using Xunit;

namespace PetService.Domain.Tests.ValueObjects;

public class PetTypeTests
{
    [Fact]
    public void Create_ValidValue_ShouldCreatePetType()
    {
        // Act
        var petType = PetType.Create("dog");

        // Assert
        petType.Should().NotBeNull();
        petType.Value.Should().Be("dog");
    }

    [Fact]
    public void Create_UpperCaseValue_ShouldNormalizeToLowerCase()
    {
        // Act
        var petType = PetType.Create("CAT");

        // Assert
        petType.Value.Should().Be("cat");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidValue_ShouldThrowDomainException(string invalidValue)
    {
        // Act & Assert
        var exception = Record.Exception(() => PetType.Create(invalidValue));
        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void PredefinedTypes_ShouldBeAvailable()
    {
        // Assert
        PetType.Dog.Value.Should().Be("dog");
        PetType.Cat.Value.Should().Be("cat");
        PetType.Bird.Value.Should().Be("bird");
        PetType.Rabbit.Value.Should().Be("rabbit");
        PetType.Hamster.Value.Should().Be("hamster");
        PetType.Fish.Value.Should().Be("fish");
        PetType.Snake.Value.Should().Be("snake");
        PetType.Other.Value.Should().Be("other");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var petType1 = PetType.Create("dog");
        var petType2 = PetType.Create("dog");

        // Act & Assert
        petType1.Should().Be(petType2);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var petType1 = PetType.Create("dog");
        var petType2 = PetType.Create("cat");

        // Act & Assert
        petType1.Should().NotBe(petType2);
    }

    [Fact]
    public void FromString_KnownType_ShouldReturnPredefinedType()
    {
        // Act
        var petType = PetType.FromString("cat");

        // Assert
        petType.Should().Be(PetType.Cat);
    }

    [Fact]
    public void FromString_UnknownType_ShouldCreateNewType()
    {
        // Act
        var petType = PetType.FromString("exotic");

        // Assert
        petType.Value.Should().Be("exotic");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var petType = PetType.Dog;

        // Act
        var result = petType.ToString();

        // Assert
        result.Should().Be("dog");
    }
}

public class BreedTests
{
    [Fact]
    public void Create_ValidValue_ShouldCreateBreed()
    {
        // Act
        var breed = Breed.Create("Siamese");

        // Assert
        breed.Should().NotBeNull();
        breed.Value.Should().Be("Siamese");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidValue_ShouldThrowDomainException(string invalidValue)
    {
        // Act & Assert
        var exception = Record.Exception(() => Breed.Create(invalidValue));
        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void Create_ExceedsMaxLength_ShouldThrowDomainException()
    {
        // Arrange
        var tooLongBreed = new string('a', 101);

        // Act & Assert
        var exception = Record.Exception(() => Breed.Create(tooLongBreed));
        exception.Should().NotBeNull();
        exception.Should().BeOfType<DomainException>();
    }

    [Fact]
    public void Create_MaxLengthBreed_ShouldSucceed()
    {
        // Arrange
        var maxLengthBreed = new string('a', 100);

        // Act
        var breed = Breed.Create(maxLengthBreed);

        // Assert
        breed.Value.Should().HaveLength(100);
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var breed1 = Breed.Create("Persian");
        var breed2 = Breed.Create("Persian");

        // Act & Assert
        breed1.Should().Be(breed2);
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var breed1 = Breed.Create("Persian");
        var breed2 = Breed.Create("Siamese");

        // Act & Assert
        breed1.Should().NotBe(breed2);
    }

    [Fact]
    public void Equality_CaseSensitive_ShouldNotBeEqual()
    {
        // Arrange
        var breed1 = Breed.Create("Persian");
        var breed2 = Breed.Create("persian");

        // Act & Assert
        breed1.Should().NotBe(breed2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var breed = Breed.Create("Labrador Retriever");

        // Act
        var result = breed.ToString();

        // Assert
        result.Should().Be("Labrador Retriever");
    }
}

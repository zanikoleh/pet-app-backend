namespace SharedKernel;

public abstract class Specification<T>
{
    /// <summary>
    /// The criteria function that defines this specification
    /// </summary>
    protected Func<T, bool>? Criteria { get; set; }
    
    /// <summary>
    /// Optional ordering function for results
    /// </summary>
    protected Func<T, object>? OrderByDescending { get; set; }

    /// <summary>
    /// Checks if the given object satisfies this specification
    /// </summary>
    public virtual bool IsSatisfiedBy(T candidate)
    {
        return Criteria?.Invoke(candidate) ?? true;
    }
    
    /// <summary>
    /// Combines this specification with another using AND operator
    /// </summary>
    public virtual Specification<T> And(Specification<T> other)
    {
        var newCriteria = (T candidate) => 
            (Criteria?.Invoke(candidate) ?? true) && 
            (other.Criteria?.Invoke(candidate) ?? true);
            
        return new CombinedSpecification<T>(newCriteria, this, other);
    }
    
    /// <summary>
    /// Combines this specification with another using OR operator
    /// </summary>
    public virtual Specification<T> Or(Specification<T> other)
    {
        var newCriteria = (T candidate) => 
            (Criteria?.Invoke(candidate) ?? false) || 
            (other.Criteria?.Invoke(candidate) ?? false);
            
        return new CombinedSpecification<T>(newCriteria, this, other);
    }
    
    /// <summary>
    /// Negates this specification
    /// </summary>
    public virtual Specification<T> Not()
    {
        var newCriteria = (T candidate) => !(Criteria?.Invoke(candidate) ?? false);
        return new NotSpecification<T>(newCriteria, this);
    }
}

/// <summary>
/// A combined specification that represents the combination of two specifications
/// </summary>
public class CombinedSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public CombinedSpecification(Func<T, bool> criteria, Specification<T> left, Specification<T> right)
    {
        Criteria = criteria;
        _left = left;
        _right = right;
    }
}

/// <summary>
/// A negated specification
/// </summary>
public class NotSpecification<T> : Specification<T>
{
    public NotSpecification(Func<T, bool> criteria, Specification<T> original)
    {
        Criteria = criteria;
    }
}

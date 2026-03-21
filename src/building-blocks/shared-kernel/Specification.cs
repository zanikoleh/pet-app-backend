namespace SharedKernel;

public abstract class Specification<T1>
{
    protected Func<T1, bool>? Criteria;
    protected Func<T1, object>? OrderByDescending;
    
    protected virtual void ApplyPaging(int skip, int pageSize)
    {
        
    }
}
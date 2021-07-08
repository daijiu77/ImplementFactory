namespace System.DJ.ImplementFactory.Pipelines
{
    public interface IClone
    {
        T Clone<T>(object obj);
        T Clone<T>();
    }
}

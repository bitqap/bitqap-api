namespace Bitqap.Middleware.Business.Extensions
{
    public interface IPropertyMergeExtension
    {
        T Merge<T>(T request, T map);
    }
}

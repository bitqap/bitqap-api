namespace Bitqap.Middleware.Business.Extensions
{
    public interface IMappingExtension
    {
        TDestination Map<TSource, TDestination>(TSource source) where TDestination : class;
    }
}

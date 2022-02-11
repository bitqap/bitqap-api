using AutoMapper;

namespace Bitqap.Middleware.Business.Extensions
{
    public class MappingExtension : IMappingExtension
    {
        public TDestination Map<TSource, TDestination>(TSource source) where TDestination : class
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<TSource, TDestination>();
            });
            IMapper mapper = config.CreateMapper();

            return mapper.Map<TSource, TDestination>(source);
        }
    }
}

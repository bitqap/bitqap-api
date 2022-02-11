using System.ComponentModel;

namespace Bitqap.Middleware.Business.Extensions
{
    public class PropertyMergeExtension : IPropertyMergeExtension
    {
        public T Merge<T>(T request, T map)
        {
            var result = (T)Activator.CreateInstance(typeof(T), null);

            var properties = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor property in properties)
            {
                var requestValue = property.GetValue(request);
                var mapValue = property.GetValue(map);
                if (requestValue == null || String.IsNullOrEmpty(requestValue.ToString()))
                {
                    property.SetValue(result, mapValue);
                }
                else
                {
                    property.SetValue(result, requestValue);
                }
            }
            return result;
        }
    }
}

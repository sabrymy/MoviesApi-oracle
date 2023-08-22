using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApi.Helper
{
    //A class implement IModelBinder to enable model to bind array of int into list of int values when send array of int via client
    public class TypeBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //key for looking up values in IvalueProvider
            var propertyName = bindingContext.ModelName;
            //LOOKING UP FOR KEY TO CATCH PROPERTY VALUE
            var valueProviderResult = bindingContext.ValueProvider.GetValue(propertyName);
            //if property had not been sent do nothing
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            try

            {
                //convert json format to List<int> as one type of .net
                var deserialized = JsonConvert.DeserializeObject<T>(valueProviderResult.FirstValue);
                //set the result of model binding process
                bindingContext.Result = ModelBindingResult.Success(deserialized);
            }
            catch (Exception)
            {
                //add error message to error state entry
                bindingContext.ModelState.TryAddModelError(propertyName, "Not a valid List of integers");
              //  throw new BadHttpRequestException("error not a valid list of integers");
            }
            return Task.CompletedTask;
        }
    }
}

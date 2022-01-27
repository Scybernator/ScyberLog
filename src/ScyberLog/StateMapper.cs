using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace ScyberLog
{
    public interface IStateMapper
    {
        bool Map<TState>(TState state, Func<TState, Exception, string> formatter, out object outObject, out Func<object, Exception, string> outFormatter);
    }

    public class FormattedLogValuesMapper : IStateMapper
    {
        private bool IncludeOriginalFormat { get; }

        public FormattedLogValuesMapper(IOptions<ScyberLogConfiguration> config)
        {
            this.IncludeOriginalFormat = config.Value.IncludeOriginalFormat;
        }

        public bool Map<TState>(TState state, Func<TState, Exception, string> formatter, out object outObject, out Func<object, Exception, string> outFormatter)
        {           
             //internal type Microsoft.Extensions.Logging.FormattedLogValues, used by built in extension methods
            if (state is IReadOnlyList<KeyValuePair<string, object>> formattedLogValues)
            {
                var wrapper = new FormattedLogValuesWrapper(formattedLogValues, this.IncludeOriginalFormat);
                //If this is a text-only message, we want to state to be null
                if(wrapper.Values == null && wrapper.Data == null)
                {
                    outObject = null;
                }else
                {
                    outObject = wrapper;
                }
                outFormatter = (_, ex) => formatter(state, ex);
                return true;
            }

            outObject = null;
            outFormatter = null;
            return false; 
        }
    }
}
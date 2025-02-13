using System;
using System.Collections.Generic;
using System.Linq;

namespace ScyberLog
{
    public class FormattedLogValuesWrapper
    {
        private IReadOnlyList<KeyValuePair<string, object>> FormattedLogValues { get; }

        public IDictionary<string, object> Data { get; }

        public IEnumerable<object> Values { get; }

        public FormattedLogValuesWrapper(IReadOnlyList<KeyValuePair<string, object>> formattedLogValues, bool includeOriginalFormat = false)
        {
            if (formattedLogValues == null) { throw new ArgumentNullException(nameof(formattedLogValues)); }

            var values = formattedLogValues.GetValues();
            var namedValues = formattedLogValues.Select(x => x.Value);
            this.FormattedLogValues = formattedLogValues;
            this.Values = values.Except(namedValues);
            this.Data = formattedLogValues.Where(x => includeOriginalFormat || x.Key != "{OriginalFormat}").ToDictionary(x => x.Key, x => x.Value);
            //We want these fields to be null if empty, so they are omitted from the json for brevity
            if (!this.Data.Any()) { this.Data = null; }
            if (!this.Values.Any()) { this.Values = null; }
        }

        public override string ToString() => this.FormattedLogValues.ToString();
    }
}
using Dapper;
using System.Data;

namespace SmartUtility.Sealed
{
    public class TimeOnlyHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(object value)
        {
            if (value is TimeOnly timeOnly)
            {
                return timeOnly;
            }
            else if (value is TimeSpan timeSpan)
            {
                return TimeOnly.FromTimeSpan(timeSpan);
            }
            else if (value is string timeString && TimeOnly.TryParse(timeString, out var parsedTime))
            {
                return parsedTime;
            }
            else
            {
                throw new DataException($"Cannot convert {value} to TimeOnly.");
            }
        }

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            throw new NotImplementedException();
        }
    }
}

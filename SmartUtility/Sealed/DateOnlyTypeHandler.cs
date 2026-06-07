using Dapper;
using System.Data;

namespace SmartUtility.Sealed
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            else if (value is string dateString && DateOnly.TryParse(dateString, out var dateOnly))
            {
                return dateOnly;
            }
            else
            {
                throw new DataException($"Cannot convert {value} to DateOnly.");
            }
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);

            parameter.DbType = DbType.Date; 
        }
    }
}

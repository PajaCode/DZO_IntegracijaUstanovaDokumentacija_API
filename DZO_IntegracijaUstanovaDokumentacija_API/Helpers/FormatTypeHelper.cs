
namespace HR_API.Helpers;
/// <summary>
/// 
/// </summary>
public class FormatTypeHelper
{
    /// <summary>
    /// Metoda prima parametar tipa DateTime koji sadrzi koordinisano univerzalno vreme
    /// </summary>
    /// <param name="datum"></param>
    /// <returns>Podatak tipa DateTime koji je konvertovan iz UTC u GMT</returns>
    public DateTime ConvertDateTimeFromFront(DateTime? datum)
    {
        DateTime datumReturn = new();

        if (datum is not null)
        {
            TimeZoneInfo zona = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

            datumReturn = TimeZoneInfo.ConvertTimeFromUtc((DateTime)datum, zona);
        }

        return datumReturn;
    }

    /// <summary>
    /// Metoda koja uzima objekat i pretvara ga u objekat tipa DataTable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    public static DataTable ToDataTableFromObject<T>(T item)
    {
        DataTable dataTable = new(typeof(T).Name);

        // Get all the properties
        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo prop in props)
        {
            // Define the type of data column to create the proper data table
            var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) ?
                Nullable.GetUnderlyingType(prop.PropertyType) :
                prop.PropertyType;

            // Set column names as property names
            dataTable.Columns.Add(prop.Name, type);
        }

        // Insert property values into DataTable rows for the single object
        var values = new object[props.Length];

        for (int i = 0; i < props.Length; i++)
        {
            values[i] = props[i].GetValue(item, null)!;
        }

        dataTable.Rows.Add(values);

        // Put a breakpoint here and check DataTable
        return dataTable;
    }

    /// <summary>
    /// lista u tabelu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static DataTable ToDataTableFromList<T>(List<T> items)
    {
        DataTable dataTable = new(typeof(T).Name);
        //Get all the properties
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in Props)
        {
            //Defining type of data column gives proper data table
            var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
            //Setting column names as Property names
            dataTable.Columns.Add(prop.Name, type);
        }
        foreach (T item in items)
        {
            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                //inserting property values to datatable rows
                values[i] = Props[i].GetValue(item, null)!;
            }
            dataTable.Rows.Add(values);
        }
        //put a breakpoint here and check datatable
        return dataTable;
    }
}

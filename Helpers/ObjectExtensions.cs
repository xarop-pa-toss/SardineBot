using System.CodeDom;
using SardineBot.Modules.Models;
namespace SardineBot.Database;

public static class ObjectExtensions
{
    /// <summary>
    /// Merges the properties of two class objects of the same type into a new one, using null as the exclusion factor.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns>New merged object of same type</returns>
    /// <exception cref="ArgumentException">Null check</exception>
    public static T Merge<T> (T? obj1, T? obj2) where T: class, new()
    {
        var _obj1 = obj1;
        var _obj2 = obj2;
        
        if (_obj1 is null || _obj2 is null)
        {
            throw new ArgumentException("Objects must not be null");
        }

        var mergedObj = new T();
        foreach (var prop in typeof(T).GetProperties())
        {
            var value1 = prop.GetValue(obj1);
            var value2 = prop.GetValue(obj2);

            if (prop.PropertyType == typeof(string))
            {
                var str1 = value1 as string;
                var str2 = value2 as string;
                prop.SetValue(mergedObj, !string.IsNullOrEmpty(str1) ? str1 : str2);
            }
            else
            {
                prop.SetValue(mergedObj, value1 ?? value2);
            }
        }
        return mergedObj;
    }
}
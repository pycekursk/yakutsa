using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace yakutsa.Services
{
  //
  // Сводка:
  //     QueryBuilder
  public class QueryBuilder
  {
    //
    // Сводка:
    //     Entry
    private struct Entry
    {
      public string Key;

      public object Value;
    }

    private readonly List<KeyValuePair<string, object>> _keyValuePairs = new List<KeyValuePair<string, object>>();

    //
    // Сводка:
    //     Build PHP like query string
    //
    // Параметры:
    //   queryData:
    //
    //   argSeperator:
    public static string BuildQueryString(object queryData, string argSeperator = "&")
    {
      QueryBuilder queryBuilder = new QueryBuilder();
      queryBuilder.AddEntry(null, queryData, allowObjects: true);
      return queryBuilder.GetUriString(argSeperator);
    }

    //
    // Сводка:
    //     GetUriString
    //
    // Параметры:
    //   argSeperator:
    private string GetUriString(string argSeperator)
    {
      return string.Join(argSeperator, _keyValuePairs.Select(delegate (KeyValuePair<string, object> kvp)
      {
        string str = HttpUtility.UrlEncode(kvp.Key);
        string str2 = HttpUtility.UrlEncode(kvp.Value.ToString());
        return str + "=" + str2;
      }));
    }

    //
    // Сводка:
    //     AddEntry
    //
    // Параметры:
    //   prefix:
    //
    //   instance:
    //
    //   allowObjects:
    private void AddEntry(string prefix, object instance, bool allowObjects)
    {
      IDictionary dictionary = instance as IDictionary;
      ICollection collection = instance as ICollection;
      if (dictionary != null)
      {
        Add(prefix, GetDictionaryAdapter(dictionary));
      }
      else if (collection != null)
      {
        Add(prefix, GetArrayAdapter(collection));
      }
      else if (allowObjects)
      {
        Add(prefix, GetObjectAdapter(instance));
      }
      else
      {
        _keyValuePairs.Add(new KeyValuePair<string, object>(prefix, instance));
      }
    }

    //
    // Сводка:
    //     Add
    //
    // Параметры:
    //   prefix:
    //
    //   datas:
    private void Add(string prefix, IEnumerable<Entry> datas)
    {
      foreach (Entry data in datas)
      {
        string prefix2 = string.IsNullOrEmpty(prefix) ? data.Key : (prefix + "[" + data.Key + "]");
        AddEntry(prefix2, data.Value, allowObjects: false);
      }
    }

    //
    // Сводка:
    //     GetObjectAdapter
    //
    // Параметры:
    //   data:
    private IEnumerable<Entry> GetObjectAdapter(object data)
    {
      PropertyInfo[] properties = data.GetType().GetProperties();
      PropertyInfo[] array = properties;
      foreach (PropertyInfo propertyInfo in array)
      {
        yield return new Entry
        {
          Key = propertyInfo.Name,
          Value = propertyInfo.GetValue(data)
        };
      }
    }

    //
    // Сводка:
    //     GetArrayAdapter
    //
    // Параметры:
    //   collection:
    private IEnumerable<Entry> GetArrayAdapter(ICollection collection)
    {
      int i = 0;
      foreach (object item in collection)
      {
        yield return new Entry
        {
          Key = i.ToString(),
          Value = item
        };
        i++;
      }
    }

    //
    // Сводка:
    //     GetDictionaryAdapter
    //
    // Параметры:
    //   collection:
    private IEnumerable<Entry> GetDictionaryAdapter(IDictionary collection)
    {
      foreach (DictionaryEntry item in collection)
      {
        yield return new Entry
        {
          Key = item.Key.ToString(),
          Value = item.Value
        };
      }
    }
  }
}

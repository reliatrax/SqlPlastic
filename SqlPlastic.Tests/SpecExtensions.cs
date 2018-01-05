using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SqlPlastic.Tests
{
    public static class SpecExtensions
    {
        #region GetSettableProps
        /// <summary>
        /// https://stackoverflow.com/questions/3762456/how-to-check-if-property-setter-is-public
        /// Finds all publically settable properties
        /// </summary>
        public static PropertyInfo[] GetSettableProps(this Type type)
        {
            return type.GetProperties().Where(pi => pi.GetSetMethod(nonPublic:false) != null ).ToArray();
        }
        #endregion

        #region CreateInstanceChecked and CreateSetChecked

        public static T CreateInstanceChecked<T>(this TechTalk.SpecFlow.Table table, bool fixAllFields = true)
        {
            var instance = table.CreateInstance<T>();

            /*
            if (fixAllFields)
                GeneralUtils.FixValue.FixAllFields(instance);
                */

            List<string> hdr = table.Header.ToList();

            // Extract the field names from the table (accounting from the two different table formats that CreateInstance accepts)
            List<string> tableFields;

            if (hdr.Count == 2 && hdr[0].ToLower() == "field" && hdr[1].ToLower() == "value")
            {
                // This is the case for an instance defined like this
                // | Field   | Value        |
                // | name    | bob          |
                // | address | 300 main st  |
                // | phone   | 111-222-3344 |
                tableFields = table.Rows.Select(x => x[0]).ToList();    // grab the field names from the 1st column
            }
            else if (table.RowCount == 1)
            {
                // This is the case for an instance defined like this
                // | name | address     | phone        |
                // | bob  | 300 main st | 111-222-3344 |
                tableFields = hdr;      // The table header is the field names
            }
            else
            {
                throw new ArgumentException("Unrecognized table format");
            }

            // regularize the 
            CheckTableForUnknownFieldNames(tableFields, typeof(T), "CreateInstanceChecked");

            return instance;
        }

        public static IEnumerable<T> CreateSetChecked<T>(this TechTalk.SpecFlow.Table table, bool fixAllFields = true)
        {
            List<T> set = table.CreateSet<T>().ToList();

            /*
            if (fixAllFields)
                set.ForEach(x => GeneralUtils.FixValue.FixAllFields(x));
                */

            List<string> tableFields = table.Header.Select(x => x.ToLower().Replace(" ", "")).ToList();

            CheckTableForUnknownFieldNames(tableFields, typeof(T), "CreateSetChecked");

            return set;
        }

        private static string regularizeFieldName(string fieldName)
        {
            // Specflow CreateInstance / CreateSet uses case-insensitive comparison and ignores spaces
            fieldName = fieldName.ToLower();     // lower case
            fieldName = Regex.Replace(fieldName, @"\s+", "");      // remove all spaces
            return fieldName;
        }

        private static void CheckTableForUnknownFieldNames(List<string> tableFields, Type type, string methodName)
        {
            // regularize the table field names
            tableFields = tableFields.Select(x => regularizeFieldName(x)).ToList();

            // Match the fields in the instance up with the table
            // don't get any properties adorned with the HideProperty attribute
            List<string> propNames = type.GetSettableProps().Select(x => x.Name.ToLower()).ToList();

            var extraFields = tableFields.Except(propNames).ToList();

            if (extraFields.Count > 0)
            {
                throw new Exception(methodName + " found unknown fields in the table: " + string.Join(", ", extraFields));
            }
        }

        #endregion
    }
}

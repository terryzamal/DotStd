﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotStd
{
    public static class Extensions
    {
        // Extensions for common .NET classes
        // Extend helpers for string, datetime, enum
        // Helper extensions for doing common things like generate fixed length strings.
        // This would be <HideModuleName()> Public Module basExtensions   <System.Runtime.CompilerServices.Extension()> in VB.net

        public static bool IsNullOrWhiteSpace(this object obj)
        {
            // Is this object the same as a whitespace string ?
            return obj == null || string.IsNullOrWhiteSpace(obj.ToString());
        }

        public static object GetPropertyValue(this object thisObject, string propertyName)
        {
            // Get a property via reflection from some acquired object. similar to 'dynamic' keyword.
            // Just return null if property does not exist.
            return PropertyUtil.GetPropertyValue(thisObject, propertyName);
        }

        // int 

        public static int RoundTo(this int value, int roundTo)
        {
            // Round some int to nearest multiple of roundTo
            return Converter.ToIntRound(value, roundTo);
        }

        // string

        public static MemoryStream ToMemoryStream(this string value)
        {
            // UTF8 stream for reading
            // Wrap this in using () {} ? 
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        public static byte[] ToByteArray(this string sIn)
        {
            // string to bytes for hashing.
            return System.Text.Encoding.Default.GetBytes(sIn);
        }

        public static string SubSafe(this string s, int i, int lenTake = short.MaxValue)
        {
            // Will NOT throw if i is outside range..
            return StringUtil.SubSafe(s, i, lenTake);
        }
        public static string Truncate(this string value, int len)
        {
            // Left len chars.
            // Take X chars and lose the rest.
            // AKA Left() in Strings (VB)
            return StringUtil.Truncate(value, len);
        }
        public static string TruncateRight(this string text, int iLenMax)
        {
            // Take the right most X characters.
            // AKA Right()
            return StringUtil.TruncateRight(text, iLenMax);
        }

        public static string FixedLengthLeftAlign(this string source, int totalWidth, char paddingChar = ' ')
        {
            return StringUtil.FieldLeft(source, totalWidth, paddingChar);
        }
        public static string FixedLengthRightAlign(this string source, int totalWidth, char paddingChar = ' ')
        {
            return StringUtil.FieldRight(source, totalWidth, paddingChar);
        }

        /// <summary>
        /// Formats a string using the current culture
        /// The original format string is NOT modified so the result must be assigned to a variable or passed as a method argument.
        /// </summary>
        /// <param name="format">The string to format</param>
        /// <param name="parameters">The substitution parameters to use during format</param>
        /// <returns>A formatted string</returns>
        public static string FormatCurrent(this string format, params object[] parameters)
        {
            ValidState.ThrowIfNull(format, nameof(format));
            return string.Format(CultureInfo.CurrentCulture, format, parameters);
        }

        /// <summary>
        /// Formats a string using the invariant culture
        /// </summary>
        /// <param name="format">The string to format</param>
        /// <param name="parameters">The parameters to use during format</param>
        /// <returns>A formatter string</returns>
        public static string FormatInvariant(this string format, params object[] parameters)
        {
            ValidState.ThrowIfNull(format, nameof(format));
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, format, parameters);
        }

        // DateTime

        public static bool IsExtremeDate(this DateTime obj)
        {
            // Is this a basically useless date? NOTE: DateTime is not nullable.
            return DateUtil.IsExtremeDate(obj);
        }

        public static bool IsExtremeDate(this DateTime? obj)
        {
            if (obj == null)
                return true;
            return DateUtil.IsExtremeDate(obj.Value);
        }

        public static DateTime Trim(this DateTime date, long roundTicks)
        {
            // roundTicks = TimeSpan.TicksPerDay, TimeSpan.TicksPerHour, TimeSpan.TicksPerMinute, etc.
            return new DateTime(date.Ticks - date.Ticks % roundTicks);
        }

        public static TimeSpan GetAgeUTC(this DateTime? from)
        {
            if (from == null)
                return TimeSpan.MaxValue;

            return DateTime.UtcNow - from.Value;
        }

        //********************

        public static string ToDtString(this DateTime dt, string format = null, IFormatProvider culture = null, string def = null)
        {
            // format DateTime output string as a string.
            // ASSUME timeZone is already accounted for ???
            // If this is a bad date just display nothing. 
            // def = use this if its a bad string.
            // e.g format = "M/d/yyyy" or default = 'yyyy-MM-dd' (for ISO)
            // culture = Use format from culture for fancy string formatting.

            if (def == null)
                def = string.Empty;
            if (dt.IsExtremeDate())    // Const.dateExtremeMin
                return def;     // null or "" ??
            if (format == null)
                format = DateUtil.kShortDate;       // "yyyy-MM-dd" = ISO
            if (culture != null)
                return dt.ToString(format, culture);
            return dt.ToString(format);
        }

        public static string ToDtTmString(this DateTime dt, IFormatProvider culture = null)
        {
            // Make a long detailed string with date and time. 
            // culture = fancy and localized. null = ISO
            // ASSUME timeZone is already accounted for ???
            if (culture != null)
                return dt.ToString(culture);
            return dt.ToString();
        }
        public static string ToDtTmString(this DateTime? obj, IFormatProvider culture = null)
        {
            // Make a long detailed string with date and time. 
            // culture = fancy and localized. null = ISO
            // ASSUME timeZone is already accounted for ???
            if (obj == null)
                return "";
            return ToDtTmString(obj.Value, culture);
        }

        public static string ToDateString(this DateTime dt)
        {
            // get Date as ISO date string. not time. assume timezone is irrelevant.
            // like ToShortDateString()/ToString("d") BUT NOT cultural.
            return ToDtString(dt, DateUtil.kShortDate);
        }

        public static string ToDateJS(this DateTime dt)
        {
            // Encode to a format that JavaScript can deal with.

            return $"new Date({dt.Year},{dt.Month-1},{dt.Day})";
        }

        public static string ToDateString(this DateTime? obj)
        {
            // get Date as ISO date string. not time. assume timezone is irrelevant.
            if (obj == null)
                return null;
            return ToDateString(obj.Value);
        }

        // Extend Nullable<> types

        public static string ToStringForm<T>(this Nullable<T> nullable, string format) where T : struct
        {
            // Format some nullable type as a string.
            return String.Format("{0:" + format + "}", nullable.GetValueOrDefault());
        }
        public static string ToStringForm<T>(this Nullable<T> nullable, string format, string defaultValue) where T : struct
        {
            // Format some nullable type as a string.
            if (nullable.HasValue)
            {
                return String.Format("{0:" + format + "}", nullable.Value);
            }
            return defaultValue;
        }

        // IDataReader

        public static bool ColumnExists(this IDataReader reader, string columnName)
        {
            // Does columnName exist?
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // IEnumerable<T>

        public static List<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            // Used for paging.
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static IEnumerable<T> WhereXStartsWith<T>(this IEnumerable<T> source, string propertyName, string filterStart)
        {
            // Used for dynamic filtering.
            // LIKE Where(s => s.Field<string>(LookupColumn).ToUpper().StartsWith(filter))
            if (!source.Any() || string.IsNullOrEmpty(propertyName))
                return source;
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return source.Where(e => propertyInfo.GetValue(e, null).ToString().ToUpper().StartsWith(filterStart));
        }

        public static System.Collections.Generic.List<ItemType> ToListDupe<ItemType>(this System.Collections.Generic.IEnumerable<ItemType> a)
        {
            return Converter.ToListDupe(a);
        }

        // DataTable 

        /// <summary>
        /// Converts The Generic List into DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iList">The Data Table</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> iList)
        {
            // similar to M$ DataTableExtensions.CopyToDataTable()  (Not available in .NET Core?)
            // Does NOT throw on empty table.
            // This is the reverse of ToListT()

            var dataTable = new DataTable();
            PropertyDescriptorCollection propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];
                Type type = propertyDescriptor.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name, type);
            }

            object[] values = new object[propertyDescriptorCollection.Count];
            foreach (T iListItem in iList)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = propertyDescriptorCollection[i].GetValue(iListItem);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DataTable CopyToDataTableSafe<T>(this IEnumerable<T> source) where T : DataRow
        {
            // The M$ version of CopyToDataTable will throw exception if the table is empty. Thats seems stupid.
            if (source.Any())   // throw if empty. ignore.
            {
                return source.ToDataTable<T>();    // use M$ CopyToDataTable ?
            }
            return new DataTable(); // empty.
        }
    }
}

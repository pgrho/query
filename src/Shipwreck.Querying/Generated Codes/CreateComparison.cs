using System;
using System.Linq.Expressions;

namespace Shipwreck.Querying
{
	public struct ByteTryParser : ITryParser<Byte>
	{
		public bool TryParse(string s, out Byte result)
			=> Byte.TryParse(s, out result);
	}
	public struct SByteTryParser : ITryParser<SByte>
	{
		public bool TryParse(string s, out SByte result)
			=> SByte.TryParse(s, out result);
	}
	public struct Int16TryParser : ITryParser<Int16>
	{
		public bool TryParse(string s, out Int16 result)
			=> Int16.TryParse(s, out result);
	}
	public struct UInt16TryParser : ITryParser<UInt16>
	{
		public bool TryParse(string s, out UInt16 result)
			=> UInt16.TryParse(s, out result);
	}
	public struct Int32TryParser : ITryParser<Int32>
	{
		public bool TryParse(string s, out Int32 result)
			=> Int32.TryParse(s, out result);
	}
	public struct UInt32TryParser : ITryParser<UInt32>
	{
		public bool TryParse(string s, out UInt32 result)
			=> UInt32.TryParse(s, out result);
	}
	public struct Int64TryParser : ITryParser<Int64>
	{
		public bool TryParse(string s, out Int64 result)
			=> Int64.TryParse(s, out result);
	}
	public struct UInt64TryParser : ITryParser<UInt64>
	{
		public bool TryParse(string s, out UInt64 result)
			=> UInt64.TryParse(s, out result);
	}
	public struct SingleTryParser : ITryParser<Single>
	{
		public bool TryParse(string s, out Single result)
			=> Single.TryParse(s, out result);
	}
	public struct DoubleTryParser : ITryParser<Double>
	{
		public bool TryParse(string s, out Double result)
			=> Double.TryParse(s, out result);
	}
	public struct DecimalTryParser : ITryParser<Decimal>
	{
		public bool TryParse(string s, out Decimal result)
			=> Decimal.TryParse(s, out result);
	}
	public struct DateTimeTryParser : ITryParser<DateTime>
	{
		public bool TryParse(string s, out DateTime result)
			=> DateTime.TryParse(s, out result);
	}
	partial class QueryProvider
	{

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Byte>> propertySelector, string value)
			=> CreateComparison<T, Byte, ByteTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Byte?>> propertySelector, string value)
			=> CreateComparison<T, Byte, ByteTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, SByte>> propertySelector, string value)
			=> CreateComparison<T, SByte, SByteTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, SByte?>> propertySelector, string value)
			=> CreateComparison<T, SByte, SByteTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int16>> propertySelector, string value)
			=> CreateComparison<T, Int16, Int16TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int16?>> propertySelector, string value)
			=> CreateComparison<T, Int16, Int16TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt16>> propertySelector, string value)
			=> CreateComparison<T, UInt16, UInt16TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt16?>> propertySelector, string value)
			=> CreateComparison<T, UInt16, UInt16TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int32>> propertySelector, string value)
			=> CreateComparison<T, Int32, Int32TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int32?>> propertySelector, string value)
			=> CreateComparison<T, Int32, Int32TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt32>> propertySelector, string value)
			=> CreateComparison<T, UInt32, UInt32TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt32?>> propertySelector, string value)
			=> CreateComparison<T, UInt32, UInt32TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int64>> propertySelector, string value)
			=> CreateComparison<T, Int64, Int64TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Int64?>> propertySelector, string value)
			=> CreateComparison<T, Int64, Int64TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt64>> propertySelector, string value)
			=> CreateComparison<T, UInt64, UInt64TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, UInt64?>> propertySelector, string value)
			=> CreateComparison<T, UInt64, UInt64TryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Single>> propertySelector, string value)
			=> CreateComparison<T, Single, SingleTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Single?>> propertySelector, string value)
			=> CreateComparison<T, Single, SingleTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Double>> propertySelector, string value)
			=> CreateComparison<T, Double, DoubleTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Double?>> propertySelector, string value)
			=> CreateComparison<T, Double, DoubleTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Decimal>> propertySelector, string value)
			=> CreateComparison<T, Decimal, DecimalTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, Decimal?>> propertySelector, string value)
			=> CreateComparison<T, Decimal, DecimalTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, DateTime>> propertySelector, string value)
			=> CreateComparison<T, DateTime, DateTimeTryParser>(propertySelector, value);

		protected static Expression<Func<T, bool>> CreateComparison<T>(Expression<Func<T, DateTime?>> propertySelector, string value)
			=> CreateComparison<T, DateTime, DateTimeTryParser>(propertySelector, value);
	}
}
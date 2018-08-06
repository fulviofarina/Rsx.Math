using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rsx.Math
{
    public partial class MyMath
    {
        public static double GetDensity(double mass, double Vol)
        {
            double density = 0;

            if (mass != 0 && Vol != 0) density = (mass / Vol);

            return density;
        }

        public static double GetVolCylinder(double diameter, double lenght)
        {
            double volumen = 0;

            if (diameter != 0 && lenght != 0)
            {
                volumen = (diameter * diameter * System.Math.PI * 0.5 * 0.5 * lenght);
            }

            return volumen;
        }

        public static Func<double, bool> IsGoodDouble = x =>
        {
            //if (x != null)
            //{
            if (double.IsNaN(x)) return false;
            if (double.IsInfinity(x)) return false;
            if (x < 1E-07)
            {
                return false;
            }
            if (x > 1000000000.0)
            {
                return false;
            }
            //}
            return true;
        };

        public static Func<DataRow, T> NonZeroSelector<T>(string Field, string FieldUnc, string Filter, object FilterValue)
        {
            Func<DataRow, T> nonZero;
            Func<DataRow, T> filteredOrnonZero;  //generic counter

            if (typeof(T).Equals(typeof(bool)))
            {
                // selects a value that it is not equal to 0
                nonZero = i =>
                {
                    bool take = false;
                    double nonzero = Convert.ToDouble(i[Field]);
                    double nonzeroUnc = Convert.ToDouble(i[FieldUnc]);
                    if (nonzero != 0.0 && nonzeroUnc != 0.0)
                    {
                        take = true;
                    }
                    object taken = take;
                    return (T)taken;
                };

                //selects: a value that is not equal to 0 (nonZero)  and  that passes the filter
                filteredOrnonZero = j =>
                {
                    bool take = false;
                    object nonnull = nonZero(j);
                    if ((bool)nonnull)
                    {
                        object value = j[Filter];
                        if (value.Equals(FilterValue))
                        {
                            take = true;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };
            }
            else if (typeof(T).Equals(typeof(DataRow)))
            {
                nonZero = i =>
                {
                    DataRow take = null;
                    double nonzero = Convert.ToDouble(i[Field]);
                    double nonzeroUnc = Convert.ToDouble(i[FieldUnc]);
                    if (nonzero != 0.0 && nonzeroUnc != 0.0)
                    {
                        take = i;
                    }
                    object taken = take;
                    return (T)taken;
                };

                filteredOrnonZero = j =>
                {
                    DataRow take = null;
                    object nonnull = nonZero(j);
                    if (nonnull != null)
                    {
                        object value = j[Filter];
                        if (value.Equals(FilterValue))
                        {
                            take = j;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };
            }
            else
            {
                nonZero = i => (T)Convert.ChangeType(i, typeof(T)); ;
                filteredOrnonZero = nonZero;
            }

            if (string.IsNullOrEmpty(Filter)) return nonZero;
            else return filteredOrnonZero;
        }

        public static Func<DataRow, T> NonZeroSelector<T>(string Field, string Filter, object FilterValue)
        {
            Func<DataRow, T> nonZero;
            Func<DataRow, T> filteredOrnonZero;  //generic counter

            if (typeof(T).Equals(typeof(bool)))
            {
                nonZero = i =>
                {
                    bool take = false;

                    if (!i.IsNull(Field))
                    {
                        double nonzero = Convert.ToDouble(i[Field]);
                        if (nonzero != 0.0)
                        {
                            take = true;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };

                filteredOrnonZero = j =>
                {
                    bool take = false;
                    object nonnull = nonZero(j);
                    if ((bool)nonnull)
                    {
                        object value = j[Filter];
                        if (value.Equals(FilterValue))
                        {
                            take = true;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };
            }
            else if (typeof(T).Equals(typeof(DataRow)))
            {
                nonZero = i =>
                {
                    DataRow take = null;
                    if (!i.IsNull(Field))
                    {
                        double nonzero = Convert.ToDouble(i[Field]);
                        if (nonzero != 0.0)
                        {
                            take = i;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };

                filteredOrnonZero = j =>
                {
                    DataRow take = null;
                    object nonnull = nonZero(j);
                    if (nonnull != null)
                    {
                        object value = j[Filter];
                        if (value.Equals(FilterValue))
                        {
                            take = j;
                        }
                    }
                    object taken = take;
                    return (T)taken;
                };
            }
            else
            {
                nonZero = i => (T)Convert.ChangeType(i, typeof(T)); ;
                filteredOrnonZero = nonZero;
            }

            if (string.IsNullOrEmpty(Filter)) return nonZero;
            else return filteredOrnonZero;
        }

        /// <summary>
        /// Gets the normal SD fromRow a given list of doubles and their avg
        /// </summary>
        /// <param name="Doubles">List of doubles toRow find the SD</param>
        /// <param name="Avg">    
        /// Average toRow use. If Avg = 0 the function will find the Avg first
        /// </param>
        /// <returns></returns>
        public static double StDev(IList<double> Doubles, double Avg)
        {
            double sd = 0.0;
            if (Doubles.Count == 0) return sd;
            if (Avg == 0.0) Avg = Doubles.Average();
            if (Avg != 0.0)
            {
                sd = Doubles.Sum<double>(x => System.Math.Pow(x - Avg, 2.0));
                sd /= (double)(Doubles.Count - 1);
            }
            return System.Math.Sqrt(sd);
        }

        public static double[] WAverageStDeV(IEnumerable<DataRow> enumerable, string Field, string FieldUncertainty)
        {
            return WAverageStDeV(enumerable, Field, FieldUncertainty, string.Empty, null);
        }

        /// <summary>
        /// Returns the weighted average, unbiased and biased StDev of a given quantity
        /// </summary>
        /// <param name="enumerable">      Collection of rows containing the Field to average</param>
        /// <param name="Field">           The Field (Column) to average</param>
        /// <param name="FieldUncertainty">
        /// The uncertainty associated to the quantity (for extrapolation of the weights)
        /// </param>
        /// <param name="FilterField">     
        /// A Field containing a boolean which will work as a filter. If 'true' is found, the data
        /// will be selected, otherwise rejected
        /// </param>
        /// <returns></returns>
        public static double[] WAverageStDeV(IEnumerable<DataRow> enumerable, string Field, string FieldUncertainty, string FilterField)
        {
            return WAverageStDeV(enumerable, Field, FieldUncertainty, FilterField, true);
        }

        /// <summary>
        /// Returns the weighted average, unbiased and biased StDev of a given quantity
        /// </summary>
        /// <param name="enumerable">      Collection of rows containing the Field to average</param>
        /// <param name="Field">           The Field (Column) to average</param>
        /// <param name="FieldUncertainty">
        /// The uncertainty associated to the quantity (for extrapolation of the weights)
        /// </param>
        /// <param name="FilterField">     
        /// A Field (Column) containing an object which will work as a filter. If 'object' is found,
        /// the data will be selected, otherwise rejected
        /// </param>
        /// <returns></returns>
        public static double[] WAverageStDeV(IEnumerable<DataRow> enumerable, string Field, string FieldUncertainty, string FilterField, object FilterValue)
        {
            double Avg = 0.0;
            double Obsvar = 0.0;
            double var = 0.0;

            if (enumerable.Count() != 0)
            {
                //fore selecting those which have nonzero weight and/or must be filtered
                Func<DataRow, DataRow> selector = MyMath.NonZeroSelector<DataRow>(Field, FieldUncertainty, FilterField, FilterValue);

                IEnumerable<DataRow> enumerable2 = enumerable.Select<DataRow, DataRow>(selector);
                enumerable2 = enumerable2.Where<DataRow>(i => i != null);

                int n = enumerable2.Count();
                if (n > 1)
                {
                    ///Basic calculation of weight
                    Func<DataRow, double> w = x => System.Math.Pow(Convert.ToDouble(x[FieldUncertainty]), -2);
                    Func<DataRow, double> weight = x => w(x);
                    Func<DataRow, double> weight2 = x => w(x) * w(x);
                    Func<DataRow, double> weightA = x => Convert.ToDouble(x[Field]) * w(x);
                    Func<DataRow, double> weightA2 = x => Convert.ToDouble(x[Field]) * weightA(x);

                    double sumw = enumerable2.Sum(weight);
                    double sumw2 = enumerable2.Sum(weight2);
                    double sumwA = enumerable2.Sum(weightA);
                    double sumwA2 = enumerable2.Sum(weightA2);
                    Avg = (sumwA / sumw);
                    if (sumw > 0 && Avg != 0)
                    {
                        Obsvar = System.Math.Abs((sumwA2 / sumw) - (Avg * Avg));
                        Obsvar = 100.0 * (System.Math.Sqrt(Obsvar) / System.Math.Abs(Avg));
                    }
                    if (sumw > 0 && Avg != 0)
                    {
                        var = System.Math.Abs(((sumw * sumwA2) - (sumwA * sumwA)) / ((sumw * sumw) - sumw2));
                        var = 100.0 * (System.Math.Sqrt(var) / System.Math.Abs(Avg));
                    }
                }
                else if (n == 1)
                {
                    DataRow found = enumerable2.First();
                    if (found != null)
                    {
                        Avg = Convert.ToDouble(found[Field]);
                        var = Convert.ToDouble(found[FieldUncertainty]);
                    }
                }
            }

            return new double[] { Avg, var, Obsvar };
        }

        /// <summary>
        /// gets a list of doubles fromRow the values of the given datacolumn, elevated at the given power
        /// </summary>
        /// <param name="Column">column toRow get the doubles fromRow</param>
        /// <param name="power"> power toRow rise the column double values</param>
        /// <returns></returns>
        public static IList<double> ListFrom(DataColumn Column, double power)
        {
            return ListFrom(Column, power, false, 0, string.Empty, null);
        }

        public static IList<double> ListFrom(IEnumerable<DataRow> rows, string field, double power)
        {
            return ListFrom(rows, field, power, false, 0, string.Empty, null);
        }

        /// <summary>
        /// gets a list of doubles fromRow the values of the given datacolumn, elevated at the given power
        /// </summary>
        /// <param name="Column">column toRow get the doubles fromRow</param>
        /// <param name="power"> power toRow rise the column double values</param>
        /// <returns></returns>
        public static IList<double> ListFrom(DataColumn Column, double power, bool log, double logbase)
        {
            return ListFrom(Column, power, log, logbase, string.Empty, null);
        }

        public static IList<double> ListFrom(IEnumerable<DataRow> rows, string field, double power, bool log, double logbase)
        {
            return ListFrom(rows, field, power, log, logbase, string.Empty, null);
        }

        /// <summary>
        /// gets a list of doubles fromRow the values of the given datacolumn tha satisfy the
        /// bool-filter criteria, elevated at the given power
        /// </summary>
        /// <param name="Column">    column toRow get the doubles fromRow</param>
        /// <param name="BoolFilter"></param>
        /// <param name="power">     power toRow rise the column double values</param>
        /// <returns></returns>
        public static IList<double> ListFrom(IEnumerable<DataRow> rows, string field, double power, string FilterField, object FilterValue)
        {
            return ListFrom(rows, field, power, false, 0, FilterField, FilterValue);
        }

        public static IList<double> ListFrom(DataColumn Column, double power, string FilterField, object FilterValue)
        {
            return ListFrom(Column, power, false, 0, FilterField, FilterValue);
        }

        public static IList<double> ListFrom(IEnumerable<DataRow> rows, string field, double power, bool log, double logbase, string FilterField, object FilterValue)
        {
            Func<DataRow, DataRow> non0selector = MyMath.NonZeroSelector<DataRow>(field, FilterField, FilterValue);
            IEnumerable<DataRow> enumerable = rows.Select(non0selector);
            enumerable = enumerable.Where<DataRow>(i => i != null && !i.IsNull(field));

            Func<DataRow, double> lister = x =>
            {
                double Adouble = 0.0;

                Adouble = Convert.ToDouble(x[field]);
                Adouble = System.Math.Pow(Adouble, power);
                if (log && Adouble != 0.0) Adouble = System.Math.Log(Adouble, logbase);
                return Adouble;
            };

            return enumerable.Select<DataRow, double>(lister).ToArray();
        }

        /// <summary>
        /// gets a list of doubles fromRow the values of the given datacolumn tha satisfy the
        /// bool-filter criteria, elevated at the given power
        /// </summary>
        /// <param name="Column">    column toRow get the doubles fromRow</param>
        /// <param name="BoolFilter"></param>
        /// <param name="power">     power toRow rise the column double values</param>
        /// <returns></returns>
        public static IList<double> ListFrom(DataColumn Column, double power, bool log, double logbase, string FilterField, object FilterValue)
        {
            string field = Column.ColumnName;

            IEnumerable<DataRow> enumerable = Column.Table.AsEnumerable();

            return ListFrom(enumerable, field, power, log, logbase, FilterField, FilterValue);
        }

        /// <summary>
        /// Determines the average fromRow the a given List of doubles
        /// </summary>
        /// <param name="Doubles"></param>
        /// <returns></returns>
    }
}
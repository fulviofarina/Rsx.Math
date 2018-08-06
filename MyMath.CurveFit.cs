using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rsx.Math
{
    public partial class MyMath
    {
        public class CurveFit
        {
            //if ((bool)Column.Table.Rows[i][BoolFilter])

            public class Linear
            {
                /// <summary>
                /// Performs Linear fit acording toRow Y and X returning vector "abR2" as Y = a + bX
                /// R2 is the R2 of the Fit abR2[1] == "b" --&gt; SLOPE abR2[0] == "a" --&gt; CutOf
                /// abR2[2] ==R2 --&gt; R-squared
                /// </summary>
                /// <param name="X"></param>
                /// <param name="Y"></param>
                /// <returns></returns>
                public static double[] LeastSquaresFit(DataColumn X, DataColumn Y, DataColumn Res, DataColumn YCalc)
                {
                    return LeastSquaresFit(X, Y, Res, YCalc, null);
                }

                public static double[] LeastSquaresFit(DataColumn X, DataColumn Y, DataColumn Res, DataColumn YCalc, DataColumn filter)
                {
                    double ssxx = 0;
                    double ssxy = 0;
                    double ssyy = 0;

                    double avgX = 0;
                    double avgY = 0;

                    string filterfield = string.Empty;
                    if (filter != null) filterfield = filter.ColumnName;

                    IList<double> lX = ListFrom(X, 1, filterfield, true);
                    IList<double> lY = ListFrom(Y, 1, filterfield, true);

                    avgX = lX.Average();
                    avgY = lY.Average();

                    ssxx = ssii(X, avgX, filter);
                    ssxy = ssij(X, Y, avgX, avgY, filter);
                    ssyy = ssii(Y, avgY, filter);

                    double[] abR2SEaSEb = new double[5]; // a is the cuttof, b the slope, R2-square, StdError of a, StdError of b

                    double[] abR2 = new double[3];
                    double[] SEaSEb = new double[2];

                    abR2 = FitParameters(ssxx, ssxy, ssyy, avgX, avgY);

                    SEaSEb = FitErrors(ssxx, ssxy, ssyy, avgX, abR2[1], lX.Count);

                    FindResiduals(X, Y, abR2[1], abR2[0], Res, filter);

                    SquareResiduals(Res, filter);

                    FindFitted(X, YCalc, abR2[1], abR2[0], filter);

                    abR2SEaSEb[0] = abR2[0];
                    abR2SEaSEb[1] = abR2[1];
                    abR2SEaSEb[2] = abR2[2];
                    abR2SEaSEb[3] = SEaSEb[0];
                    abR2SEaSEb[4] = SEaSEb[1];

                    return abR2SEaSEb;
                }

                private static double ssii(DataColumn I, double avgI)
                {
                    return ssii(I, avgI, null);
                }

                private static double ssii(DataColumn I, double avgI, DataColumn filter)
                {
                    double ssii = 0;

                    if (avgI != 0)
                    {
                        for (int z = 0; z < I.Table.Rows.Count; z++)
                        {
                            if (I.Table.Rows[z].IsNull(I)) continue;
                            double i = 0;

                            if (filter != null)
                            {
                                if ((bool)I.Table.Rows[z][filter])
                                {
                                    i = Convert.ToDouble(I.Table.Rows[z][I]);
                                    ssii += ((i - avgI) * (i - avgI));
                                }
                            }
                            else
                            {
                                i = Convert.ToDouble(I.Table.Rows[z][I]);
                                ssii += ((i - avgI) * (i - avgI));
                            }
                        }
                    }
                    return ssii;
                }

                /// <summary>
                /// Cloumns I and J must have the samke Rank!!
                /// </summary>
                /// <param name="I">   </param>
                /// <param name="J">   </param>
                /// <param name="avgI"></param>
                /// <param name="avgJ"></param>
                /// <returns></returns>
                private static double ssij(DataColumn I, DataColumn J, double avgI, double avgJ)
                {
                    //Columns I and J must have the same Rank

                    return ssij(I, J, avgI, avgJ, null);
                }

                private static double ssij(DataColumn I, DataColumn J, double avgI, double avgJ, DataColumn filter)
                {
                    //Columns I and J must have the same Rank

                    double ssij = 0;

                    if (avgI != 0 && avgJ != 0)
                    {
                        if (I.Table.Rows.Count == J.Table.Rows.Count)
                        {
                            for (int z = 0; z < I.Table.Rows.Count; z++)
                            {
                                if (I.Table.Rows[z].IsNull(I)) continue;
                                double i = 0;
                                double j = 0;

                                if (filter != null)
                                {
                                    if ((bool)I.Table.Rows[z][filter])
                                    {
                                        i = Convert.ToDouble(I.Table.Rows[z][I]);
                                        j = Convert.ToDouble(J.Table.Rows[z][J]);

                                        ssij = ssij + ((i - avgI) * (j - avgJ));
                                    }
                                }
                                else
                                {
                                    i = Convert.ToDouble(I.Table.Rows[z][I]);
                                    j = Convert.ToDouble(J.Table.Rows[z][J]);

                                    ssij = ssij + ((i - avgI) * (j - avgJ));
                                }
                            }
                        }
                    }
                    return ssij;
                }

                private static double[] FitParameters(double ssii, double ssij, double ssjj, double avgI, double avgJ)
                {
                    //Columns I and J must have the same Rank

                    double[] abR2 = new double[3];

                    abR2[0] = 0;
                    abR2[1] = 0;
                    abR2[2] = 0;

                    if (ssii != 0 && ssij != 0 && ssjj != 0 && avgI != 0 && avgJ != 0)
                    {
                        abR2[1] = (ssij / ssii); //this is b
                        abR2[0] = avgJ - (abR2[1] * avgI); //this is a
                        abR2[2] = ((ssij * ssij) / (ssii * ssjj)); //this is R2
                    }
                    return abR2;
                }

                /// <summary>
                /// Finds the residuals Res = Y - slope*X - cutoff
                /// </summary>
                /// <param name="I">     </param>
                /// <param name="J">     </param>
                /// <param name="slope"> </param>
                /// <param name="cutoff"></param>
                /// <param name="Res">   </param>
                private static void FindResiduals(DataColumn I, DataColumn J, double slope, double cutoff, DataColumn Res)
                {
                    //Columns I and J must have the same Rank

                    FindResiduals(I, J, slope, cutoff, Res, null);
                }

                private static void FindResiduals(DataColumn I, DataColumn J, double slope, double cutoff, DataColumn Res, DataColumn filter)
                {
                    //Columns I and J must have the same Rank

                    for (int z = 0; z < J.Table.Rows.Count; z++)
                    {
                        if (I.Table.Rows[z].IsNull(I)) continue;
                        if (J.Table.Rows[z].IsNull(J)) continue;

                        if (filter != null)
                        {
                            if ((bool)I.Table.Rows[z][filter])
                            {
                                Res.Table.Rows[z][Res] = Convert.ToDouble(J.Table.Rows[z][J]) - (slope * (Convert.ToDouble(I.Table.Rows[z][I]))) - cutoff;
                            }
                        }
                        else
                        {
                            Res.Table.Rows[z][Res] = Convert.ToDouble(J.Table.Rows[z][J]) - (slope * (Convert.ToDouble(I.Table.Rows[z][I]))) - cutoff;
                        }
                    }
                }

                /// <summary>
                /// Finds the Calculated (or Fitted) Y axis values YCalc = slope*X + cutoff
                /// </summary>
                /// <param name="I">     the X axis</param>
                /// <param name="JCalc"> the YCalc axis</param>
                /// <param name="slope"> the slope</param>
                /// <param name="cutoff">the cutoff</param>
                private static void FindFitted(DataColumn I, DataColumn JCalc, double slope, double cutoff)
                {
                    //Columns I and JCalc must have the same Rank

                    FindFitted(I, JCalc, slope, cutoff, null);
                }

                private static void FindFitted(DataColumn I, DataColumn JCalc, double slope, double cutoff, DataColumn filter)
                {
                    //Columns I and JCalc must have the same Rank

                    int z;

                    for (z = 0; z < I.Table.Rows.Count; z++)
                    {
                        if (I.Table.Rows[z].IsNull(I)) continue;
                        if (filter != null)
                        {
                            if ((bool)I.Table.Rows[z][filter])
                            {
                                JCalc.Table.Rows[z][JCalc] = (slope * (Convert.ToDouble(I.Table.Rows[z][I]))) + cutoff;
                            }
                        }
                        else JCalc.Table.Rows[z][JCalc] = (slope * (Convert.ToDouble(I.Table.Rows[z][I]))) + cutoff;
                    }
                }

                private static void SquareResiduals(DataColumn Res)
                {
                    SquareResiduals(Res, null);
                }

                private static void SquareResiduals(DataColumn Res, DataColumn filter)
                {
                    int z;

                    for (z = 0; z < Res.Table.Rows.Count; z++)
                    {
                        if (Res.Table.Rows[z].IsNull(Res)) continue;
                        if (filter != null)
                        {
                            if ((bool)Res.Table.Rows[z][filter])
                            {
                                Res.Table.Rows[z][Res] = Decimal.Round(Convert.ToDecimal(System.Math.Pow(Convert.ToDouble(Res.Table.Rows[z][Res]), 2)), 3);
                            }
                        }
                        else Res.Table.Rows[z][Res] = Decimal.Round(Convert.ToDecimal(System.Math.Pow(Convert.ToDouble(Res.Table.Rows[z][Res]), 2)), 3);
                    }
                }

                /// <summary>
                /// Returns the Errors s = Sqrt [ (ssjj - b*ssij) / (n-2) ]
                /// </summary>
                /// <param name="ssii"></param>
                /// <param name="ssij"></param>
                /// <param name="ssjj"></param>
                /// <param name="avgI"></param>
                /// <param name="avgJ"></param>
                /// <returns></returns>
                private static double[] FitErrors(double ssii, double ssij, double ssjj, double avgI, double slope, int points)
                {
                    //Columns I and J must have the same Rank

                    double s2 = 0;
                    double o2 = 0;

                    double[] SESlope_SECutoff = new double[2];

                    SESlope_SECutoff[0] = 0;
                    SESlope_SECutoff[1] = 0;

                    if (ssii != 0 && ssij != 0 && ssjj != 0 && avgI != 0 && points > 2)
                    {
                        s2 = ((ssjj - slope * ssij)) / (points - 2);  //s-squared

                        o2 = (1 / points) + (avgI * avgI / ssii); // o-squared

                        SESlope_SECutoff[0] = System.Math.Sqrt(s2 / ssii); //SESlope

                        SESlope_SECutoff[1] = System.Math.Sqrt(s2 * o2);
                    }
                    return SESlope_SECutoff;
                }
            }
        }
    }
}
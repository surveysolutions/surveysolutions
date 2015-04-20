using System;

namespace WB.Core.SharedKernels.DataCollection.V2.CustomFunctions
{
    /** 
 * 
 * 
 * http://www.who.int/childgrowth/standards/en/
 * 
 * @author Sergiy Radyakin, Economist, Development Research Group, The World Bank
 * 
 * 
 * 
This library provides a convenient access to the child growth 
standards produced by WHO. It can be used by software developers 
in their applications (for example in data entry applications),
for validation of input data for plausibility and for other quality
control and related tasks.
     
For publications describing the methodology, assumptions and limitations
consult the WHO publications on the following page:
http://www.who.int/childgrowth/publications/en/
 * */



    /// <summary>
    /// The WHO Child Growth Standards    
    /// </summary>
    public static class ZScore
    {
        /// <summary>
        /// BMI-for-age: Birth to 5 years.
        /// \note Series for 0-2 years and 2-5 years are combined.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">BMI, kg/m2.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a measurement is less than or equal to zero. 
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [0, 60].
        /// 
        /// \note Do not confuse this function with Bmi, which computes the value of the body mass index.
        public static double Bmifa(long? ageMonths, bool? isBoy, double? measurement)
        {
            return LMS.GetScore(ageMonths, isBoy, measurement, BmifaBoys, BmifaGirls);
        }

        /// <summary>
        /// BMI-for-age: Birth to 5 years.
        /// \note Series for 0-2 years and 2-5 years are combined.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="weight">Weight, kg</param>
        /// <param name="height">Height, m</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if any of the @a weight or @a height measurements is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [0, 60].
        /// 
        /// \note Do not confuse this function with Bmi, which computes the value of the body mass index.
        public static double Bmifa(long? ageMonths, bool? isBoy, double? weight, double? height)
        {
            var bmi = new AbstractConditionalLevelInstanceFunctions().Bmi(weight, height);
            return Bmifa(ageMonths, isBoy, bmi);
        }

        /// <summary>
        /// Weight-for-age: Birth to 5 years.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Weight, kg.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a measurement is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [0, 60].
        public static double Wfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            return LMS.GetScore(ageMonths, isBoy, measurement, WfaBoys, WfaGirls);
        }

        /// <summary>
        /// Length/Height-for-age: Birth to 5 years.
        /// \note Series for 0-2 years and 2-5 years are combined.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Length or height, cm.</param>
        /// <returns>Z-score</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a measurement is less than or equal to zero
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [0, 60].
        public static double Lhfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            return LMS.GetScore(ageMonths, isBoy, measurement, LhfaBoys, LhfaGirls);
        }

        /// <summary>
        /// Head circumference-for-age: Birth to 5 years.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Head circumference, cm.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a measurement is less than or equal to zero;
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [0, 60].
        public static double Hcfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            return LMS.GetScore(ageMonths, isBoy, measurement, HcfaBoys, HcfaGirls);
        }

        /// <summary>
        /// Triceps skinfold-for-age: 3 months to 5 years.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Triceps skinfold, mm.</param>
        /// <returns>Z-score.</returns>
        ///
        /// @throws ArgumentNullException if any of the arguments is null. 
        /// @throws ArgumentException if the @a measurement is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [3, 60].
        public static double Tsfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            if (ageMonths < 3)
                throw new ArgumentOutOfRangeException("ageMonths", "Error. Age is out of range.");

            return LMS.GetScore(ageMonths, isBoy, measurement, TsfaBoys, TsfaGirls);
        }

        /// <summary>
        /// Arm circumference-for-age: 3 months to 5 years.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Arm circumference, cm.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a measurement is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [3, 60].
        public static double Acfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            if (ageMonths < 3)
                throw new ArgumentOutOfRangeException("ageMonths", "Error. Age is out of range.");

            return LMS.GetScore(ageMonths, isBoy, measurement, AcfaBoys, AcfaGirls);
        }

        /// <summary>
        /// Subscapular skinfold-for-age: 3 months to 5 years.
        /// </summary>
        /// <param name="ageMonths">Age in months.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="measurement">Subscapular skinfold, mm.</param>
        /// <returns>Z-score.</returns>
        ///
        /// @throws ArgumentNullException if any of the arguments is null. 
        /// @throws ArgumentException if the @a measurement is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a ageMonths is out of range [3, 60].
        public static double Ssfa(long? ageMonths, bool? isBoy, double? measurement)
        {
            if (ageMonths < 3)
                throw new ArgumentOutOfRangeException("ageMonths", "Error. Age is out of range.");

            return LMS.GetScore(ageMonths, isBoy, measurement, SsfaBoys, SsfaGirls);
        }

        /// <summary>
        /// Weight-for-length: Birth to 2 years.
        /// 
        /// \warning The function does not receive age as an argument and is 
        /// thus not checking the child is of applicable age range.
        /// </summary>
        /// <param name="length">Length, cm.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="weight">Weight, kg.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a weight is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a length is out of range [45.0, 110.0].
        public static double Wfl(double? length, bool? isBoy, double? weight)
        {
            const double min = 45.0;
            const double max = 110.0;

            if (length.HasValue == false)
                throw new ArgumentNullException("length", "Error. Length may not be null.");

            if (weight <= 0)
                throw new ArgumentException("Error. Weight must be positive.", "weight");

            if (length < min || length > max)
                throw new ArgumentOutOfRangeException("length", "Error. Length is out of range.");

            long index = (long)Math.Floor((double)((length - min) * 2.0));
            return LMS.GetScore(index, isBoy, weight, WflBoys, WflGirls);
        }

        /// <summary>
        /// Weight-for-height: 2 to 5 years.
        /// 
        /// \warning The function does not receive age as an argument and is 
        /// thus not checking the child is of applicable age range.
        /// </summary>
        /// <param name="height">Height, cm.</param>
        /// <param name="isBoy">True for boys, false for girls.</param>
        /// <param name="weight">Weight, kg.</param>
        /// <returns>Z-score.</returns>
        /// 
        /// @throws ArgumentNullException if any of the arguments is null.
        /// @throws ArgumentException if the @a weight is less than or equal to zero.
        /// @throws ArgumentOutOfRangeException if the @a height is out of range [65.0, 120.0].
        public static double Wfh(double? height, bool? isBoy, double? weight)
        {
            const double min = 65.0;
            const double max = 120.0;

            if (height.HasValue == false)
                throw new ArgumentNullException("height", "Error. Height may not be null.");

            if (weight <= 0)
                throw new ArgumentException("Error. Weight must be positive.", "weight");

            if (height < min || height > max)
                throw new ArgumentOutOfRangeException("height", "Error. Height is out of range.");


            var index = (int)Math.Floor((double)((height - min) * 2.0));
            return LMS.GetScore(index, isBoy, weight, WfhBoys, WfhGirls);
        }

        /** 
    * Using data from: http://www.who.int/childgrowth/standards/
    * as of 22 Dec 2014
    */


        #region _bmiBoys

        private static readonly LMS[] BmifaBoys = new[]
        {
            new LMS(-0.30530, 13.40690, 0.09560),
            new LMS(0.27080, 14.94410, 0.09027),
            new LMS(0.11180, 16.31950, 0.08677),
            new LMS(0.00680, 16.89870, 0.08495),
            new LMS(-0.07270, 17.15790, 0.08378),
            new LMS(-0.13700, 17.29190, 0.08296),
            new LMS(-0.19130, 17.34220, 0.08234),
            new LMS(-0.23850, 17.32880, 0.08183),
            new LMS(-0.28020, 17.26470, 0.08140),
            new LMS(-0.31760, 17.16620, 0.08102),
            new LMS(-0.35160, 17.04880, 0.08068),
            new LMS(-0.38280, 16.92390, 0.08037),
            new LMS(-0.41150, 16.79810, 0.08009),
            new LMS(-0.43820, 16.67430, 0.07982),
            new LMS(-0.46300, 16.55480, 0.07958),
            new LMS(-0.48630, 16.44090, 0.07935),
            new LMS(-0.50820, 16.33350, 0.07913),
            new LMS(-0.52890, 16.23290, 0.07892),
            new LMS(-0.54840, 16.13920, 0.07873),
            new LMS(-0.56690, 16.05280, 0.07854),
            new LMS(-0.58460, 15.97430, 0.07836),
            new LMS(-0.60140, 15.90390, 0.07818),
            new LMS(-0.61740, 15.84120, 0.07802),
            new LMS(-0.63280, 15.78520, 0.07786),
            new LMS(-0.64730, 15.73560, 0.07771),
            new LMS(-0.58400, 15.98000, 0.07792),
            new LMS(-0.54970, 15.94140, 0.07800),
            new LMS(-0.51660, 15.90360, 0.07808),
            new LMS(-0.48500, 15.86670, 0.07818),
            new LMS(-0.45520, 15.83060, 0.07829),
            new LMS(-0.42740, 15.79530, 0.07841),
            new LMS(-0.40160, 15.76060, 0.07854),
            new LMS(-0.37820, 15.72670, 0.07867),
            new LMS(-0.35720, 15.69340, 0.07882),
            new LMS(-0.33880, 15.66100, 0.07897),
            new LMS(-0.32310, 15.62940, 0.07914),
            new LMS(-0.31010, 15.59880, 0.07931),
            new LMS(-0.30000, 15.56930, 0.07950),
            new LMS(-0.29270, 15.54100, 0.07969),
            new LMS(-0.28840, 15.51400, 0.07990),
            new LMS(-0.28690, 15.48850, 0.08012),
            new LMS(-0.28810, 15.46450, 0.08036),
            new LMS(-0.29190, 15.44200, 0.08061),
            new LMS(-0.29810, 15.42100, 0.08087),
            new LMS(-0.30670, 15.40130, 0.08115),
            new LMS(-0.31740, 15.38270, 0.08144),
            new LMS(-0.33030, 15.36520, 0.08174),
            new LMS(-0.34520, 15.34850, 0.08205),
            new LMS(-0.36220, 15.33260, 0.08238),
            new LMS(-0.38110, 15.31740, 0.08272),
            new LMS(-0.40190, 15.30290, 0.08307),
            new LMS(-0.42450, 15.28910, 0.08343),
            new LMS(-0.44880, 15.27590, 0.08380),
            new LMS(-0.47470, 15.26330, 0.08418),
            new LMS(-0.50190, 15.25140, 0.08457),
            new LMS(-0.53030, 15.24000, 0.08496),
            new LMS(-0.55990, 15.22910, 0.08536),
            new LMS(-0.59050, 15.21880, 0.08577),
            new LMS(-0.62230, 15.20910, 0.08617),
            new LMS(-0.65520, 15.20000, 0.08659),
            new LMS(-0.68920, 15.19160, 0.08700)
        };

        #endregion

        #region _bmiGirls

        private static readonly LMS[] BmifaGirls = new[]
        {
            new LMS(-0.06310, 13.33630, 0.09272),
            new LMS(0.34480, 14.56790, 0.09556),
            new LMS(0.17490, 15.76790, 0.09371),
            new LMS(0.06430, 16.35740, 0.09254),
            new LMS(-0.01910, 16.67030, 0.09166),
            new LMS(-0.08640, 16.83860, 0.09096),
            new LMS(-0.14290, 16.90830, 0.09036),
            new LMS(-0.19160, 16.90200, 0.08984),
            new LMS(-0.23440, 16.84040, 0.08939),
            new LMS(-0.27250, 16.74060, 0.08898),
            new LMS(-0.30680, 16.61840, 0.08861),
            new LMS(-0.33810, 16.48750, 0.08828),
            new LMS(-0.36670, 16.35680, 0.08797),
            new LMS(-0.39320, 16.23110, 0.08768),
            new LMS(-0.41770, 16.11280, 0.08741),
            new LMS(-0.44070, 16.00280, 0.08716),
            new LMS(-0.46230, 15.90170, 0.08693),
            new LMS(-0.48250, 15.80960, 0.08671),
            new LMS(-0.50170, 15.72630, 0.08650),
            new LMS(-0.51990, 15.65170, 0.08630),
            new LMS(-0.53720, 15.58550, 0.08612),
            new LMS(-0.55370, 15.52780, 0.08594),
            new LMS(-0.56950, 15.47870, 0.08577),
            new LMS(-0.58460, 15.43800, 0.08560),
            new LMS(-0.59890, 15.40520, 0.08545),
            new LMS(-0.56840, 15.65900, 0.08452),
            new LMS(-0.56840, 15.63080, 0.08449),
            new LMS(-0.56840, 15.60370, 0.08446),
            new LMS(-0.56840, 15.57770, 0.08444),
            new LMS(-0.56840, 15.55230, 0.08443),
            new LMS(-0.56840, 15.52760, 0.08444),
            new LMS(-0.56840, 15.50340, 0.08448),
            new LMS(-0.56840, 15.47980, 0.08455),
            new LMS(-0.56840, 15.45720, 0.08467),
            new LMS(-0.56840, 15.43560, 0.08484),
            new LMS(-0.56840, 15.41550, 0.08506),
            new LMS(-0.56840, 15.39680, 0.08535),
            new LMS(-0.56840, 15.37960, 0.08569),
            new LMS(-0.56840, 15.36380, 0.08609),
            new LMS(-0.56840, 15.34930, 0.08654),
            new LMS(-0.56840, 15.33580, 0.08704),
            new LMS(-0.56840, 15.32330, 0.08757),
            new LMS(-0.56840, 15.31160, 0.08813),
            new LMS(-0.56840, 15.30070, 0.08872),
            new LMS(-0.56840, 15.29050, 0.08931),
            new LMS(-0.56840, 15.28140, 0.08991),
            new LMS(-0.56840, 15.27320, 0.09051),
            new LMS(-0.56840, 15.26610, 0.09110),
            new LMS(-0.56840, 15.26020, 0.09168),
            new LMS(-0.56840, 15.25560, 0.09227),
            new LMS(-0.56840, 15.25230, 0.09286),
            new LMS(-0.56840, 15.25030, 0.09345),
            new LMS(-0.56840, 15.24960, 0.09403),
            new LMS(-0.56840, 15.25020, 0.09460),
            new LMS(-0.56840, 15.25190, 0.09515),
            new LMS(-0.56840, 15.25440, 0.09568),
            new LMS(-0.56840, 15.25750, 0.09618),
            new LMS(-0.56840, 15.26120, 0.09665),
            new LMS(-0.56840, 15.26530, 0.09709),
            new LMS(-0.56840, 15.26980, 0.09750),
            new LMS(-0.56840, 15.27470, 0.09789)
        };

        #endregion

        #region _hcfaBoys

        private static readonly LMS[] HcfaBoys = new[]
        {
            new LMS(1.00000, 34.46180, 0.03686),
            new LMS(1.00000, 37.27590, 0.03133),
            new LMS(1.00000, 39.12850, 0.02997),
            new LMS(1.00000, 40.51350, 0.02918),
            new LMS(1.00000, 41.63170, 0.02868),
            new LMS(1.00000, 42.55760, 0.02837),
            new LMS(1.00000, 43.33060, 0.02817),
            new LMS(1.00000, 43.98030, 0.02804),
            new LMS(1.00000, 44.53000, 0.02796),
            new LMS(1.00000, 44.99980, 0.02792),
            new LMS(1.00000, 45.40510, 0.02790),
            new LMS(1.00000, 45.75730, 0.02789),
            new LMS(1.00000, 46.06610, 0.02789),
            new LMS(1.00000, 46.33950, 0.02789),
            new LMS(1.00000, 46.58440, 0.02791),
            new LMS(1.00000, 46.80600, 0.02792),
            new LMS(1.00000, 47.00880, 0.02795),
            new LMS(1.00000, 47.19620, 0.02797),
            new LMS(1.00000, 47.37110, 0.02800),
            new LMS(1.00000, 47.53570, 0.02803),
            new LMS(1.00000, 47.69190, 0.02806),
            new LMS(1.00000, 47.84080, 0.02810),
            new LMS(1.00000, 47.98330, 0.02813),
            new LMS(1.00000, 48.12010, 0.02817),
            new LMS(1.00000, 48.25150, 0.02821),
            new LMS(1.00000, 48.37770, 0.02825),
            new LMS(1.00000, 48.49890, 0.02830),
            new LMS(1.00000, 48.61510, 0.02834),
            new LMS(1.00000, 48.72640, 0.02838),
            new LMS(1.00000, 48.83310, 0.02842),
            new LMS(1.00000, 48.93510, 0.02847),
            new LMS(1.00000, 49.03270, 0.02851),
            new LMS(1.00000, 49.12600, 0.02855),
            new LMS(1.00000, 49.21530, 0.02859),
            new LMS(1.00000, 49.30070, 0.02863),
            new LMS(1.00000, 49.38260, 0.02867),
            new LMS(1.00000, 49.46120, 0.02871),
            new LMS(1.00000, 49.53670, 0.02875),
            new LMS(1.00000, 49.60930, 0.02878),
            new LMS(1.00000, 49.67910, 0.02882),
            new LMS(1.00000, 49.74650, 0.02886),
            new LMS(1.00000, 49.81160, 0.02889),
            new LMS(1.00000, 49.87450, 0.02893),
            new LMS(1.00000, 49.93540, 0.02896),
            new LMS(1.00000, 49.99420, 0.02899),
            new LMS(1.00000, 50.05120, 0.02903),
            new LMS(1.00000, 50.10640, 0.02906),
            new LMS(1.00000, 50.15980, 0.02909),
            new LMS(1.00000, 50.21150, 0.02912),
            new LMS(1.00000, 50.26170, 0.02915),
            new LMS(1.00000, 50.31050, 0.02918),
            new LMS(1.00000, 50.35780, 0.02921),
            new LMS(1.00000, 50.40390, 0.02924),
            new LMS(1.00000, 50.44880, 0.02927),
            new LMS(1.00000, 50.49260, 0.02929),
            new LMS(1.00000, 50.53540, 0.02932),
            new LMS(1.00000, 50.57720, 0.02935),
            new LMS(1.00000, 50.61830, 0.02938),
            new LMS(1.00000, 50.65870, 0.02940),
            new LMS(1.00000, 50.69840, 0.02943),
            new LMS(1.00000, 50.73750, 0.02946)
        };

        #endregion

        #region _hcfaGirls

        private static readonly LMS[] HcfaGirls = new[]
        {
            new LMS(1.00000, 33.87870, 0.03496),
            new LMS(1.00000, 36.54630, 0.03210),
            new LMS(1.00000, 38.25210, 0.03168),
            new LMS(1.00000, 39.53280, 0.03140),
            new LMS(1.00000, 40.58170, 0.03119),
            new LMS(1.00000, 41.45900, 0.03102),
            new LMS(1.00000, 42.19950, 0.03087),
            new LMS(1.00000, 42.82900, 0.03075),
            new LMS(1.00000, 43.36710, 0.03063),
            new LMS(1.00000, 43.83000, 0.03053),
            new LMS(1.00000, 44.23190, 0.03044),
            new LMS(1.00000, 44.58440, 0.03035),
            new LMS(1.00000, 44.89650, 0.03027),
            new LMS(1.00000, 45.17520, 0.03019),
            new LMS(1.00000, 45.42650, 0.03012),
            new LMS(1.00000, 45.65510, 0.03006),
            new LMS(1.00000, 45.86500, 0.02999),
            new LMS(1.00000, 46.05980, 0.02993),
            new LMS(1.00000, 46.24240, 0.02987),
            new LMS(1.00000, 46.41520, 0.02982),
            new LMS(1.00000, 46.58010, 0.02977),
            new LMS(1.00000, 46.73840, 0.02972),
            new LMS(1.00000, 46.89130, 0.02967),
            new LMS(1.00000, 47.03910, 0.02962),
            new LMS(1.00000, 47.18220, 0.02957),
            new LMS(1.00000, 47.32040, 0.02953),
            new LMS(1.00000, 47.45360, 0.02949),
            new LMS(1.00000, 47.58170, 0.02945),
            new LMS(1.00000, 47.70450, 0.02941),
            new LMS(1.00000, 47.82190, 0.02937),
            new LMS(1.00000, 47.93400, 0.02933),
            new LMS(1.00000, 48.04100, 0.02929),
            new LMS(1.00000, 48.14320, 0.02926),
            new LMS(1.00000, 48.24080, 0.02922),
            new LMS(1.00000, 48.33430, 0.02919),
            new LMS(1.00000, 48.42390, 0.02915),
            new LMS(1.00000, 48.50990, 0.02912),
            new LMS(1.00000, 48.59260, 0.02909),
            new LMS(1.00000, 48.67220, 0.02906),
            new LMS(1.00000, 48.74890, 0.02903),
            new LMS(1.00000, 48.82280, 0.02900),
            new LMS(1.00000, 48.89410, 0.02897),
            new LMS(1.00000, 48.96290, 0.02894),
            new LMS(1.00000, 49.02940, 0.02891),
            new LMS(1.00000, 49.09370, 0.02888),
            new LMS(1.00000, 49.15600, 0.02886),
            new LMS(1.00000, 49.21640, 0.02883),
            new LMS(1.00000, 49.27510, 0.02880),
            new LMS(1.00000, 49.33210, 0.02878),
            new LMS(1.00000, 49.38770, 0.02875),
            new LMS(1.00000, 49.44190, 0.02873),
            new LMS(1.00000, 49.49470, 0.02870),
            new LMS(1.00000, 49.54640, 0.02868),
            new LMS(1.00000, 49.59690, 0.02865),
            new LMS(1.00000, 49.64640, 0.02863),
            new LMS(1.00000, 49.69470, 0.02861),
            new LMS(1.00000, 49.74210, 0.02859),
            new LMS(1.00000, 49.78850, 0.02856),
            new LMS(1.00000, 49.83410, 0.02854),
            new LMS(1.00000, 49.87890, 0.02852),
            new LMS(1.00000, 49.92290, 0.02850)
        };

        #endregion

        #region _wfaBoys

        private static readonly LMS[] WfaBoys = new[]
        {
            new LMS(0.34870, 3.34640, 0.14602),
            new LMS(0.22970, 4.47090, 0.13395),
            new LMS(0.19700, 5.56750, 0.12385),
            new LMS(0.17380, 6.37620, 0.11727),
            new LMS(0.15530, 7.00230, 0.11316),
            new LMS(0.13950, 7.51050, 0.11080),
            new LMS(0.12570, 7.93400, 0.10958),
            new LMS(0.11340, 8.29700, 0.10902),
            new LMS(0.10210, 8.61510, 0.10882),
            new LMS(0.09170, 8.90140, 0.10881),
            new LMS(0.08200, 9.16490, 0.10891),
            new LMS(0.07300, 9.41220, 0.10906),
            new LMS(0.06440, 9.64790, 0.10925),
            new LMS(0.05630, 9.87490, 0.10949),
            new LMS(0.04870, 10.09530, 0.10976),
            new LMS(0.04130, 10.31080, 0.11007),
            new LMS(0.03430, 10.52280, 0.11041),
            new LMS(0.02750, 10.73190, 0.11079),
            new LMS(0.02110, 10.93850, 0.11119),
            new LMS(0.01480, 11.14300, 0.11164),
            new LMS(0.00870, 11.34620, 0.11211),
            new LMS(0.00290, 11.54860, 0.11261),
            new LMS(-0.00280, 11.75040, 0.11314),
            new LMS(-0.00830, 11.95140, 0.11369),
            new LMS(-0.01370, 12.15150, 0.11426),
            new LMS(-0.01890, 12.35020, 0.11485),
            new LMS(-0.02400, 12.54660, 0.11544),
            new LMS(-0.02890, 12.74010, 0.11604),
            new LMS(-0.03370, 12.93030, 0.11664),
            new LMS(-0.03850, 13.11690, 0.11723),
            new LMS(-0.04310, 13.30000, 0.11781),
            new LMS(-0.04760, 13.47980, 0.11839),
            new LMS(-0.05200, 13.65670, 0.11896),
            new LMS(-0.05640, 13.83090, 0.11953),
            new LMS(-0.06060, 14.00310, 0.12008),
            new LMS(-0.06480, 14.17360, 0.12062),
            new LMS(-0.06890, 14.34290, 0.12116),
            new LMS(-0.07290, 14.51130, 0.12168),
            new LMS(-0.07690, 14.67910, 0.12220),
            new LMS(-0.08080, 14.84660, 0.12271),
            new LMS(-0.08460, 15.01400, 0.12322),
            new LMS(-0.08830, 15.18130, 0.12373),
            new LMS(-0.09200, 15.34860, 0.12425),
            new LMS(-0.09570, 15.51580, 0.12478),
            new LMS(-0.09930, 15.68280, 0.12531),
            new LMS(-0.10280, 15.84970, 0.12586),
            new LMS(-0.10630, 16.01630, 0.12643),
            new LMS(-0.10970, 16.18270, 0.12700),
            new LMS(-0.11310, 16.34890, 0.12759),
            new LMS(-0.11650, 16.51500, 0.12819),
            new LMS(-0.11980, 16.68110, 0.12880),
            new LMS(-0.12300, 16.84710, 0.12943),
            new LMS(-0.12620, 17.01320, 0.13005),
            new LMS(-0.12940, 17.17920, 0.13069),
            new LMS(-0.13250, 17.34520, 0.13133),
            new LMS(-0.13560, 17.51110, 0.13197),
            new LMS(-0.13870, 17.67680, 0.13261),
            new LMS(-0.14170, 17.84220, 0.13325),
            new LMS(-0.14470, 18.00730, 0.13389),
            new LMS(-0.14770, 18.17220, 0.13453),
            new LMS(-0.15060, 18.33660, 0.13517)
        };

        #endregion

        #region _wfaGirls

        private static readonly LMS[] WfaGirls = new[]
        {
            new LMS(0.38090, 3.23220, 0.14171),
            new LMS(0.17140, 4.18730, 0.13724),
            new LMS(0.09620, 5.12820, 0.13000),
            new LMS(0.04020, 5.84580, 0.12619),
            new LMS(-0.00500, 6.42370, 0.12402),
            new LMS(-0.04300, 6.89850, 0.12274),
            new LMS(-0.07560, 7.29700, 0.12204),
            new LMS(-0.10390, 7.64220, 0.12178),
            new LMS(-0.12880, 7.94870, 0.12181),
            new LMS(-0.15070, 8.22540, 0.12199),
            new LMS(-0.17000, 8.48000, 0.12223),
            new LMS(-0.18720, 8.71920, 0.12247),
            new LMS(-0.20240, 8.94810, 0.12268),
            new LMS(-0.21580, 9.16990, 0.12283),
            new LMS(-0.22780, 9.38700, 0.12294),
            new LMS(-0.23840, 9.60080, 0.12299),
            new LMS(-0.24780, 9.81240, 0.12303),
            new LMS(-0.25620, 10.02260, 0.12306),
            new LMS(-0.26370, 10.23150, 0.12309),
            new LMS(-0.27030, 10.43930, 0.12315),
            new LMS(-0.27620, 10.64640, 0.12323),
            new LMS(-0.28150, 10.85340, 0.12335),
            new LMS(-0.28620, 11.06080, 0.12350),
            new LMS(-0.29030, 11.26880, 0.12369),
            new LMS(-0.29410, 11.47750, 0.12390),
            new LMS(-0.29750, 11.68640, 0.12414),
            new LMS(-0.30050, 11.89470, 0.12441),
            new LMS(-0.30320, 12.10150, 0.12472),
            new LMS(-0.30570, 12.30590, 0.12506),
            new LMS(-0.30800, 12.50730, 0.12545),
            new LMS(-0.31010, 12.70550, 0.12587),
            new LMS(-0.31200, 12.90060, 0.12633),
            new LMS(-0.31380, 13.09300, 0.12683),
            new LMS(-0.31550, 13.28370, 0.12737),
            new LMS(-0.31710, 13.47310, 0.12794),
            new LMS(-0.31860, 13.66180, 0.12855),
            new LMS(-0.32010, 13.85030, 0.12919),
            new LMS(-0.32160, 14.03850, 0.12988),
            new LMS(-0.32300, 14.22650, 0.13059),
            new LMS(-0.32430, 14.41400, 0.13135),
            new LMS(-0.32570, 14.60100, 0.13213),
            new LMS(-0.32700, 14.78730, 0.13293),
            new LMS(-0.32830, 14.97270, 0.13376),
            new LMS(-0.32960, 15.15730, 0.13460),
            new LMS(-0.33090, 15.34100, 0.13545),
            new LMS(-0.33220, 15.52400, 0.13630),
            new LMS(-0.33350, 15.70640, 0.13716),
            new LMS(-0.33480, 15.88820, 0.13800),
            new LMS(-0.33610, 16.06970, 0.13884),
            new LMS(-0.33740, 16.25110, 0.13968),
            new LMS(-0.33870, 16.43220, 0.14051),
            new LMS(-0.34000, 16.61330, 0.14132),
            new LMS(-0.34140, 16.79420, 0.14213),
            new LMS(-0.34270, 16.97480, 0.14293),
            new LMS(-0.34400, 17.15510, 0.14371),
            new LMS(-0.34530, 17.33470, 0.14448),
            new LMS(-0.34660, 17.51360, 0.14525),
            new LMS(-0.34790, 17.69160, 0.14600),
            new LMS(-0.34920, 17.86860, 0.14675),
            new LMS(-0.35050, 18.04450, 0.14748),
            new LMS(-0.35180, 18.21930, 0.14821)
        };

        #endregion

        #region _lhfaBoys

        private static readonly LMS[] LhfaBoys = new[]
        {
            new LMS(1.00000, 49.88420, 0.03795),
            new LMS(1.00000, 54.72440, 0.03557),
            new LMS(1.00000, 58.42490, 0.03424),
            new LMS(1.00000, 61.42920, 0.03328),
            new LMS(1.00000, 63.88600, 0.03257),
            new LMS(1.00000, 65.90260, 0.03204),
            new LMS(1.00000, 67.62360, 0.03165),
            new LMS(1.00000, 69.16450, 0.03139),
            new LMS(1.00000, 70.59940, 0.03124),
            new LMS(1.00000, 71.96870, 0.03117),
            new LMS(1.00000, 73.28120, 0.03118),
            new LMS(1.00000, 74.53880, 0.03125),
            new LMS(1.00000, 75.74880, 0.03137),
            new LMS(1.00000, 76.91860, 0.03154),
            new LMS(1.00000, 78.04970, 0.03174),
            new LMS(1.00000, 79.14580, 0.03197),
            new LMS(1.00000, 80.21130, 0.03222),
            new LMS(1.00000, 81.24870, 0.03250),
            new LMS(1.00000, 82.25870, 0.03279),
            new LMS(1.00000, 83.24180, 0.03310),
            new LMS(1.00000, 84.19960, 0.03342),
            new LMS(1.00000, 85.13480, 0.03376),
            new LMS(1.00000, 86.04770, 0.03410),
            new LMS(1.00000, 86.94100, 0.03445),
            new LMS(1.00000, 87.81610, 0.03479),
            new LMS(1.00000, 87.97200, 0.03542),
            new LMS(1.00000, 88.80650, 0.03576),
            new LMS(1.00000, 89.61970, 0.03610),
            new LMS(1.00000, 90.41200, 0.03642),
            new LMS(1.00000, 91.18280, 0.03674),
            new LMS(1.00000, 91.93270, 0.03704),
            new LMS(1.00000, 92.66310, 0.03733),
            new LMS(1.00000, 93.37530, 0.03761),
            new LMS(1.00000, 94.07110, 0.03787),
            new LMS(1.00000, 94.75320, 0.03812),
            new LMS(1.00000, 95.42360, 0.03836),
            new LMS(1.00000, 96.08350, 0.03858),
            new LMS(1.00000, 96.73370, 0.03879),
            new LMS(1.00000, 97.37490, 0.03900),
            new LMS(1.00000, 98.00730, 0.03919),
            new LMS(1.00000, 98.63100, 0.03937),
            new LMS(1.00000, 99.24590, 0.03954),
            new LMS(1.00000, 99.85150, 0.03971),
            new LMS(1.00000, 100.44850, 0.03986),
            new LMS(1.00000, 101.03740, 0.04002),
            new LMS(1.00000, 101.61860, 0.04016),
            new LMS(1.00000, 102.19330, 0.04031),
            new LMS(1.00000, 102.76250, 0.04045),
            new LMS(1.00000, 103.32730, 0.04059),
            new LMS(1.00000, 103.88860, 0.04073),
            new LMS(1.00000, 104.44730, 0.04086),
            new LMS(1.00000, 105.00410, 0.04100),
            new LMS(1.00000, 105.55960, 0.04113),
            new LMS(1.00000, 106.11380, 0.04126),
            new LMS(1.00000, 106.66680, 0.04139),
            new LMS(1.00000, 107.21880, 0.04152),
            new LMS(1.00000, 107.76970, 0.04165),
            new LMS(1.00000, 108.31980, 0.04177),
            new LMS(1.00000, 108.86890, 0.04190),
            new LMS(1.00000, 109.41700, 0.04202),
            new LMS(1.00000, 109.96380, 0.04214)
        };

        #endregion

        #region _lhfaGirls

        private static readonly LMS[] LhfaGirls = new[]
        {
            new LMS(1.00000, 49.14770, 0.03790),
            new LMS(1.00000, 53.68720, 0.03640),
            new LMS(1.00000, 57.06730, 0.03568),
            new LMS(1.00000, 59.80290, 0.03520),
            new LMS(1.00000, 62.08990, 0.03486),
            new LMS(1.00000, 64.03010, 0.03463),
            new LMS(1.00000, 65.73110, 0.03448),
            new LMS(1.00000, 67.28730, 0.03441),
            new LMS(1.00000, 68.74980, 0.03440),
            new LMS(1.00000, 70.14350, 0.03444),
            new LMS(1.00000, 71.48180, 0.03452),
            new LMS(1.00000, 72.77100, 0.03464),
            new LMS(1.00000, 74.01500, 0.03479),
            new LMS(1.00000, 75.21760, 0.03496),
            new LMS(1.00000, 76.38170, 0.03514),
            new LMS(1.00000, 77.50990, 0.03534),
            new LMS(1.00000, 78.60550, 0.03555),
            new LMS(1.00000, 79.67100, 0.03576),
            new LMS(1.00000, 80.70790, 0.03598),
            new LMS(1.00000, 81.71820, 0.03620),
            new LMS(1.00000, 82.70360, 0.03643),
            new LMS(1.00000, 83.66540, 0.03666),
            new LMS(1.00000, 84.60400, 0.03688),
            new LMS(1.00000, 85.52020, 0.03711),
            new LMS(1.00000, 86.41530, 0.03734),
            new LMS(1.00000, 86.59040, 0.03786),
            new LMS(1.00000, 87.44620, 0.03808),
            new LMS(1.00000, 88.28300, 0.03830),
            new LMS(1.00000, 89.10040, 0.03851),
            new LMS(1.00000, 89.89910, 0.03872),
            new LMS(1.00000, 90.67970, 0.03893),
            new LMS(1.00000, 91.44300, 0.03913),
            new LMS(1.00000, 92.19060, 0.03933),
            new LMS(1.00000, 92.92390, 0.03952),
            new LMS(1.00000, 93.64440, 0.03971),
            new LMS(1.00000, 94.35330, 0.03989),
            new LMS(1.00000, 95.05150, 0.04006),
            new LMS(1.00000, 95.73990, 0.04024),
            new LMS(1.00000, 96.41870, 0.04041),
            new LMS(1.00000, 97.08850, 0.04057),
            new LMS(1.00000, 97.74930, 0.04073),
            new LMS(1.00000, 98.40150, 0.04089),
            new LMS(1.00000, 99.04480, 0.04105),
            new LMS(1.00000, 99.67950, 0.04120),
            new LMS(1.00000, 100.30580, 0.04135),
            new LMS(1.00000, 100.92380, 0.04150),
            new LMS(1.00000, 101.53370, 0.04164),
            new LMS(1.00000, 102.13600, 0.04179),
            new LMS(1.00000, 102.73120, 0.04193),
            new LMS(1.00000, 103.31970, 0.04206),
            new LMS(1.00000, 103.90210, 0.04220),
            new LMS(1.00000, 104.47860, 0.04233),
            new LMS(1.00000, 105.04940, 0.04246),
            new LMS(1.00000, 105.61480, 0.04259),
            new LMS(1.00000, 106.17480, 0.04272),
            new LMS(1.00000, 106.72950, 0.04285),
            new LMS(1.00000, 107.27880, 0.04298),
            new LMS(1.00000, 107.82270, 0.04310),
            new LMS(1.00000, 108.36130, 0.04322),
            new LMS(1.00000, 108.89480, 0.04334),
            new LMS(1.00000, 109.42330, 0.04347)
        };

        #endregion

        #region _tsfaBoys

        private static readonly LMS[] TsfaBoys = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00270, 9.76390, 0.16618),
            new LMS(-0.01650, 9.58400, 0.17264),
            new LMS(-0.03260, 9.38850, 0.17824),
            new LMS(-0.04660, 9.17290, 0.18304),
            new LMS(-0.05900, 8.95350, 0.18685),
            new LMS(-0.07030, 8.74350, 0.18968),
            new LMS(-0.08060, 8.55180, 0.19166),
            new LMS(-0.09010, 8.38120, 0.19300),
            new LMS(-0.09900, 8.23230, 0.19389),
            new LMS(-0.10730, 8.10410, 0.19453),
            new LMS(-0.11520, 7.99580, 0.19506),
            new LMS(-0.12270, 7.90640, 0.19558),
            new LMS(-0.12970, 7.83450, 0.19612),
            new LMS(-0.13650, 7.77810, 0.19668),
            new LMS(-0.14300, 7.73510, 0.19728),
            new LMS(-0.14920, 7.70360, 0.19793),
            new LMS(-0.15520, 7.68210, 0.19862),
            new LMS(-0.16090, 7.66970, 0.19937),
            new LMS(-0.16650, 7.66520, 0.20018),
            new LMS(-0.17190, 7.66750, 0.20105),
            new LMS(-0.17710, 7.67500, 0.20196),
            new LMS(-0.18210, 7.68630, 0.20293),
            new LMS(-0.18700, 7.70030, 0.20394),
            new LMS(-0.19180, 7.71560, 0.20497),
            new LMS(-0.19650, 7.73120, 0.20603),
            new LMS(-0.20100, 7.74630, 0.20710),
            new LMS(-0.20540, 7.76020, 0.20818),
            new LMS(-0.20970, 7.77260, 0.20928),
            new LMS(-0.21390, 7.78320, 0.21039),
            new LMS(-0.21800, 7.79200, 0.21153),
            new LMS(-0.22210, 7.79890, 0.21269),
            new LMS(-0.22600, 7.80400, 0.21389),
            new LMS(-0.22990, 7.80740, 0.21513),
            new LMS(-0.23360, 7.80940, 0.21641),
            new LMS(-0.23740, 7.81010, 0.21773),
            new LMS(-0.24100, 7.80960, 0.21909),
            new LMS(-0.24460, 7.80800, 0.22049),
            new LMS(-0.24810, 7.80510, 0.22194),
            new LMS(-0.25150, 7.80090, 0.22343),
            new LMS(-0.25490, 7.79540, 0.22496),
            new LMS(-0.25830, 7.78850, 0.22653),
            new LMS(-0.26160, 7.78040, 0.22813),
            new LMS(-0.26480, 7.77100, 0.22975),
            new LMS(-0.26800, 7.76050, 0.23140),
            new LMS(-0.27110, 7.74890, 0.23306),
            new LMS(-0.27420, 7.73640, 0.23473),
            new LMS(-0.27720, 7.72330, 0.23642),
            new LMS(-0.28020, 7.70960, 0.23811),
            new LMS(-0.28320, 7.69550, 0.23981),
            new LMS(-0.28610, 7.68120, 0.24151),
            new LMS(-0.28900, 7.66690, 0.24322),
            new LMS(-0.29180, 7.65250, 0.24494),
            new LMS(-0.29460, 7.63830, 0.24666),
            new LMS(-0.29740, 7.62420, 0.24839),
            new LMS(-0.30010, 7.61040, 0.25013),
            new LMS(-0.30280, 7.59680, 0.25186),
            new LMS(-0.30550, 7.58350, 0.25360),
            new LMS(-0.30810, 7.57060, 0.25533)
        };

        #endregion

        #region _tsfaGirls

        private static readonly LMS[] TsfaGirls = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.18750, 9.75160, 0.17535),
            new LMS(0.12560, 9.58660, 0.18337),
            new LMS(0.07610, 9.37160, 0.19007),
            new LMS(0.03490, 9.11940, 0.19540),
            new LMS(-0.00030, 8.86210, 0.19934),
            new LMS(-0.03070, 8.62280, 0.20192),
            new LMS(-0.05720, 8.41640, 0.20339),
            new LMS(-0.07990, 8.24680, 0.20413),
            new LMS(-0.09950, 8.11140, 0.20442),
            new LMS(-0.11610, 8.00420, 0.20445),
            new LMS(-0.13030, 7.91970, 0.20432),
            new LMS(-0.14240, 7.85380, 0.20409),
            new LMS(-0.15270, 7.80410, 0.20384),
            new LMS(-0.16150, 7.76810, 0.20363),
            new LMS(-0.16900, 7.74430, 0.20350),
            new LMS(-0.17550, 7.73150, 0.20350),
            new LMS(-0.18110, 7.72870, 0.20364),
            new LMS(-0.18590, 7.73470, 0.20393),
            new LMS(-0.19010, 7.74840, 0.20437),
            new LMS(-0.19390, 7.76920, 0.20496),
            new LMS(-0.19730, 7.79580, 0.20568),
            new LMS(-0.20040, 7.82730, 0.20652),
            new LMS(-0.20320, 7.86280, 0.20748),
            new LMS(-0.20580, 7.90060, 0.20855),
            new LMS(-0.20810, 7.93960, 0.20971),
            new LMS(-0.21030, 7.97860, 0.21096),
            new LMS(-0.21220, 8.01670, 0.21228),
            new LMS(-0.21400, 8.05350, 0.21366),
            new LMS(-0.21550, 8.08870, 0.21509),
            new LMS(-0.21700, 8.12240, 0.21657),
            new LMS(-0.21830, 8.15450, 0.21809),
            new LMS(-0.21950, 8.18550, 0.21964),
            new LMS(-0.22070, 8.21560, 0.22122),
            new LMS(-0.22170, 8.24500, 0.22282),
            new LMS(-0.22270, 8.27380, 0.22444),
            new LMS(-0.22370, 8.30190, 0.22608),
            new LMS(-0.22460, 8.32940, 0.22772),
            new LMS(-0.22540, 8.35600, 0.22937),
            new LMS(-0.22620, 8.38180, 0.23101),
            new LMS(-0.22700, 8.40680, 0.23264),
            new LMS(-0.22780, 8.43110, 0.23427),
            new LMS(-0.22850, 8.45500, 0.23587),
            new LMS(-0.22920, 8.47860, 0.23747),
            new LMS(-0.22980, 8.50190, 0.23904),
            new LMS(-0.23040, 8.52500, 0.24060),
            new LMS(-0.23100, 8.54810, 0.24215),
            new LMS(-0.23160, 8.57110, 0.24367),
            new LMS(-0.23210, 8.59420, 0.24517),
            new LMS(-0.23260, 8.61740, 0.24665),
            new LMS(-0.23310, 8.64060, 0.24811),
            new LMS(-0.23360, 8.66410, 0.24954),
            new LMS(-0.23410, 8.68760, 0.25095),
            new LMS(-0.23460, 8.71120, 0.25233),
            new LMS(-0.23500, 8.73490, 0.25369),
            new LMS(-0.23550, 8.75860, 0.25502),
            new LMS(-0.23590, 8.78240, 0.25633),
            new LMS(-0.23630, 8.80610, 0.25761),
            new LMS(-0.23680, 8.82980, 0.25887)
        };

        #endregion

        #region _acfaBoys

        private static readonly LMS[] AcfaBoys = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.39280, 13.48170, 0.07475),
            new LMS(0.34750, 13.80970, 0.07523),
            new LMS(0.30920, 14.05850, 0.07566),
            new LMS(0.27550, 14.23890, 0.07601),
            new LMS(0.24530, 14.36780, 0.07629),
            new LMS(0.21790, 14.45910, 0.07650),
            new LMS(0.19250, 14.52450, 0.07665),
            new LMS(0.16900, 14.57330, 0.07676),
            new LMS(0.14690, 14.61190, 0.07683),
            new LMS(0.12610, 14.64490, 0.07689),
            new LMS(0.10640, 14.67580, 0.07694),
            new LMS(0.08760, 14.70630, 0.07699),
            new LMS(0.06970, 14.73800, 0.07703),
            new LMS(0.05260, 14.77230, 0.07707),
            new LMS(0.03620, 14.80950, 0.07710),
            new LMS(0.02040, 14.84960, 0.07713),
            new LMS(0.00510, 14.89260, 0.07717),
            new LMS(-0.00970, 14.93880, 0.07721),
            new LMS(-0.02390, 14.98830, 0.07725),
            new LMS(-0.03780, 15.04100, 0.07731),
            new LMS(-0.05120, 15.09640, 0.07738),
            new LMS(-0.06430, 15.15360, 0.07746),
            new LMS(-0.07700, 15.21150, 0.07755),
            new LMS(-0.08940, 15.26930, 0.07767),
            new LMS(-0.10140, 15.32590, 0.07780),
            new LMS(-0.11320, 15.38080, 0.07794),
            new LMS(-0.12480, 15.43360, 0.07810),
            new LMS(-0.13600, 15.48390, 0.07827),
            new LMS(-0.14700, 15.53170, 0.07846),
            new LMS(-0.15780, 15.57710, 0.07866),
            new LMS(-0.16840, 15.62010, 0.07887),
            new LMS(-0.17880, 15.66110, 0.07909),
            new LMS(-0.18900, 15.70030, 0.07933),
            new LMS(-0.19890, 15.73800, 0.07956),
            new LMS(-0.20870, 15.77450, 0.07981),
            new LMS(-0.21840, 15.81010, 0.08006),
            new LMS(-0.22780, 15.84500, 0.08032),
            new LMS(-0.23720, 15.87930, 0.08058),
            new LMS(-0.24630, 15.91320, 0.08085),
            new LMS(-0.25530, 15.94670, 0.08112),
            new LMS(-0.26420, 15.97970, 0.08139),
            new LMS(-0.27300, 16.01240, 0.08166),
            new LMS(-0.28160, 16.04470, 0.08194),
            new LMS(-0.29010, 16.07670, 0.08222),
            new LMS(-0.29850, 16.10850, 0.08250),
            new LMS(-0.30670, 16.14000, 0.08278),
            new LMS(-0.31490, 16.17140, 0.08307),
            new LMS(-0.32290, 16.20270, 0.08335),
            new LMS(-0.33090, 16.23400, 0.08364),
            new LMS(-0.33870, 16.26540, 0.08392),
            new LMS(-0.34640, 16.29680, 0.08421),
            new LMS(-0.35410, 16.32830, 0.08450),
            new LMS(-0.36160, 16.35990, 0.08479),
            new LMS(-0.36910, 16.39160, 0.08508),
            new LMS(-0.37650, 16.42330, 0.08537),
            new LMS(-0.38380, 16.45510, 0.08566),
            new LMS(-0.39100, 16.48710, 0.08595),
            new LMS(-0.39810, 16.51910, 0.08624)
        };

        #endregion

        #region _acfaGirls

        private static readonly LMS[] AcfaGirls = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(-0.17330, 13.02840, 0.08263),
            new LMS(-0.17330, 13.36490, 0.08298),
            new LMS(-0.17330, 13.60610, 0.08325),
            new LMS(-0.17330, 13.77710, 0.08343),
            new LMS(-0.17330, 13.90180, 0.08352),
            new LMS(-0.17330, 13.99520, 0.08351),
            new LMS(-0.17330, 14.06650, 0.08342),
            new LMS(-0.17330, 14.12170, 0.08326),
            new LMS(-0.17330, 14.16670, 0.08305),
            new LMS(-0.17330, 14.20650, 0.08280),
            new LMS(-0.17330, 14.24550, 0.08254),
            new LMS(-0.17330, 14.28590, 0.08227),
            new LMS(-0.17330, 14.32890, 0.08202),
            new LMS(-0.17330, 14.37520, 0.08179),
            new LMS(-0.17330, 14.42540, 0.08160),
            new LMS(-0.17330, 14.47950, 0.08143),
            new LMS(-0.17330, 14.53720, 0.08131),
            new LMS(-0.17330, 14.59870, 0.08123),
            new LMS(-0.17330, 14.66390, 0.08118),
            new LMS(-0.17330, 14.73280, 0.08118),
            new LMS(-0.17330, 14.80490, 0.08121),
            new LMS(-0.17330, 14.87950, 0.08127),
            new LMS(-0.17330, 14.95590, 0.08136),
            new LMS(-0.17330, 15.03270, 0.08147),
            new LMS(-0.17330, 15.10850, 0.08161),
            new LMS(-0.17330, 15.18170, 0.08178),
            new LMS(-0.17330, 15.25140, 0.08196),
            new LMS(-0.17330, 15.31680, 0.08217),
            new LMS(-0.17330, 15.37790, 0.08240),
            new LMS(-0.17330, 15.43510, 0.08265),
            new LMS(-0.17330, 15.48950, 0.08292),
            new LMS(-0.17330, 15.54230, 0.08320),
            new LMS(-0.17330, 15.59410, 0.08351),
            new LMS(-0.17330, 15.64560, 0.08383),
            new LMS(-0.17330, 15.69690, 0.08416),
            new LMS(-0.17330, 15.74830, 0.08451),
            new LMS(-0.17330, 15.79970, 0.08487),
            new LMS(-0.17330, 15.85090, 0.08525),
            new LMS(-0.17330, 15.90160, 0.08563),
            new LMS(-0.17330, 15.95180, 0.08602),
            new LMS(-0.17330, 16.00160, 0.08642),
            new LMS(-0.17330, 16.05090, 0.08683),
            new LMS(-0.17330, 16.10010, 0.08723),
            new LMS(-0.17330, 16.14910, 0.08765),
            new LMS(-0.17330, 16.19830, 0.08806),
            new LMS(-0.17330, 16.24770, 0.08848),
            new LMS(-0.17330, 16.29740, 0.08890),
            new LMS(-0.17330, 16.34750, 0.08932),
            new LMS(-0.17330, 16.39810, 0.08974),
            new LMS(-0.17330, 16.44900, 0.09016),
            new LMS(-0.17330, 16.50010, 0.09057),
            new LMS(-0.17330, 16.55140, 0.09099),
            new LMS(-0.17330, 16.60260, 0.09140),
            new LMS(-0.17330, 16.65340, 0.09181),
            new LMS(-0.17330, 16.70390, 0.09221),
            new LMS(-0.17330, 16.75390, 0.09262),
            new LMS(-0.17330, 16.80340, 0.09301),
            new LMS(-0.17330, 16.85260, 0.09341)
        };

        #endregion

        #region _ssfaBoys

        private static readonly LMS[] SsfaBoys = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(-0.30330, 7.68990, 0.17020),
            new LMS(-0.32780, 7.49680, 0.17097),
            new LMS(-0.35030, 7.32070, 0.17167),
            new LMS(-0.37120, 7.15880, 0.17232),
            new LMS(-0.39090, 7.01040, 0.17293),
            new LMS(-0.40970, 6.87530, 0.17352),
            new LMS(-0.42760, 6.75300, 0.17408),
            new LMS(-0.44490, 6.64280, 0.17462),
            new LMS(-0.46160, 6.54420, 0.17514),
            new LMS(-0.47770, 6.45620, 0.17564),
            new LMS(-0.49340, 6.37800, 0.17613),
            new LMS(-0.50870, 6.30850, 0.17660),
            new LMS(-0.52360, 6.24680, 0.17707),
            new LMS(-0.53810, 6.19210, 0.17752),
            new LMS(-0.55240, 6.14350, 0.17797),
            new LMS(-0.56630, 6.10030, 0.17840),
            new LMS(-0.58000, 6.06170, 0.17883),
            new LMS(-0.59340, 6.02740, 0.17925),
            new LMS(-0.60660, 5.99720, 0.17966),
            new LMS(-0.61960, 5.97060, 0.18006),
            new LMS(-0.63240, 5.94700, 0.18046),
            new LMS(-0.64490, 5.92580, 0.18085),
            new LMS(-0.65730, 5.90670, 0.18124),
            new LMS(-0.66950, 5.88910, 0.18162),
            new LMS(-0.68160, 5.87290, 0.18199),
            new LMS(-0.69350, 5.85760, 0.18237),
            new LMS(-0.70530, 5.84310, 0.18273),
            new LMS(-0.71690, 5.82900, 0.18309),
            new LMS(-0.72830, 5.81500, 0.18345),
            new LMS(-0.73970, 5.80110, 0.18381),
            new LMS(-0.75090, 5.78700, 0.18416),
            new LMS(-0.76200, 5.77270, 0.18450),
            new LMS(-0.77300, 5.75800, 0.18485),
            new LMS(-0.78390, 5.74300, 0.18519),
            new LMS(-0.79470, 5.72780, 0.18552),
            new LMS(-0.80540, 5.71250, 0.18585),
            new LMS(-0.81590, 5.69710, 0.18618),
            new LMS(-0.82640, 5.68150, 0.18651),
            new LMS(-0.83680, 5.66580, 0.18684),
            new LMS(-0.84710, 5.65000, 0.18716),
            new LMS(-0.85740, 5.63390, 0.18748),
            new LMS(-0.86750, 5.61740, 0.18779),
            new LMS(-0.87750, 5.60060, 0.18811),
            new LMS(-0.88750, 5.58340, 0.18842),
            new LMS(-0.89740, 5.56590, 0.18873),
            new LMS(-0.90730, 5.54820, 0.18903),
            new LMS(-0.91700, 5.53030, 0.18934),
            new LMS(-0.92670, 5.51250, 0.18964),
            new LMS(-0.93630, 5.49480, 0.18994),
            new LMS(-0.94590, 5.47740, 0.19024),
            new LMS(-0.95540, 5.46060, 0.19053),
            new LMS(-0.96480, 5.44430, 0.19083),
            new LMS(-0.97420, 5.42880, 0.19112),
            new LMS(-0.98350, 5.41400, 0.19141),
            new LMS(-0.99280, 5.40000, 0.19170),
            new LMS(-1.00200, 5.38680, 0.19199),
            new LMS(-1.01110, 5.37440, 0.19227),
            new LMS(-1.02020, 5.36280, 0.19255)
        };

        #endregion

        #region _ssfaGirls

        private static readonly LMS[] SsfaGirls = new[]
        {
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(0.00000, 0.00000, 0.00000),
            new LMS(-0.20260, 7.78460, 0.18428),
            new LMS(-0.25770, 7.54050, 0.18430),
            new LMS(-0.30200, 7.33840, 0.18428),
            new LMS(-0.33940, 7.16370, 0.18425),
            new LMS(-0.37180, 7.01180, 0.18421),
            new LMS(-0.40050, 6.88070, 0.18412),
            new LMS(-0.42630, 6.76790, 0.18399),
            new LMS(-0.44980, 6.67070, 0.18387),
            new LMS(-0.47130, 6.58670, 0.18381),
            new LMS(-0.49120, 6.51380, 0.18383),
            new LMS(-0.50980, 6.45050, 0.18394),
            new LMS(-0.52720, 6.39550, 0.18415),
            new LMS(-0.54350, 6.34740, 0.18446),
            new LMS(-0.55900, 6.30550, 0.18487),
            new LMS(-0.57360, 6.26890, 0.18538),
            new LMS(-0.58760, 6.23730, 0.18598),
            new LMS(-0.60090, 6.21010, 0.18666),
            new LMS(-0.61360, 6.18680, 0.18741),
            new LMS(-0.62570, 6.16690, 0.18823),
            new LMS(-0.63740, 6.15000, 0.18911),
            new LMS(-0.64870, 6.13550, 0.19005),
            new LMS(-0.65950, 6.12320, 0.19104),
            new LMS(-0.67000, 6.11290, 0.19207),
            new LMS(-0.68010, 6.10410, 0.19315),
            new LMS(-0.68990, 6.09680, 0.19426),
            new LMS(-0.69940, 6.09050, 0.19540),
            new LMS(-0.70860, 6.08510, 0.19657),
            new LMS(-0.71750, 6.08060, 0.19776),
            new LMS(-0.72620, 6.07660, 0.19898),
            new LMS(-0.73470, 6.07330, 0.20021),
            new LMS(-0.74290, 6.07050, 0.20145),
            new LMS(-0.75090, 6.06830, 0.20270),
            new LMS(-0.75870, 6.06650, 0.20395),
            new LMS(-0.76640, 6.06520, 0.20521),
            new LMS(-0.77380, 6.06430, 0.20647),
            new LMS(-0.78110, 6.06370, 0.20773),
            new LMS(-0.78820, 6.06330, 0.20899),
            new LMS(-0.79520, 6.06320, 0.21024),
            new LMS(-0.80200, 6.06320, 0.21149),
            new LMS(-0.80870, 6.06340, 0.21273),
            new LMS(-0.81520, 6.06370, 0.21396),
            new LMS(-0.82170, 6.06410, 0.21518),
            new LMS(-0.82800, 6.06470, 0.21638),
            new LMS(-0.83410, 6.06530, 0.21758),
            new LMS(-0.84020, 6.06610, 0.21876),
            new LMS(-0.84620, 6.06690, 0.21993),
            new LMS(-0.85200, 6.06790, 0.22109),
            new LMS(-0.85780, 6.06900, 0.22223),
            new LMS(-0.86340, 6.07030, 0.22335),
            new LMS(-0.86900, 6.07170, 0.22447),
            new LMS(-0.87450, 6.07320, 0.22556),
            new LMS(-0.87990, 6.07480, 0.22664),
            new LMS(-0.88520, 6.07650, 0.22771),
            new LMS(-0.89040, 6.07840, 0.22876),
            new LMS(-0.89550, 6.08030, 0.22979),
            new LMS(-0.90060, 6.08230, 0.23081),
            new LMS(-0.90560, 6.08440, 0.23182),
            new LMS(-0.91050, 6.08650, 0.23280)
        };

        #endregion

        #region _wflBoys

        private static readonly LMS[] WflBoys = new[]
        {
            new LMS(-0.35210, 2.44100, 0.09182),
            new LMS(-0.35210, 2.52440, 0.09153),
            new LMS(-0.35210, 2.60770, 0.09124),
            new LMS(-0.35210, 2.69130, 0.09094),
            new LMS(-0.35210, 2.77550, 0.09065),
            new LMS(-0.35210, 2.86090, 0.09036),
            new LMS(-0.35210, 2.94800, 0.09007),
            new LMS(-0.35210, 3.03770, 0.08977),
            new LMS(-0.35210, 3.13080, 0.08948),
            new LMS(-0.35210, 3.22760, 0.08919),
            new LMS(-0.35210, 3.32780, 0.08890),
            new LMS(-0.35210, 3.43110, 0.08861),
            new LMS(-0.35210, 3.53760, 0.08831),
            new LMS(-0.35210, 3.64770, 0.08801),
            new LMS(-0.35210, 3.76200, 0.08771),
            new LMS(-0.35210, 3.88140, 0.08741),
            new LMS(-0.35210, 4.00600, 0.08711),
            new LMS(-0.35210, 4.13540, 0.08681),
            new LMS(-0.35210, 4.26930, 0.08651),
            new LMS(-0.35210, 4.40660, 0.08621),
            new LMS(-0.35210, 4.54670, 0.08592),
            new LMS(-0.35210, 4.68920, 0.08563),
            new LMS(-0.35210, 4.83380, 0.08535),
            new LMS(-0.35210, 4.97960, 0.08507),
            new LMS(-0.35210, 5.12590, 0.08481),
            new LMS(-0.35210, 5.27210, 0.08455),
            new LMS(-0.35210, 5.41800, 0.08430),
            new LMS(-0.35210, 5.56320, 0.08406),
            new LMS(-0.35210, 5.70740, 0.08383),
            new LMS(-0.35210, 5.85010, 0.08362),
            new LMS(-0.35210, 5.99070, 0.08342),
            new LMS(-0.35210, 6.12840, 0.08324),
            new LMS(-0.35210, 6.26320, 0.08308),
            new LMS(-0.35210, 6.39540, 0.08292),
            new LMS(-0.35210, 6.52510, 0.08279),
            new LMS(-0.35210, 6.65270, 0.08266),
            new LMS(-0.35210, 6.77860, 0.08255),
            new LMS(-0.35210, 6.90280, 0.08245),
            new LMS(-0.35210, 7.02550, 0.08236),
            new LMS(-0.35210, 7.14670, 0.08229),
            new LMS(-0.35210, 7.26660, 0.08223),
            new LMS(-0.35210, 7.38540, 0.08218),
            new LMS(-0.35210, 7.50340, 0.08215),
            new LMS(-0.35210, 7.62060, 0.08213),
            new LMS(-0.35210, 7.73700, 0.08212),
            new LMS(-0.35210, 7.85260, 0.08212),
            new LMS(-0.35210, 7.96740, 0.08214),
            new LMS(-0.35210, 8.08160, 0.08216),
            new LMS(-0.35210, 8.19550, 0.08219),
            new LMS(-0.35210, 8.30920, 0.08224),
            new LMS(-0.35210, 8.42270, 0.08229),
            new LMS(-0.35210, 8.53580, 0.08235),
            new LMS(-0.35210, 8.64800, 0.08241),
            new LMS(-0.35210, 8.75940, 0.08248),
            new LMS(-0.35210, 8.86970, 0.08254),
            new LMS(-0.35210, 8.97880, 0.08262),
            new LMS(-0.35210, 9.08650, 0.08269),
            new LMS(-0.35210, 9.19270, 0.08276),
            new LMS(-0.35210, 9.29740, 0.08283),
            new LMS(-0.35210, 9.40100, 0.08289),
            new LMS(-0.35210, 9.50320, 0.08295),
            new LMS(-0.35210, 9.60410, 0.08301),
            new LMS(-0.35210, 9.70330, 0.08307),
            new LMS(-0.35210, 9.80070, 0.08311),
            new LMS(-0.35210, 9.89630, 0.08314),
            new LMS(-0.35210, 9.99020, 0.08317),
            new LMS(-0.35210, 10.08270, 0.08318),
            new LMS(-0.35210, 10.17410, 0.08318),
            new LMS(-0.35210, 10.26490, 0.08316),
            new LMS(-0.35210, 10.35580, 0.08313),
            new LMS(-0.35210, 10.44750, 0.08308),
            new LMS(-0.35210, 10.54050, 0.08301),
            new LMS(-0.35210, 10.63520, 0.08293),
            new LMS(-0.35210, 10.73220, 0.08284),
            new LMS(-0.35210, 10.83210, 0.08273),
            new LMS(-0.35210, 10.93500, 0.08260),
            new LMS(-0.35210, 11.04150, 0.08246),
            new LMS(-0.35210, 11.15160, 0.08231),
            new LMS(-0.35210, 11.26510, 0.08215),
            new LMS(-0.35210, 11.38170, 0.08198),
            new LMS(-0.35210, 11.50070, 0.08181),
            new LMS(-0.35210, 11.62180, 0.08163),
            new LMS(-0.35210, 11.74440, 0.08145),
            new LMS(-0.35210, 11.86780, 0.08128),
            new LMS(-0.35210, 11.99160, 0.08111),
            new LMS(-0.35210, 12.11520, 0.08096),
            new LMS(-0.35210, 12.23820, 0.08082),
            new LMS(-0.35210, 12.36030, 0.08069),
            new LMS(-0.35210, 12.48150, 0.08058),
            new LMS(-0.35210, 12.60170, 0.08048),
            new LMS(-0.35210, 12.72090, 0.08041),
            new LMS(-0.35210, 12.83920, 0.08034),
            new LMS(-0.35210, 12.95690, 0.08030),
            new LMS(-0.35210, 13.07420, 0.08026),
            new LMS(-0.35210, 13.19100, 0.08025),
            new LMS(-0.35210, 13.30750, 0.08025),
            new LMS(-0.35210, 13.42390, 0.08026),
            new LMS(-0.35210, 13.54040, 0.08029),
            new LMS(-0.35210, 13.65720, 0.08034),
            new LMS(-0.35210, 13.77460, 0.08040),
            new LMS(-0.35210, 13.89280, 0.08047),
            new LMS(-0.35210, 14.01200, 0.08056),
            new LMS(-0.35210, 14.13250, 0.08067),
            new LMS(-0.35210, 14.25440, 0.08078),
            new LMS(-0.35210, 14.37820, 0.08092),
            new LMS(-0.35210, 14.50380, 0.08106),
            new LMS(-0.35210, 14.63160, 0.08122),
            new LMS(-0.35210, 14.76140, 0.08139),
            new LMS(-0.35210, 14.89340, 0.08157),
            new LMS(-0.35210, 15.02750, 0.08177),
            new LMS(-0.35210, 15.16370, 0.08198),
            new LMS(-0.35210, 15.30180, 0.08220),
            new LMS(-0.35210, 15.44190, 0.08243),
            new LMS(-0.35210, 15.58380, 0.08267),
            new LMS(-0.35210, 15.72760, 0.08292),
            new LMS(-0.35210, 15.87320, 0.08317),
            new LMS(-0.35210, 16.02060, 0.08343),
            new LMS(-0.35210, 16.16970, 0.08370),
            new LMS(-0.35210, 16.32040, 0.08397),
            new LMS(-0.35210, 16.47280, 0.08425),
            new LMS(-0.35210, 16.62680, 0.08453),
            new LMS(-0.35210, 16.78260, 0.08481),
            new LMS(-0.35210, 16.94010, 0.08510),
            new LMS(-0.35210, 17.09950, 0.08539),
            new LMS(-0.35210, 17.26070, 0.08568),
            new LMS(-0.35210, 17.42370, 0.08599),
            new LMS(-0.35210, 17.58850, 0.08629),
            new LMS(-0.35210, 17.75530, 0.08660),
            new LMS(-0.35210, 17.92420, 0.08691),
            new LMS(-0.35210, 18.09540, 0.08723),
            new LMS(-0.35210, 18.26890, 0.08755)
        };

        #endregion

        #region _wflGirls

        private static readonly LMS[] WflGirls = new[]
        {
            new LMS(-0.38330, 2.46070, 0.09029),
            new LMS(-0.38330, 2.54570, 0.09033),
            new LMS(-0.38330, 2.63060, 0.09037),
            new LMS(-0.38330, 2.71550, 0.09040),
            new LMS(-0.38330, 2.80070, 0.09044),
            new LMS(-0.38330, 2.88670, 0.09048),
            new LMS(-0.38330, 2.97410, 0.09052),
            new LMS(-0.38330, 3.06360, 0.09056),
            new LMS(-0.38330, 3.15600, 0.09060),
            new LMS(-0.38330, 3.25200, 0.09064),
            new LMS(-0.38330, 3.35180, 0.09068),
            new LMS(-0.38330, 3.45570, 0.09072),
            new LMS(-0.38330, 3.56360, 0.09076),
            new LMS(-0.38330, 3.67540, 0.09080),
            new LMS(-0.38330, 3.79110, 0.09085),
            new LMS(-0.38330, 3.91050, 0.09089),
            new LMS(-0.38330, 4.03320, 0.09093),
            new LMS(-0.38330, 4.15910, 0.09098),
            new LMS(-0.38330, 4.28750, 0.09102),
            new LMS(-0.38330, 4.41790, 0.09106),
            new LMS(-0.38330, 4.54980, 0.09110),
            new LMS(-0.38330, 4.68270, 0.09114),
            new LMS(-0.38330, 4.81620, 0.09118),
            new LMS(-0.38330, 4.95000, 0.09121),
            new LMS(-0.38330, 5.08370, 0.09125),
            new LMS(-0.38330, 5.21730, 0.09128),
            new LMS(-0.38330, 5.35070, 0.09130),
            new LMS(-0.38330, 5.48340, 0.09132),
            new LMS(-0.38330, 5.61510, 0.09134),
            new LMS(-0.38330, 5.74540, 0.09135),
            new LMS(-0.38330, 5.87420, 0.09136),
            new LMS(-0.38330, 6.00140, 0.09137),
            new LMS(-0.38330, 6.12700, 0.09137),
            new LMS(-0.38330, 6.25110, 0.09136),
            new LMS(-0.38330, 6.37380, 0.09135),
            new LMS(-0.38330, 6.49480, 0.09133),
            new LMS(-0.38330, 6.61440, 0.09131),
            new LMS(-0.38330, 6.73280, 0.09129),
            new LMS(-0.38330, 6.85010, 0.09126),
            new LMS(-0.38330, 6.96620, 0.09123),
            new LMS(-0.38330, 7.08120, 0.09119),
            new LMS(-0.38330, 7.19500, 0.09115),
            new LMS(-0.38330, 7.30760, 0.09110),
            new LMS(-0.38330, 7.41890, 0.09106),
            new LMS(-0.38330, 7.52880, 0.09101),
            new LMS(-0.38330, 7.63750, 0.09096),
            new LMS(-0.38330, 7.74480, 0.09090),
            new LMS(-0.38330, 7.85090, 0.09085),
            new LMS(-0.38330, 7.95590, 0.09079),
            new LMS(-0.38330, 8.05990, 0.09074),
            new LMS(-0.38330, 8.16300, 0.09068),
            new LMS(-0.38330, 8.26510, 0.09062),
            new LMS(-0.38330, 8.36660, 0.09056),
            new LMS(-0.38330, 8.46760, 0.09050),
            new LMS(-0.38330, 8.56790, 0.09043),
            new LMS(-0.38330, 8.66740, 0.09037),
            new LMS(-0.38330, 8.76610, 0.09031),
            new LMS(-0.38330, 8.86380, 0.09025),
            new LMS(-0.38330, 8.96010, 0.09018),
            new LMS(-0.38330, 9.05520, 0.09012),
            new LMS(-0.38330, 9.14900, 0.09005),
            new LMS(-0.38330, 9.24180, 0.08999),
            new LMS(-0.38330, 9.33370, 0.08992),
            new LMS(-0.38330, 9.42520, 0.08985),
            new LMS(-0.38330, 9.51660, 0.08979),
            new LMS(-0.38330, 9.60860, 0.08972),
            new LMS(-0.38330, 9.70150, 0.08965),
            new LMS(-0.38330, 9.79570, 0.08959),
            new LMS(-0.38330, 9.89150, 0.08952),
            new LMS(-0.38330, 9.98920, 0.08946),
            new LMS(-0.38330, 10.08910, 0.08940),
            new LMS(-0.38330, 10.19160, 0.08934),
            new LMS(-0.38330, 10.29650, 0.08928),
            new LMS(-0.38330, 10.40410, 0.08923),
            new LMS(-0.38330, 10.51400, 0.08918),
            new LMS(-0.38330, 10.62630, 0.08914),
            new LMS(-0.38330, 10.74100, 0.08910),
            new LMS(-0.38330, 10.85780, 0.08906),
            new LMS(-0.38330, 10.97670, 0.08903),
            new LMS(-0.38330, 11.09740, 0.08900),
            new LMS(-0.38330, 11.21980, 0.08898),
            new LMS(-0.38330, 11.34350, 0.08897),
            new LMS(-0.38330, 11.46840, 0.08895),
            new LMS(-0.38330, 11.59400, 0.08895),
            new LMS(-0.38330, 11.72010, 0.08895),
            new LMS(-0.38330, 11.84610, 0.08895),
            new LMS(-0.38330, 11.97200, 0.08896),
            new LMS(-0.38330, 12.09760, 0.08898),
            new LMS(-0.38330, 12.22290, 0.08900),
            new LMS(-0.38330, 12.34770, 0.08903),
            new LMS(-0.38330, 12.47230, 0.08906),
            new LMS(-0.38330, 12.59650, 0.08909),
            new LMS(-0.38330, 12.72050, 0.08913),
            new LMS(-0.38330, 12.84430, 0.08918),
            new LMS(-0.38330, 12.96810, 0.08923),
            new LMS(-0.38330, 13.09200, 0.08928),
            new LMS(-0.38330, 13.21580, 0.08934),
            new LMS(-0.38330, 13.33990, 0.08941),
            new LMS(-0.38330, 13.46430, 0.08948),
            new LMS(-0.38330, 13.58920, 0.08955),
            new LMS(-0.38330, 13.71460, 0.08963),
            new LMS(-0.38330, 13.84080, 0.08972),
            new LMS(-0.38330, 13.96760, 0.08981),
            new LMS(-0.38330, 14.09530, 0.08990),
            new LMS(-0.38330, 14.22390, 0.09000),
            new LMS(-0.38330, 14.35370, 0.09010),
            new LMS(-0.38330, 14.48480, 0.09021),
            new LMS(-0.38330, 14.61740, 0.09033),
            new LMS(-0.38330, 14.75190, 0.09044),
            new LMS(-0.38330, 14.88820, 0.09057),
            new LMS(-0.38330, 15.02670, 0.09069),
            new LMS(-0.38330, 15.16760, 0.09083),
            new LMS(-0.38330, 15.31080, 0.09096),
            new LMS(-0.38330, 15.45640, 0.09110),
            new LMS(-0.38330, 15.60460, 0.09125),
            new LMS(-0.38330, 15.75530, 0.09139),
            new LMS(-0.38330, 15.90870, 0.09155),
            new LMS(-0.38330, 16.06450, 0.09170),
            new LMS(-0.38330, 16.22290, 0.09186),
            new LMS(-0.38330, 16.38370, 0.09203),
            new LMS(-0.38330, 16.54700, 0.09219),
            new LMS(-0.38330, 16.71290, 0.09236),
            new LMS(-0.38330, 16.88140, 0.09254),
            new LMS(-0.38330, 17.05270, 0.09271),
            new LMS(-0.38330, 17.22690, 0.09289),
            new LMS(-0.38330, 17.40390, 0.09307),
            new LMS(-0.38330, 17.58390, 0.09326),
            new LMS(-0.38330, 17.76680, 0.09344),
            new LMS(-0.38330, 17.95260, 0.09363),
            new LMS(-0.38330, 18.14120, 0.09382),
            new LMS(-0.38330, 18.33240, 0.09401)
        };

        #endregion

        #region _wfhBoys

        private static readonly LMS[] WfhBoys = new[]
        {
            new LMS(-0.35210, 7.43270, 0.08217),
            new LMS(-0.35210, 7.55040, 0.08214),
            new LMS(-0.35210, 7.66730, 0.08212),
            new LMS(-0.35210, 7.78340, 0.08212),
            new LMS(-0.35210, 7.89860, 0.08213),
            new LMS(-0.35210, 8.01320, 0.08214),
            new LMS(-0.35210, 8.12720, 0.08217),
            new LMS(-0.35210, 8.24100, 0.08221),
            new LMS(-0.35210, 8.35470, 0.08226),
            new LMS(-0.35210, 8.46800, 0.08231),
            new LMS(-0.35210, 8.58080, 0.08237),
            new LMS(-0.35210, 8.69270, 0.08243),
            new LMS(-0.35210, 8.80360, 0.08250),
            new LMS(-0.35210, 8.91350, 0.08257),
            new LMS(-0.35210, 9.02210, 0.08264),
            new LMS(-0.35210, 9.12920, 0.08272),
            new LMS(-0.35210, 9.23470, 0.08278),
            new LMS(-0.35210, 9.33900, 0.08285),
            new LMS(-0.35210, 9.44200, 0.08292),
            new LMS(-0.35210, 9.54380, 0.08298),
            new LMS(-0.35210, 9.64400, 0.08303),
            new LMS(-0.35210, 9.74250, 0.08308),
            new LMS(-0.35210, 9.83920, 0.08312),
            new LMS(-0.35210, 9.93410, 0.08315),
            new LMS(-0.35210, 10.02740, 0.08317),
            new LMS(-0.35210, 10.11940, 0.08318),
            new LMS(-0.35210, 10.21050, 0.08317),
            new LMS(-0.35210, 10.30120, 0.08315),
            new LMS(-0.35210, 10.39230, 0.08311),
            new LMS(-0.35210, 10.48450, 0.08305),
            new LMS(-0.35210, 10.57810, 0.08298),
            new LMS(-0.35210, 10.67370, 0.08290),
            new LMS(-0.35210, 10.77180, 0.08279),
            new LMS(-0.35210, 10.87280, 0.08268),
            new LMS(-0.35210, 10.97720, 0.08255),
            new LMS(-0.35210, 11.08510, 0.08241),
            new LMS(-0.35210, 11.19660, 0.08225),
            new LMS(-0.35210, 11.31140, 0.08209),
            new LMS(-0.35210, 11.42900, 0.08191),
            new LMS(-0.35210, 11.54900, 0.08174),
            new LMS(-0.35210, 11.67070, 0.08156),
            new LMS(-0.35210, 11.79370, 0.08138),
            new LMS(-0.35210, 11.91730, 0.08121),
            new LMS(-0.35210, 12.04110, 0.08105),
            new LMS(-0.35210, 12.16450, 0.08090),
            new LMS(-0.35210, 12.28710, 0.08076),
            new LMS(-0.35210, 12.40890, 0.08064),
            new LMS(-0.35210, 12.52980, 0.08054),
            new LMS(-0.35210, 12.64950, 0.08045),
            new LMS(-0.35210, 12.76830, 0.08038),
            new LMS(-0.35210, 12.88640, 0.08032),
            new LMS(-0.35210, 13.00380, 0.08028),
            new LMS(-0.35210, 13.12090, 0.08025),
            new LMS(-0.35210, 13.23760, 0.08024),
            new LMS(-0.35210, 13.35410, 0.08025),
            new LMS(-0.35210, 13.47050, 0.08027),
            new LMS(-0.35210, 13.58700, 0.08031),
            new LMS(-0.35210, 13.70410, 0.08036),
            new LMS(-0.35210, 13.82170, 0.08043),
            new LMS(-0.35210, 13.94030, 0.08051),
            new LMS(-0.35210, 14.06000, 0.08060),
            new LMS(-0.35210, 14.18110, 0.08071),
            new LMS(-0.35210, 14.30370, 0.08083),
            new LMS(-0.35210, 14.42820, 0.08097),
            new LMS(-0.35210, 14.55470, 0.08112),
            new LMS(-0.35210, 14.68320, 0.08129),
            new LMS(-0.35210, 14.81400, 0.08146),
            new LMS(-0.35210, 14.94680, 0.08165),
            new LMS(-0.35210, 15.08180, 0.08185),
            new LMS(-0.35210, 15.21870, 0.08206),
            new LMS(-0.35210, 15.35760, 0.08229),
            new LMS(-0.35210, 15.49850, 0.08252),
            new LMS(-0.35210, 15.64120, 0.08277),
            new LMS(-0.35210, 15.78570, 0.08302),
            new LMS(-0.35210, 15.93200, 0.08328),
            new LMS(-0.35210, 16.08010, 0.08354),
            new LMS(-0.35210, 16.22980, 0.08381),
            new LMS(-0.35210, 16.38120, 0.08408),
            new LMS(-0.35210, 16.53420, 0.08436),
            new LMS(-0.35210, 16.68890, 0.08464),
            new LMS(-0.35210, 16.84540, 0.08493),
            new LMS(-0.35210, 17.00360, 0.08521),
            new LMS(-0.35210, 17.16370, 0.08551),
            new LMS(-0.35210, 17.32560, 0.08580),
            new LMS(-0.35210, 17.48940, 0.08611),
            new LMS(-0.35210, 17.65500, 0.08641),
            new LMS(-0.35210, 17.82260, 0.08673),
            new LMS(-0.35210, 17.99240, 0.08704),
            new LMS(-0.35210, 18.16450, 0.08736),
            new LMS(-0.35210, 18.33900, 0.08768),
            new LMS(-0.35210, 18.51580, 0.08800),
            new LMS(-0.35210, 18.69480, 0.08832),
            new LMS(-0.35210, 18.87590, 0.08864),
            new LMS(-0.35210, 19.05900, 0.08896),
            new LMS(-0.35210, 19.24390, 0.08928),
            new LMS(-0.35210, 19.43040, 0.08960),
            new LMS(-0.35210, 19.61850, 0.08991),
            new LMS(-0.35210, 19.80810, 0.09022),
            new LMS(-0.35210, 19.99900, 0.09054),
            new LMS(-0.35210, 20.19120, 0.09085),
            new LMS(-0.35210, 20.38460, 0.09116),
            new LMS(-0.35210, 20.57890, 0.09147),
            new LMS(-0.35210, 20.77410, 0.09177),
            new LMS(-0.35210, 20.97000, 0.09208),
            new LMS(-0.35210, 21.16660, 0.09239),
            new LMS(-0.35210, 21.36360, 0.09270),
            new LMS(-0.35210, 21.56110, 0.09300),
            new LMS(-0.35210, 21.75880, 0.09331),
            new LMS(-0.35210, 21.95680, 0.09362),
            new LMS(-0.35210, 22.15490, 0.09393),
            new LMS(-0.35210, 22.35300, 0.09424)
        };

        #endregion

        #region _wfhGirls

        private static readonly LMS[] WfhGirls = new[]
        {
            new LMS(-0.38330, 7.24020, 0.09113),
            new LMS(-0.38330, 7.35230, 0.09109),
            new LMS(-0.38330, 7.46300, 0.09104),
            new LMS(-0.38330, 7.57240, 0.09099),
            new LMS(-0.38330, 7.68060, 0.09094),
            new LMS(-0.38330, 7.78740, 0.09088),
            new LMS(-0.38330, 7.89300, 0.09083),
            new LMS(-0.38330, 7.99760, 0.09077),
            new LMS(-0.38330, 8.10120, 0.09071),
            new LMS(-0.38330, 8.20390, 0.09065),
            new LMS(-0.38330, 8.30580, 0.09059),
            new LMS(-0.38330, 8.40710, 0.09053),
            new LMS(-0.38330, 8.50780, 0.09047),
            new LMS(-0.38330, 8.60780, 0.09041),
            new LMS(-0.38330, 8.70700, 0.09035),
            new LMS(-0.38330, 8.80530, 0.09028),
            new LMS(-0.38330, 8.90250, 0.09022),
            new LMS(-0.38330, 8.99830, 0.09016),
            new LMS(-0.38330, 9.09280, 0.09009),
            new LMS(-0.38330, 9.18620, 0.09003),
            new LMS(-0.38330, 9.27860, 0.08996),
            new LMS(-0.38330, 9.37030, 0.08989),
            new LMS(-0.38330, 9.46170, 0.08983),
            new LMS(-0.38330, 9.55330, 0.08976),
            new LMS(-0.38330, 9.64560, 0.08969),
            new LMS(-0.38330, 9.73900, 0.08963),
            new LMS(-0.38330, 9.83380, 0.08956),
            new LMS(-0.38330, 9.93030, 0.08950),
            new LMS(-0.38330, 10.02890, 0.08943),
            new LMS(-0.38330, 10.12980, 0.08937),
            new LMS(-0.38330, 10.23320, 0.08932),
            new LMS(-0.38330, 10.33930, 0.08926),
            new LMS(-0.38330, 10.44770, 0.08921),
            new LMS(-0.38330, 10.55860, 0.08916),
            new LMS(-0.38330, 10.67190, 0.08912),
            new LMS(-0.38330, 10.78740, 0.08908),
            new LMS(-0.38330, 10.90510, 0.08905),
            new LMS(-0.38330, 11.02480, 0.08902),
            new LMS(-0.38330, 11.14620, 0.08899),
            new LMS(-0.38330, 11.26910, 0.08897),
            new LMS(-0.38330, 11.39340, 0.08896),
            new LMS(-0.38330, 11.51860, 0.08895),
            new LMS(-0.38330, 11.64440, 0.08895),
            new LMS(-0.38330, 11.77050, 0.08895),
            new LMS(-0.38330, 11.89650, 0.08896),
            new LMS(-0.38330, 12.02230, 0.08897),
            new LMS(-0.38330, 12.14780, 0.08899),
            new LMS(-0.38330, 12.27290, 0.08901),
            new LMS(-0.38330, 12.39760, 0.08904),
            new LMS(-0.38330, 12.52200, 0.08907),
            new LMS(-0.38330, 12.64610, 0.08911),
            new LMS(-0.38330, 12.77000, 0.08915),
            new LMS(-0.38330, 12.89390, 0.08920),
            new LMS(-0.38330, 13.01770, 0.08925),
            new LMS(-0.38330, 13.14150, 0.08931),
            new LMS(-0.38330, 13.26540, 0.08937),
            new LMS(-0.38330, 13.38960, 0.08944),
            new LMS(-0.38330, 13.51420, 0.08951),
            new LMS(-0.38330, 13.63930, 0.08959),
            new LMS(-0.38330, 13.76500, 0.08967),
            new LMS(-0.38330, 13.89140, 0.08975),
            new LMS(-0.38330, 14.01860, 0.08984),
            new LMS(-0.38330, 14.14660, 0.08994),
            new LMS(-0.38330, 14.27570, 0.09004),
            new LMS(-0.38330, 14.40590, 0.09015),
            new LMS(-0.38330, 14.53760, 0.09026),
            new LMS(-0.38330, 14.67100, 0.09037),
            new LMS(-0.38330, 14.80620, 0.09049),
            new LMS(-0.38330, 14.94340, 0.09062),
            new LMS(-0.38330, 15.08280, 0.09075),
            new LMS(-0.38330, 15.22460, 0.09088),
            new LMS(-0.38330, 15.36870, 0.09102),
            new LMS(-0.38330, 15.51540, 0.09116),
            new LMS(-0.38330, 15.66460, 0.09131),
            new LMS(-0.38330, 15.81640, 0.09146),
            new LMS(-0.38330, 15.97070, 0.09161),
            new LMS(-0.38330, 16.12760, 0.09177),
            new LMS(-0.38330, 16.28700, 0.09193),
            new LMS(-0.38330, 16.44880, 0.09209),
            new LMS(-0.38330, 16.61310, 0.09226),
            new LMS(-0.38330, 16.78000, 0.09243),
            new LMS(-0.38330, 16.94960, 0.09261),
            new LMS(-0.38330, 17.12200, 0.09278),
            new LMS(-0.38330, 17.29730, 0.09296),
            new LMS(-0.38330, 17.47550, 0.09315),
            new LMS(-0.38330, 17.65670, 0.09333),
            new LMS(-0.38330, 17.84070, 0.09352),
            new LMS(-0.38330, 18.02770, 0.09371),
            new LMS(-0.38330, 18.21740, 0.09390),
            new LMS(-0.38330, 18.40960, 0.09409),
            new LMS(-0.38330, 18.60430, 0.09428),
            new LMS(-0.38330, 18.80150, 0.09448),
            new LMS(-0.38330, 19.00090, 0.09467),
            new LMS(-0.38330, 19.20240, 0.09487),
            new LMS(-0.38330, 19.40600, 0.09507),
            new LMS(-0.38330, 19.61160, 0.09527),
            new LMS(-0.38330, 19.81900, 0.09546),
            new LMS(-0.38330, 20.02800, 0.09566),
            new LMS(-0.38330, 20.23850, 0.09586),
            new LMS(-0.38330, 20.45020, 0.09606),
            new LMS(-0.38330, 20.66290, 0.09626),
            new LMS(-0.38330, 20.87660, 0.09646),
            new LMS(-0.38330, 21.09090, 0.09666),
            new LMS(-0.38330, 21.30590, 0.09686),
            new LMS(-0.38330, 21.52130, 0.09707),
            new LMS(-0.38330, 21.73700, 0.09727),
            new LMS(-0.38330, 21.95290, 0.09747),
            new LMS(-0.38330, 22.16900, 0.09767),
            new LMS(-0.38330, 22.38510, 0.09788),
            new LMS(-0.38330, 22.60120, 0.09808),
            new LMS(-0.38330, 22.81730, 0.09828)
        };

        #endregion
    }
}
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MaterialSkin
{
    /// <summary>
    /// Defines the <see cref="Extensions" />
    /// These add functions on default C# types and classes
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// The HasProperty
        /// </summary>
        /// <param name="objectToCheck">The objectToCheck<see cref="object"/></param>
        /// <param name="propertyName">The propertyName<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool HasProperty(this object objectToCheck, string propertyName)
        {
            try
            {
                var type = objectToCheck.GetType();

                return type.GetProperty(propertyName) != null;
            }
            catch (AmbiguousMatchException)
            {
                // ambiguous means there is more than one result,
                // which means: a method with that name does exist
                return true;
            }
        }

        /// <summary>
        /// The IsMaterialControl
        /// </summary>
        /// <param name="obj">The obj<see cref="Object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsMaterialControl(this Object obj)
        {
            if (obj is IMaterialControl)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Transforms every char on a string to this dot: ●
        /// </summary>
        /// <param name="plainString"></param>
        /// <returns></returns>
        public static string ToSecureString(this string plainString)
        {
            if (plainString == null)
                return null;

            string secureString = "";
            for (uint i = 0; i < plainString.Length; i++)
            {
                secureString += '\u25CF';
            }
            return secureString;
        }

        // Color extensions
        public static Color ToColor(this int argb)
        {
            return Color.FromArgb(
                (argb & 0xFF0000) >> 16,
                (argb & 0x00FF00) >> 8,
                 argb & 0x0000FF);
        }

        public static Color ToColor(this string hexColor)
        {
            return ColorTranslator.FromHtml(hexColor);
        }

        public static Color ToColor(this Enum enumColor)
        {
            var argb = Convert.ToInt32(enumColor);
            return Color.FromArgb(argb);
        }

        public static Color RemoveAlpha(this Color color)
        {
            return Color.FromArgb(color.R, color.G, color.B);
        }

        public static int PercentageToColorComponent(this int percentage)
        {
            return (int)((percentage / 100d) * 255d);
        }

        public static void InvokeIfRequired(this Control control, System.Windows.Forms.MethodInvoker action)
        {
            // See Update 2 for edits Mike de Klerk suggests to insert here.

            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        // Simulate Clamp function from .NET Core 2.0, Core 2.1, Core 2.2, Core 3.0, Core 3.1, 5, 6, 7, 8
		// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Math.cs,538
		internal static int Clamp(this int value, int min, int max)
        {
			if (min > max)
			{
				throw new ArgumentException(string.Format("'{0}' cannot be greater than {1}.", min, max));
			}

			if (value < min)
			{
				return min;
			}
			else if (value > max)
			{
				return max;
			}

			return value;
		}
    }
}
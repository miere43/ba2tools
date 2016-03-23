using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Utility
{
    /// <summary>
    /// Static class containing various formatters for Version class
    /// </summary>
    /// <seealso cref="Version"/>
    public static class VersionFormatter
    {
        /// <summary>
        /// Formats specified version, getting rid of minor and minor revision
        /// values if they are zero.
        /// </summary>
        /// <param name="version">Version to format.</param>
        /// <returns>Formatted string representing version.</returns>
        public static string Format(Version version)
        {
            StringBuilder builder = new StringBuilder(16);
            builder.Append(version.Major);
            builder.Append('.');
            builder.Append(version.MinorRevision);
            if (version.Minor != 0 || version.MinorRevision != 0)
            {
                builder.Append('.');
                builder.Append(version.Minor);
            }
            if (version.MinorRevision != 0)
            {
                builder.Append('.');
                builder.Append(version.MinorRevision);
            }

            return builder.ToString();
        }
    }
}

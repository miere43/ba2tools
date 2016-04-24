using System;

namespace Ba2Tools
{
    /// <summary>
    /// Defines behaviour flags for BA2Loader methods.
    /// <see cref="BA2Loader" />
    /// </summary>
    [Flags]
    public enum BA2LoaderFlags
    {
        None = 0,
        /// <summary>
        /// Ignore archive version 
        /// </summary>
        IgnoreVersion = 1,
        /// <summary>
        /// Load unknown archive types. BA2Archive instance will be returned 
        /// instead of throwing exception.
        /// </summary>
        IgnoreArchiveType = 2,
        /// <summary>
        /// Use multithreaded extraction for ExtractAll and ExtractFiles methods.
        /// </summary>
        Multithreaded = 4
    }
}

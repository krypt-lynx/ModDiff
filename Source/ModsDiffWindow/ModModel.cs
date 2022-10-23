using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDiff
{
    public class ModModel
    {
        /// <summary>
        /// mod's package id as it was passed by game
        /// </summary>
        public string PackageId;

        /// <summary>
        /// mod's package id cleared from any junk
        /// </summary>
        public string NormalizedId;

        /// <summary>
        /// mod info stored if the save
        /// </summary>
        public ModInfo Left = null;
        /// <summary>
        /// mod info loaded by game
        /// </summary>
        public ModInfo Right = null;

        /// <summary>
        /// index of left entry in merged list
        /// </summary>
        public int LeftIndex = -1;
        /// <summary>
        /// index of right entry in merged list
        /// </summary>
        public int RightIndex = -1;

        /// <summary>
        /// mod was moved
        /// </summary>
        public bool IsMoved = false;
        /// <summary>
        /// mod is no available
        /// </summary>
        public bool IsMissing = false;

        public bool IsRequired = false;

        public ModInfo Any => Right ?? Left;

        /// <summary>
        /// Name for item in merged list
        /// </summary>
        public string Name => Any?.Name;

        public string KeyForCompare => ModDiff.Settings.steamSameAsLocal ? NormalizedId : PackageId;

        public override int GetHashCode()
        {
            return KeyForCompare.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ModModel other)
            {
                return KeyForCompare.Equals(other.KeyForCompare);
            }
            else
            {
                return false;
            }
        }
    }

}

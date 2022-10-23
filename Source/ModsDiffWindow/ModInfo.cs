using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff
{
    public class ModInfo
    {
        public string PackageId { get; private set; }
        public string Name { get; private set; }
        public string NormalizedId { get; private set; }
        public ContentSource Source { get; private set; }
        public bool Compatible { get; private set; }

        public ModInfo(string name, string packageId)
        {
            Name = name;
            PackageId = packageId;
            NormalizedId = packageId.Split('_').FirstOrFallback("");
            var meta = ModLister.GetModWithIdentifier(packageId);
            Source = meta?.Source ?? ContentSource.Undefined;
            Compatible = meta?.VersionCompatible ?? true;
        }

        public string KeyForCompare => ModDiff.Settings.steamSameAsLocal ? NormalizedId : PackageId;

        public override bool Equals(object obj)
        {
            if (!(obj is ModInfo))
            {
                return false;
            }
            var other = (ModInfo)obj;

            return KeyForCompare == other.KeyForCompare;
        }

        public override int GetHashCode()
        {
            return KeyForCompare.GetHashCode();
        }

        public override string ToString()
        {
            return Name ?? "<none>";
        }
    }

}

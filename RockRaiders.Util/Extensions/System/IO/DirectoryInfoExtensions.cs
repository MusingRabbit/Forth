using System;
using System.IO;

namespace RockRaiders.Util.Extensions.System.IO
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo TraverseUp(this DirectoryInfo dir, Func<DirectoryInfo, bool> expr)
        {
            if (expr.Invoke(dir))
            {
                return dir;
            }

            if (dir.Parent == null)
            {
                return null;
            }

            return TraverseUp(dir.Parent, expr);
        }
    }
}

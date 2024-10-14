using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class PhysicsExtensions
    {
        public class ArcCastArgs
        {
            public Vector3 Centre{ get; set; }
            public Quaternion Rotation { get; set; }
            public float Angle{ get; set; }
            public float Radius{ get; set; }
            public int Resolution{ get; set; }
            public int Layer{ get; set; }
            public bool DrawDebug{ get; set; }
            public Color DebugColour{ get; set; }

            public ArcCastArgs()
            {
                this.DrawDebug = true;
                this.DebugColour = Color.blue;
                this.Angle = 270;
                this.Radius = 1.0f;
                this.Resolution = 7;
            }
        }

        public static bool ArcCast(ArcCastArgs args, out RaycastHit hitInfo)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var rotation = args.Rotation * Quaternion.Euler(-args.Angle / 2, 0, 0);

            for (int i = 0; i < args.Resolution; i++)
            {
                Vector3 a = args.Centre + rotation * Vector3.forward * args.Radius;
                rotation *= Quaternion.Euler(args.Angle / args.Resolution, 0, 0);
                Vector3 b = args.Centre + rotation * Vector3.forward * args.Radius;
                Vector3 ab = b - a;

                if (args.DrawDebug)
                {
                    Debug.DrawRay(a, ab, Color.blue);
                }

                if (Physics.Raycast(a, ab, out hitInfo, ab.magnitude * 1.001f, args.Layer))
                {
                    return true;
                }
            }

            hitInfo = new RaycastHit();
            return false;
        }
    }
}

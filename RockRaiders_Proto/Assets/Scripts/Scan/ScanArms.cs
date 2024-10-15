using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;
using static Assets.Scripts.Util.PhysicsExtensions;

namespace Assets.Scripts.Scan
{
    internal class ScanArms : Scan
    {
        [SerializeField, Range(0, 50)]
        private int m_armCount;

        [SerializeField]
        private float m_armLength;

        [SerializeField]
        private int m_armPoints;

        [SerializeField]
        private bool m_weightByDist;

        [SerializeField, Range(0,360)]
        private float m_arcAngle;

        [SerializeField]
        private int m_arcResolution;

        [SerializeField]
        private LayerMask m_arcLayer;

        [SerializeField]
        private bool m_debugDraw;

        public ScanArms()
        {
            m_armCount = 11;
            m_armLength = 2.0f;
            m_armPoints = 4;
            m_weightByDist = false;
            m_arcAngle = 270;
            m_arcResolution = 4;
        }

        public override List<PointData> GetPoints()
        {
            return Scan(m_debugDraw);
        }

        private List<PointData> Scan(bool drawDebug = false)
        {
            var result = new List<PointData>();

            var arcRadius = m_armLength / m_armPoints;

            for (int i = 0; i < m_armCount; i++)
            {
                var angle = 360 * i / m_armCount;
                var pos = this.transform.position;
                var rot = this.transform.rotation * Quaternion.Euler(0, angle, 0);

                var args = new ArcCastArgs
                {
                    Centre = pos,
                    Rotation = rot,
                    Angle = angle,
                    Radius = arcRadius,
                    Resolution = m_arcResolution,
                    Layer = m_arcLayer,
                    DrawDebug = drawDebug
                };

                for (int j = 0; j < m_armPoints && PhysicsExtensions.ArcCast(args, out var hitInfo); j++)
                {
                    float weight = m_weightByDist ? 1 - (float)j / m_armPoints : 1;
                    
                    if (drawDebug)
                    {
                        //Gizmos.DrawLine(pos, hitInfo.point);
                    }

                    pos = hitInfo.point;
                    rot.MatchUp(hitInfo.normal);
                    result.Add(new PointData { Position = pos, Rotation = rot * Quaternion.Euler(0, -angle, 0), Weight = weight, Normal = hitInfo.normal });

                    if (drawDebug)
                    {
                        //Gizmos.DrawSphere(pos, 0.1f);
                    }
                }
            }

            return result;
        }
    }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace delaunayTriangulation
{
    public class Tetra
    {
        public Vertex P1;
        public Vertex P2;
        public Vertex P3;
        public Vertex P4;

        public HashSet<TriangleFace> Faces = new HashSet<TriangleFace>();

        private float _a;
        private Vector3 _d;
        private float _c;

        private bool IsTetraDirect
        {
            get
            {
                float signedDoubleVolumeOfTetra = Vector3.Dot(Vector3.Cross(P2.Pos - P1.Pos, P3.Pos - P1.Pos), P4.Pos - P1.Pos);
                if (signedDoubleVolumeOfTetra == 0f)
                {
                    Debug.LogWarning("A tetrahedron is flat");
                }
                return signedDoubleVolumeOfTetra < 0;
            }
        }

        /// <summary>
        /// Constructor to intantiate a Tetra without neighbors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public Tetra(Vertex p1, Vertex p2, Vertex p3, Vertex p4)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
            if (!IsTetraDirect)
            {
                MakeTetraDirect();
            }
            ComputeCircumcircleInfos();
            Faces.Add(new TriangleFace(P1, P2, P3, this));
            Faces.Add(new TriangleFace(P1, P2, P4, this));
            Faces.Add(new TriangleFace(P1, P3, P4, this));
            Faces.Add(new TriangleFace(P2, P3, P4, this));
        }

        public Tetra(TriangleFace face1, TriangleFace face2, TriangleFace face3, TriangleFace face4)
        {
            P1 = face1.P1;
            P2 = face1.P2;
            P3 = face1.P3;
            FindLastVertexOfTetra(face2);
            if (!IsTetraDirect)
            {
                MakeTetraDirect();
            }
            ComputeCircumcircleInfos();
            Faces.Add(face1);
            Faces.Add(face2);
            Faces.Add(face3);
            Faces.Add(face4);
        }

        public Tetra(TriangleFace baseFace, Vertex vertex)
        {
            P1 = baseFace.P1;
            P2 = baseFace.P2;
            P3 = baseFace.P3;
            P4 = vertex;
            if (!IsTetraDirect)
            {
                MakeTetraDirect();
            }
            ComputeCircumcircleInfos();
            if (!baseFace.HasOneNeighborTetraNull())
            {
                throw new Exception("Trying to construct a tetrahedron with a base that already has 2 neighboring tetrahedrons registered.");
            }
            else
            {
                baseFace.RegisterNeighbor(this);
            }
            Faces.Add(baseFace);
        }

        public void AddNeighbor(TriangleFace face)
        {
            if (Faces.Count < 4)
            {
                Faces.Add(face);
            }
        }

        public bool IsNeighborOf(Tetra otherTetra)
        {
            HashSet<Vertex> tetraVertices = new HashSet<Vertex>() { P1, P2, P3, P4 };
            HashSet<Vertex> otherTetraVertices = new HashSet<Vertex>() { otherTetra.P1, otherTetra.P2, otherTetra.P3, otherTetra.P4 };
            tetraVertices.IntersectWith(otherTetraVertices);
            // Two tetrahedrons are neighbors if and only if they share 3 vertices;
            return tetraVertices.Count == 3;
        }

        public bool HasACommonVertexWith(Tetra otherTetra)
        {
            HashSet<Vertex> tetraVertices = new HashSet<Vertex>() { P1, P2, P3, P4 };
            HashSet<Vertex> otherTetraVertices = new HashSet<Vertex>() { otherTetra.P1, otherTetra.P2, otherTetra.P3, otherTetra.P4 };
            tetraVertices.IntersectWith(otherTetraVertices);
            return tetraVertices.Count >= 1;
        }

        public void GetCommonVerticesWith(Tetra otherTetra, out Vertex p1, out Vertex p2, out Vertex p3)
        {
            if (IsNeighborOf(otherTetra))
            {
                HashSet<Vertex> tetraVertices = new HashSet<Vertex>() { P1, P2, P3, P4 };
                HashSet<Vertex> otherTetraVertices = new HashSet<Vertex>() { otherTetra.P1, otherTetra.P2, otherTetra.P3, otherTetra.P4 };
                tetraVertices.IntersectWith(otherTetraVertices);
                List<Vertex> commonVertices = new List<Vertex>(tetraVertices);
                p1 = commonVertices[0];
                p2 = commonVertices[1];
                p3 = commonVertices[2];
            }
            else
            {
                throw new Exception("The two tetrahedrons are not neighbors");
            }
        }

        private void MakeTetraDirect()
        {
            Vertex p0 = P1;
            P1 = P2;
            P2 = p0;
        }


        private void FindLastVertexOfTetra(TriangleFace face2)
        {
            Vertex[] vertices = new Vertex[3] { P1, P2, P3 };
            if (!vertices.Contains(face2.P1))
            {
                P4 = face2.P1;
            }
            if (!vertices.Contains(face2.P2))
            {
                P4 = face2.P2;
            }
            if (!vertices.Contains(face2.P3))
            {
                P4 = face2.P3;
            }
        }

        public static Tetra ComputeCircumTetraOfSphere(Vector3 center, float radius)
        {
            Vertex p1 = new Vertex(VertexType.Undefined, center + 3 * radius * Vector3.forward);
            Vertex p2 = new Vertex(VertexType.Undefined, center + radius * new Vector3(0f, 2 * Mathf.Sqrt(2), -1));
            Vertex p3 = new Vertex(VertexType.Undefined, center + radius * new Vector3(-Mathf.Sqrt(6), -Mathf.Sqrt(2), -1));
            Vertex p4 = new Vertex(VertexType.Undefined, center + radius * new Vector3(Mathf.Sqrt(6), -Mathf.Sqrt(2), -1));
            return new Tetra(p1, p2, p3, p4);
        }

        public void Show(Color color, float duration)
        {
            Debug.DrawLine(P1.Pos, P2.Pos, color, duration);
            Debug.DrawLine(P1.Pos, P3.Pos, color, duration);
            Debug.DrawLine(P1.Pos, P4.Pos, color, duration);
            Debug.DrawLine(P2.Pos, P3.Pos, color, duration);
            Debug.DrawLine(P2.Pos, P4.Pos, color, duration);
            Debug.DrawLine(P3.Pos, P4.Pos, color, duration);
        }

        public static void Show(HashSet<Tetra> tetras, Color color, float duration)
        {
            foreach (Tetra tetra in tetras)
            {
                tetra.Show(color, duration);
            }
        }

        public void ShowCircumsphere(Color color, float duration)
        {
            Vector3 circumCenter = new Vector3(_d.x / (2 * _a), _d.y / (2 * _a), _d.z / (2 * _a));
            float circumRadius = Mathf.Sqrt(_d.sqrMagnitude - 4 * _a * _c) / (2 * Mathf.Abs(_a));
            ExtDrawGuizmos.DebugWireSphere(circumCenter, color, circumRadius, duration, false);
        }

        private void ComputeCircumcircleInfos()
        {
            _a = ExtMathf.Determinant3x3(P1.Pos, P2.Pos, P3.Pos)
               - ExtMathf.Determinant3x3(P1.Pos, P2.Pos, P4.Pos)
               + ExtMathf.Determinant3x3(P1.Pos, P3.Pos, P4.Pos)
               - ExtMathf.Determinant3x3(P2.Pos, P3.Pos, P4.Pos);

            float n1 = P1.Pos.sqrMagnitude;
            float n2 = P2.Pos.sqrMagnitude;
            float n3 = P3.Pos.sqrMagnitude;
            float n4 = P4.Pos.sqrMagnitude;
            float Dx = ExtMathf.Determinant3x3(new Vector3(n1, P1.Y, P1.Z), new Vector3(n2, P2.Y, P2.Z), new Vector3(n3, P3.Y, P3.Z))
                     - ExtMathf.Determinant3x3(new Vector3(n1, P1.Y, P1.Z), new Vector3(n2, P2.Y, P2.Z), new Vector3(n4, P4.Y, P4.Z))
                     + ExtMathf.Determinant3x3(new Vector3(n1, P1.Y, P1.Z), new Vector3(n3, P3.Y, P3.Z), new Vector3(n4, P4.Y, P4.Z))
                     - ExtMathf.Determinant3x3(new Vector3(n2, P2.Y, P2.Z), new Vector3(n3, P3.Y, P3.Z), new Vector3(n4, P4.Y, P4.Z));

            float Dy = -(ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Z), new Vector3(n2, P2.X, P2.Z), new Vector3(n3, P3.X, P3.Z))
                       - ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Z), new Vector3(n2, P2.X, P2.Z), new Vector3(n4, P4.X, P4.Z))
                       + ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Z), new Vector3(n3, P3.X, P3.Z), new Vector3(n4, P4.X, P4.Z))
                       - ExtMathf.Determinant3x3(new Vector3(n2, P2.X, P2.Z), new Vector3(n3, P3.X, P3.Z), new Vector3(n4, P4.X, P4.Z)));

            float Dz = ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Y), new Vector3(n2, P2.X, P2.Y), new Vector3(n3, P3.X, P3.Y))
                     - ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Y), new Vector3(n2, P2.X, P2.Y), new Vector3(n4, P4.X, P4.Y))
                     + ExtMathf.Determinant3x3(new Vector3(n1, P1.X, P1.Y), new Vector3(n3, P3.X, P3.Y), new Vector3(n4, P4.X, P4.Y))
                     - ExtMathf.Determinant3x3(new Vector3(n2, P2.X, P2.Y), new Vector3(n3, P3.X, P3.Y), new Vector3(n4, P4.X, P4.Y));

            _d = new Vector3(Dx, Dy, Dz);

            _c = n1 * ExtMathf.Determinant3x3(P2.Pos, P3.Pos, P4.Pos)
               - n2 * ExtMathf.Determinant3x3(P1.Pos, P3.Pos, P4.Pos)
               + n3 * ExtMathf.Determinant3x3(P1.Pos, P2.Pos, P4.Pos)
               - n4 * ExtMathf.Determinant3x3(P1.Pos, P2.Pos, P3.Pos);
        }

        public bool IsPointInCircumcircle(Vector3 newVertex)
        {
            float value = _a * newVertex.sqrMagnitude - Vector3.Dot(newVertex, _d) + _c;
            return value < -0.00001f; // We have to avoid float inaccuracy
        }

        public override bool Equals(object obj)
        {
            Tetra tetra = obj as Tetra;
            if (tetra == null)
            {
                return false;
            }
            Vertex[] tetraVertices = new Vertex[4];
            tetraVertices[0] = tetra.P1;
            tetraVertices[1] = tetra.P2;
            tetraVertices[2] = tetra.P3;
            tetraVertices[3] = tetra.P4;
            return tetra != null
                   && tetraVertices.Contains(P1)
                   && tetraVertices.Contains(P2)
                   && tetraVertices.Contains(P3)
                   && tetraVertices.Contains(P4);
        }

        public override int GetHashCode()
        {
            var hashCode = -653366440;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(P1);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(P2);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(P3);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(P4);
            return hashCode;
        }
    }
}

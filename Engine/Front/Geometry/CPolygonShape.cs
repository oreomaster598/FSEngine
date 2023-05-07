using System;
using System.Collections;
using System.Numerics;

namespace FSEngine.Geometry
{
    /// <summary>
    /// Summary description for NoValidReturnException.
    /// </summary>
    public class NonValidReturnException : ApplicationException
    {
        public NonValidReturnException() : base()
        {

        }
        public NonValidReturnException(string msg)
            : base(msg)
        {
            string errMsg = "\nThere is no valid return value available!";
            throw new NonValidReturnException(errMsg);
        }
        public NonValidReturnException(string msg,
            Exception inner) : base(msg, inner)
        {

        }
    }

    public class InvalidInputGeometryDataException : ApplicationException
    {
        public InvalidInputGeometryDataException() : base()
        {

        }
        public InvalidInputGeometryDataException(string msg)
            : base(msg)
        {

        }
        public InvalidInputGeometryDataException(string msg,
            Exception inner) : base(msg, inner)
        {

        }
    }
    public enum PolygonDirection
    {
        Unknown,
        Clockwise,
        Count_Clockwise
    }

    public enum PolygonType
    {
        Unknown,
        Convex,
        Concave
    }

    public enum VertexType
    {
        ErrorPoint,
        ConvexPoint,
        ConcavePoint
    }

    public struct ConstantValue
    {
        internal const double BigValue = 99999;
        internal const double SmallValue = 0.00001;
    }

    public static class Point2D
    {
        public static double DistanceTo(this Vector2 a, Vector2 point)
        {
            return Math.Sqrt((point.X - a.X) * (point.X - a.X)
                + (point.Y - a.Y) * (point.Y - a.Y));
        }

        public static bool EqualsPoint(this Vector2 a, Vector2 newPoint)
        {
            double dDeff_X =
                Math.Abs(a.X - newPoint.X);
            double dDeff_Y =
                Math.Abs(a.Y - newPoint.Y);

            if ((dDeff_X < ConstantValue.SmallValue)
                && (dDeff_Y < ConstantValue.SmallValue))
                return true;
            else
                return false;
        }

        public static bool InLine(this Vector2 a, CLineSegment lineSegment)
        {
            bool bInline = false;

            double Ax, Ay, Bx, By, Cx, Cy;
            Bx = lineSegment.EndPoint.X;
            By = lineSegment.EndPoint.Y;
            Ax = lineSegment.StartPoint.X;
            Ay = lineSegment.StartPoint.Y;
            Cx = a.X;
            Cy = a.Y;

            double L = lineSegment.GetLineSegmentLength();
            double s = Math.Abs(((Ay - Cy) * (Bx - Ax) - (Ax - Cx) * (By - Ay)) / (L * L));

            if (Math.Abs(s - 0) < ConstantValue.SmallValue)
            {
                if ((SamePoints(a, lineSegment.StartPoint)) ||
                    (SamePoints(a, lineSegment.EndPoint)))
                    bInline = true;
                else if ((Cx < lineSegment.GetXmax())
                    && (Cx > lineSegment.GetXmin())
                    && (Cy < lineSegment.GetYmax())
                    && (Cy > lineSegment.GetYmin()))
                    bInline = true;
            }
            return bInline;
        }

        public static bool PointInsidePolygon(this Vector2 a, Vector2[] polygonVertices)
        {
            if (polygonVertices.Length < 3) return false;

            int nCounter = 0;
            int nPoints = polygonVertices.Length;

            Vector2 s1, p1, p2;
            s1 = a;
            p1 = polygonVertices[0];

            for (int i = 1; i < nPoints; i++)
            {
                p2 = polygonVertices[i % nPoints];
                if (s1.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (s1.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (s1.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xInters = (s1.Y - p1.Y) * (p2.X - p1.X) /
                                    (p2.Y - p1.Y) + p1.X;
                                if ((p1.X == p2.X) || (s1.X <= xInters))
                                {
                                    nCounter++;
                                }
                            }
                        }
                    }
                }
                p1 = p2;
            }
            if ((nCounter % 2) == 0)
                return false;
            else
                return true;
        }

        public static bool SamePoints(Vector2 Point1, Vector2 Point2)
        {
            double dDeff_X =
                Math.Abs(Point1.X - Point2.X);
            double dDeff_Y =
                Math.Abs(Point1.Y - Point2.Y);

            if ((dDeff_X < ConstantValue.SmallValue)
                && (dDeff_Y < ConstantValue.SmallValue))
                return true;
            else
                return false;
        }
        public static void SortPointsByX(Vector2[] points)
        {
            if (points.Length > 1)
            {
                Vector2 tempPt;
                for (int i = 0; i < points.Length - 2; i++)
                {
                    for (int j = i + 1; j < points.Length - 1; j++)
                    {
                        if (points[i].X > points[j].X)
                        {
                            tempPt = points[j];
                            points[j] = points[i];
                            points[i] = tempPt;
                        }
                    }
                }
            }
        }

        public static void SortPointsByY(Vector2[] points)
        {
            if (points.Length > 1)
            {
                Vector2 tempPt;
                for (int i = 0; i < points.Length - 2; i++)
                {
                    for (int j = i + 1; j < points.Length - 1; j++)
                    {
                        if (points[i].Y > points[j].Y)
                        {
                            tempPt = points[j];
                            points[j] = points[i];
                            points[i] = tempPt;
                        }
                    }
                }
            }
        }
    }

    public class CLine
    {
        protected float a;
        protected float b;
        protected float c;

        public CLine(float angleInRad, Vector2 point)
        {
            Initialize(angleInRad, point);
        }

        public CLine(Vector2 point1, Vector2 point2)
        {
            try
            {
                if (Point2D.SamePoints(point1, point2))
                {
                    string errMsg = "The input points are the same";
                    InvalidInputGeometryDataException ex = new
                        InvalidInputGeometryDataException(errMsg);
                    throw ex;
                }

                if ((float)Math.Abs(point1.X - point2.X)
    < ConstantValue.SmallValue)
                {
                    Initialize((float)Math.PI / 2, point1);
                }
                else if ((float)Math.Abs(point1.Y - point2.Y)
                    < ConstantValue.SmallValue)
                {
                    Initialize(0, point1);
                }
                else
                {
                    float m = (point2.Y - point1.Y) / (point2.X - point1.X);
                    float alphaInRad = (float)Math.Atan(m);
                    Initialize(alphaInRad, point1);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message + e.StackTrace);
            }
        }

        public CLine(CLine copiedLine)
        {
            this.a = copiedLine.a;
            this.b = copiedLine.b;
            this.c = copiedLine.c;
        }

        public float GetDistance(Vector2 point)
        {
            float x0 = point.X;
            float y0 = point.Y;

            float d = (float)Math.Abs(a * x0 + b * y0 + c);
            d = d / ((float)Math.Sqrt(a * a + b * b));

            return d;
        }

        public float GetLineAngle()
        {
            if (b == 0)
            {
                return (float)Math.PI / 2;
            }
            else
            {
                float tanA = -a / b;
                return (float)Math.Atan(tanA);
            }
        }

        public float GetX(float y)
        {
            float x;
            try
            {
                if ((float)Math.Abs(a) < ConstantValue.SmallValue)
                {
                    throw new NonValidReturnException();
                }

                x = -(b * y + c) / a;
            }
            catch (Exception e)
            {
                x = float.PositiveInfinity;
                System.Diagnostics.Trace.
                    WriteLine(e.Message + e.StackTrace);
            }

            return x;
        }

        public float GetY(float x)
        {
            float y;
            try
            {
                if ((float)Math.Abs(b) < ConstantValue.SmallValue)
                {
                    throw new NonValidReturnException();
                }
                y = -(a * x + c) / b;
            }
            catch (Exception e)
            {
                y = float.PositiveInfinity;
                System.Diagnostics.Trace.
                    WriteLine(e.Message + e.StackTrace);
            }
            return y;
        }

        public bool HorizontalLine()
        {
            if ((float)Math.Abs(a - 0) < ConstantValue.SmallValue)
                return true;
            else
                return false;
        }

        public Vector2 IntersecctionWith(CLine line)
        {
            Vector2 point = new Vector2();
            float a1 = this.a;
            float b1 = this.b;
            float c1 = this.c;

            float a2 = line.a;
            float b2 = line.b;
            float c2 = line.c;

            if (!(this.Parallel(line)))
            {
                point.X = (c2 * b1 - c1 * b2) / (a1 * b2 - a2 * b1);
                point.Y = (a1 * c2 - c1 * a2) / (a2 * b2 - a1 * b2);
            }
            return point;
        }

        public bool Parallel(CLine line)
        {
            bool bParallel = false;
            if (this.a / this.b == line.a / line.b)
                bParallel = true;

            return bParallel;
        }

        public bool VerticalLine()
        {
            if ((float)Math.Abs(b - 0) < ConstantValue.SmallValue)
                return true;
            else
                return false;
        }

        private void Initialize(float angleInRad, Vector2 point)
        {
            try
            {
                if (angleInRad > 2 * (float)Math.PI)
                {
                    string errMsg = string.Format(
                        "The input line angle" +
                        " {0} is wrong. It should be between 0-2*PI.", angleInRad);

                    InvalidInputGeometryDataException ex = new
                        InvalidInputGeometryDataException(errMsg);

                    throw ex;
                }

                if ((float)Math.Abs(angleInRad - (float)Math.PI / 2) <
                    ConstantValue.SmallValue)
                {
                    a = 1;
                    b = 0;
                    c = -point.X;
                }
                else
                {
                    a = -(float)Math.Tan(angleInRad);
                    b = 1;
                    c = -a * point.X - b * point.Y;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message + e.StackTrace);
            }
        }
        /**************************************
		 Calculate intersection point of two lines
		 if two lines are parallel, return null
		 * ************************************/
    }

    public class CLineSegment : CLine
    {
        private Vector2 m_endPoint;
        private Vector2 m_startPoint;
        public CLineSegment(Vector2 startPoint, Vector2 endPoint)
            : base(startPoint, endPoint)
        {
            this.m_startPoint = startPoint;
            this.m_endPoint = endPoint;
        }

        public Vector2 EndPoint
        {
            get
            {
                return m_endPoint;
            }
        }

        public Vector2 StartPoint
        {
            get
            {
                return m_startPoint;
            }
        }
        public void ChangeLineDirection()
        {
            Vector2 tempPt;
            tempPt = this.m_startPoint;
            this.m_startPoint = this.m_endPoint;
            this.m_endPoint = tempPt;
        }

        public float GetLineSegmentLength()
        {
            float d = (m_endPoint.X - m_startPoint.X) * (m_endPoint.X - m_startPoint.X);
            d += (m_endPoint.Y - m_startPoint.Y) * (m_endPoint.Y - m_startPoint.Y);
            d = (float)Math.Sqrt(d);

            return d;
        }

        public int GetPointLocation(Vector2 point)
        {
            float Ax, Ay, Bx, By, Cx, Cy;
            Bx = m_endPoint.X;
            By = m_endPoint.Y;

            Ax = m_startPoint.X;
            Ay = m_startPoint.Y;

            Cx = point.X;
            Cy = point.Y;

            if (this.HorizontalLine())
            {
                if ((float)Math.Abs(Ay - Cy) < ConstantValue.SmallValue) return 0;
                else if (Ay > Cy)
                    return -1;
                else return 1;
            }
            else
            {
                if (m_endPoint.Y > m_startPoint.Y)
                    this.ChangeLineDirection();

                float L = this.GetLineSegmentLength();
                float s = ((Ay - Cy) * (Bx - Ax) - (Ax - Cx) * (By - Ay)) / (L * L);

                if ((float)Math.Abs(s - 0) < ConstantValue.SmallValue) return 0;
                else if (s > 0)
                    return -1;
                else return 1;
            }
        }

        public float GetXmax()
        {
            return (float)Math.Max(m_startPoint.X, m_endPoint.X);
        }

        public float GetXmin()
        {
            return (float)Math.Min(m_startPoint.X, m_endPoint.X);
        }
        public float GetYmax()
        {
            return (float)Math.Max(m_startPoint.Y, m_endPoint.Y);
        }

        public float GetYmin()
        {
            return (float)Math.Min(m_startPoint.Y, m_endPoint.Y);
        }
        public bool InLine(CLineSegment longerLineSegment)
        {
            bool bInLine = false;
            if ((m_startPoint.InLine(longerLineSegment)) &&
                (m_endPoint.InLine(longerLineSegment)))
                bInLine = true;
            return bInLine;
        }

        public bool IntersectedWith(CLineSegment line)
        {
            float x1 = this.m_startPoint.X;
            float y1 = this.m_startPoint.Y;
            float x2 = this.m_endPoint.X;
            float y2 = this.m_endPoint.Y;
            float x3 = line.m_startPoint.X;
            float y3 = line.m_startPoint.Y;
            float x4 = line.m_endPoint.X;
            float y4 = line.m_endPoint.Y;

            float de = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if ((float)Math.Abs(de - 0) < ConstantValue.SmallValue)
            {
                float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / de;
                float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / de;

                if ((ub > 0) && (ub < 1))
                    return true;
                else
                    return false;
            }
            else return false;
        }

        public CLineSegment OffsetLine(float distance, bool rightOrDown)
        {
            CLineSegment line;
            Vector2 newStartPoint = new Vector2();
            Vector2 newEndPoint = new Vector2();

            float alphaInRad = this.GetLineAngle(); if (rightOrDown)
            {
                if (this.HorizontalLine())
                {
                    newStartPoint.X = this.m_startPoint.X;
                    newStartPoint.Y = this.m_startPoint.Y + distance;

                    newEndPoint.X = this.m_endPoint.X;
                    newEndPoint.Y = this.m_endPoint.Y + distance;
                    line = new CLineSegment(newStartPoint, newEndPoint);
                }
                else
                {
                    if ((float)Math.Sin(alphaInRad) > 0)
                    {
                        newStartPoint.X = m_startPoint.X + (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newStartPoint.Y = m_startPoint.Y - (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));

                        newEndPoint.X = m_endPoint.X + (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newEndPoint.Y = m_endPoint.Y - (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));

                        line = new CLineSegment(
                                       newStartPoint, newEndPoint);
                    }
                    else
                    {
                        newStartPoint.X = m_startPoint.X + (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newStartPoint.Y = m_startPoint.Y + (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));
                        newEndPoint.X = m_endPoint.X + (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newEndPoint.Y = m_endPoint.Y + (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));

                        line = new CLineSegment(
                            newStartPoint, newEndPoint);
                    }
                }
            }
            else
            {
                if (this.HorizontalLine())
                {
                    newStartPoint.X = m_startPoint.X;
                    newStartPoint.Y = m_startPoint.Y - distance;

                    newEndPoint.X = m_endPoint.X;
                    newEndPoint.Y = m_endPoint.Y - distance;
                    line = new CLineSegment(
                        newStartPoint, newEndPoint);
                }
                else
                {
                    if ((float)Math.Sin(alphaInRad) >= 0)
                    {
                        newStartPoint.X = m_startPoint.X - (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newStartPoint.Y = m_startPoint.Y + (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));
                        newEndPoint.X = m_endPoint.X - (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newEndPoint.Y = m_endPoint.Y + (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));

                        line = new CLineSegment(
                            newStartPoint, newEndPoint);
                    }
                    else
                    {
                        newStartPoint.X = m_startPoint.X - (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newStartPoint.Y = m_startPoint.Y - (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));
                        newEndPoint.X = m_endPoint.X - (float)Math.Abs(distance * (float)Math.Sin(alphaInRad));
                        newEndPoint.Y = m_endPoint.Y - (float)Math.Abs(distance * (float)Math.Cos(alphaInRad));

                        line = new CLineSegment(
                            newStartPoint, newEndPoint);
                    }
                }
            }
            return line;
        }
    }

    public class CPolygon
    {
        private Vector2[] m_aVertices;

        public CPolygon()
        {
        }

        public CPolygon(Vector2[] points)
        {
            int nNumOfPoitns = points.Length;
            try
            {
                if (nNumOfPoitns < 3)
                {
                    InvalidInputGeometryDataException ex =
                        new InvalidInputGeometryDataException();
                    throw ex;
                }
                else
                {
                    m_aVertices = new Vector2[nNumOfPoitns];
                    Array.Copy(points, m_aVertices, points.Length);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(
                    e.Message + e.StackTrace);
            }
        }

        public Vector2 this[int index]
        {
            set
            {
                m_aVertices[index] = value;
            }
            get
            {
                return m_aVertices[index];
            }
        }
        public static PolygonDirection PointsDirection(
            Vector2[] points)
        {
            int nCount = 0, j = 0, k = 0;
            int nPoints = points.Length;

            if (nPoints < 3)
                return PolygonDirection.Unknown;

            for (int i = 0; i < nPoints; i++)
            {
                j = (i + 1) % nPoints; k = (i + 2) % nPoints;
                double crossProduct = (points[j].X - points[i].X)
                    * (points[k].Y - points[j].Y);
                crossProduct = crossProduct - (
                    (points[j].Y - points[i].Y)
                    * (points[k].X - points[j].X)
                    );

                if (crossProduct > 0)
                    nCount++;
                else
                    nCount--;
            }

            if (nCount < 0)
                return PolygonDirection.Count_Clockwise;
            else if (nCount > 0)
                return PolygonDirection.Clockwise;
            else
                return PolygonDirection.Unknown;
        }

        public static double PolygonArea(Vector2[] points)
        {
            double dblArea = 0;
            int nNumOfPts = points.Length;

            int j;
            for (int i = 0; i < nNumOfPts; i++)
            {
                j = (i + 1) % nNumOfPts;
                dblArea += points[i].X * points[j].Y;
                dblArea -= (points[i].Y * points[j].X);
            }

            dblArea = dblArea / 2;
            return dblArea;
        }

        public static void ReversePointsDirection(
            Vector2[] points)
        {
            int nVertices = points.Length;
            Vector2[] aTempPts = new Vector2[nVertices];

            for (int i = 0; i < nVertices; i++)
                aTempPts[i] = points[i];

            for (int i = 0; i < nVertices; i++)
                points[i] = aTempPts[nVertices - 1 - i];
        }

        public bool Diagonal(Vector2 vertex1, Vector2 vertex2)
        {
            bool bDiagonal = false;
            int nNumOfVertices = m_aVertices.Length;
            int j = 0;
            for (int i = 0; i < nNumOfVertices; i++)
            {
                bDiagonal = true;
                j = (i + 1) % nNumOfVertices;
                double x1 = vertex1.X;
                double y1 = vertex1.Y;
                double x2 = vertex1.X;
                double y2 = vertex1.Y;

                double x3 = m_aVertices[i].X;
                double y3 = m_aVertices[i].Y;
                double x4 = m_aVertices[j].X;
                double y4 = m_aVertices[j].Y;

                double de = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
                double ub = -1;

                if (Math.Abs(de - 0) > ConstantValue.SmallValue) ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / de;

                if ((ub > 0) && (ub < 1))
                {
                    bDiagonal = false;
                }
            }
            return bDiagonal;
        }

        public PolygonType GetPolygonType()
        {
            int nNumOfVertices = m_aVertices.Length;
            bool bSignChanged = false;
            int nCount = 0;
            int j = 0, k = 0;

            for (int i = 0; i < nNumOfVertices; i++)
            {
                j = (i + 1) % nNumOfVertices; k = (i + 2) % nNumOfVertices;
                double crossProduct = (m_aVertices[j].X - m_aVertices[i].X)
                    * (m_aVertices[k].Y - m_aVertices[j].Y);
                crossProduct = crossProduct - (
                    (m_aVertices[j].Y - m_aVertices[i].Y)
                    * (m_aVertices[k].X - m_aVertices[j].X)
                    );

                if ((crossProduct > 0) && (nCount == 0))
                    nCount = 1;
                else if ((crossProduct < 0) && (nCount == 0))
                    nCount = -1;

                if (((nCount == 1) && (crossProduct < 0))
                    || ((nCount == -1) && (crossProduct > 0)))
                    bSignChanged = true;
            }

            if (bSignChanged)
                return PolygonType.Concave;
            else
                return PolygonType.Convex;
        }

        public Vector2 NextPoint(Vector2 vertex)
        {
            Vector2 nextPt = new Vector2();

            int nIndex;
            nIndex = VertexIndex(vertex);
            if (nIndex == -1)
                return default(Vector2);
            else
            {
                int nNumOfPt = m_aVertices.Length;
                if (nIndex == nNumOfPt - 1)
                {
                    return m_aVertices[0];
                }
                else return m_aVertices[nIndex + 1];
            }
        }

        public double PolygonArea()
        {
            double dblArea = 0;
            int nNumOfVertices = m_aVertices.Length;

            int j;
            for (int i = 0; i < nNumOfVertices; i++)
            {
                j = (i + 1) % nNumOfVertices;
                dblArea += m_aVertices[i].X * m_aVertices[j].Y;
                dblArea -= (m_aVertices[i].Y * m_aVertices[j].X);
            }

            dblArea = dblArea / 2;
            return Math.Abs(dblArea);
        }

        public bool PolygonVertex(Vector2 point)
        {
            bool bVertex = false;
            int nIndex = VertexIndex(point);

            if ((nIndex >= 0) && (nIndex <= m_aVertices.Length - 1))
                bVertex = true;

            return bVertex;
        }

        public VertexType PolygonVertexType(Vector2 vertex)
        {
            VertexType vertexType = VertexType.ErrorPoint;

            if (PolygonVertex(vertex))
            {
                Vector2 pti = vertex;
                Vector2 ptj = PreviousPoint(vertex);
                Vector2 ptk = NextPoint(vertex);

                double dArea = PolygonArea(new Vector2[] { ptj, pti, ptk });

                if (dArea < 0)
                    vertexType = VertexType.ConvexPoint;
                else if (dArea > 0)
                    vertexType = VertexType.ConcavePoint;
            }
            return vertexType;
        }

        public Vector2 PreviousPoint(Vector2 vertex)
        {
            int nIndex;

            nIndex = VertexIndex(vertex);
            if (nIndex == -1)
                return default(Vector2);
            else
            {
                if (nIndex == 0)
                {
                    int nPoints = m_aVertices.Length;
                    return m_aVertices[nPoints - 1];
                }
                else return m_aVertices[nIndex - 1];
            }
        }

        public bool PrincipalVertex(Vector2 vertex)
        {
            bool bPrincipal = false;
            if (PolygonVertex(vertex))
            {
                Vector2 pt1 = PreviousPoint(vertex);
                Vector2 pt2 = NextPoint(vertex);

                if (Diagonal(pt1, pt2))
                    bPrincipal = true;
            }
            return bPrincipal;
        }

        public void ReverseVerticesDirection()
        {
            int nVertices = m_aVertices.Length;
            Vector2[] aTempPts = new Vector2[nVertices];

            for (int i = 0; i < nVertices; i++)
                aTempPts[i] = m_aVertices[i];

            for (int i = 0; i < nVertices; i++)
                m_aVertices[i] = aTempPts[nVertices - 1 - i];
        }

        public int VertexIndex(Vector2 vertex)
        {
            int nIndex = -1;

            int nNumPts = m_aVertices.Length;
            for (int i = 0; i < nNumPts; i++)
            {
                if (Point2D.SamePoints(m_aVertices[i], vertex))
                    nIndex = i;
            }
            return nIndex;
        }
        public PolygonDirection VerticesDirection()
        {
            int nCount = 0, j = 0, k = 0;
            int nVertices = m_aVertices.Length;

            for (int i = 0; i < nVertices; i++)
            {
                j = (i + 1) % nVertices; k = (i + 2) % nVertices;
                double crossProduct = (m_aVertices[j].X - m_aVertices[i].X)
                    * (m_aVertices[k].Y - m_aVertices[j].Y);
                crossProduct = crossProduct - (
                    (m_aVertices[j].Y - m_aVertices[i].Y)
                    * (m_aVertices[k].X - m_aVertices[j].X)
                    );

                if (crossProduct > 0)
                    nCount++;
                else
                    nCount--;
            }

            if (nCount < 0)
                return PolygonDirection.Count_Clockwise;
            else if (nCount > 0)
                return PolygonDirection.Clockwise;
            else
                return PolygonDirection.Unknown;
        }
    }
    public class CPolygonShape
    {
        public Vector2[][] m_aPolygons;
        private Vector2[] m_aInputVertices;
        private ArrayList m_alEars = new ArrayList();
        private Vector2[] m_aUpdatedPolygonVertices;
        public CPolygonShape(Vector2[] vertices)
        {
            int nVertices = vertices.Length;
            if (nVertices < 3)
            {
                System.Diagnostics.Trace.WriteLine("To make a polygon, "
                    + " at least 3 points are required!");
                return;
            }

            m_aInputVertices = new Vector2[nVertices];


            Array.Copy(vertices, m_aInputVertices, nVertices);
            //for (int i = 0; i < nVertices; i++)
            //    m_aInputVertices[i] = vertices[i];

            SetUpdatedPolygonVertices();
        }

        public int NumberOfPolygons
        {
            get
            {
                return m_aPolygons.Length;
            }
        }

        public void CutEar()
        {
            CPolygon polygon = new CPolygon(m_aUpdatedPolygonVertices);
            bool bFinish = false;

            if (m_aUpdatedPolygonVertices.Length == 3) bFinish = true;

            Vector2 pt = new Vector2();
            while (bFinish == false)
            {
                int i = 0;
                bool bNotFound = true;
                while (bNotFound
                    && (i < m_aUpdatedPolygonVertices.Length))
                {
                    pt = m_aUpdatedPolygonVertices[i];
                    if (IsEarOfUpdatedPolygon(pt))
                        bNotFound = false;
                    else
                        i++;
                }
                if (pt != null)
                    UpdatePolygonVertices(pt);

                polygon = new CPolygon(m_aUpdatedPolygonVertices);
                if (m_aUpdatedPolygonVertices.Length == 3)
                    bFinish = true;
            }
            SetPolygons();
        }
        private bool IsEarOfUpdatedPolygon(Vector2 vertex)
        {
            CPolygon polygon = new CPolygon(m_aUpdatedPolygonVertices);

            if (polygon.PolygonVertex(vertex))
            {
                bool bEar = true;
                if (polygon.PolygonVertexType(vertex) == VertexType.ConvexPoint)
                {
                    Vector2 pi = vertex;
                    Vector2 pj = polygon.PreviousPoint(vertex); Vector2 pk = polygon.NextPoint(vertex);
                    for (int i = m_aUpdatedPolygonVertices.GetLowerBound(0);
                        i < m_aUpdatedPolygonVertices.GetUpperBound(0); i++)
                    {
                        Vector2 pt = m_aUpdatedPolygonVertices[i];
                        if (!(pt.EqualsPoint(pi) || pt.EqualsPoint(pj) || pt.EqualsPoint(pk)))
                        {
                            if (TriangleContainsPoint(new Vector2[] { pj, pi, pk }, pt))
                                bEar = false;
                        }
                    }
                }
                else bEar = false; return bEar;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("IsEarOfUpdatedPolygon: " +
                    "Not a polygon vertex");
                return false;
            }
        }

        int index(int x, int y)
        {
            return x * 3 + y;
        }
        private void SetPolygons()
        {
            int nPolygon = m_alEars.Count + 1;
            m_aPolygons = new Vector2[nPolygon][];

            for (int i = 0; i < nPolygon - 1; i++)
            {               

                Vector2[] points = (Vector2[])m_alEars[i];

                 m_aPolygons[i] = new Vector2[3]; 
                 m_aPolygons[i][0] = points[0];
                 m_aPolygons[i][1] = points[1];
                 m_aPolygons[i][2] = points[2];
            }

            m_aPolygons[nPolygon - 1] = new    Vector2[m_aUpdatedPolygonVertices.Length];


            for (int i = 0; i < m_aUpdatedPolygonVertices.Length; i++)
            {
                 m_aPolygons[nPolygon - 1][i] = m_aUpdatedPolygonVertices[i];

        
            }
        }

        private void SetUpdatedPolygonVertices()
        {
            int nVertices = m_aInputVertices.Length;
            m_aUpdatedPolygonVertices = new Vector2[nVertices];

            for (int i = 0; i < nVertices; i++)
                m_aUpdatedPolygonVertices[i] = m_aInputVertices[i];

            if (CPolygon.PointsDirection(m_aUpdatedPolygonVertices)    == PolygonDirection.Clockwise)
                CPolygon.ReversePointsDirection(m_aUpdatedPolygonVertices);
        }

        private bool TriangleContainsPoint(Vector2[] trianglePts, Vector2 pt)
        {
            if (trianglePts.Length != 3)
                return false;

            for (int i = trianglePts.GetLowerBound(0);
                i < trianglePts.GetUpperBound(0); i++)
            {
                if (pt.EqualsPoint(trianglePts[i]))
                    return true;
            }

            bool bIn = false;

            CLineSegment line0 = new CLineSegment(trianglePts[0], trianglePts[1]);
            CLineSegment line1 = new CLineSegment(trianglePts[1], trianglePts[2]);
            CLineSegment line2 = new CLineSegment(trianglePts[2], trianglePts[0]);

            if (pt.InLine(line0) || pt.InLine(line1)
                || pt.InLine(line2))
                bIn = true;
            else
            {
                double dblArea0 = CPolygon.PolygonArea(new Vector2[]
            {trianglePts[0],trianglePts[1], pt});
                double dblArea1 = CPolygon.PolygonArea(new Vector2[]
            {trianglePts[1],trianglePts[2], pt});
                double dblArea2 = CPolygon.PolygonArea(new Vector2[]
            {trianglePts[2],trianglePts[0], pt});

                if (dblArea0 > 0)
                {
                    if ((dblArea1 > 0) && (dblArea2 > 0))
                        bIn = true;
                }
                else if (dblArea0 < 0)
                {
                    if ((dblArea1 < 0) && (dblArea2 < 0))
                        bIn = true;
                }
            }
            return bIn;
        }
        private void UpdatePolygonVertices(Vector2 vertex)
        {
            ArrayList alTempPts = new ArrayList();

            for (int i = 0; i < m_aUpdatedPolygonVertices.Length; i++)
            {
                if (vertex.EqualsPoint(
                    m_aUpdatedPolygonVertices[i]))
                {
                    CPolygon polygon = new CPolygon(m_aUpdatedPolygonVertices);
                    Vector2 pti = vertex;
                    Vector2 ptj = polygon.PreviousPoint(vertex); Vector2 ptk = polygon.NextPoint(vertex);
                    Vector2[] aEar = new Vector2[3]; aEar[0] = ptj;
                    aEar[1] = pti;
                    aEar[2] = ptk;

                    m_alEars.Add(aEar);
                }
                else
                {
                    alTempPts.Add(m_aUpdatedPolygonVertices[i]);
                }
            }

            if (m_aUpdatedPolygonVertices.Length
                - alTempPts.Count == 1)
            {
                int nLength = m_aUpdatedPolygonVertices.Length;
                m_aUpdatedPolygonVertices = new Vector2[nLength - 1];

                for (int i = 0; i < alTempPts.Count; i++)
                    m_aUpdatedPolygonVertices[i] = (Vector2)alTempPts[i];
            }
        }
    }
}
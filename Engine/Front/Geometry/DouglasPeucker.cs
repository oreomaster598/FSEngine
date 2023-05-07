
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Geometry
{
        public class DouglasPeucker
    {
            /// <summary>
            ///     Minimum number of points required to run the algorithm.
            /// </summary>
            private const int MinPoints = 3;

            /// <summary>
            ///     Class representing a Douglas Peucker
            ///     segment. Contains the start and end index of the line,
            ///     the biggest distance of a point from the line and the
            ///     points index.
            /// </summary>
            private class Segment
            {
                /// <summary>
                ///     The start index of the line.
                /// </summary>
                public int Start { get; set; }

                /// <summary>
                ///     The end index of the line.
                /// </summary>
                public int End { get; set; }

                /// <summary>
                ///     The biggest perpendicular distance index of a point.
                /// </summary>
                public int Perpendicular { get; set; }

                /// <summary>
                ///     The max perpendicular distance of a point along the
                ///     line.
                /// </summary>
                public double Distance { get; set; }
            }

            /// <summary>
            ///     Gets the perpendicular distance of a point to the line between start
            ///     and end.
            /// </summary>
            /// <param name="start">The start point of the line.</param>
            /// <param name="end">The end point of the line.</param>
            /// <param name="point">
            ///     The point to calculate the perpendicular distance of.
            /// </param>
            /// <returns>The perpendicular distance.</returns>
            private static double GetDistance(Vector2 start, Vector2 end, Vector2 point)
            {
                var x = end.X - start.X;
                var y = end.Y - start.Y;

                var m = x * x + y * y;

                var u = ((point.X - start.X) * x + (point.Y - start.Y) * y) / m;

                if (u < 0)
                {
                    x = start.X;
                    y = start.Y;
                }
                else if (u > 1)
                {
                    x = end.X;
                    y = end.Y;
                }
                else
                {
                    x = start.X + u * x;
                    y = start.Y + u * y;
                }

                x = point.X - x;
                y = point.Y - y;

                return Math.Sqrt(x * x + y * y);
            }

        /// <summary>
        ///     Creates a new <see cref="Segment"/> with the start and end indices.
        ///     Calculates the max perpendicular distance for each specified point
        ///     against the line between start and end.
        /// </summary>
        /// <param name="start">The start index of the line.</param>
        /// <param name="end">The end index of the line.</param>
        /// <param name="points">The points.</param>
        /// <returns>The Segment</returns>
        /// <remarks>
        ///     If the segment doesnt contain enough values to be split again the
        ///     segment distance property is left as 0. This ensures that the segment
        ///     wont be selected again from the <see cref="Reduce(ref List{Segment},
        ///     List{Vector2}, int, double)"/> part of the algorithm.
        /// </remarks>
        private static Segment CreateSegment(int start, int end, List<Vector2> points)
            {
                var count = end - start;

                if (count >= MinPoints - 1)
                {
                    var first = points[start];
                    var last = points[end];

                    var max = points.GetRange(start + 1, count - 1)
                        .Select((point, index) => new
                        {
                            Index = start + 1 + index,
                            Distance = GetDistance(first, last, point)
                        }).OrderByDescending(p => p.Distance).First();

                    return new Segment
                    {
                        Start = start,
                        End = end,
                        Perpendicular = max.Index,
                        Distance = max.Distance
                    };
                }

                return new Segment
                {
                    Start = start,
                    End = end,
                    Perpendicular = -1
                };
            }

            /// <summary>
            ///     Splits the specified segment about the perpendicular index and return
            ///     the segment before and after with calculated values.
            /// </summary>
            /// <param name="segment">The segment to split.</param>
            /// <param name="points">The points.</param>
            /// <returns>The two segments.</returns>
            private static IEnumerable<Segment> SplitSegment(Segment segment,
                List<Vector2> points)
            {
                return new[]
                {
                CreateSegment(segment.Start, segment.Perpendicular, points),
                CreateSegment(segment.Perpendicular, segment.End, points)
            };
            }

            /// <summary>
            ///     Check to see if the point has valid values and returns false if not.
            /// </summary>
            /// <param name="point">The point to check.</param>
            /// <returns>True if the points values are valid.</returns>
            private static bool IsValid(Vector2 point)
            {
                return !double.IsNaN(point.X) && !double.IsNaN(point.Y);
            }

            /// <summary>
            ///     Interpolates the sepcified points by reducing until the sepcified
            ///     tolerance is met or the specified max number of points is met.
            /// </summary>
            /// <param name="points">The points to reduce.</param>
            /// <param name="max">The max number of points to return.</param>
            /// <param name="tolerance">
            ///     The min distance tolerance of points to return.
            /// </param>
            /// <returns>The interpolated reduced points.</returns>
            public static IEnumerable<Vector2> Interpolate(List<Vector2> points, int max,
                double tolerance = 0d)
            {
                if (max < MinPoints || points.Count < max)
                {
                    return points;
                }

                var segments = GetSegments(points).ToList();

                Reduce(ref segments, points, max, tolerance);

                return segments
                    .OrderBy(p => p.Start)
                    .SelectMany((s, i) => GetPoints(s, segments.Count, i, points));
            }

            /// <summary>
            ///     Gets the reduced points from the <see cref="Segment"/>. Invalid values
            ///     are included in the result as well as last point of the last segment.
            /// </summary>
            /// <param name="segment">The segment to get the indices from.</param>
            /// <param name="count">The total number of segments in the algorithm.</param>
            /// <param name="index">The index of the current segment.</param>
            /// <param name="points">The points.</param>
            /// <returns>The valid points from the segment.</returns>
            private static IEnumerable<Vector2> GetPoints(Segment segment, int count,
                int index, List<Vector2> points)
            {
                yield return points[segment.Start];

                var next = segment.End + 1;

                var isGap = next < points.Count && !IsValid(points[next]);

                if (index == count - 1 || isGap)
                {
                    yield return points[segment.End];

                    if (isGap)
                    {
                        yield return points[next];
                    }
                }
            }

            /// <summary>
            ///     Gets the initial <see cref="Segment"/> for the algorithm. If points
            ///     contains invalid values then multiple segments are returned for each
            ///     side of the invalid value.
            /// </summary>
            /// <param name="points">The points.</param>
            /// <returns>The segments.</returns>
            private static IEnumerable<Segment> GetSegments(List<Vector2> points)
            {
                var previous = 0;

                foreach (var p in points.Select((p, i) => new
                {
                    Point = p,
                    Index = i
                })
                .Where(p => !IsValid(p.Point)))
                {
                    yield return CreateSegment(previous, p.Index - 1, points);

                    previous = p.Index + 1;
                }

                yield return CreateSegment(previous, points.Count - 1, points);
            }

            /// <summary>
            ///     Reduces the segments until the specified max or tolerance has been met
            ///     or the points can no longer be reduced.
            /// </summary>
            /// <param name="segments">The segements to reduce.</param>
            /// <param name="points">The points.</param>
            /// <param name="max">The max number of points to return.</param>
            /// <param name="tolerance">The min distance tolerance for the points.</param>
            private static void Reduce(ref List<Segment> segments, List<Vector2> points,
                int max,
                double tolerance)
            {
                var gaps = points.Count(p => !IsValid(p));

                // Check to see if max numbers has been reached.
                while (segments.Count + gaps < max - 1)
                {
                    // Get the largest perpendicular distance segment.
                    var current = segments.OrderByDescending(s => s.Distance).First();

                    // Check if tolerance has been met yet or can no longer reduce.
                    if (current.Distance <= tolerance)
                    {
                        break;
                    }

                    segments.Remove(current);

                    var split = SplitSegment(current, points);

                    segments.AddRange(split);
                }
            }
        }
    }

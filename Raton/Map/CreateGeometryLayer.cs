using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Raton.Map.Models;
using Raton.Models.DbModels;
using System;
using System.Collections.Generic;

namespace Raton.Map
{
    public class CreateGeometryLayer
    {
        public static Layer CreateAnimalPositionsLayer(List<MapDatedPointModel> animalActivePoints)
        {
            var features = new List<GeometryFeature>();

            foreach (var activePoint in animalActivePoints)
            {
                var feature = new GeometryFeature();

                feature.Geometry = new Point(activePoint.Longitude, activePoint.Latitude);

                var circle = new SymbolStyle
                {
                    SymbolScale = 0.8,
                    Fill = new Mapsui.Styles.Brush()
                    {
                        Color = activePoint.Color
                    },
                };

                feature.Styles.Add(circle);

                feature.Geometry.UserData = activePoint.Name;

                features.Add(feature);
            }

            var memoryProvider = new MemoryProvider(features)
            {
                CRS = "EPSG:4326" // The DataSource CRS needs to be set
            };

            var dataSource = new ProjectingProvider(memoryProvider)
            {
                CRS = "EPSG:3857"
            };

            return new Layer
            {
                DataSource = dataSource,
                Name = "WGS84 Geometries",
                IsMapInfoLayer = true,
                Style = new SymbolStyle()
                {
                    SymbolScale = 0.01
                }
            };
        }

        public static Layer CreateRulerLayer (List<PointModel> points, Avalonia.Media.SolidColorBrush aColor)
        {
            var features = new List<GeometryFeature>();

            var previousPoint = new PointModel();

            IStyle linestringStyle = new VectorStyle()
            {
                Fill = null,
                Outline = null,
                Line = { Color = Color.FromArgb(aColor.Color.A, aColor.Color.R, aColor.Color.G, aColor.Color.B),
                    Width = 3 }
            };

            for (int pointNumber = points.Count - 1; pointNumber > -1; pointNumber--)
            {
                var point = points[pointNumber];
                if (pointNumber > 0)
                {
                    previousPoint = points[pointNumber - 1];
                    var pins = new Coordinate[2];
                    pins[0] = new Coordinate(previousPoint.Longitude, previousPoint.Latitude);
                    pins[1] = new Coordinate(point.Longitude, point.Latitude);

                    var line = new GeometryFeature()
                    {
                        Geometry = new LineString(pins),
                    };

                    line.Styles.Add(linestringStyle);

                    var distance = GetDistance(previousPoint.Longitude, previousPoint.Latitude,
                    point.Longitude, point.Latitude);

                    var labelStyle = new LabelStyle
                    {
                        Text = Math.Round(distance).ToString(),
                        Offset = new Offset(0, 0),
                        BackColor = new Mapsui.Styles.Brush()
                        {
                            Color = Color.FromArgb(aColor.Color.A, aColor.Color.R, aColor.Color.G, aColor.Color.B)
                        },
                    };

                    line.Styles.Add(labelStyle);

                    features.Add(line);
                }

                var endline = new GeometryFeature();
                endline.Geometry = new Point(point.Longitude, point.Latitude);
                var circle = new SymbolStyle
                {
                    SymbolScale = 0.2,
                    Fill = new Mapsui.Styles.Brush()
                    {
                        Color = Color.FromArgb(aColor.Color.A, aColor.Color.R, aColor.Color.G, aColor.Color.B)
                    },
                };
 
                var circleTextStyle = new LabelStyle
                {
                    Text = pointNumber.ToString(),
                    Offset = new Offset(0, -2, true),
                    BackColor = new Mapsui.Styles.Brush()
                    {
                        Color = Color.FromArgb(aColor.Color.A, aColor.Color.R, aColor.Color.G, aColor.Color.B)
                    },
                };
                endline.Styles.Add(circle);
                endline.Styles.Add(circleTextStyle);
                features.Add(endline);
            }

            var memoryProvider = new MemoryProvider(features)
            {
                CRS = "EPSG:4326" // The DataSource CRS needs to be set
            };

            var dataSource = new ProjectingProvider(memoryProvider)
            {
                CRS = "EPSG:3857"
            };

            return new Layer
            {
                DataSource = dataSource,
                Name = "WGS84 Geometries",
                IsMapInfoLayer = true,
                Style = new SymbolStyle()
                {
                    SymbolScale = 0.001
                }
            };
        }

        public static double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        public static Offset GenerateOffsetCounterpartToLine(double firstLongitude, double firstLatitude, double secondLongitude, double secondLatitude)
        {
            var firstPos = new Point(firstLongitude, firstLatitude);
            var secondPos = new Point(secondLongitude, secondLatitude);
            var abs = Math.Sqrt((secondPos.X - firstPos.X) * (secondPos.X - firstPos.X) +
                (secondPos.Y - firstPos.Y) * (secondPos.Y - firstPos.Y));
            var x = (-1) * (secondPos.X - firstPos.X) / abs;
            var y = (-1) * (secondPos.Y - firstPos.Y) / abs;
            return new Offset(2*y, 2*x, true);

        }
    }
}

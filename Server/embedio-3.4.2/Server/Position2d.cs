using Swan.Logging;
using System.Numerics;

[Serializable]
public class Position2d
{
    public double X { get; set; }
    public double Y { get; set; }

    public Position2d()
    {

    }

    public Position2d(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    public static double Distance(Position2d p1, Position2d p2)
    {
        var baseRad = Math.PI * p1.X / 180;
        var targetRad = Math.PI * p2.X / 180;
        var theta = p1.Y - p2.Y;
        var thetaRad = Math.PI * theta / 180;

        double dist =
            Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
            Math.Cos(targetRad) * Math.Cos(thetaRad);
        dist = Math.Acos(dist);

        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;
        
        $"{dist*1.609344}".Info();
        return dist* 1.609344;
    }
}

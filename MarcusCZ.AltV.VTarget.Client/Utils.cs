using System.Numerics;

namespace MarcusCZ.AltV.VTarget.Client;

public class Utils
{
    public static string RandomId()
    {
        Random rand = new Random();
        string str = "";
        for (int i = 0; i < 20; i++)
        {
            str += Convert.ToChar(rand.Next(0, 26) + 65);
        }

        return str;
    }
    
    public static Vector2 AsVector2(Vector3 vector3)
    {
        return new Vector2(vector3.X, vector3.Y);
    }

    public static Vector3 ToRadians(Vector3 v)
    {
        const float pi180 = (float)Math.PI / 180;
        return new Vector3(pi180 * v.X, pi180 * v.Y, pi180 * v.Z);
    }
    
    public static Vector3 RayEnd(Vector3 startPos, Vector3 direction, float distance)
    {
        return new Vector3(
            startPos.X + (float)-Math.Sin(direction.Z) * (float)Math.Abs(Math.Cos(direction.X)) * distance,
            startPos.Y + (float)Math.Cos(direction.Z) * (float)Math.Abs(Math.Cos(direction.X)) * distance,
            startPos.Z + (float)Math.Sin(direction.X) * distance
        );
    }
}
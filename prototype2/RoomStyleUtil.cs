using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomStyleUtil
{
    float totalProbability = 0f;
    public RoomStyle defaultStyle;
    List<RoomStyle> roomsStyleTemplates = new List<RoomStyle>();

    public RoomStyleUtil(RoomStyle[] roomsStyle, TileBase w, TileBase f)
    {
        roomsStyleTemplates = new List<RoomStyle>(roomsStyle);

        foreach (RoomStyle obj in roomsStyleTemplates)
        {
            if (!obj.specialStyle)
                totalProbability += obj.apparitionProbability;
        }

        defaultStyle = new RoomStyle(w, f);
    }
    public RoomStyle GetRandomStyle()
    {
        
        float randomPoint = Random.Range(0f, totalProbability);

        foreach (var st in roomsStyleTemplates)
        {
            if (!st.specialStyle)
            {
                if (randomPoint < st.apparitionProbability)
                    return st;
                randomPoint -= st.apparitionProbability;
            }
        }

        return defaultStyle;
    }
}

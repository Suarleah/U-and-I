using UnityEngine;

public static class CanvasWorldConversion //functions in this class only work in orthographic mode
{

    //Note: for size it seems that a scale 1 square is approximately equal to width/height 100 square in canvas
    //Note: if I scale directly using the ratios, it seems to be off by a factor of exactly 108
    public static Vector3 ctwSize(Vector3 scale, Camera cam, Canvas c) // converts a canvas vector3 to a world vector3
    {
        float yratio = (cam.orthographicSize * 2) / c.GetComponent<RectTransform>().rect.height *108; //just trust me its 108
        float xratio = (cam.orthographicSize * 2 * cam.aspect) / c.GetComponent<RectTransform>().rect.width * 108;
        Vector3 ret = new Vector3(scale.x, scale.y, 1f);
        ret.x *= xratio;
        ret.y *= yratio;
        return ret;
    }

    public static Vector3 wtcSize(Vector3 scale, Camera cam, Canvas c) // converts a world vector3 to a canvas vector3
    {
        float yratio = (cam.orthographicSize * 2) / c.GetComponent<RectTransform>().rect.height*108;
        float xratio = (cam.orthographicSize * 2 * cam.aspect) / c.GetComponent<RectTransform>().rect.width*108;
        Vector3 ret = new Vector3(scale.x, scale.y, 1f);
        ret.x /= xratio;
        ret.y /= yratio;
        return ret;
    }



}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KDebugDraw : MonoBehaviour {
    public const int INVALID_HANDLE = -1;
    Material lineMaterial;

    class Line
    {
        public bool bDrawed = false;
        public Vector3 start;
        public Vector3 end;
        public Color color;
    }

    class Circle
    {
        public bool bDrawed = false;
        public Vector3 center;
        public float radius;
        public Color color;
        public float y;
    }

    class Grid
    {
        public bool bDrawed = false;
        public Vector2 min;
        public Vector2 max;
        public Vector2 cellsize;
        public float y;
        public Color color;

    }


    class Rect2D
    {
        public Vector2 min;
        public Vector2 max;
        public float y;
        public Color color;
    }


    Dictionary<int, List<Line>> m_lines;
    Dictionary<int, List<Circle>> m_circles;
    Dictionary<int, List<Grid>> m_grides;
    Dictionary<int, List<Rect2D>> m_rects;

    float[] m_circleDir;
    const int NUM_SEG = 40;

    void Awake()
    {
        m_lines = new Dictionary<int, List<Line>>();
        m_circles = new Dictionary<int, List<Circle>>();
        m_grides = new Dictionary<int, List<Grid>>();
        m_rects = new Dictionary<int, List<Rect2D>>();

        //
        m_circleDir = new float[40 * 2];
        for (int i = 0; i < NUM_SEG; ++i)
        {
            float a = (float)i / (float)NUM_SEG * Mathf.PI * 2;
            m_circleDir[i * 2] = Mathf.Cos(a);
            m_circleDir[i * 2 + 1] = Mathf.Sin(a);
        }
    }

    public int addDrawLine()
    {
        int count = m_lines.Count;
        m_lines.Add(count, new List<Line>());
        return count;
    }

    public int addDrawCircle()
    {
        int count = m_circles.Count;
        m_circles.Add(count, new List<Circle>());
        return count;
    }

    public int addDrawRact2D()
    {
        int count = m_rects.Count;
        m_rects.Add(count, new List<Rect2D>());
        return count;
    }

    public int addDrawGrid()
    {
        int count = m_grides.Count;
        m_grides.Add(count, new List<Grid>());
        return count;
    }

    public void ClearLine(int handle)
    {
        List<Line> lineList;
        if (!m_lines.TryGetValue(handle, out lineList))
            return;

        lineList.Clear();
    }

    public bool DrawLine(int handle, Vector3 start, Vector3 end, Color color)
    {
        List<Line> lineList;
        if (!m_lines.TryGetValue(handle, out lineList))
            return false;
        Line line = new Line();
        line.start = start;
        line.end = end;
        line.color = color;
        line.bDrawed = false;

        lineList.Add(line);

        return true;
    }

    public void ClearCircle(int handle)
    {
        List<Circle> circleList;
        if (!m_circles.TryGetValue(handle, out circleList))
            return;

        circleList.Clear();
    }

    public bool DrawCircle(int handle, Vector3 center, float radius, float y, Color color)
    {
        List<Circle> circleList ;
        if (!m_circles.TryGetValue(handle, out circleList))
            return false;
        Circle circle = new Circle();
        circle.center = center;
        circle.radius = radius;
        circle.color = color;
        circle.bDrawed = false;
        circle.y = y;
        circleList.Add(circle);
        return true;
    }

    public void ClearGrid(int handle)
    {
        List<Grid> gridList;
        if (!m_grides.TryGetValue(handle, out gridList))
            return;

        gridList.Clear();
    }

    public bool DrawGrid(int handle, Vector2 min, Vector2 max, Vector2 cellSize, float y, Color color)
    {
        List<Grid> gridList;
        if (!m_grides.TryGetValue(handle, out gridList))
            return false;
        Grid grid = new Grid();
        grid.min = min;
        grid.max = max;
        grid.cellsize = cellSize;
        grid.y = y;
        grid.color = color;
        grid.bDrawed = false;
        gridList.Add(grid);

        return true;
    }

    public void ClearRact2D(int handle)
    {
        List<Rect2D> rectList;
        if (!m_rects.TryGetValue(handle, out rectList))
            return;

        rectList.Clear();
    }

    public bool DrawRact2D(int handle, Vector2 min, Vector2 max, float y, Color color)
    {
        List<Rect2D> rectList;
        if (!m_rects.TryGetValue(handle, out rectList))
            return false;

        Rect2D rect = new Rect2D();
        rect.min = min;
        rect.max = max;
        rect.y = y;
        rect.color = color;
        rectList.Add(rect);

        return true;
    }

    public void Clear()
    {
        m_lines.Clear();
        m_circles.Clear();
        m_grides.Clear();
        m_rects.Clear();
    }

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
#if FORTEST
    
    int m_hDrawLine;
    int m_hDrawCircle;
    int m_hDrawGrid;
    int m_hDrawRect;
    Vector3 startPos;
    void Start () {

        m_hDrawLine = addDrawLine();
        m_hDrawCircle = addDrawCircle();
        m_hDrawGrid = addDrawGrid();
        m_hDrawRect = addDrawRact2D();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out hit, 1000))
                startPos = hit.point;
        }

        if(Input.GetMouseButton(0))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000))
                DrawCircle(m_hDrawCircle, startPos, (hit.point - startPos).magnitude, Color.blue);
        }

        ClearGrid(m_hDrawGrid);
        Vector2 min, max, cellSize;
        min.x = min.y = 0;
        max.x = 10;
        max.y = 10;
        cellSize.x = cellSize.y = 1;
        DrawGrid(m_hDrawGrid, min, max, cellSize, 0.01f, Color.green);

        min = new Vector2(2, 2);
        max = new Vector2(4, 6);
        DrawRact2D(m_hDrawRect, min, max, 0.01f, Color.green);
    }
#endif

    void OnPostRender()
    {
        CreateLineMaterial();
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        DrawLines();
        DrawCircles();
        DrawGrids();
        DrawRects();

        GL.PopMatrix();
    }

    void DrawLines()
    {
        GL.Begin(GL.LINES);
        foreach (var item in m_lines)
        {
            List<Line> lineList = item.Value;
            foreach (var line in lineList)
            {
                if (line.bDrawed) continue;
                GL.Color(line.color);
                GL.Vertex3(line.start.x, line.start.y, line.start.z);
                GL.Vertex3(line.end.x, line.end.y, line.end.z);
                line.bDrawed = true;
            }
			lineList.Clear();
        }
        GL.End();
    }

    void DrawCircles()
    {
        GL.Begin(GL.LINES);
        foreach (var item in m_circles)
        {
            List<Circle> roundList = item.Value;
            foreach (var round in roundList)
            {
                GL.Color(round.color);
                if (round.bDrawed) continue;
                for (int i = 0, j = NUM_SEG - 1; i < NUM_SEG; j = i++)
                {
                    GL.Vertex3(round.center.x + m_circleDir[j * 2 + 0] * round.radius, round.center.y, round.center.z + m_circleDir[j * 2 + 1] * round.radius);
                    GL.Vertex3(round.center.x + m_circleDir[i * 2 + 0] * round.radius, round.center.y, round.center.z + m_circleDir[i * 2 + 1] * round.radius);
                }
                round.bDrawed = true;
            }
            roundList.Clear();
        }
        GL.End();
    }
    void DrawGrids()
    {
        GL.Begin(GL.LINES);
        foreach (var item in m_grides)
        {
            List<Grid> gridList = item.Value;
            foreach (var grid in gridList)
            {
                GL.Color(grid.color);
                if (grid.bDrawed) continue;
                Vector3 from, to;
                from.y = to.y = grid.y;
                for (float x = grid.min.x; x <= grid.max.x; x += grid.cellsize.x)
                {
                    from.x = to.x = x;
                    from.z = grid.min.y; to.z = grid.max.y;
                    GL.Vertex3(from.x, from.y, from.z);
                    GL.Vertex3(to.x, to.y, to.z);
                }
                for (float z = grid.min.y; z <= grid.max.y; z += grid.cellsize.y)
                {
                    from.z = to.z = z;
                    from.x = grid.min.x; to.x = grid.max.x;
                    GL.Vertex3(from.x, from.y, from.z);
                    GL.Vertex3(to.x, to.y, to.z);
                }
                grid.bDrawed = true;
            }
            gridList.Clear();
        }
        GL.End();
    }

    void DrawRects()
    {
        GL.Begin(GL.TRIANGLES);

        Vector3 vert1;
        Vector3 vert2;
        Vector3 vert3;
        Vector3 vert4;
        foreach (var item in m_rects)
        {
            List<Rect2D> rectList = item.Value;
            foreach (var rect in rectList)
            {
                GL.Color(rect.color);
                vert1.y = vert2.y = vert3.y = vert4.y = rect.y;
                vert1.x = rect.min.x; vert1.z = rect.min.y;
                vert2.x = rect.max.x; vert2.z = rect.min.y;
                vert3.x = rect.min.x; vert3.z = rect.max.y;
                vert4.x = rect.max.x; vert4.z = rect.max.y;

                GL.Vertex3(vert1.x, vert1.y, vert1.z);
                GL.Vertex3(vert4.x, vert4.y, vert4.z);
                GL.Vertex3(vert2.x, vert2.y, vert2.z);

                GL.Vertex3(vert1.x, vert1.y, vert1.z);
                GL.Vertex3(vert3.x, vert3.y, vert3.z);
                GL.Vertex3(vert4.x, vert4.y, vert4.z);
            }
            rectList.Clear();
        }
        GL.End();
    }


}

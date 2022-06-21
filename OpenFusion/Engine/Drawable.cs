using System;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;

namespace OpenFusion.Engine
{
    public class Drawable
    {
        public Shader _shader;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private const int faggot = 4;

        private float[] _vertices = new float[8];

        /*float[] _vertices = {
            -0.5f,0.5f,
            0.5f,0.5f,
            0.5f,-0.5f,
            -0.5f,-0.5f,
        };*/


        private int xPos;
        private int yPos;
        private int xSize;
        private int ySize;
        public Drawable(int xPosition, int yPosition,int width,int height)
        {
            xPos = xPosition;
            yPos = yPosition;
            this.xSize = width;
            this.ySize = height;




        }

        float ToOpenGLSpaceX(int x,int width)
        {
            return (x - width / 2f) / (width / 2f);
        }

        float ToOpenGLSpaceY(int y, int height)
        {
            return (y-height / 2f) / (height / 2f)*-1;
        }
        void AdjustVertices(int width,int height)
        {
            float num1=ToOpenGLSpaceX(xPos,width);
            float num2 = ToOpenGLSpaceY(yPos+ySize, height);
            float num3 = ToOpenGLSpaceX(xPos+xSize,width);
            float num4=ToOpenGLSpaceY(yPos, height);
            
            //first
            _vertices[0] = num1;
            _vertices[1] = num4;
            //second
            _vertices[2] = num3;
            _vertices[3] = num4;
            //third
            _vertices[4] = num3;
            _vertices[5] = num2;
            //fourth
            _vertices[6] = num1;
            _vertices[7] = num2;



            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }
        public void Init()
        {
            /*_vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);*/


            _shader = new Shader("shader.vert", "shader.frag");

           
            _shader.Use();
        }
        public void OnRender(MainWindow window)
        {
            AdjustVertices((int)window.OpenTkControl.ActualWidth,(int)window.OpenTkControl.ActualHeight);
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }
        
        
    }
}
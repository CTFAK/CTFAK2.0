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
        float[] _vertices = {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };



        public Drawable(float xOffset,float yOffset)
        {
            /*_vertices[0] -= xOffset;
            _vertices[3] -= xOffset;
            _vertices[6] -= xOffset;
            _vertices[9] -= xOffset;

            _vertices[1] -= yOffset;
            _vertices[4] -= yOffset;
            _vertices[7] -= yOffset;
            _vertices[10] -= yOffset;*/
        }
        public void Init()
        {
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);


            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);


            _shader = new Shader("shader.vert", "shader.frag");

           
            _shader.Use();
        }
        public void OnRender()
        {
            _shader.Use();

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }
        
        
    }
}
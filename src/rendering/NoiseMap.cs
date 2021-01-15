using perlin_noise_visualization.src.shaders;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using perlin_noise_visualization.src.others;

using System;

namespace perlin_noise_visualization.src.rendering {

    class NoiseMap : Shader {

        int Resolution = 400;
        float[] vertices;
        float seed = new Random().Next(-1000000,1000000);
        Noise noise = new Noise();

        public NoiseMap(string v, string f) : base(v,f) {

            int R = Resolution/2;
            List<float> vertices_list = new List<float>();

            for (int x = -R; x <= R; x++) {
                for (int y = -R; y <= R; y++) {
                    Vector2 toScreenCoords = new Vector2((float)x/(float)R, (float)y/(float)R);
                    float multiplier = CreateNoiseLayer(5, 2, 0.5f, toScreenCoords.X, toScreenCoords.Y, seed);
                    vertices_list.Add(toScreenCoords.X);
                    vertices_list.Add(toScreenCoords.Y);
                    vertices_list.Add(1*multiplier);
                    vertices_list.Add(1*multiplier);
                    vertices_list.Add(1*multiplier);
                }   
            }

            vertices = new float[vertices_list.Count];
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = vertices_list[i];
            }

            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
        }

        float CreateNoiseLayer(int octaves, float lac, float per, float x, float y, float s) {

            float frequency = 2,
                  amplitude = 10;

            float n = 0;

            for (int i = 0; i < octaves; i++) {
                n += (float)noise.noise(x*frequency, y*frequency, s)*amplitude;

                frequency *= lac;
                amplitude *= per;
            }
            return n+.4f;
        }

        public override void Render() {
            base.Render();
            GL.Enable(EnableCap.ProgramPointSize);

            int R = Resolution/2;
            List<float> vertices_list = new List<float>();

            for (int x = -R; x <= R; x++) {
                for (int y = -R; y <= R; y++) {
                    Vector2 toScreenCoords = new Vector2((float)x/(float)R, (float)y/(float)R);
                    float multiplier = CreateNoiseLayer(5, 2, 0.5f, toScreenCoords.X+R, toScreenCoords.Y+R, seed);
                    vertices_list.Add(toScreenCoords.X);
                    vertices_list.Add(toScreenCoords.Y);
                    vertices_list.Add(1*multiplier);
                    vertices_list.Add(1*multiplier);
                    vertices_list.Add(1*multiplier);
                }   
            }

            vertices = new float[vertices_list.Count];
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = vertices_list[i];
            }
            seed += .05f;
            Console.WriteLine(seed);

            Use();
            int pointSizeLocation = GL.GetUniformLocation(Program, "pointSize");
            GL.Uniform1(pointSizeLocation, (float)Scene.ScreenSize.X/Resolution);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length);
        }
    }
}
/*******************************************************************************
*
* Copyright (C) 2013-2014 Frozen North Computing
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FrozenNorth.OpenGL.FN2D
{
	public enum FN2DTessellation
	{
		CoreFade = 0,
		Core = 1,
		OuterFade = 2,
		InnerFade = 3
	}

	public enum FN2DLineCap
	{
		Butt = 0,
		Round = 1,
		Square = 2,
		Rect = 3
	}
			
	public enum FN2DLineJoin
	{
		Miter = 0,
		Bevel = 1,
		Round = 2
	}
			
	public class FN2DPolyLine
	{
		private const float ApproximationCutoff = 1.6f;
	
		protected List<Point> points = new List<Point>();
		protected Color color;
		protected float width;
		protected FN2DTessellation tessellation;
		protected FN2DLineJoin lineJoin;
		protected FN2DLineCap startCap;
		protected FN2DLineCap endCap;
		
		public FN2DPolyLine(Color color, float width, FN2DTessellation tessellation, FN2DLineJoin lineJoin,
							FN2DLineCap startCap, FN2DLineCap endCap)
		{
			this.color = color;
			this.width = width;
			this.tessellation = tessellation;
			this.lineJoin = lineJoin;
			this.startCap = startCap;
			this.endCap = endCap;
		}
		
		double PointLength(Point p)
		{
			return Math.Sqrt(p.X * p.X + p.Y * p.Y);
		}
		
		double PointNormalize(Point p)
		{
			double length = PointLength(p);
			if (length > double.Epsilon)
			{
				p.X /= length;
				p.Y /= length;
			}
			return length;
		}
		
		public void Refresh()
		{
			if (width < ApproximationCutoff)
			{
				Exact();
				return;
			}

			int a = 0;
			int b = 0;
			bool on = false;
			for (int i = 1; i < points.Count - 1; i++)
			{
				Point v1 = points[i] - points[i - 1];
				Point v2 = points[i + 1] - points[i];
				double len = PointNormalize(v1) * 0.5;
				len += PointNormalize(v2) * 0.5;
				double costho = v1.X * v2.X + v1.Y * v2.Y;
				const double cos_a = Math.Cos(15 * Math.PI / 180);
				const double cos_b = Math.Cos(10 * Math.PI / 180);
				const double cos_c = Math.Cos(25 * Math.PI / 180);
				bool approx = (width < 7 && costho > cos_a) || (costho > cos_b) ||  (len < width && costho > cos_c);
				if (approx && !on)
				{
					a = (i == 1) ? 0 : i;
					on = true;
					if (a > 1)
						Range(b, a, false);
				}
				else if (!approx && on)
				{
					b = i;
					on = false;
					Range(a, b, true);
				}
			}
			if (on && b < points.Count - 1)
			{
				b = points.Count - 1;
				Range(a, b, true);
			}
			else if (!on && a < points.Count - 1)
			{
				a = points.Count - 1;
				Range(b, a, false);
			}
		}
		
		private Point PointInter(int index, double t)
		{
			if (t == 0)
			{
				return points[index];
			}
			if (t == 1)
			{
				return points[index + 1];
			}
			return new Point(points[index].X * points[index + 1].X * t, points[index].Y * points[index + 1].Y * t);
		}
		
		private void Exact()
		{
			Point mid_l, mid_n; //the last and the next mid point
			Color c_l, c_n;
			double w_l, w_n;
			mid_l = PointInter(0, 0.5);
		
			st_anchor SA;
			if (points.Count == 2)
			{
				segment();
			}
			else
			{
				for (int i = 1; i < points.Count - 1; i++)
				{
					if ( i==size_of_P-2 && !join_last)
						mid_n = PointInter(i, 1.0);
					else
						mid_n = PointInter(i, 0.5);
			
					SA.P[0]=mid_l.vec(); SA.C[0]=c_l;  SA.W[0]=w_l;
					SA.P[2]=mid_n.vec(); SA.C[2]=c_n;  SA.W[2]=w_n;
			
					SA.P[1]=P[i];
					SA.C[1]=color(i);
					SA.W[1]=weight(i);
			
					anchor( SA, opt, i==1&&cap_first, i==size_of_P-2&&cap_last);
			
					mid_l = mid_n;
					c_l = c_n;
					w_l = w_n;
				}
			}
			//draw or not
			if( opt && opt->tess && opt->tess->tessellate_only && opt->tess->holder)
				(*(vertex_array_holder*)opt->tess->holder).push(SA.vah);
			else
				SA.vah.draw();
			//draw triangles
			if( opt && opt->tess && opt->tess->triangulation)
				SA.vah.draw_triangles();
		}
		
		//the struct to hold info for anchor_late() to perform triangluation
		private class PolyLine
		{
			//for all joints
			Point vP; //vector to intersection point
			Point vR; //fading vector at sharp end
				//all vP,vR are outward
			
			//for djoint==PLJ_bevel
			Point T; //core thickness of a line
			Point R; //fading edge of a line
			Point bR; //out stepping vector, same direction as cap
			Point T1,R1; //alternate vectors, same direction as T21
				//all T,R,T1,R1 are outward
			
			//for djoint==PLJ_round
			float t,r;
			
			//for degeneration case
			bool degenT; //core degenerated
			bool degenR; //fade degenerated
			bool pre_full; //draw the preceding segment in full
			Point PT,PR;
			float pt; //parameter at intersection
			bool R_full_degen;
			
			char djoint; //determined joint
					// e.g. originally a joint is PLJ_miter. but it is smaller than critical angle, should then set djoint to PLJ_bevel
		}
		
		private class Anchor
		{
			Point P = new Point[3];
			Point cap_start, cap_end;
			PolyLine SL = new PolyLine[3];
			vertex_array_holder vah;
		}
	}
}
